using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using UnityEditorInternal;
using Object = UnityEngine.Object;
using System.Collections;

namespace MaxyGames.uNode.Editors {
	public static class VariableEditorUtility {
		static Dictionary<object, ReorderableList> _reorderabeMap = new Dictionary<object, ReorderableList>();
		static Dictionary<object, ReorderableList> _reorderabeMemberMap = new Dictionary<object, ReorderableList>();

		public static void DrawCustomList<T>(
			IList<T> values, 
			string headerLabel, 
			Action<Rect, int, T> drawElement, 
			Action<Rect> add,
			Action<int> remove,
			ReorderableList.ReorderCallbackDelegateWithDetails reorder = null,
			ReorderableList.ElementHeightCallbackDelegate elementHeight = null) {
			if(values == null) {
				throw new ArgumentNullException(nameof(values));
			}
			ReorderableList reorderable;
			if(!_reorderabeMap.TryGetValue(values, out reorderable)) {
				reorderable = new ReorderableList(values as System.Collections.IList, typeof(T));
				reorderable.drawHeaderCallback = (pos) => {
					EditorGUI.LabelField(pos, headerLabel);
				};
				reorderable.drawElementCallback = (pos, index, isActive, isFocused) => {
					drawElement(pos, index, values[index]);
				};
				if(elementHeight != null) {
					reorderable.elementHeightCallback = elementHeight;
				}
				reorderable.displayAdd = add != null;
				reorderable.displayRemove = remove != null;
				if(reorderable.displayAdd) {
					reorderable.onAddDropdownCallback = (pos, list) => {
						add(pos);
						reorderable.list = values as System.Collections.IList;
					};
				}
				if(reorderable.displayRemove) {
					reorderable.onRemoveCallback = (list) => {
						remove(reorderable.index);
						reorderable.list = values as System.Collections.IList;
					};
				}
				reorderable.onReorderCallbackWithDetails = reorder;
				_reorderabeMap[values] = reorderable;
			}
			reorderable.DoLayoutList();
		}

		public static void DrawNamespace(string header, IList<string> namespaces, UnityEngine.Object targetObject, Action<IList<string>> action = null) {
			if(namespaces == null) {
				namespaces = new List<string>();
				if(action != null) {
					action(namespaces);
				}
			}
			ReorderableList reorderable;
			if(!_reorderabeMap.TryGetValue(namespaces, out reorderable)) {
				reorderable = new ReorderableList(namespaces as IList, typeof(string));
				reorderable.drawHeaderCallback = (pos) => {
					EditorGUI.LabelField(pos, header);
				};
				reorderable.drawElementCallback = (pos, index, isActive, isFocused) => {
					namespaces[index] = EditorGUI.TextField(pos, namespaces[index]);
					if(GUI.changed) {
						if(targetObject) {
							uNodeEditorUtility.MarkDirty(targetObject);
						}
					}
				};
				reorderable.onAddDropdownCallback = (pos, list) => {
					List<ItemSelector.CustomItem> items = new List<ItemSelector.CustomItem>();
					var ns = EditorReflectionUtility.GetNamespaces();
					if(ns != null && ns.Count > 0) {
						foreach(var n in ns) {
							if(!string.IsNullOrEmpty(n) && !namespaces.Contains(n)) {
								items.Add(ItemSelector.CustomItem.Create(n, (obj) => {
									uNodeUtility.AddList(ref namespaces, n);
									if(action != null) {
										action(namespaces);
									}
									reorderable.list = namespaces as System.Collections.IList;
									if(targetObject && uNodeEditorUtility.IsPrefab(targetObject)) {
										uNodeEditorUtility.MarkDirty(targetObject);
									}
								}, "Namespaces"));
							}
						}
						items.Sort((x, y) => string.CompareOrdinal(x.name, y.name));
					}
					ItemSelector.ShowWindow(null, null, null, false, items).ChangePosition(pos.ToScreenRect()).displayDefaultItem = false;
				};
				reorderable.onRemoveCallback = (list) => {
					if(targetObject)
						uNodeEditorUtility.RegisterUndo(targetObject, "Remove Namespace: " + namespaces[list.index]);
					uNodeUtility.RemoveListAt(ref namespaces, list.index);
					if(action != null) {
						action(namespaces);
					}
					if(targetObject && uNodeEditorUtility.IsPrefab(targetObject)) {
						uNodeEditorUtility.MarkDirty(targetObject);
					}
				};
				_reorderabeMap[namespaces] = reorderable;
			}
			reorderable.DoLayoutList();
		}

		public static void DrawLinkedVariables(IVariableSystem owner, IVariableSystem linked, string header = "Variables", bool publicOnly = true) {
			if(owner == null || linked == null)
				return;
			List<VariableData> linkedVariable = linked.Variables;
			if(linkedVariable.Count == 0) return;
			if(!string.IsNullOrEmpty(header)) {//Header
				Rect headerRect = GUILayoutUtility.GetRect(EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight + 1, uNodeGUIStyle.headerStyle);
				headerRect.y += 1f;
				GUI.Box(headerRect, new GUIContent(header), uNodeGUIStyle.headerStyle);
				EditorGUILayout.BeginVertical("RL Background");
				GUILayout.Space(1);
			}
			var ownerVariable = owner.Variables;
			for(int i=0; i < ownerVariable.Count;i++) {
				if(!linkedVariable.Any((v) => v.Name == ownerVariable[i].Name)) {
					//Remove the variable when the variable doesn't exist in the linked variable.
					ownerVariable.RemoveAt(i);
				}
			}
			for (int x = 0; x < linkedVariable.Count; x++) {
				VariableData linkedVar = linkedVariable[x];
				if(publicOnly && !linkedVar.showInInspector) {
					continue;
				}
				VariableData ownerVar = null;
				for (int y = 0; y < ownerVariable.Count; y++) {
					if (linkedVar.Name == ownerVariable[y].Name) {
						ownerVar = ownerVariable[y];
						break;
					}
				}
				if(ownerVar != null) {
					ownerVar.attributes = linkedVar.attributes;
					if(ownerVar.Type != linkedVar.Type) {
						ownerVar.Type = linkedVar.Type;
					}
				}
				VariableData variable = ownerVar ?? linkedVar;
				if(variable != null) {
					FieldDecorator.DrawDecorators(variable.GetAttributes());
				}
				using(new EditorGUILayout.HorizontalScope()) {
					var rect = uNodeGUIUtility.GetRect(GUILayout.Width(18));
					bool flag = EditorGUI.Toggle(rect, ownerVar != null);
					if(flag != (ownerVar != null)) {
						if(flag) {
							ownerVariable.Add(new VariableData(linkedVar));
						} else {
							ownerVariable.Remove(ownerVar);
						}
					} else if(ownerVar != null && ownerVar.Type != null && !ownerVar.Type.IsCastableTo(linkedVar.Type)) {
						ownerVariable.Remove(ownerVar);
						ownerVariable.Add(new VariableData(linkedVar));
					}
					EditorGUI.BeginDisabledGroup(ownerVar == null);
					using(new EditorGUILayout.VerticalScope()) {
						uNodeGUIUtility.EditVariableValue(variable, owner as Object, false);
					}
					EditorGUI.EndDisabledGroup();
				}
			}
			// uNodeEditorUtility.DrawVariablesInspector(owner.Variables, owner as Object, null, publicOnly);
			if (!string.IsNullOrEmpty(header)) {
				GUILayout.Space(5);
				EditorGUILayout.EndVertical();
			}
		}

