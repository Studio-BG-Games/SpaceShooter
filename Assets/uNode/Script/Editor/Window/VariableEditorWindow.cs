using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MaxyGames.uNode.Editors {
	public class VariableEditorWindow : EditorWindow {
		public static VariableEditorWindow window;
		public static UnityEngine.Object targetObj;
		public static List<VariableData> ESV;

		private string searchText = "";

		public bool autoInitializeSupportedType = true;

		public static VariableEditorWindow ShowWindow(UnityEngine.Object from, List<VariableData> variable) {
			window = GetWindow(typeof(VariableEditorWindow), true) as VariableEditorWindow;
			window.titleContent = new GUIContent("Variable Editor");
			targetObj = from;
			ESV = variable;
			window.autoInitializeSupportedType = true;
			window.minSize = new Vector2(250, 300);
			window.Show();
			return window;
		}

		void OnGUI() {
			if((targetObj == null || ESV == null) && Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint) {
				Close();
				return;
			}
			searchText = uNodeGUIUtility.DrawSearchBar(searchText, GUIContent.none);
			if(targetObj != null && ESV != null) {
				DrawVariableEditor(ESV, targetObj, searchText, autoInitializeSupportedType);
				uNodeEditorUtility.MarkDirty(targetObj);
			}
		}

		private VariableData editedVar;
		private Dictionary<int, Vector2[]> ScrollPOS = new Dictionary<int, Vector2[]>();
		
		void DrawVariableEditor(List<VariableData> ESVariable,
			UnityEngine.Object target,
			string search = null,
			bool autoInitializeDefaultType = true,
			float editorHeight = 150f,
			bool canResizeHeight = false,
			System.Action<float> resizeCallback = null) {

			if(target == null)
				return;
			GUI.changed = false;
			Vector2[] scrollpos;
			{
				if(ScrollPOS.ContainsKey(target.GetInstanceID())) {
					scrollpos = ScrollPOS[target.GetInstanceID()];
				} else {
					scrollpos = new Vector2[3] { Vector2.zero, Vector2.zero, Vector2.zero };
					ScrollPOS.Add(target.GetInstanceID(), scrollpos);
				}
			}
			bool searching = !string.IsNullOrEmpty(search);
			List<string> vName = new List<string>();
			{
				bool isValid = false;
				foreach(VariableData variable in ESVariable) {
					if(editedVar != null && editedVar == variable) {
						isValid = true;
					}
					vName.Add(variable.Name);
				}
				if(!isValid) {
					editedVar = null;
				}
			}
			GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
			style.alignment = TextAnchor.MiddleLeft;
			Event currentEvent = Event.current;
			scrollpos[0] = EditorGUILayout.BeginScrollView(scrollpos[0]);
			ScrollPOS[target.GetInstanceID()] = scrollpos;
			foreach(VariableData variable in ESVariable) {
				if(searching) {
					if(string.IsNullOrEmpty(variable.Name))
						continue;
					bool canDraw = true;
					char[] separator = new char[] { ' ' };
					string[] strArray = search.ToLower().Split(separator);
					string str = variable.Name.ToLower().Replace(" ", string.Empty);
					for(int i = 0; i < strArray.Length; i++) {
						string str2 = strArray[i];
						if(str.Contains(str2)) {
							if((i == 0) && str.StartsWith(str2)) {
								canDraw = true;
							}
						} else {
							canDraw = false;
							break;
						}
					}
					if(!canDraw)
						continue;
				}
				vName.Remove(variable.Name);
				if(!uNodeUtility.IsValidVariableName(variable.Name, vName)) {
					GUI.backgroundColor = Color.red;
				}
				vName.Add(variable.Name);

				Rect varRect = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight);
				bool selected = editedVar == variable;
				if(varRect.Contains(currentEvent.mousePosition) && currentEvent.button == 1 && currentEvent.type == EventType.MouseUp) {
					GenericMenu menu = new GenericMenu();
					if(!searching) {
						if(target is uNodeRoot) {
							uNodeRoot UNR = target as uNodeRoot;
							menu.AddItem(new GUIContent("Analyze"), false, delegate () {
								uNodeGUIUtility.AnalizeVariable(variable, UNR);
							});
							menu.AddSeparator("");
						}
						menu.AddItem(new GUIContent("Move To Top"), false, delegate () {
							//uNodeEditorUtility.RegisterUndo(target, "Move To Top Variable: " + variable.Name);
							ESVariable.Remove(variable);
							ESVariable.Insert(0, variable);
						});
						menu.AddItem(new GUIContent("Move Up"), false, delegate () {
							//uNodeEditorUtility.RegisterUndo(target, "Move Up Variable: " + variable.Name);
							int index = 0;
							bool valid = false;
							foreach(VariableData var in ESVariable) {
								if(var == variable) {
									valid = true;
									break;
								}
								index++;
							}
							if(valid) {
								if(index != 0) {
									ESVariable.RemoveAt(index);
									ESVariable.Insert(index - 1, variable);
								}
							}
						});
						menu.AddItem(new GUIContent("Move Down"), false, delegate () {
							//uNodeEditorUtility.RegisterUndo(target, "Move Down Variable: " + variable.Name);
							int index = 0;
							bool valid = false;
							foreach(VariableData var in ESVariable) {
								if(var == variable) {
									valid = true;
									break;
								}
								index++;
							}
							if(valid) {
								if(index + 1 != ESVariable.Count) {
									ESVariable.RemoveAt(index);
									ESVariable.Insert(index + 1, variable);
								}
							}
						});
						menu.AddItem(new GUIContent("Move To Bottom"), false, delegate () {
							//uNodeEditorUtility.RegisterUndo(target, "Move To End Variable: " + variable.Name);
							ESVariable.Remove(variable);
							ESVariable.Add(variable);
						});
						menu.AddSeparator("");
					}
					menu.AddItem(new GUIContent("Duplicate"), false, delegate () {
						uNodeEditorUtility.RegisterUndo(target, "Duplicate Variable: " + variable.Name);
						int index = 0;
						bool valid = false;
						foreach(VariableData var in ESVariable) {
							if(var == variable) {
								valid = true;
								break;
							}
							index++;
						}
						if(valid) {
							if(index + 1 != ESVariable.Count) {
								ESVariable.Insert(index + 1, new VariableData(variable));
							} else {
								ESVariable.Add(new VariableData(variable));
							}
						}
					});
					menu.AddItem(new GUIContent("Remove"), false, delegate () {
						uNodeEditorUtility.RegisterUndo(target, "Remove Variable: " + variable.Name);
						ESVariable.Remove(variable);
						editedVar = null;
					});
					menu.ShowAsContext();
					break;
				}
				string tooltip = variable.Name;
				if(variable.Type != null) {
					tooltip = variable.Type.PrettyName() + " " + variable.Name;
				}
				selected = GUI.Toggle(varRect, selected, new GUIContent(variable.Name, tooltip), style);
				if(selected && editedVar != variable) {
					editedVar = variable;
				}
				GUI.backgroundColor = Color.white;
			}
			EditorGUILayout.EndScrollView();
			GUILayout.FlexibleSpace();
			if(canResizeHeight) {
				EditorGUILayout.BeginVertical();
			} else {
				EditorGUILayout.BeginVertical("Box");
			}
			if(editedVar != null) {
				if(canResizeHeight) {
					Rect resizeRect = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, 5);
					resizeRect.y -= 2;
					GUI.Box(resizeRect, new GUIContent(""), (GUIStyle)"WindowBottomResize");
					EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeVertical);
					if(Event.current.button == 0 && Event.current.type == EventType.MouseDown && resizeRect.Contains(Event.current.mousePosition)) {
						scrollpos[2] = Event.current.mousePosition;
						scrollpos[2].x = editorHeight;
					}
					if(scrollpos[2] != Vector2.zero && Event.current.type != EventType.Repaint) {
						if(resizeCallback != null) {
							resizeCallback((scrollpos[2] - Event.current.mousePosition).y + scrollpos[2].x);
						}
					}
					if(Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore) {
						scrollpos[2] = Vector2.zero;
					}
				}
				scrollpos[1] = EditorGUILayout.BeginScrollView(scrollpos[1], GUILayout.Height(editorHeight));
				VariableData variable = editedVar;
				uNodeGUIUtility.DrawVariable(variable, target, autoInitializeDefaultType, ESVariable, false);
				EditorGUILayout.EndScrollView();
				EditorGUILayout.BeginHorizontal("Box");
				if(!searching) {
					if(GUILayout.Button("Up", GUILayout.Width(40))) {
						//uNodeEditorUtility.RegisterUndo(target, "Move Up Variable: " + variable.Name);
						int index = 0;
						bool valid = false;
						foreach(VariableData var in ESVariable) {
							if(var == editedVar) {
								valid = true;
								break;
							}
							index++;
						}
						if(valid) {
							if(index != 0) {
								ESVariable.RemoveAt(index);
								ESVariable.Insert(index - 1, editedVar);
							}
						} else {
							editedVar = null;
						}
					}
					if(GUILayout.Button("Down", GUILayout.Width(50))) {
						//uNodeEditorUtility.RegisterUndo(target, "Move Down Variable: " + variable.Name);
						int index = 0;
						bool valid = false;
						foreach(VariableData var in ESVariable) {
							if(var == editedVar) {
								valid = true;
								break;
							}
							index++;
						}
						if(valid) {
							if(index + 1 != ESVariable.Count) {
								ESVariable.RemoveAt(index);
								ESVariable.Insert(index + 1, editedVar);
							}
						} else {
							editedVar = null;
						}
					}
				}
				if(GUILayout.Button("+", GUILayout.Width(25))) {
					uNodeEditorUtility.RegisterUndo(target, "Duplicate Variable: " + variable.Name);
					int index = 0;
					bool valid = false;
					foreach(VariableData var in ESVariable) {
						if(var == editedVar) {
							valid = true;
							break;
						}
						index++;
					}
					if(valid) {
						if(index + 1 != ESVariable.Count) {
							ESVariable.Insert(index + 1, new VariableData(editedVar));
						} else {
							ESVariable.Add(new VariableData(editedVar));
						}
					} else {
						editedVar = null;
					}
				}
				if(GUILayout.Button("-", GUILayout.Width(25))) {
					uNodeEditorUtility.RegisterUndo(target, "Remove Variable: " + variable.Name);
					ESVariable.Remove(variable);
					editedVar = null;
				}
			} else {
				EditorGUILayout.BeginHorizontal();
			}
			var rect = uNodeGUIUtility.GetRect();
			if(GUI.Button(rect, new GUIContent("Add New", "Add new Variable"))) {
				uNodeEditorUtility.ShowAddVariableMenu(GUIUtility.GUIToScreenPoint(rect.position), ESVariable, target);
			};
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			ScrollPOS[target.GetInstanceID()] = scrollpos;
			if(GUI.changed) {
				uNodeEditorUtility.MarkDirty(target);
			}
		}
	}
}