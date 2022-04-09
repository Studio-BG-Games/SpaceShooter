using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using MaxyGames.uNode.Nodes;
using System.Reflection;

namespace MaxyGames.uNode.Editors {
	[CustomEditor(typeof(ExposedNode), true)]
	class ExposedNodeEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			ExposedNode node = target as ExposedNode;
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PrefixLabel(new GUIContent("Comment"));
			node.comment = EditorGUILayout.TextArea(node.comment);
			if(node.value.type != null) {
				DrawGUI(node);
			}
			if(EditorGUI.EndChangeCheck()) {
				uNodeEditorUtility.MarkDirty(node);
				uNodeGUIUtility.GUIChanged(node);
			}
		}

		void DrawGUI(ExposedNode node) {
			VariableEditorUtility.DrawCustomList(node.outputDatas, "Exposed Ports",
				drawElement: (position, index, value) => {
					var vType = value.type;
					string vName = ObjectNames.NicifyVariableName(value.name);
					if(vType != null) {
						position = EditorGUI.PrefixLabel(position, new GUIContent(vName));
						EditorGUI.LabelField(position, vType.PrettyName());
					} else {
						position = EditorGUI.PrefixLabel(position, new GUIContent(vName));
						EditorGUI.HelpBox(position, "Type not found", MessageType.Error);
					}
				},
				add: (position) => {
					var type = node.value.type;
					bool hasAddMenu = false;
					GenericMenu menu = new GenericMenu();
					var fields = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
					foreach(var vv in fields) {
						if(vv is FieldInfo || vv is PropertyInfo && (vv as PropertyInfo).CanRead && (vv as PropertyInfo).GetIndexParameters().Length == 0) {
							bool valid = true;
							foreach(var v in node.outputDatas) {
								if(v.name == vv.Name) {
									valid = false;
									break;
								}
							}
							if(valid) {
								hasAddMenu = true;
								break;
							}
						}
					}
					if(hasAddMenu) {
						menu.AddItem(new GUIContent("Add All Fields"), false, delegate () {
							foreach(var v in fields) {
								if(v is FieldInfo field) {
									if(field.Attributes.HasFlags(FieldAttributes.InitOnly))
										continue;
								} else if(v is PropertyInfo property) {
									if(!property.CanRead || property.GetIndexParameters().Length > 0) {
										continue;
									}
								} else {
									continue;
								}
								var t = ReflectionUtils.GetMemberType(v);
								bool valid = true;
								foreach(var vv in node.outputDatas) {
									if(v.Name == vv.name) {
										valid = false;
										break;
									}
								}
								if(valid) {
									uNodeEditorUtility.RegisterUndo(node, "");
									uNodeUtility.AddArray(ref node.outputDatas, new ExposedNode.OutputData() {
										name = v.Name,
										type = t,
									});
								}
							}
							uNodeGUIUtility.GUIChanged(node);
						});
					}
					foreach(var v in fields) {
						if(v is FieldInfo field) {
							if(field.Attributes.HasFlags(FieldAttributes.InitOnly))
								continue;
						} else if(v is PropertyInfo property) {
							if(!property.CanRead || property.GetIndexParameters().Length > 0) {
								continue;
							}
						} else {
							continue;
						}
						var t = ReflectionUtils.GetMemberType(v);
						bool valid = true;
						foreach(var vv in node.outputDatas) {
							if(v.Name == vv.name) {
								valid = false;
								break;
							}
						}
						if(valid) {
							menu.AddItem(new GUIContent("Add Field/" + v.Name), false, delegate () {
								uNodeEditorUtility.RegisterUndo(node, "Add Field:" + v.Name);
								uNodeUtility.AddArray(ref node.outputDatas, new ExposedNode.OutputData() {
									name = v.Name,
									type = t,
								});
								uNodeGUIUtility.GUIChanged(node);
							});
						}
					}
					for(int i = 0; i < node.outputDatas.Length; i++) {
						var v = node.outputDatas[i];
						menu.AddItem(new GUIContent("Remove Field/" + v.name), false, delegate (object obj) {
							uNodeEditorUtility.RegisterUndo(node, "Remove Field:" + v.name);
							uNodeUtility.RemoveArray(ref node.outputDatas, v);
							uNodeGUIUtility.GUIChanged(node);
						}, v);
					}
					menu.ShowAsContext();
				},
				remove: (index) => {
					uNodeEditorUtility.RegisterUndo(node, "Remove Field:" + node.outputDatas[index].name);
					uNodeUtility.RemoveArrayAt(ref node.outputDatas, index);
					uNodeGUIUtility.GUIChanged(node);
				},
				reorder: (list, oldIndex, newIndex) => {
					uNodeGUIUtility.GUIChanged(node);
				}
			);
			//EditorGUILayout.BeginHorizontal();
			//if(GUILayout.Button(new GUIContent("Refresh", ""), EditorStyles.miniButtonLeft)) {
			//	node.Refresh();
			//	uNodeGUIUtility.GUIChanged(node);
			//}
			//EditorGUILayout.EndHorizontal();
		}
	}
}