		public static void DrawGraphVariable(
			uNodeRoot graph, 
			Action<List<VariableData>> action) {
			var variables = (graph as IVariableSystem).Variables;
			if(variables == null) {
				variables = new List<VariableData>();
				if(action != null) {
					action(variables);
				}
			}
			bool allowUnityReference = true;
			if(!graph.IsRuntimeGraph()) {
				allowUnityReference = false;
			}
			ReorderableList reorderable;
			if(!_reorderabeMap.TryGetValue(variables, out reorderable)) {
				reorderable = new ReorderableList(variables, typeof(VariableData));
				reorderable.drawHeaderCallback = (pos) => {
					EditorGUI.LabelField(pos, "Variables");
				};
				object dragedObj = null;
				reorderable.drawElementCallback = (pos, index, isActive, isFocused) => {
					var element = variables[index];
					EditorGUI.LabelField(
						new Rect(pos.x, pos.y, pos.height, pos.height), 
						new GUIContent(uNodeEditorUtility.GetTypeIcon(variables[index].Type))
					);
					var position = EditorGUI.PrefixLabel(
						new Rect(pos.x + pos.height, pos.y, pos.width - pos.height, pos.height), 
						new GUIContent(variables[index].Name, variables[index].type.DisplayName(false, false) + " " + variables[index].Name)
					);
					pos = new Rect(pos.x, pos.y, pos.width - position.width, pos.height);
					var type = variables[index].Type;
					if(type != null && element.variable == null && uNodeGUIUtility.IsSupportedType(type) && ReflectionUtils.CanCreateInstance(type)) {
						element.variable = ReflectionUtils.CreateInstance(type);
					}
					if(element.variable != null || type != null && type.IsCastableTo(typeof(Object)) || type is RuntimeType) {
						position.height = EditorGUIUtility.singleLineHeight;
						uNodeGUIUtility.EditValue(position, GUIContent.none, element.variable, type, (val) => {
							variables[index].variable = val;
							variables[index].Serialize();
						}, new uNodeUtility.EditValueSettings() {
							unityObject = graph,
							acceptUnityObject = allowUnityReference,
						});
					} else {
						EditorGUI.HelpBox(position, "null", MessageType.None);
					}
					if(pos.Contains(Event.current.mousePosition)) {
						if (Event.current.type == EventType.MouseDown) {
							dragedObj = element;
						} else if(Event.current.type == EventType.MouseDrag && dragedObj == element) {
							dragedObj = null;
							DragAndDrop.PrepareStartDrag();
							DragAndDrop.SetGenericData("uNode", element);
							DragAndDrop.SetGenericData("uNode-Target", graph);
							DragAndDrop.StartDrag("Draging Variable");
							Event.current.Use();
							reorderable.ReleaseKeyboardFocus();
							GUIUtility.hotControl = 0;
						}
						if(Event.current.button == 0 && Event.current.clickCount == 2) {
							// ActionWindow.ShowWindow(variables[index], (ref object obj) => {
							// 	VariableData val = obj as VariableData;
							// 	uNodeEditorUtility.DrawVariable(val, graph, false, Variables, isLocalVariable, null);
							// });
						} else if(Event.current.button == 1) {
							GenericMenu menu = new GenericMenu();
							var mPos = uNodeGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
							mPos.x -= 250;
							mPos.y -= 100;
							// menu.AddItem(new GUIContent("Rename"), false, delegate (object p) {
								
							// }, variables[index]);

							menu.AddItem(new GUIContent("Change Type"), false, delegate (object g) {
								TypeSelectorWindow.ShowWindow(mPos, FilterAttribute.Default, members => {
									uNodeEditorUtility.RegisterUndo(graph, "Change Type");
									(g as VariableData).type = members[0] as MemberData;
									ActionPopupWindow.CloseLast();
									if(action != null) {
										action(variables);
									}
								}, new TypeItem((g as ParameterData).type));
							}, variables[index]);
							menu.ShowAsContext();
						}
					}
				};
				reorderable.onReorderCallbackWithDetails = (list, oldIndex, newIndex) => {
					var val = variables[newIndex];
					variables.RemoveAt(newIndex);
					if(oldIndex >= variables.Count) {
						variables.Add(val);
					} else {
						variables.Insert(oldIndex, val);
					}
					if(action != null) {
						action(variables);
					}
					if(graph)
						uNodeEditorUtility.RegisterUndo(graph, "Reorder List");
					variables.RemoveAt(oldIndex);
					if(newIndex >= variables.Count) {
						variables.Add(val);
					} else {
						variables.Insert(newIndex, val);
					}
					reorderable.list = variables;
					if(action != null) {
						action(variables);
					}
				};
				reorderable.onAddDropdownCallback = (pos, list) => {
					uNodeEditorUtility.ShowAddVariableMenu(pos.ToScreenRect().position, variables, graph);
				};
				reorderable.onRemoveCallback = (list) => {
					if(graph)
						uNodeEditorUtility.RegisterUndo(graph, "Remove Variable: " + variables[list.index].Name);
					variables.RemoveAt(list.index);
					if(action != null) {
						action(variables);
					}
					if(graph && uNodeEditorUtility.IsPrefab(graph)) {
						uNodeEditorUtility.MarkDirty(graph);
					}
				};
				_reorderabeMap[variables] = reorderable;
			}
			reorderable.DoLayoutList();
		}

