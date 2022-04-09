using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Xml;
using System.Reflection;

namespace MaxyGames.uNode.Editors {
	public class CommandWindow : EditorWindow {
		#region Styles
		private static class Styles {
			public static readonly GUIStyle textAreaStyle;

			// Default background Color(0.76f, 0.76f, 0.76f)
			private static readonly Color bgColorLightSkin = new Color(0.87f, 0.87f, 0.87f);
			// Default background Color(0.22f, 0.22f, 0.22f)
			private static readonly Color bgColorDarkSkin = new Color(0.2f, 0.2f, 0.2f);
			// Default text Color(0.0f, 0.0f, 0.0f)
			private static readonly Color textColorLightSkin = new Color(0.0f, 0.0f, 0.0f);
			// Default text Color(0.706f, 0.706f, 0.706f)
			private static readonly Color textColorDarkSkin = new Color(0.706f, 0.706f, 0.706f);

			private static Texture2D _backgroundTexture;
			public static Texture2D backgroundTexture {
				get {
					if(_backgroundTexture == null) {
						_backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
						_backgroundTexture.SetPixel(0, 0, EditorGUIUtility.isProSkin ? bgColorDarkSkin : bgColorLightSkin);
						_backgroundTexture.Apply();
					}
					return _backgroundTexture;
				}
			}

			static Styles() {
				textAreaStyle = new GUIStyle(EditorStyles.textArea);
				textAreaStyle.padding = new RectOffset();

				var style = textAreaStyle.focused;
				style.background = backgroundTexture;
				style.textColor = EditorGUIUtility.isProSkin ? textColorDarkSkin : textColorLightSkin;

				textAreaStyle.focused = style;
				textAreaStyle.active = style;
				textAreaStyle.onActive = style;
				textAreaStyle.hover = style;
				textAreaStyle.normal = style;
				textAreaStyle.onNormal = style;
			}
		}
		#endregion

		#region Create Window
		public static CommandWindow CreateWindow(Rect position, Func<CompletionInfo[], bool> onConfirm, CompletionEvaluator.CompletionSetting setting = null) {
			var window = CreateInstance(typeof(CommandWindow)) as CommandWindow;
			window.onConfirm = onConfirm;
			if(setting != null) {
				window.completionEvaluator = new CompletionEvaluator(setting);
			} else {
				window.completionEvaluator = new CompletionEvaluator();
			}
			window.ShowAsDropDown(position, new Vector2(200, 100));
			return window;
		}

		public static CommandWindow CreateWindow(Vector2 position, Func<CompletionInfo[], bool> onConfirm, CompletionEvaluator.CompletionSetting setting = null) {
			var window = CreateInstance(typeof(CommandWindow)) as CommandWindow;
			window.onConfirm = onConfirm;
			if(setting != null) {
				window.completionEvaluator = new CompletionEvaluator(setting);
			} else {
				window.completionEvaluator = new CompletionEvaluator();
			}
			Vector2 windowSize = new Vector2(300, 20);
			window.ShowAsDropDown(WindowUtility.MousePosToRect(position, Vector2.zero), windowSize);
			return window;
		}
		#endregion

		private const string textAreaControlName = "-TextArea-";

		private string _text = "";
		private string text {
			get {
				return textEditor.text;
			}
			set {
				textEditor.text = value;
				_text = value;
			}
		}

		private Func<CompletionInfo[], bool> onConfirm;

		private TooltipWindow tooltipWindow;

		[SerializeField]
		private TextEditor textEditor;
		[SerializeField]
		private CompletionEvaluator completionEvaluator;
		[SerializeField]
		private CompletionInfo[] completions;
		[SerializeField]
		private List<CompletionInfo> memberPaths;
		[SerializeField]
		private AutocompleteBox autocompleteBox;

		[SerializeField]
		private List<string> inputHistory = new List<string>();
		private int positionInHistory;

		public int overrideIndex { get; private set; }
		private int newOverrideIndex;

		private bool requestMoveCursorToEnd;
		private bool requestFocusOnTextArea;
		private bool _hasFocus, confirm;

		private string input = "";
		private string lastWord = "";
		private string savedInput;

		private float currentHeight;

		private Vector2 lastCursorPos;

		private void OnEnable() {
			ClearText();
			requestFocusOnTextArea = true;
			autocompleteBox = new AutocompleteBox();

			ScheduleMoveCursorToEnd();
			autocompleteBox.onConfirm += OnAutocompleteConfirm;
			autocompleteBox.Clear();
		}
		private void OnDisable() {
			if(tooltipWindow != null) {
				tooltipWindow.Close();
			}
		}

