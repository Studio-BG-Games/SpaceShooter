﻿using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace MaxyGames.uNode.Editors {
	public class PreviewSourceWindow : EditorWindow {
		private static PreviewSourceWindow window;

		public ScriptInformation[] informations;

		protected ScriptInformation[] selectedInfos;

		static GUIStyle m_RichLabel;
		public static GUIStyle RichLabel {
			get {
				if(m_RichLabel == null) {
					m_RichLabel = new GUIStyle(GUI.skin.label);
					m_RichLabel.richText = true;
					m_RichLabel.padding = new RectOffset(0, 0, 0, 0);
				}
				return m_RichLabel;
			}
		}

		static GUIStyle m_Lines;
		public static GUIStyle LinesStyle {
			get {
				if(m_Lines == null) {
					m_Lines = new GUIStyle(GUI.skin.label);
					m_Lines.richText = true;
					m_Lines.padding = new RectOffset(0, 0, 0, 0);
					m_Lines.alignment = TextAnchor.UpperRight;
				}
				return m_Lines;
			}
		}

		static Texture2D m_LineBackground;
		public static Texture2D LineBackground {
			get {
				if(m_LineBackground == null) {
					m_LineBackground = uNodeEditorUtility.MakeTexture(1, 1, Color.gray);
				}
				return m_LineBackground;
			}
		}

		static Texture2D m_selectionTexture;
		protected static Texture selectionTexture {
			get {
				if(m_selectionTexture == null) {
					m_selectionTexture = uNodeEditorUtility.MakeTexture(1, 1, new Color(0, 0.5f, 1, 0.3f));
				}
				return m_selectionTexture;
			}
		}

		static Texture2D m_backgroundTexture;
		protected static Texture backgroundTexture {
			get {
				if(m_backgroundTexture == null) {
					m_backgroundTexture = uNodeEditorUtility.MakeTexture(1, 1, EditorGUIUtility.isProSkin ? new Color(.2f, .2f, .2f) : new Color(.85f, .85f, .85f));
				}
				return m_backgroundTexture;
			}
		}

		protected bool focus;
		protected string[] lines;
		protected string[] pureLines;
		protected string script;
		protected CompileResult compileResult;
		protected Vector2 scrollPos;

		public static PreviewSourceWindow ShowWindow(string highlightedScript, string originalScript) {
			window = GetWindow(typeof(PreviewSourceWindow), true) as PreviewSourceWindow;
			window.minSize = new Vector2(500, 400);
			window.focus = true;
			{
				window.lines = highlightedScript.Split('\n');
				window.pureLines = originalScript.Split('\n');
				window.script = originalScript;
				window.compileResult = null;
			}
			window.titleContent = new GUIContent("C# Preview");
			window.Show();
			return window;
		}

		protected virtual void OnEnable() {
			uNodeEditor.onSelectionChanged -= OnChanged;
			uNodeEditor.onSelectionChanged += OnChanged;
		}

		protected virtual  void OnDisable() {
			uNodeEditor.onSelectionChanged -= OnChanged;
		}

		protected void OnChanged(GraphEditorData editorData) {
			if(informations != null) {
				if(editorData.selected == editorData.selectedNodes && editorData.selectedNodes.Count > 0) {
					var ids = editorData.selectedNodes.Select(n => n.GetInstanceID().ToString());
					selectedInfos = informations.Where(info => ids.Contains(info.id)).ToArray();
				} else if(editorData.selected is Component comp) {
					selectedInfos = informations.Where(info => info.id == comp.GetInstanceID().ToString()).ToArray();
				} else if(editorData.selected is VariableData variable) {
					selectedInfos = informations.Where(info => info.id == CG.KEY_INFORMATION_VARIABLE + variable.Name).ToArray();
				} else {
					selectedInfos = null;
				}
				Repaint();
			}
		}

		void OnGUI() {
			if(Event.current.type == EventType.Repaint) {
				GUI.DrawTexture(new Rect(-10, -10, position.width + 100, position.height + 100), backgroundTexture);
			}
			DrawToolbar();
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			ShowPreviewSource(lines, pureLines, informations, ref selectedInfos);
			EditorGUILayout.EndScrollView();
			DrawErrors();
			if(Event.current.type == EventType.MouseDown && Event.current.button == 1) {
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Copy"), false, () => {
					uNodeEditorUtility.CopyToClipboard(script);
				});
				menu.ShowAsContext();
			}
			if(!focus) {
				GUI.FocusControl(null);
				EditorGUI.FocusTextInControl(null);
				focus = true;
			}
		}

		protected void DrawToolbar() {
			using(new EditorGUILayout.HorizontalScope()) {
				if(GUILayout.Button("Copy To Clipboard", EditorStyles.toolbarButton)) {
					uNodeEditorUtility.CopyToClipboard(script);
				}
				if(GUILayout.Button("Check Errors", EditorStyles.toolbarButton)) {
					try {
						EditorUtility.DisplayProgressBar("Loading", "Compiling Scripts", 1);
						compileResult = GenerationUtility.CompileScript(script);
						if(compileResult.errors != null && 
							uNodePreference.preferenceData.generatorData.compilationMethod == CompilationMethod.Roslyn && 
							System.IO.File.Exists(GenerationUtility.tempAssemblyPath)) {
							var errors = compileResult.errors.ToList();
							if(errors.Count > 0) {
								if(System.IO.Directory.Exists(GenerationUtility.projectScriptPath)) {
									errors.Insert(0, new CompileResult.CompileError() {
										isWarning = true,
										errorText = $"Warning: You're using Roslyn Compilation method but there's a generated script located on: '{GenerationUtility.projectScriptPath}' folder, please delete it to ensure 'Check Errors' is working correctly.\nIf you're not delete the folder the 'Check Errors' will have error like a Type is exist in 2 assembly."
									});
								}
								compileResult.errors = errors;
							}
						}
					} finally {
						EditorUtility.ClearProgressBar();
					}
				}
			}
		}

		Vector2 errorScroolPos;
		protected void DrawErrors() {
			if(compileResult == null || compileResult.errors == null) return;
			GUILayout.FlexibleSpace();
			errorScroolPos = EditorGUILayout.BeginScrollView(errorScroolPos);
			foreach(var error in compileResult.errors) {
				Rect position = EditorGUILayout.BeginVertical();
				EditorGUILayout.HelpBox(error.errorMessage, error.isWarning ? MessageType.Warning : MessageType.Error);
				EditorGUILayout.EndVertical();
				if(Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition)) {
					uNodeEditor.HighlightNode(informations, error.errorLine - 1, error.errorColumn - 1);
					// Debug.Log(error.errorMessage);
					// Debug.Log(pureLines[error.errorLine - 1]);
					// Debug.Log(pureLines[error.errorLine - 1][error.errorColumn - 1]);
				}
			}
			EditorGUILayout.EndScrollView();
		}

		private static float tabWidth, spaceWidth;
		public static void ShowPreviewSource(string[] lines, string[] pureLines, ScriptInformation[] infos, ref ScriptInformation[] selectedInfos) {
			if (lines == null || lines.Length == 0) {
				return;
			}
			if(pureLines == null || pureLines.Length != lines.Length) {
				pureLines = lines;
			}
			tabWidth = RichLabel.CalcSize(new GUIContent("\t")).x;
			spaceWidth = RichLabel.CalcSize(new GUIContent(" ")).x;
			var lineWidth = LinesStyle.CalcSize(new GUIContent(lines.Length.ToString())).x;
			Rect area = EditorGUILayout.BeginVertical();
			ScriptInformation clickedInfo = null;
			for (int i = 0; i < lines.Length; i++) {
				Rect rect = uNodeGUIUtility.GetRect(new GUIContent(lines[i]), RichLabel);
				EditorGUI.LabelField(new Rect(rect.x, rect.y, lineWidth, rect.height), (i + 1).ToString(), LinesStyle);
				rect.x += lineWidth + 6;
				rect.width -= lineWidth + 6;
				if (Event.current.type == EventType.Repaint) {
					string label = lines[i].Replace("\t", "      ");
					// string label = lines[i];
					RichLabel.Draw(rect, label, false, false, false, false);
					if(selectedInfos == null) continue;
					CalculatePosition(rect, pureLines[i], i, selectedInfos, (position, info) => {
						GUI.DrawTexture(position, selectionTexture);
					});
				} else if(Event.current.type == EventType.MouseDown) {
					if(infos == null) continue;
					CalculatePosition(rect, pureLines[i], i, infos, (position, info) => {
						if(position.Contains(Event.current.mousePosition)) {
							if(clickedInfo != null) {
								if(clickedInfo.lineRange > info.lineRange || clickedInfo.lineRange == info.lineRange && clickedInfo.columnRange >= info.columnRange) {
									clickedInfo = info;
								}
							} else {
								clickedInfo = info;
							}
						}
					});
				}
			}
			if(clickedInfo != null) {
				selectedInfos = new ScriptInformation[] { clickedInfo };
				if(int.TryParse(clickedInfo.id, out var id)) {
					var obj = EditorUtility.InstanceIDToObject(id);
					if(obj is NodeComponent) {
						uNodeEditor.HighlightNode(obj as NodeComponent);
					} else if(obj is RootObject) {
						var root = obj as RootObject;
						if(root.startNode != null) {
							uNodeEditor.HighlightNode(root.startNode);
						} else {
							uNodeEditor.Open(root);
							uNodeEditor.window.Refresh();
						}
					}
				} else if(clickedInfo.id.StartsWith(CG.KEY_INFORMATION_VARIABLE)) {
					
				}
				window?.Repaint();
			}
			EditorGUILayout.EndVertical();
			GUI.Box(new Rect(area.x + lineWidth, area.y, 2, area.height), "");
		}

		private static void CalculatePosition(Rect rect, string pureLabel, int currentLine, ScriptInformation[] infos, Action<Rect, ScriptInformation> action) {
			foreach(var info in infos) {
				if (info != null && info.startLine <= currentLine && info.endLine >= currentLine) {
					var position = rect;
					int startColumn = 0;
					int endColumn = pureLabel.Length - 1;
					position.width = RichLabel.CalcSize(new GUIContent(pureLabel)).x;
					if (info.startLine == currentLine || info.endLine == currentLine) {
						if (info.startLine == currentLine) {
							startColumn = info.startColumn;
						}
						if (info.endLine == currentLine) {
							endColumn = info.endColumn;
							if (endColumn + 1 > pureLabel.Length) {
								endColumn = pureLabel.Length - 1;
							}
						}
					}
					{//For correcting the position and width, because we replace tab with space
						int tabCount = pureLabel.Count(l => l == '\t');
						int offset = Mathf.Min(tabCount, startColumn);
						position.x -= tabWidth * offset;
						position.x += spaceWidth * 6 * offset;
						if (tabCount > startColumn) {
							position.width -= tabWidth * (tabCount - startColumn);
							position.width += spaceWidth * 6 * (tabCount - startColumn);
						}
					}
					if (info.startLine == currentLine) {
						if (startColumn > 0 && pureLabel.Length > startColumn) {
							float width = RichLabel.CalcSize(new GUIContent(pureLabel.Substring(0, startColumn))).x;
							position.x += width;
							position.width -= width;
						}
					}
					if (info.endLine == currentLine) {
						if (endColumn + 1 < pureLabel.Length && pureLabel.Length - endColumn > 0) {
							float width = RichLabel.CalcSize(new GUIContent(pureLabel.Substring(endColumn, pureLabel.Length - endColumn))).x;
							position.width -= width;
						}
					}
					action(position, info);
				}
			}
		}
	}
}