		public static void DrawVariable(
			List<VariableData> variables, 
			UnityEngine.Object targetObject, 
			Action<List<VariableData>> action,
			Action<VariableData, string> onRename,
			bool isLocalVariable = false) {

			if(variables == null) {
				variables = new List<VariableData>();
				if(action != null) {
					action(variables);
				}
			}
			ReorderableList reorderable;
			if(!_reorderabeMap.TryGetValue(variables, out reorderable)) {
				reorderable = new ReorderableList(variables, typeof(VariableData));
				reorderable.drawHeaderCallback = (pos) => {
					EditorGUI.LabelField(pos, "Variables");
				};
				bool allowUnityReference = true;
				if(targetObject is uNodeRoot graph) {
					if(!graph.IsRuntimeGraph()) {
						allowUnityReference = false;
					}
				} else if(targetObject is INode<uNodeRoot> node) {
					if(!node.GetOwner().IsRuntimeGraph()) {
						allowUnityReference = false;
					}
				}
				reorderable.drawElementCallback = (pos, index, isActive, isFocused) => {
					var element = variables[index];
					var position = EditorGUI.PrefixLabel(pos, new GUIContent(variables[index].Name,
						uNodeEditorUtility.GetTypeIcon(variables[index].Type),
						variables[index].type.DisplayName(false, false) + " " + variables[index].Name));
					pos = new Rect(pos.x, pos.y, pos.width - position.width, pos.height);
					var type = variables[index].Type;
					if(type != null && element.variable == null && uNodeGUIUtility.IsSupportedType(type) && ReflectionUtils.CanCreateInstance(type)) {
						element.variable = ReflectionUtils.CreateInstance(type);
					}
					if(element.variable != null || type != null && type.IsCastableTo(typeof(Object))) {
						position.height = EditorGUIUtility.singleLineHeight;
						uNodeGUIUtility.EditValue(position, GUIContent.none, element.variable, type, (val) => {
							variables[index].variable = val;
						}, new uNodeUtility.EditValueSettings() {
							unityObject = targetObject,
							acceptUnityObject = allowUnityReference,
						});
					} else {
						EditorGUI.HelpBox(position, "null", MessageType.None);
					}
					if(pos.Contains(Event.current.mousePosition)) {
						// if (Event.current.type == EventType.MouseDown ) {
						// 	DragAndDrop.PrepareStartDrag();
						// 	DragAndDrop.SetGenericData("A", "B");
						// 	DragAndDrop.StartDrag("A");
						// }
						if(Event.current.button == 0 && Event.current.clickCount == 2) {
							ActionWindow.ShowWindow(variables[index], (ref object obj) => {
								VariableData val = obj as VariableData;
								uNodeGUIUtility.DrawVariable(val, targetObject, !isLocalVariable, variables, isLocalVariable, onRename);
							});
						} else if(Event.current.button == 1) {
							GenericMenu menu = new GenericMenu();
							var mPos = uNodeGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
							mPos.x -= 250;
							mPos.y -= 100;
							menu.AddItem(new GUIContent("Rename"), false, delegate (object p) {
								ActionPopupWindow.ShowWindow(mPos,
									new object[] { (p as VariableData).Name, p as VariableData },
									delegate (ref object obj) {
										object[] o = obj as object[];
										o[0] = EditorGUILayout.TextField("Variable Name", o[0] as string);
									}, null, delegate (ref object obj) {
										if(GUILayout.Button("Apply") || Event.current.keyCode == KeyCode.Return) {
											object[] o = obj as object[];
											o[0] = uNodeUtility.AutoCorrectName(o[0] as string);
											string varName = o[0] as string;
											bool hasVariable = false;
											if(variables != null && variables.Count > 0) {
												foreach(VariableData V in variables) {
													if(V.Name == varName) {
														hasVariable = true;
														break;
													}
												}
											}
											if(!hasVariable) {
												VariableData V = o[1] as VariableData;
												if(targetObject)
													uNodeEditorUtility.RegisterUndo(targetObject, "Rename Variable: " + V.Name);
												if(onRename != null) {
													try {
														onRename(V, varName);
													} catch(System.Exception ex) {
														Debug.LogException(ex);
													}
												}
												V.Name = varName;
											}
											ActionPopupWindow.CloseLast();
										}
									}).headerName = "Rename Variable";
							}, variables[index]);

							menu.AddItem(new GUIContent("Change Type"), false, delegate (object g) {
								TypeSelectorWindow.ShowWindow(mPos, FilterAttribute.Default, members => {
									uNodeEditorUtility.RegisterUndo(targetObject, "Change Type");
									(g as VariableData).type = members[0] as MemberData;
									ActionPopupWindow.CloseLast();
									if(action != null) {
										action(variables);
									}
								}, new TypeItem((g as ParameterData).type));
							}, variables[index]);
							menu.ShowAsContext();
						}
					}
				};
				reorderable.onReorderCallbackWithDetails = (list, oldIndex, newIndex) => {
					var val = variables[newIndex];
					variables.RemoveAt(newIndex);
					if(oldIndex >= variables.Count) {
						variables.Add(val);
					} else {
						variables.Insert(oldIndex, val);
					}
					if(action != null) {
						action(variables);
					}
					if(targetObject)
						uNodeEditorUtility.RegisterUndo(targetObject, "Reorder List");
					variables.RemoveAt(oldIndex);
					if(newIndex >= variables.Count) {
						variables.Add(val);
					} else {
						variables.Insert(newIndex, val);
					}
					reorderable.list = variables;
					if(action != null) {
						action(variables);
					}
				};
				reorderable.onAddDropdownCallback = (pos, list) => {
					uNodeEditorUtility.ShowAddVariableMenu(pos.position.ToScreenPoint(), variables, targetObject);
				};
				reorderable.onRemoveCallback = (list) => {
					if(targetObject)
						uNodeEditorUtility.RegisterUndo(targetObject, "Remove Variable: " + variables[list.index].Name);
					variables.RemoveAt(list.index);
					if(action != null) {
						action(variables);
					}
					if(targetObject && uNodeEditorUtility.IsPrefab(targetObject)) {
						uNodeEditorUtility.MarkDirty(targetObject);
					}
				};
				_reorderabeMap[variables] = reorderable;
			}
			reorderable.DoLayoutList();
		}