		private void ClearText() {
			if(textEditor != null) {
				text = "";
			}
		}

		private void OnAutocompleteConfirm(CompletionInfo confirmedInput) {
			string oldText = text;
			string str = confirmedInput.name.ToLower();
			while(str.Length > 0) {
				if(text.EndsWith(".")) {
					text += confirmedInput.name;
				} else if(text.ToLower().EndsWith(str)) {
					int index = text.ToLower().LastIndexOf(str);
					text = text.Remove(index);
					text += confirmedInput.name;
					break;
				} else {
					str = str.RemoveLast();
				}
			}
			lastWord = text;
			textEditor.MoveTextEnd();
			confirm = oldText.Equals(lastWord);
			if(confirm) {
				if(onConfirm != null) {
					completionEvaluator.Evaluate(text, confirmedInput, (infos) => {
						if(onConfirm(infos)) {
							Close();
						}
					});
				}
			}

		}

		private void OnInspectorUpdate() {
			Repaint();
		}

		private void OnGUI() {
			textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

			autocompleteBox.HandleEvents();
			HandleHistory();
			DoAutoComplete();
			HandleRequests();

			//{//Border
			//	GUI.color = new Color(0.78f, 0.78f, 0.78f);
			//	GUI.Label(new Rect(2, 2, position.width - 4, position.height - 4), "", Styles.resultsBorderStyle);
			//	GUI.color = Color.white;
			//}
			//Draw GUI
			//GUILayout.BeginArea(new Rect(1, 4, position.width - 2, position.height - 8));
			{
				float height = 20;
				EditorGUILayout.BeginVertical();
				{//Draw TextField
					var current = Event.current;

					if(!_hasFocus) {
						GUI.FocusControl(textAreaControlName);
						if(current.type == EventType.Repaint) {
							_hasFocus = true;
							textEditor.text = _text ?? "";
							textEditor.MoveTextEnd();
						}
					}
					if(current.type == EventType.KeyDown) {
						if(current.keyCode == KeyCode.Return && !current.shift) {
							textEditor.MoveTextEnd();
						} else if(current.keyCode == KeyCode.Tab) {
							current.Use();
							_hasFocus = false;
							textEditor.MoveTextEnd();
						}
					}
					GUI.SetNextControlName(textAreaControlName);
					GUILayout.TextField(text, EditorStyles.textField, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
				}
				completions = completionEvaluator.completions;
				if(completions != null && completions.Length > 0) {
					autocompleteBox.results = completions;
					bool flag = true;
					if(completions.Length == 1) {
						if(completions[0].name.Trim() == text.Trim() ||
							completions[0].isSymbol ||
							completions[0].isDot ||
							completions[0].kind == CompletionKind.Literal) {
							flag = false;
						}
					}
					if(flag) {
						height += height * completions.Length + 3;
						if(height > 300) {
							height = 300;
						}
						Rect completionRect = uNodeGUIUtility.GetRect(EditorGUIUtility.fieldWidth, height - 20);
						autocompleteBox.OnGUI(lastWord, completionRect);
					}
				}
				if(Event.current.type == EventType.KeyUp) {
					if(Event.current.keyCode == KeyCode.UpArrow) {
						newOverrideIndex--;
						if(newOverrideIndex < 0) {
							newOverrideIndex = 0;
						}
						Event.current.Use();
					} else if(Event.current.keyCode == KeyCode.DownArrow) {
						newOverrideIndex++;
						Event.current.Use();
					}
				}
				if(Event.current.type == EventType.Repaint && (memberPaths != completionEvaluator.memberPaths || newOverrideIndex != overrideIndex)) {
					overrideIndex = newOverrideIndex;
					memberPaths = completionEvaluator.memberPaths;
					bool closeTooltip = true;
					MethodBase methodBase = null;
					int numOfOverload = 0;
					if(completionEvaluator.memberPaths.Count > 0) {
						bool flag = false;
						for(int i = memberPaths.Count - 1; i > 0; i--) {
							var mPath = memberPaths[i];
							if(mPath.isSymbol) {
								switch(mPath.name) {
									case "(":
									case "<": {//For constructor, function and genric.
										var member = memberPaths[i - 1];
										if(member.member is MethodInfo) {
											MethodInfo[] memberInfos = null;
											if(member.member.ReflectedType != null) {
												memberInfos = member.member.ReflectedType.GetMethods();
											} else if(member.member.DeclaringType != null) {
												memberInfos = member.member.DeclaringType.GetMethods();
											}
											if(memberInfos != null) {
												memberInfos = memberInfos.Where(m =>
													m.Name.Equals(member.name)).ToArray();
												if(memberInfos != null && memberInfos.Length > 0) {
													if(overrideIndex + 1 > memberInfos.Length) {
														overrideIndex = memberInfos.Length - 1;
														newOverrideIndex = overrideIndex;
													}
													methodBase = memberInfos[overrideIndex];
													numOfOverload = memberInfos.Length;
												}
											}
										} else if(member.member is ConstructorInfo) {
											ConstructorInfo[] memberInfos = null;
											if(member.member.ReflectedType != null) {
												memberInfos = member.member.ReflectedType.GetConstructors(
													BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
											} else if(member.member.DeclaringType != null) {
												memberInfos = member.member.DeclaringType.GetConstructors(
													BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
											}
											if(memberInfos != null) {
												memberInfos = memberInfos.Where(m =>
													m.Name.Equals(member.name)).ToArray();
												if(memberInfos != null && memberInfos.Length > 0) {
													if(overrideIndex + 1 > memberInfos.Length) {
														overrideIndex = memberInfos.Length - 1;
														newOverrideIndex = overrideIndex;
													}
													methodBase = memberInfos[overrideIndex];
													numOfOverload = memberInfos.Length;
												}
											}
										}
										break;
									}
									case "["://For indexer

										break;
									case ")":
									case ">":
									case "]":
										flag = true;
										break;
								}
							}
							if(methodBase != null || flag)
								break;
						}
					}
					List<GUIContent> contents = new List<GUIContent>();
					if(autocompleteBox.selectedCompletion != null) {
						var selectedCompletion = autocompleteBox.selectedCompletion;
						switch(selectedCompletion.kind) {
							case CompletionKind.Type: {
								Type type = selectedCompletion.member as Type;
								if(type != null) {
									contents.Add(new GUIContent(type.PrettyName(true), uNodeEditorUtility.GetIcon(type)));
								}
								break;
							}
							case CompletionKind.Field: {
								FieldInfo field = selectedCompletion.member as FieldInfo;
								if(field != null) {

								}
								break;
							}
							case CompletionKind.Property: {
								PropertyInfo property = selectedCompletion.member as PropertyInfo;
								if(property != null) {

								}
								break;
							}
							case CompletionKind.Method: {
								MethodInfo method = selectedCompletion.member as MethodInfo;
								if(method != null && method != methodBase) {
									ResolveMethodTooltip(method, numOfOverload, contents);
								}
								break;
							}
						}
						if(contents.Count > 0) {
							contents.Add(new GUIContent(""));
						}
					}
					if(methodBase != null) {
						ResolveMethodTooltip(methodBase, numOfOverload, contents);
					}

					if(contents.Count > 0) {
						GUIContent c = null;
						for(int i = 0; i < contents.Count; i++) {
							if(c == null ||
								uNodeEditorUtility.RemoveHTMLTag(c.text).Length <
								uNodeEditorUtility.RemoveHTMLTag(contents[i].text).Length) {
								c = contents[i];
							}
						}
						float width = uNodeGUIStyle.RichLabel.CalcSize(c).x + 20;
						if(position.x + position.width + width <= Screen.currentResolution.width) {
							tooltipWindow = TooltipWindow.Show(new Vector2(position.x + position.width, position.y), contents, width);
						} else {
							tooltipWindow = TooltipWindow.Show(new Vector2(position.x - width, position.y), contents, width);
						}
						closeTooltip = false;
					}
					if(closeTooltip && tooltipWindow != null) {
						tooltipWindow.Close();
					}
				}
				uNodeGUIUtility.GetRect(EditorGUIUtility.fieldWidth, 3);
				EditorGUILayout.EndVertical();

				if(Event.current.type == EventType.Repaint && currentHeight != height) {
					currentHeight = height;
					ShowAsDropDown(new Rect(position.x, position.y, 0, 0), new Vector2(position.width, currentHeight));
				}

			}
			//GUILayout.EndArea();
		}

		private void ResolveMethodTooltip(MethodBase methodBase, int numOfOverload, List<GUIContent> contents) {
			if(methodBase is MethodInfo) {
				contents.Add(new GUIContent(
					EditorReflectionUtility.GetOverloadingMethodNames(methodBase as MethodInfo),
					uNodeEditorUtility.GetIcon(methodBase)));
			} else if(methodBase is ConstructorInfo) {
				contents.Add(new GUIContent(
					EditorReflectionUtility.GetOverloadingConstructorNames(methodBase as ConstructorInfo),
					uNodeEditorUtility.GetIcon(methodBase)));
			}
			var mType = ReflectionUtils.GetMemberType(methodBase);
			#region Docs
			if(XmlDoc.hasLoadDoc) {
				XmlElement documentation = XmlDoc.XMLFromMember(methodBase);
				if(documentation != null) {
					contents.Add(new GUIContent("Documentation ▼ " + documentation["summary"].InnerText.Trim().AddLineInFirst()));
				}
				var parameters = methodBase.GetParameters();
				if(parameters.Length > 0) {
					for(int x = 0; x < parameters.Length; x++) {
						Type PType = parameters[x].ParameterType;
						if(PType != null) {
							contents.Add(new GUIContent(parameters[x].Name + " : " +
								uNodeUtility.GetDisplayName(PType),
								uNodeEditorUtility.GetTypeIcon(PType)));
							if(documentation != null && documentation["param"] != null) {
								XmlNode paramDoc = null;
								XmlNode doc = documentation["param"];
								while(doc.NextSibling != null) {
									if(doc.Attributes["name"] != null && doc.Attributes["name"].Value.Equals(parameters[x].Name)) {
										paramDoc = doc;
										break;
									}
									doc = doc.NextSibling;
								}
								if(paramDoc != null && !string.IsNullOrEmpty(paramDoc.InnerText)) {
									contents.Add(new GUIContent(paramDoc.InnerText.Trim()));
								}
							}
						}
					}
				}
			}
			#endregion
			//contents.Add(new GUIContent("Return	: " + mType.PrettyName(true), uNodeEditorUtility.GetTypeIcon(mType)));
			if(numOfOverload > 0)
				contents.Add(new GUIContent("▲ " + (overrideIndex + 1).ToString() + " of " + numOfOverload + " ▼"));
		}

		private void HandleHistory() {
			var current = Event.current;
			if(current.type == EventType.KeyDown) {
				var changed = false;
				if(current.keyCode == KeyCode.DownArrow) {
					positionInHistory++;
					changed = true;
					current.Use();
				}
				if(current.keyCode == KeyCode.UpArrow) {
					positionInHistory--;
					changed = true;
					current.Use();
				}

				if(changed) {
					if(savedInput == null) {
						savedInput = input;
					}

					if(positionInHistory < 0) {
						positionInHistory = 0;
					} else if(positionInHistory >= inputHistory.Count) {
						ReplaceCurrentCommand(savedInput);
						positionInHistory = inputHistory.Count;
						savedInput = null;
					} else {
						ReplaceCurrentCommand(inputHistory[positionInHistory]);
					}
				}
			}
		}

		private void ReplaceCurrentCommand(string replacement) {
			text = text.Substring(0, text.Length - input.Length);
			text += replacement;
			textEditor.MoveTextEnd();
		}

		private void DoAutoComplete() {
			var newInput = GetInput();
			if(newInput != null && input != newInput) {
				input = newInput;

				lastWord = input;
				//var lastWordIndex = input.LastIndexOfAny(new[] { ' ' });
				//if(lastWordIndex != -1) {
				//	lastWord = input.Substring(lastWordIndex + 1);
				//}

				completionEvaluator.SetInput(lastWord);
			}
		}

		private string GetInput() {
			return text;
		}

		private void HandleRequests() {
			var current = Event.current;
			if(requestMoveCursorToEnd && current.type == EventType.Repaint) {
				textEditor.MoveTextEnd();
				requestMoveCursorToEnd = false;
				Repaint();
			} else if(focusedWindow == this && requestFocusOnTextArea) {
				GUI.FocusControl(textAreaControlName);
				requestFocusOnTextArea = false;
				Repaint();
			}

			var cursorPos = textEditor.graphicalCursorPos;

			lastCursorPos = cursorPos;
		}

		private void ScheduleMoveCursorToEnd() {
			requestMoveCursorToEnd = true;
		}

		private void Append(object result) {
			text += "\n" + result + "\n";
		}
	}
}