		public static void DrawParameter(ParameterData[] parameters, UnityEngine.Object targetObject, uNodeRoot uNode, Action<ParameterData[]> action) {
			if(parameters == null) {
				parameters = new ParameterData[0];
				if(action != null) {
					action(parameters);
				}
			}
			ReorderableList reorderable;
			if(!_reorderabeMap.TryGetValue(parameters, out reorderable)) {
				reorderable = new ReorderableList(parameters, typeof(ParameterData));
				reorderable.drawHeaderCallback = (pos) => {
					EditorGUI.LabelField(pos, "Parameters");
				};
				reorderable.drawElementCallback = (pos, index, isActive, isFocused) => {
					EditorGUI.LabelField(pos, new GUIContent(parameters[index].name + " : " + parameters[index].type.DisplayName(false, false), parameters[index].type.DisplayName(true, false) + " " + parameters[index].name));
					if(pos.Contains(Event.current.mousePosition)) {
						if(Event.current.button == 0 && Event.current.clickCount == 2) {
							var pIndex = index;
							ActionWindow.ShowWindow(SerializerUtility.Duplicate(parameters[index]), (ref object obj) => {
								ParameterData p = obj as ParameterData;
								EditorGUI.BeginDisabledGroup(true);
								uNodeGUIUtility.ShowField("name", p, targetObject);
								EditorGUI.EndDisabledGroup();
								uNodeGUIUtility.EditValueLayouted(nameof(p.type), p, (val) => {
									Action updateAction = RefactorUtility.GetUpdateReferencesAction(targetObject);
									p.type.CopyTo(parameters[pIndex].type);
									action?.Invoke(parameters);
									updateAction?.Invoke();
									if(targetObject) {
										uNodeEditorUtility.MarkDirty(targetObject);
										uNodeGUIUtility.GUIChanged(targetObject);
									}
								}, new uNodeUtility.EditValueSettings() {
									unityObject = targetObject
								});
								uNodeGUIUtility.EditValueLayouted(nameof(p.refKind), p, (val) => {
									p.refKind = (ParameterData.RefKind)val;
									parameters[pIndex].refKind = p.refKind;
									action?.Invoke(parameters);
									if(targetObject) {
										uNodeEditorUtility.MarkDirty(targetObject);
										uNodeGUIUtility.GUIChanged(targetObject);
									}
								});
							});
						} else if(Event.current.button == 1) {
							GenericMenu menu = new GenericMenu();
							var mPos = uNodeGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
							mPos.x -= 250;
							mPos.y -= 100;
							menu.AddItem(new GUIContent("Rename"), false, delegate (object p) {
								ActionPopupWindow.ShowWindow(mPos,
									new object[] { (p as ParameterData).name, p as ParameterData },
									delegate (ref object obj) {
										object[] o = obj as object[];
										o[0] = EditorGUILayout.TextField("Name", o[0] as string);
									}, null, delegate (ref object obj) {
										if(GUILayout.Button("Apply") || Event.current.keyCode == KeyCode.Return) {
											object[] o = obj as object[];
											o[0] = uNodeUtility.AutoCorrectName(o[0] as string);
											string varName = o[0] as string;
											bool hasVariable = false;
											if(parameters != null && parameters.Length > 0) {
												foreach(ParameterData V in parameters) {
													if(V.name == varName) {
														hasVariable = true;
														break;
													}
												}
											}
											if(!hasVariable) {
												ParameterData V = o[1] as ParameterData;
												if(targetObject)
													uNodeEditorUtility.RegisterUndo(targetObject, "Rename Parameter: " + V.name);
												string oldVarName = V.name;
												V.name = varName;
												Func<object, bool> validation = delegate (object OBJ) {
													if(OBJ is MemberData) {
														MemberData member = OBJ as MemberData;
														if(member.targetType == MemberData.TargetType.uNodeParameter && 
															member.instance as UnityEngine.Object == targetObject) {
															if(member.name.Equals(oldVarName)) {
																member.name = V.name;
																return true;
															}
														} else if(member.targetType == MemberData.TargetType.uNodeFunction) {
															return member.GetUnityObject() == targetObject;
														}
													}
													return false;
												};
												if(uNode) {
													uNode.Refresh();
													Array.ForEach(uNode.nodes, item => {
														if(AnalizerUtility.AnalizeObject(item, validation)) {
															UGraphView.ClearCache(item);
														}
													});
												}
												if(action != null) {
													action(parameters);
												}
												if(targetObject && uNodeEditorUtility.IsPrefab(targetObject)) {
													uNodeEditorUtility.MarkDirty(targetObject);
												}
											}
											ActionPopupWindow.CloseLast();
										}
									}).headerName = "Rename Parameter";
							}, parameters[index]);
							menu.AddItem(new GUIContent("Change Type"), false, delegate (object g) {
								TypeSelectorWindow.ShowWindow(mPos, FilterAttribute.Default, members => {
									Action updateAction = RefactorUtility.GetUpdateReferencesAction(targetObject);
									(g as ParameterData).type = members[0];
									action?.Invoke(parameters);
									updateAction?.Invoke();
								}, new TypeItem((g as ParameterData).type));
							}, parameters[index]);
							menu.ShowAsContext();
						}
					}
				};
				reorderable.onReorderCallbackWithDetails = (list, oldIndex, newIndex) => {
					var val = parameters[newIndex];
					ArrayUtility.RemoveAt(ref parameters, newIndex);
					if(oldIndex >= parameters.Length) {
						ArrayUtility.Add(ref parameters, val);
					} else {
						ArrayUtility.Insert(ref parameters, oldIndex, val);
					}
					action?.Invoke(parameters);
					Action updateAction = RefactorUtility.GetUpdateReferencesAction(targetObject);
					if(targetObject)
						uNodeEditorUtility.RegisterUndo(targetObject, "Reorder List");
					val = parameters[oldIndex];
					ArrayUtility.RemoveAt(ref parameters, oldIndex);
					if(newIndex >= parameters.Length) {
						ArrayUtility.Add(ref parameters, val);
					} else {
						ArrayUtility.Insert(ref parameters, newIndex, val);
					}
					reorderable.list = parameters;
					action?.Invoke(parameters);
					updateAction?.Invoke();
				};
				reorderable.onAddDropdownCallback = (pos, list) => {
					//ActionWindow.ShowWindow(pos, )
					ActionWindow.ShowWindow(
						new object[] { "parameter", new MemberData(typeof(object), MemberData.TargetType.Type) },
						delegate (ref object obj) {
							object[] o = obj as object[];
							o[0] = EditorGUILayout.TextField("Name", o[0] as string);
							EditorReflectionUtility.RenderVariable(o[1] as MemberData, new GUIContent("Parameter Type"), targetObject,
								new FilterAttribute() { OnlyGetType = true });
						}, 
						null, 
						delegate (ref object obj) {
						if(GUILayout.Button("Add")) {
							ActionWindow.CloseLast();
							object[] o = obj as object[];
							o[0] = uNodeUtility.AutoCorrectName(o[0] as string);
							string name = o[0] as string;
							if(parameters.Length > 0) {
								bool hasVariable = false;
								foreach(ParameterData parameter in parameters) {
									if(parameter.name == name) {
										hasVariable = true;
										break;
									}
								}
								int index = 0;
								while(hasVariable) {
									index++;
									hasVariable = false;
									name = (o[0] as string) + index.ToString();
									foreach(ParameterData parameter in parameters) {
										if(parameter.name == name) {
											hasVariable = true;
											break;
										}
									}
								}
							}
							Action updateAction = RefactorUtility.GetUpdateReferencesAction(targetObject);
							ArrayUtility.Add(ref parameters, new ParameterData() { name = name, type = (o[1] as MemberData) });
							action?.Invoke(parameters);
							updateAction?.Invoke();
							if(targetObject) {
								uNodeEditorUtility.MarkDirty(targetObject);
								uNodeGUIUtility.GUIChanged(targetObject);
							}
						}
					}).headerName = "New Parameter";
				};
				reorderable.onRemoveCallback = (list) => {
					if(targetObject)
						uNodeEditorUtility.RegisterUndo(targetObject, "Remove Parameter: " + parameters[list.index].name);
					Action updateAction = RefactorUtility.GetUpdateReferencesAction(targetObject);
					ArrayUtility.Remove(ref parameters, parameters[list.index]);
					action?.Invoke(parameters);
					updateAction?.Invoke();
					if(targetObject) {
						uNodeEditorUtility.MarkDirty(targetObject);
						uNodeGUIUtility.GUIChanged(targetObject);
					}
				};
				_reorderabeMap[parameters] = reorderable;
			}
			reorderable.DoLayoutList();
		}

		public static void DrawGenericParameter(IList<GenericParameterData> parameters,
			UnityEngine.Object targetObject,
			Action<IList<GenericParameterData>> action) {
			DrawGenericParameter(parameters, targetObject, null, action);
		}

		public static void DrawGenericParameter(IList<GenericParameterData> parameters,
		UnityEngine.Object targetObject,
		uNodeRoot uNode,
		Action<IList<GenericParameterData>> action) {
			if(parameters == null) {
				parameters = new GenericParameterData[0];
				if(action != null) {
					action(parameters);
				}
			}
			ReorderableList reorderable;
			if(!_reorderabeMap.TryGetValue(parameters, out reorderable)) {
				reorderable = new ReorderableList(parameters as System.Collections.IList, typeof(GenericParameterData));
				reorderable.drawHeaderCallback = (pos) => {
					EditorGUI.LabelField(pos, "Generic Parameters");
				};
				reorderable.drawElementCallback = (pos, index, isActive, isFocused) => {
					EditorGUI.LabelField(pos, new GUIContent(parameters[index].name));
					if(pos.Contains(Event.current.mousePosition)) {
						if(Event.current.button == 0 && Event.current.clickCount == 2) {
							ActionWindow.ShowWindow(parameters[index], (ref object obj) => {
								ParameterData p = obj as ParameterData;
								EditorGUI.BeginDisabledGroup(true);
								uNodeGUIUtility.ShowField("name", p, targetObject);
								EditorGUI.EndDisabledGroup();
								uNodeGUIUtility.ShowField("type", p, targetObject);
								uNodeGUIUtility.ShowField("refKind", p, targetObject);
							});
						} else if(Event.current.button == 1) {
							GenericMenu menu = new GenericMenu();
							var mPos = uNodeGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
							mPos.x -= 250;
							mPos.y -= 100;
							menu.AddItem(new GUIContent("Rename"), false, delegate (object g) {
								ActionPopupWindow.ShowWindow(mPos,
								new object[] { (g as GenericParameterData).name, g as GenericParameterData },
								delegate (ref object obj) {
									object[] o = obj as object[];
									o[0] = EditorGUILayout.TextField("Name", o[0] as string);
								}, null, delegate (ref object obj) {
									if(GUILayout.Button("Apply") || Event.current.keyCode == KeyCode.Return) {
										object[] o = obj as object[];
										o[0] = uNodeUtility.AutoCorrectName(o[0] as string);
										string varName = o[0] as string;
										bool hasVariable = false;
										if(parameters != null && parameters.Count > 0) {
											foreach(GenericParameterData V in parameters) {
												if(V.name == varName) {
													hasVariable = true;
													break;
												}
											}
										}
										if(!hasVariable) {
											GenericParameterData V = o[1] as GenericParameterData;
											uNodeEditorUtility.RegisterUndo(targetObject, "Rename Generic Parameter: " + V.name);
											string oldVarName = V.name;
											V.name = varName;
											Func<object, bool> validation = delegate (object OBJ) {
												if(OBJ is MemberData) {
													MemberData member = OBJ as MemberData;
													if(member.instance as UnityEngine.Object == targetObject &&
														(member.targetType == MemberData.TargetType.uNodeGenericParameter)) {
														if(member.name.Equals(oldVarName)) {
															member.name = V.name;
															return true;
														}
													}
												}
												return false;
											};
											if(uNode) {
												uNode.Refresh();
												Array.ForEach(uNode.nodes, item => AnalizerUtility.AnalizeObject(item, validation));
											}
										}
										ActionPopupWindow.CloseLast();
										if(action != null) {
											action(parameters);
										}
									}
								}).headerName = "Rename Generic Parameter";
							}, parameters[index]);
							menu.AddItem(new GUIContent("Change Type"), false, delegate (object g) {
								TypeSelectorWindow.ShowWindow(
									mPos, 
									new FilterAttribute() { OnlyGetType = true, ArrayManipulator = false, UnityReference = false, DisplaySealedType = false }, 
									members => {
									uNodeEditorUtility.RegisterUndo(targetObject, "Change Type");
									(g as GenericParameterData).typeConstraint = members[0];
									ActionPopupWindow.CloseLast();
									if(action != null) {
										action(parameters);
									}
									}, new TypeItem((g as ParameterData).type));
							}, parameters[index]);
							menu.ShowAsContext();
						}
					}
				};
				reorderable.onReorderCallbackWithDetails = (list, oldIndex, newIndex) => {
					var val = parameters[newIndex];
					uNodeUtility.RemoveListAt(ref parameters, newIndex);
					if(oldIndex >= parameters.Count) {
						uNodeUtility.AddList(ref parameters, val);
					} else {
						uNodeUtility.InsertList(ref parameters, oldIndex, val);
					}
					if(action != null) {
						action(parameters);
					}
					if(targetObject)
						uNodeEditorUtility.RegisterUndo(targetObject, "Reorder List");
					val = parameters[oldIndex];
					uNodeUtility.RemoveListAt(ref parameters, oldIndex);
					if(newIndex >= parameters.Count) {
						uNodeUtility.AddList(ref parameters, val);
					} else {
						uNodeUtility.InsertList(ref parameters, newIndex, val);
					}
					reorderable.list = parameters as System.Collections.IList;
					if(action != null) {
						action(parameters);
					}
				};
				reorderable.onAddDropdownCallback = (pos, list) => {
					ActionPopupWindow.ShowWindow(pos,
					"T",
					delegate (ref object obj) {
						obj = EditorGUILayout.TextField("Name", obj as string);
					}, null, delegate (ref object obj) {
						if(GUILayout.Button("Add")) {
							obj = uNodeUtility.AutoCorrectName(obj as string);
							string name = obj as string;
							if(parameters.Count > 0) {
								bool hasVariable = false;
								foreach(GenericParameterData parameter in parameters) {
									if(parameter.name == name) {
										hasVariable = true;
										break;
									}
								}
								int index = 0;
								while(hasVariable) {
									index++;
									hasVariable = false;
									name = (obj as string) + index.ToString();
									foreach(GenericParameterData parameter in parameters) {
										if(parameter.name == name) {
											hasVariable = true;
											break;
										}
									}
								}
							}
							uNodeEditorUtility.RegisterUndo(targetObject, "Add Generic Parameter: " + name);
							uNodeUtility.AddList(ref parameters, new GenericParameterData() { name = name });
							reorderable.list = parameters as System.Collections.IList;
							ActionPopupWindow.CloseLast();
							if(action != null) {
								action(parameters);
							}
						}
					}).headerName = "New Generic Parameter";
				};
				reorderable.onRemoveCallback = (list) => {
					if(targetObject)
						uNodeEditorUtility.RegisterUndo(targetObject, "Remove Generic Parameter: " + parameters[list.index].name);
					uNodeUtility.RemoveListAt(ref parameters, list.index);
					if(action != null) {
						action(parameters);
					}
					if(targetObject && uNodeEditorUtility.IsPrefab(targetObject)) {
						uNodeEditorUtility.MarkDirty(targetObject);
					}
				};
				_reorderabeMap[parameters] = reorderable;
			}
			reorderable.DoLayoutList();
		}
		
		public static void DrawAttribute(IList<AttributeData> attributes, UnityEngine.Object targetObject, Action<IList<AttributeData>> action, AttributeTargets attributeTargets = AttributeTargets.All) {
			if(attributes == null) {
				attributes = new AttributeData[0];
				if(action != null) {
					action(attributes);
				}
			}
			ReorderableList reorderable;
			if(!_reorderabeMap.TryGetValue(attributes, out reorderable)) {
				reorderable = new ReorderableList(attributes as System.Collections.IList, typeof(AttributeData));
				_reorderabeMap[attributes] = reorderable;
				reorderable.drawHeaderCallback = (pos) => {
					EditorGUI.LabelField(pos, "Attribute");
				};
				reorderable.drawElementCallback = (pos, index, isActive, isFocused) => {
					if(pos.Contains(Event.current.mousePosition) && Event.current.button == 0 && Event.current.clickCount == 2) {
						FieldsEditorWindow.ShowWindow(attributes[index], targetObject, delegate (object obj) {
							return attributes[(int)obj];
						}, index);
					}
					var attName = attributes[index].type.DisplayName(false, false);
					if(attName.EndsWith("Attribute")) {
						attName = attName.RemoveLast("Attribute".Length);
					}
					EditorGUI.LabelField(pos, new GUIContent(attName));
				};
				reorderable.onReorderCallbackWithDetails = (list, oldIndex, newIndex) => {
					var val = attributes[newIndex];
					uNodeUtility.RemoveListAt(ref attributes, newIndex);
					if(oldIndex >= attributes.Count) {
						uNodeUtility.AddList(ref attributes, val);
					} else {
						uNodeUtility.InsertList(ref attributes, oldIndex, val);
					}
					if(action != null) {
						action(attributes);
					}
					if(targetObject)
						uNodeEditorUtility.RegisterUndo(targetObject, "Reorder List");
					val = attributes[oldIndex];
					uNodeUtility.RemoveListAt(ref attributes, oldIndex);
					if(newIndex >= attributes.Count) {
						uNodeUtility.AddList(ref attributes, val);
					} else {
						uNodeUtility.InsertList(ref attributes, newIndex, val);
					}
					reorderable.list = attributes as System.Collections.IList;
					if(action != null) {
						action(attributes);
					}
				};

				reorderable.onAddDropdownCallback = (pos, list) => {
					ItemSelector.ShowWindow(targetObject, new FilterAttribute(typeof(Attribute)) { 
						DisplayAbstractType = false, 
						DisplayInterfaceType = false,
						OnlyGetType = true, 
						ArrayManipulator = false,
						UnityReference = false,
						attributeTargets = attributeTargets }, m => {
						var type = m.Get<Type>();
						var att = new AttributeData() { type = new MemberData(type) };
						if(type != null && !(type.IsAbstract || type.IsInterface)) {
							var ctor = type.GetConstructors().FirstOrDefault();
							if(ctor != null) {
								att.value = ValueData.CreateFromConstructor(ctor);
							}
						}
						uNodeUtility.AddList(ref attributes, att);
						if(action != null) {
							action(attributes);
						}
						reorderable.list = attributes as System.Collections.IList;
					}).ChangePosition(pos.ToScreenRect());
				};
				reorderable.onRemoveCallback = (list) => {
					if(targetObject)
						uNodeEditorUtility.RegisterUndo(targetObject, "Remove Attribute: " + attributes[reorderable.index].type);
					uNodeUtility.RemoveListAt(ref attributes, reorderable.index);
					if(action != null) {
						action(attributes);
					}
					reorderable.list = attributes as System.Collections.IList;
				};
			}
			reorderable.DoLayoutList();
		}


		public static void DrawMembers(GUIContent label, 
			List<MemberData> members, 
			Object targetObject, 
			FilterAttribute filter, 
			Action<List<MemberData>> action, 
			Action onDropDownClick = null,
			Action<int, int> onReorderCallback = null) {
			if(members == null) {
				members = new List<MemberData>();
				if(action != null) {
					action(members);
				}
			}
			ReorderableList reorderable;
			if(!_reorderabeMemberMap.TryGetValue(members, out reorderable)) {
				reorderable = new ReorderableList(members, typeof(AttributeData));
				reorderable.drawHeaderCallback = (pos) => {
					EditorGUI.LabelField(pos, label);
				};
				if(onReorderCallback != null) {
					reorderable.onReorderCallbackWithDetails = (list, oldIndex, newIndex) => {
						onReorderCallback(oldIndex, newIndex);
					};
				}
				reorderable.drawElementCallback = (pos, index, isActive, isFocused) => {
					pos = EditorGUI.PrefixLabel(pos, new GUIContent("Element " + index));
					pos.height = EditorGUIUtility.singleLineHeight;
					EditorReflectionUtility.ShowGUI(pos, members[index], filter, targetObject, (obj) => {
						members[index] = obj;
						if(action != null) {
							action(members);
						}
					});
				};
				reorderable.onAddDropdownCallback = (pos, list) => {
					if(onDropDownClick == null) {
						if(members.Count > 0){
							members.Add(new MemberData(members[members.Count-1]));
						} else {
							members.Add(MemberData.none);
						}
					} else {
						onDropDownClick();
					}
				};
				reorderable.onRemoveCallback = (list) => {
					if(targetObject)
						uNodeEditorUtility.RegisterUndo(targetObject, "Remove Member: " + members[reorderable.index]);
					members.RemoveAt(reorderable.index);
					if(action != null) {
						action(members);
					}
				};
				reorderable.onChangedCallback = (list) => {
					uNodeGUIUtility.GUIChanged(targetObject);
				};
				_reorderabeMemberMap[members] = reorderable;
			}
			reorderable.DoLayoutList();
		}

		public static void DrawInterfaces(IInterfaceSystem system, string headerLabel = "Interfaces", Action onChanged = null) {
			if(system.Interfaces == null) {
				system.Interfaces = new List<MemberData>();
				onChanged?.Invoke();
			}
			ReorderableList reorderable;
			if(!_reorderabeMap.TryGetValue(system, out reorderable)) {
				var interfaces = system.Interfaces.ToList();
				reorderable = new ReorderableList(interfaces, typeof(MemberData));
				reorderable.drawHeaderCallback = (pos) => {
					EditorGUI.LabelField(pos, headerLabel);
				};
				reorderable.drawElementCallback = (pos, index, isActive, isFocused) => {
					EditorGUI.LabelField(pos, new GUIContent(interfaces[index].DisplayName(false, false), uNodeEditorUtility.GetTypeIcon(interfaces[index])));
				};
				reorderable.onReorderCallbackWithDetails = (list, oldIndex, newIndex) => {
					var val = interfaces[newIndex];
					interfaces.RemoveAt(newIndex);
					if(oldIndex >= interfaces.Count) {
						interfaces.Add(val);
					} else {
						interfaces.Insert(oldIndex, val);
					}
					if(system as UnityEngine.Object)
						uNodeEditorUtility.RegisterUndo(system as UnityEngine.Object, "Reorder List");
					val = interfaces[oldIndex];
					interfaces.RemoveAt(oldIndex);
					if(newIndex >= interfaces.Count) {
						interfaces.Add(val);
					} else {
						interfaces.Insert(newIndex, val);
					}
					reorderable.list = interfaces;
					system.Interfaces = interfaces;
					onChanged?.Invoke();
				};
				reorderable.onAddDropdownCallback = (pos, list) => {
					var filter = new FilterAttribute() {
						OnlyGetType = true,
						ArrayManipulator = false,
						DisplayValueType = false,
						DisplayReferenceType = false,
						UnityReference = false,
						DisplayRuntimeType = system is IRuntimeInterfaceSystem,
						DisplayNativeType = !(system is IRuntimeInterfaceSystem),
					};
					ItemSelector.ShowWindow(system as Object, filter, (member) => {
						if(system as UnityEngine.Object)
							uNodeEditorUtility.RegisterUndo(system as UnityEngine.Object, "Add Interface");
						interfaces.Add(member);
						system.Interfaces = interfaces;
						onChanged?.Invoke();
						if(system as UnityEngine.Object && uNodeEditorUtility.IsPrefab(system as UnityEngine.Object)) {
							uNodeEditorUtility.MarkDirty(system as UnityEngine.Object);
						}
					}).ChangePosition(pos.ToScreenRect());
				};
				reorderable.onRemoveCallback = (list) => {
					if(system as UnityEngine.Object)
						uNodeEditorUtility.RegisterUndo(system as UnityEngine.Object, "Remove Interface: " + interfaces[list.index].DisplayName());
					interfaces.RemoveAt(list.index);
					system.Interfaces = interfaces;
					onChanged?.Invoke();
					if(system as UnityEngine.Object && uNodeEditorUtility.IsPrefab(system as UnityEngine.Object)) {
						uNodeEditorUtility.MarkDirty(system as UnityEngine.Object);
					}
				};
				_reorderabeMap[system] = reorderable;
			}
			reorderable.DoLayoutList();
		}

		public static void DrawInterfaceFunction(
			IList<InterfaceFunction> functions, 
			UnityEngine.Object targetObject, 
			Action<IList<InterfaceFunction>> action) {
			if (functions == null) {
				functions = new InterfaceFunction[0];
				if (action != null) {
					action(functions);
				}
			}
			DrawCustomList<InterfaceFunction>(
				values: functions,
				headerLabel: "Functions",
				drawElement: (position, index, value) => {
					var element = functions[index];
					position = EditorGUI.PrefixLabel(position, new GUIContent(element.name));
					position.x += position.width - 50;
					position.width = 50;
					position.height = EditorGUIUtility.singleLineHeight;
					if (GUI.Button(position, new GUIContent("Edit"), "minibutton")) {
						if (Event.current.button == 0) {
							FieldsEditorWindow.ShowWindow(element, targetObject, (obj) => {
								return functions[(int)obj];
							}, index);
						}
					}
				},
				add: (pos) => {
					uNodeEditorUtility.RegisterUndo(targetObject, "Add new function");
					uNodeUtility.AddList(ref functions, new InterfaceFunction() { name = "NewFunction" });
					if(action != null) {
						action(functions);
					}
				},
				remove: (index) => {
					uNodeEditorUtility.RegisterUndo(targetObject, "Remove Function: " + functions[index].name);
					uNodeUtility.RemoveListAt(ref functions, index);
					if(action != null) {
						action(functions);
					}
				});
		}

		public static void DrawInterfaceProperty(
			IList<InterfaceProperty> properties, 
			UnityEngine.Object targetObject, 
			Action<IList<InterfaceProperty>> action) {
			if (properties == null) {
				properties = new InterfaceProperty[0];
				if (action != null) {
					action(properties);
				}
			}
			DrawCustomList<InterfaceProperty>(
				values: properties,
				headerLabel: "Properties",
				drawElement: (position, index, value) => {
					var element = properties[index];
					EditorGUI.PrefixLabel(position, new GUIContent(element.name));
					position.x += position.width - 50;
					position.width = 50;
					position.height = EditorGUIUtility.singleLineHeight;
					if (GUI.Button(position, new GUIContent("Edit"), "minibutton")) {
						if (Event.current.button == 0) {
							FieldsEditorWindow.ShowWindow(element, targetObject, (obj) => {
								return properties[(int)obj];
							}, index);
						}
					}
				},
				add: (pos) => {
					uNodeEditorUtility.RegisterUndo(targetObject, "Add new property");
					uNodeUtility.AddList(ref properties, new InterfaceProperty() { name = "NewProperty" });
					if(action != null) {
						action(properties);
					}
				},
				remove: (index) => {
					uNodeEditorUtility.RegisterUndo(targetObject, "Remove property: " + properties[index].name);
					uNodeUtility.RemoveListAt(ref properties, index);
					if(action != null) {
						action(properties);
					}
				});
		}

		public static void DrawParameter(MemberData parameter, GUIContent label, System.Type parameterType, Object unityObject) {
			Rect fieldPos = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight);
			DrawParameter(fieldPos, parameter, label, parameterType, unityObject);
		}

		public static void DrawParameter(Rect fieldPos, MemberData parameter, GUIContent label, System.Type parameterType, Object unityObject) {
			if(parameterType != null) {
				fieldPos = EditorGUI.PrefixLabel(fieldPos, label);
				if(parameterType != typeof(void)) {
					FilterAttribute rFilter = new FilterAttribute(parameterType);
					EditorReflectionUtility.RenderVariable(fieldPos, parameter, GUIContent.none, unityObject, rFilter);
				} else {
					EditorGUI.HelpBox(fieldPos, "Void", MessageType.None);
				}
			}
		}

		public static void DrawMultipurposeMember(Rect position, SerializedProperty property, GUIContent label) {
			position.height = EditorGUIUtility.singleLineHeight;
			MultipurposeMember variable = PropertyDrawerUtility.GetActualObjectForSerializedProperty<MultipurposeMember>(property);
			if(variable != null) {
				DrawMultipurposeMember(position, variable, property.serializedObject.targetObject, label);
			}
		}

		public static void DrawMultipurposeMember(Rect position, MultipurposeMember member, Object unityObject, GUIContent label, FilterAttribute filter = null) {
			if(member == null)
				return;
			if(filter == null) {
				filter = new FilterAttribute();
				filter.MaxMethodParam = int.MaxValue;
				//filter.DisplayProperty = false;
				//filter.DisplayField = false;
				filter.SetMember = true;
				filter.VoidType = true;
			}
			bool setMember = filter.SetMember;
			filter.SetMember = false;
			filter.InvalidTargetType |= MemberData.TargetType.Values | MemberData.TargetType.Null;
			EditorReflectionUtility.RenderVariable(position, member.target, label, unityObject, filter);
			if(member.parameters == null) {
				member.parameters = new MemberData[0];
			}
			if(member.target.isAssigned) {
				if(member.target.SerializedItems?.Length > 0) {
					MemberInfo[] members;
					{//For documentation
						members = member.target.GetMembers(false);
						if(members != null && members.Length > 0 && members.Length + 1 != member.target.SerializedItems.Length) {
							members = null;
						}
					}
					uNodeFunction objRef = null;
					switch (member.target.targetType) {
						case MemberData.TargetType.uNodeFunction: {
								uNodeRoot root = member.target.GetInstance() as uNodeRoot;
								if (root != null) {
									var gTypes = MemberData.Utilities.SafeGetGenericTypes(member.target)[0];
									objRef = root.GetFunction(member.target.startName, gTypes != null ? gTypes.Length : 0, MemberData.Utilities.SafeGetParameterTypes(member.target)[0]);
								}
								break;
							}
					}
					int totalParam = 0;
					int methodDrawCount = 0;
					bool flag = false;
					for(int i = 0; i < member.target.SerializedItems.Length; i++) {
						if(i != 0) {
							if(members != null && (member.target.isDeepTarget || !member.target.IsTargetingUNode)) {
								MemberInfo memberInfo = members[i - 1];
								if(memberInfo is IRuntimeMember) {
									if(flag) {
										EditorGUILayout.Space();
									}
									EditorGUILayout.LabelField(new GUIContent(memberInfo.Name), EditorStyles.boldLabel);
									if(memberInfo is ISummary summary) {
										EditorGUI.indentLevel++;
										EditorGUILayout.LabelField(summary.GetSummary(), EditorStyles.wordWrappedLabel);
										EditorGUI.indentLevel--;
									}
									if(memberInfo is RuntimeMethod runtimeMethod) {
										var parameters = runtimeMethod.GetParameters();
										while(parameters.Length + totalParam > member.parameters.Length) {
											ArrayUtility.Add(ref member.parameters, MemberData.empty);
										}
										for (int x = 0; x < parameters.Length; x++) {
											var PType = parameters[x].ParameterType;
											if(PType != null) {
												DrawParameter(uNodeGUIUtility.GetRect(), 
													member.parameters[totalParam], 
													new GUIContent(ObjectNames.NicifyVariableName(parameters[x].Name), 
													uNodeEditorUtility.GetTypeIcon(PType), 
													PType.PrettyName(true)), 
													PType, 
													unityObject);
												if (parameters[x] is ISummary paramSummary) {
													EditorGUI.indentLevel++;
													//Show documentation
													EditorGUILayout.LabelField(paramSummary.GetSummary(), EditorStyles.wordWrappedLabel);
													EditorGUI.indentLevel--;
												}
											}
											totalParam++;
										}
										flag = true;
										continue;
									}
								} else if(memberInfo is MethodInfo || memberInfo is ConstructorInfo) {
									var method = memberInfo as MethodInfo;
									if(flag) {
										EditorGUILayout.Space();
									}
									var documentation = XmlDoc.XMLFromMember(memberInfo);
									EditorGUILayout.LabelField(new GUIContent(method != null ? method.Name : EditorReflectionUtility.GetConstructorPrettyName(memberInfo as ConstructorInfo)), EditorStyles.boldLabel);
									if(documentation != null && documentation["summary"] != null) {
										EditorGUI.indentLevel++;
										EditorGUILayout.LabelField(documentation["summary"].InnerText.Trim(), EditorStyles.wordWrappedLabel);
										EditorGUI.indentLevel--;
									}
									var parameters = method != null ? method.GetParameters() : (memberInfo as ConstructorInfo).GetParameters();
									if(parameters.Length > 0) {
										while(parameters.Length + totalParam > member.parameters.Length) {
											ArrayUtility.Add(ref member.parameters, MemberData.empty);
										}
										for(int x = 0; x < parameters.Length; x++) {
											System.Type PType = parameters[x].ParameterType;
											if(PType != null) {
												Rect fieldPos = uNodeGUIUtility.GetRect();
												EditorGUI.indentLevel++;
												DrawParameter(fieldPos, member.parameters[totalParam], new GUIContent(ObjectNames.NicifyVariableName(parameters[x].Name), uNodeEditorUtility.GetTypeIcon(PType), PType.PrettyName(true)), PType, unityObject);
												EditorGUI.indentLevel++;
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
														//Show documentation
														EditorGUILayout.LabelField(paramDoc.InnerText.Trim(), EditorStyles.wordWrappedLabel);
													}
												}
												EditorGUI.indentLevel--;
												EditorGUI.indentLevel--;
											}
											totalParam++;
										}
										flag = true;
										continue;
									}
								}
							}
						}
						System.Type[] paramsType = MemberData.Utilities.SafeGetParameterTypes(member.target)[i];
						if(paramsType != null && paramsType.Length > 0) {
							if(methodDrawCount > 0) {
								EditorGUILayout.LabelField("Method " + (methodDrawCount), EditorStyles.boldLabel);
							}
							while(paramsType.Length + totalParam > member.parameters.Length) {
								ArrayUtility.Add(ref member.parameters, MemberData.none);
							}
							for(int x = 0; x < paramsType.Length; x++) {
								System.Type PType = paramsType[x];
								if(member.parameters[totalParam] == null) {
									member.parameters[totalParam] = MemberData.none;
								}
								if(PType != null) {
									Rect fieldPos = uNodeGUIUtility.GetRect();
									GUIContent pLabel;
									if(objRef != null && methodDrawCount == 0) {
										pLabel = new GUIContent(objRef.parameters[x].name, PType.PrettyName());
									} else {
										pLabel = new GUIContent("P" + (x + 1));
									}
									DrawParameter(fieldPos, member.parameters[totalParam], pLabel, PType, unityObject);
								}
								totalParam++;
							}
							methodDrawCount++;
						}
					}
					while(member.parameters.Length > totalParam) {
						ArrayUtility.RemoveAt(ref member.parameters, member.parameters.Length - 1);
					}
				}
			}
		}

		public static void DrawMultipurposeMember(MultipurposeMember variable, Object unityObject, GUIContent label, FilterAttribute filter = null) {
			if(variable == null)
				return;
			float width = EditorGUIUtility.fieldWidth;
			float height = EditorGUIUtility.singleLineHeight;
			Rect position = GUILayoutUtility.GetRect(width, height);
			DrawMultipurposeMember(position, variable, unityObject, label, filter);
		}

		#region DrawFields
		public static void DrawBoolField(Rect position, ref bool variable, Object unityObject, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			var newVar = EditorGUI.Toggle(position, label, variable);
			if(EditorGUI.EndChangeCheck()) {
				if(newVar != variable) {
					uNodeEditorUtility.RegisterUndo(unityObject, label.text);
					variable = newVar;
				}
			}
		}

		public static void DrawStringField(Rect position, ref string variable, Object unityObject, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			var newVar = EditorGUI.TextField(position, label, variable);
			if(EditorGUI.EndChangeCheck()) {
				if(newVar != variable) {
					uNodeEditorUtility.RegisterUndo(unityObject, label.text);
					variable = newVar;
				}
			}
		}

		public static void DrawIntField(Rect position, ref int variable, Object unityObject, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			var newVar = EditorGUI.IntField(position, label, variable);
			if(EditorGUI.EndChangeCheck()) {
				if(newVar != variable) {
					uNodeEditorUtility.RegisterUndo(unityObject, label.text);
					variable = newVar;
				}
			}
		}

		public static void DrawFloatField(Rect position, ref float variable, Object unityObject, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			var newVar = EditorGUI.FloatField(position, label, variable);
			if(EditorGUI.EndChangeCheck()) {
				if(newVar != variable) {
					uNodeEditorUtility.RegisterUndo(unityObject, label.text);
					variable = newVar;
				}
			}
		}

		public static void DrawVector2Field(Rect position, ref Vector2 variable, Object unityObject, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			var newVar = EditorGUI.Vector2Field(position, label, variable);
			if(EditorGUI.EndChangeCheck()) {
				if(newVar != variable) {
					uNodeEditorUtility.RegisterUndo(unityObject, label.text);
					variable = newVar;
				}
			}
		}

		public static void DrawVector2Field(ref Vector2 variable, Object unityObject, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			var newVar = EditorGUILayout.Vector2Field(label, variable);
			if(EditorGUI.EndChangeCheck()) {
				if(newVar != variable) {
					uNodeEditorUtility.RegisterUndo(unityObject, label.text);
					variable = newVar;
				}
			}
		}

		public static void DrawVector3Field(Rect position, ref Vector3 variable, Object unityObject, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			var newVar = EditorGUI.Vector3Field(position, label, variable);
			if(EditorGUI.EndChangeCheck()) {
				if(newVar != variable) {
					uNodeEditorUtility.RegisterUndo(unityObject, label.text);
					variable = newVar;
				}
			}
		}

		public static void DrawVector3Field(ref Vector3 variable, Object unityObject, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			var newVar = EditorGUILayout.Vector3Field(label, variable);
			if(EditorGUI.EndChangeCheck()) {
				if(newVar != variable) {
					uNodeEditorUtility.RegisterUndo(unityObject, label.text);
					variable = newVar;
				}
			}
		}

		public static void DrawColorField(Rect position, ref Color variable, Object unityObject, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			var newVar = EditorGUI.ColorField(position, label, variable);
			if(EditorGUI.EndChangeCheck()) {
				if(newVar != variable) {
					uNodeEditorUtility.RegisterUndo(unityObject, label.text);
					variable = newVar;
				}
			}
		}

		public static void DrawColorField(ref Color variable, Object unityObject, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			var newVar = EditorGUILayout.ColorField(label, variable);
			if(EditorGUI.EndChangeCheck()) {
				if(newVar != variable) {
					uNodeEditorUtility.RegisterUndo(unityObject, label.text);
					variable = newVar;
				}
			}
		}

		public static void DrawObjectField(Rect position, ref Object variable, System.Type objectType, Object unityObject, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			var newVar = EditorGUI.ObjectField(position, label, variable, objectType, uNodeEditorUtility.IsSceneObject(unityObject));
			if(EditorGUI.EndChangeCheck()) {
				if(newVar != variable) {
					uNodeEditorUtility.RegisterUndo(unityObject, label.text);
					variable = newVar;
				}
			}
		}

		public static System.Enum DrawEnumField(Rect position, ref string variable, System.Type enumType, Object unityObject, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			System.Enum newVar = null;
			{
				bool createNew = true;
				if(!string.IsNullOrEmpty(variable)) {
					string[] EnumNames = System.Enum.GetNames(enumType);
					foreach(string str in EnumNames) {
						if(str == variable) {
							newVar = (System.Enum)System.Enum.Parse(enumType, variable);
							createNew = false;
							break;
						}
					}
				}
				if(createNew) {
					newVar = (System.Enum)System.Activator.CreateInstance(enumType);
				}
			}
			newVar = EditorGUI.EnumPopup(position, label, newVar);
			if(EditorGUI.EndChangeCheck()) {
				if(newVar.ToString() != variable) {
					uNodeEditorUtility.RegisterUndo(unityObject, label.text);
					variable = newVar.ToString();
				}
			}
			return newVar;
		}
		#endregion
	}
}