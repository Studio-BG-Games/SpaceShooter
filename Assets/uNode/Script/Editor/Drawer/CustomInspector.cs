using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;

namespace MaxyGames.uNode.Editors {
	public class CustomInspector : ScriptableObject {
		public GraphEditorData editorData;

		private static CustomInspector _default;
		internal static CustomInspector Default {
			get {
				if(_default == null) {
					_default = CreateInstance<CustomInspector>();
				}
				return _default;
			}
		}

		private static Dictionary<UnityEngine.Object, Editor> customEditors = new Dictionary<UnityEngine.Object, Editor>();

		public static Editor GetEditor(UnityEngine.Object obj) {
			if(!customEditors.TryGetValue(obj, out var editor)) {
				editor = Editor.CreateEditor(obj);
				customEditors[obj] = editor;
			}
			return editor;
		}

		public static void ResetEditor() {
			customEditors.Clear();
		}

		public static void ShowInspector(GraphEditorData editorData, int limitMultiEdit = 5) {
			if(editorData.selected == null) {
				if(editorData.currentCanvas != null) {
					EditorGUI.DropShadowLabel(uNodeGUIUtility.GetRect(), "Graph");
					EditorGUILayout.Space();
					DrawGraphInspector(editorData.graph, true);
					if(editorData.currentCanvas != editorData.graph) {
						EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
						EditorGUI.DropShadowLabel(uNodeGUIUtility.GetRect(), "Edited Canvas");
						EditorGUILayout.Space();
						DrawUnitObject(editorData.currentCanvas);
					}
				} else if(editorData.graphData != null) {
					DrawUnitObject(editorData.graphData);
				}
				return;
			}
			EditorGUI.BeginDisabledGroup(
				(!Application.isPlaying || uNodePreference.GetPreference().preventEditingPrefab) &&
				uNodeEditorUtility.IsPrefab(editorData.owner));
			if(editorData.selected is List<NodeComponent> && editorData.selectedNodes.Count > 0 && editorData.graph != null) {
				editorData.selectedNodes.RemoveAll(item => item == null);
				int drawCount = 0;
				for(int i = 0; i < editorData.selectedNodes.Count; i++) {
					if(i >= editorData.selectedNodes.Count)
						break;
					NodeComponent selectedNode = editorData.selectedNodes[i];
					if(drawCount >= limitMultiEdit) {
						break;
					}
					if(editorData.selectedNodes.Count == 1) {
						EditorGUILayout.BeginHorizontal();
						GUI.DrawTexture(uNodeGUIUtility.GetRect(GUILayout.Width(32), GUILayout.Height(32)), uNodeEditorUtility.GetTypeIcon(selectedNode.GetNodeIcon()));
						EditorGUILayout.BeginVertical();
						EditorGUILayout.BeginHorizontal();
						GUILayout.Label(selectedNode.gameObject.name);
						//var name = uNodeEditorGUI.TextInput("", "(Title)", true);
						var rect = uNodeGUIUtility.GetRect(GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight), GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight));
						GUI.DrawTexture(rect, Resources.Load<Texture2D>("uNodeIcon/edit_button"));
						if(rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0) {
							RenameNode(selectedNode, rect);
						}
						EditorGUILayout.EndHorizontal();
						GUILayout.Space(2);
						selectedNode.comment = uNodeEditorGUI.TextInput(selectedNode.comment, "(Summary)", true);
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
						DrawNodeEditor(selectedNode);
					} else {
						if(drawCount > 0) {
							EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
						}
						if(selectedNode is Node) {
							EditorGUILayout.BeginHorizontal();
							Rect rect1 = GUILayoutUtility.GetRect(220, 18);
							//GUI.backgroundColor = Color.gray;
							GUI.Box(new Rect(rect1.x - 30, rect1.y, rect1.width + 50, rect1.height), GUIContent.none, (GUIStyle)"dockarea");
							//GUI.backgroundColor = Color.white;
							GUI.Label(rect1, selectedNode.gameObject.name, EditorStyles.boldLabel);
							if(Event.current.type == EventType.MouseUp && Event.current.button == 1 && rect1.Contains(Event.current.mousePosition)) {
								GenericMenu menu = new GenericMenu();
								ShowInspectorMenu(menu, selectedNode);
							}
							if(Event.current.clickCount == 2 && Event.current.button == 0 && rect1.Contains(Event.current.mousePosition)) {
								RenameNode(selectedNode, rect1);
							}
							EditorGUILayout.EndHorizontal();
						}
						if(DrawNodeEditor(selectedNode)) {
							drawCount++;
						}
					}
				}
				if(drawCount >= limitMultiEdit) {
					EditorGUILayout.BeginVertical("Box");
					EditorGUILayout.HelpBox("Multi Editing Limit : " + limitMultiEdit, MessageType.Info);
					EditorGUILayout.EndVertical();
				}
			} else if(editorData.selected is TransitionEvent transitionEvent && editorData.graph != null) {
				EditorGUILayout.BeginVertical("Box");
				Editor editor = GetEditor(transitionEvent);
				if(editor != null && transitionEvent.owner == editorData.graph) {
					editor.OnInspectorGUI();
					if(transitionEvent.GetType().GetCustomAttributes(typeof(DescriptionAttribute), false).Length != 0) {
						DescriptionAttribute descriptionEvent = (DescriptionAttribute)transitionEvent.GetType().GetCustomAttributes(typeof(DescriptionAttribute), false)[0];
						if(descriptionEvent.description != null && descriptionEvent != null) {
							GUI.backgroundColor = Color.yellow;
							EditorGUILayout.HelpBox("Description: " + descriptionEvent.description, MessageType.None);
							GUI.backgroundColor = Color.white;
						}
					}
				}
				EditorGUILayout.EndVertical();
				if(GUI.changed) {
					uNodeEditor.GUIChanged(editorData.selected);
				}
			} else if(editorData.selected is VariableData) {
				List<VariableData> listVariable = null;
				if(editorData.graph) {
					if(editorData.graph.Variables.Contains(editorData.selected as VariableData)) {
						listVariable = editorData.graph.Variables;
					}
				}
				uNodeGUIUtility.DrawVariable(editorData.selected as VariableData, editorData.graph, true, listVariable, listVariable == null);
			} else if(editorData.selected is uNodeRoot) {
				DrawGraphInspector(editorData.selected as uNodeRoot, true);
			} else if(editorData.selected is NodeComponent) {
				DrawNodeEditor(editorData.selected as NodeComponent);
			} else if(editorData.selected is UnityEngine.Object) {
				DrawUnitObject(editorData.selected as UnityEngine.Object);
			} else if(editorData.selected is InterfaceData) {
				InterfaceData data = editorData.selected as InterfaceData;
				uNodeGUIUtility.DrawInterfaceData(data, editorData.graphData);
			} else if(editorData.selected is EnumData) {
				EnumData data = editorData.selected as EnumData;
				uNodeGUIUtility.DrawEnumData(data, editorData.graphData);
			} else if(editorData.selected is uNodeEditor.ValueInspector) {
				uNodeEditor.ValueInspector inspector = editorData.selected as uNodeEditor.ValueInspector;
				if(inspector.value != null) {
					if(inspector.value is EdgeView) {
						var edge = inspector.value as EdgeView;
						var input = edge.Output;
						var output = edge.Input;
						if(edge is ConversionEdgeView) {
							DrawNodeEditor((edge as ConversionEdgeView).node);
							EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
						}
						if(input != null) {
							EditorGUI.DropShadowLabel(uNodeGUIUtility.GetRect(), "Input");
							if(edge.isFlow) {
								EditorGUI.LabelField(
									EditorGUI.PrefixLabel(uNodeGUIUtility.GetRect(), new GUIContent("Type")),
									new GUIContent("Flow", uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
							} else {
								var portType = input.GetPortType();
								EditorGUI.LabelField(
									EditorGUI.PrefixLabel(uNodeGUIUtility.GetRect(), new GUIContent("Type")),
									new GUIContent(portType.PrettyName(false), uNodeEditorUtility.GetTypeIcon(portType)));
							}
							//DrawNodeEditor(inputNode);
						}
						if(output != null) {
							EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
							EditorGUI.DropShadowLabel(uNodeGUIUtility.GetRect(), "Output");
							if(edge.isFlow) {
								EditorGUI.LabelField(
									EditorGUI.PrefixLabel(uNodeGUIUtility.GetRect(), new GUIContent("Type")),
									new GUIContent("Flow", uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
							} else {
								var portType = output.GetPortType();
								EditorGUI.LabelField(
									EditorGUI.PrefixLabel(uNodeGUIUtility.GetRect(), new GUIContent("Type")),
									new GUIContent(portType.PrettyName(false), uNodeEditorUtility.GetTypeIcon(portType)));
							}
							//DrawNodeEditor(outputNode);
						}
						if(Application.isPlaying && GraphDebug.useDebug && !edge.isFlow && edge.Input != null) {
							PortView portView = edge.Input;
							GraphDebug.DebugData debugData = portView.owner.owner.graph.GetDebugInfo();
							if(debugData != null) {
								MemberData member = portView.portData.GetPortValue();
								if(member != null) {
									var debugValue = debugData.GetDebugValue(member);
									if(debugValue.isValid) {
										GUIContent debugContent;
										if(debugValue.value != null) {
											debugContent = new GUIContent(
												uNodeUtility.GetDebugName(debugValue.value),
												uNodeEditorUtility.GetTypeIcon(debugValue.value.GetType())
											);
										} else {
											debugContent = new GUIContent("null");
										}
										EditorGUI.LabelField(EditorGUI.PrefixLabel(uNodeGUIUtility.GetRect(), new GUIContent("Debug Value")), debugContent);
									}
								}
							}
						}
					} else {
						uNodeGUIUtility.EditValueLayouted(GUIContent.none, inspector.value, inspector.value.GetType(), null, new uNodeUtility.EditValueSettings() { unityObject = inspector.owner });
					}
				}
			}
			EditorGUI.EndDisabledGroup();
		}

		private static void RenameNode(NodeComponent node, Rect rect) {
			ActionPopupWindow.ShowWindow(rect,
				new object[] { node.gameObject.name, node.gameObject },
				(ref object obj) => {
					object[] o = obj as object[];
					o[0] = EditorGUILayout.TextField("Name", o[0] as string);
				}, null, delegate (ref object obj) {
					if(GUILayout.Button("Apply") || Event.current.keyCode == KeyCode.Return) {
						object[] o = obj as object[];
						uNodeEditorUtility.RegisterUndo((GameObject)o[1], "Rename node");
						((GameObject)o[1]).name = o[0] as string;
						ActionPopupWindow.CloseLast();
						uNodeGUIUtility.GUIChanged(node);
					}
				}).headerName = "Rename Node";
		}

		// private static void DrawLine(float height = 1, float expandingWidth = 15) {
		// 	var rect = uNodeEditorUtility.GetRectCustomHeight(height);
		// 	Handles.color = Color.gray;
		// 	Handles.DrawLine(new Vector2(rect.x - expandingWidth, rect.y), new Vector2(rect.width + expandingWidth, rect.y));
		// }

		public static void DrawUnitObject(UnityEngine.Object obj) {
			if(obj == null)
				return;
			if(obj is uNodeFunction) {
				uNodeFunction function = obj as uNodeFunction;
				var system = GraphUtility.GetGraphSystem(function.owner);
				Editor editor = GetEditor(function);
				if(editor != null) {
					editor.OnInspectorGUI();
					MemberData oldMember = MemberData.Clone(function.returnType);
					uNodeGUIUtility.EditValueLayouted(nameof(function.returnType), function, (val) => {
						function.returnType = oldMember;
						uNodeEditorUtility.RegisterUndo(function, "Change Type");
						Action updateAction = RefactorUtility.GetUpdateReferencesAction(function);
						(val as MemberData).CopyTo(function.returnType);
						updateAction?.Invoke();
						uNodeEditorUtility.MarkDirty(function);
						uNodeGUIUtility.GUIChanged(function);
					});
					if(system != null && system.supportAttribute) {
						if(function.attributes == null) {
							function.attributes = new AttributeData[0];
						}
						VariableEditorUtility.DrawAttribute(function.attributes, function, (a) => {
							function.attributes = a.ToArray();
						}, AttributeTargets.Method);
					}
					EditorGUI.BeginDisabledGroup(function.transform.parent?.GetComponent<uNodeProperty>() != null);
					VariableEditorUtility.DrawParameter(function.parameters, function, function.owner, delegate (ParameterData[] p) {
						function.parameters = p;
					});
					if(system != null && system.supportGeneric) {
						VariableEditorUtility.DrawGenericParameter(function.genericParameters, function, function.owner, p => {
							function.genericParameters = p.ToArray();
						});
					}
					EditorGUI.EndDisabledGroup();
				}
			} else if(obj is uNodeProperty) {
				var selected = obj as uNodeProperty;
				var system = GraphUtility.GetGraphSystem(selected.owner);
				Editor editor = GetEditor(selected);
				if(editor != null) {
					editor.OnInspectorGUI();
					if(system.supportAttribute) {
						VariableEditorUtility.DrawAttribute(selected.attributes, selected, (a) => {
							selected.attributes = a.ToArray();
						}, AttributeTargets.Property);
					}
					if(Application.isPlaying && selected.AutoProperty) {
						uNodeGUIUtility.ShowField("autoPropertyValue", selected, selected, new uNodeUtility.EditValueSettings(selected) { nullable = true });
					}
				}
			} else if(obj is uNodeConstuctor) {
				uNodeConstuctor function = obj as uNodeConstuctor;
				var system = GraphUtility.GetGraphSystem(function.owner);
				Editor editor = GetEditor(function);
				if(editor != null) {
					editor.OnInspectorGUI();
					VariableEditorUtility.DrawParameter(function.parameters, function, function.owner, (p) => {
						function.parameters = p;
					});
				}
			} else {
				Editor editor = GetEditor(obj);
				if(editor != null) {
					editor.OnInspectorGUI();
				}
				if(GUI.changed) {
					uNodeGUIUtility.GUIChanged(obj);
				}
			}
		}

		public static void DrawGraphInspector(uNodeRoot root, bool showAllStuff = false) {
			var system = GraphUtility.GetGraphSystem(root);
			Editor editor = GetEditor(root);
			if(editor == null)
				return;
			if(root is uNodeRuntime) {
				uNodeRuntime UNR = root as uNodeRuntime;
				if(showAllStuff) {
					editor.DrawDefaultInspector();
					VariableEditorUtility.DrawNamespace("Using Namespaces", UNR.usingNamespaces, root, (arr) => {
						UNR.usingNamespaces = arr.ToList();
						uNodeEditorUtility.MarkDirty(root);
					});
					VariableEditorUtility.DrawGraphVariable(root, (variables) => {
						// root.Variables = variables;
						uNodeEditorUtility.MarkDirty(root);
					});
					uNodeGUIUtility.EditValueLayouted(nameof(root.graphData.graphLayout), root.graphData, val => {
						root.graphData.graphLayout = (GraphLayout)val;
						UGraphView.ClearCache(root);
					});
				} else {
					uNodeGUIUtility.ShowField(nameof(UNR.Name), UNR, UNR);
					uNodeGUIUtility.DrawVariablesInspector(UNR.Variables, UNR, null);
				}
			} else {
				editor.DrawDefaultInspector();
				var graphSystem = GraphUtility.GetGraphSystem(root);
				if(graphSystem.supportVariable) {
					VariableEditorUtility.DrawGraphVariable(root, (variables) => {
						// root.Variables = variables;
						uNodeEditorUtility.MarkDirty(root);
					});
				}
				IEnumerable<string> usingNamespaces = null;
				if(root is IIndependentGraph independentGraph) {
					usingNamespaces = independentGraph.UsingNamespaces;
					VariableEditorUtility.DrawNamespace("Using Namespaces", independentGraph.UsingNamespaces, root, (arr) => {
						independentGraph.UsingNamespaces = arr as List<string> ?? arr.ToList();
						uNodeEditorUtility.MarkDirty(root);
					});
				} else if(!uNodeEditorUtility.IsPrefab(root)) {
					var data = root.GetComponent<uNodeData>();
					if(data == null) {
						data = root.gameObject.AddComponent<uNodeData>();
					}
					usingNamespaces = data.generatorSettings.usingNamespace;
					VariableEditorUtility.DrawNamespace("Using Namespaces", data.generatorSettings.usingNamespace, root, (arr) => {
						data.generatorSettings.usingNamespace = arr.ToArray();
						uNodeEditorUtility.MarkDirty(root);
					});
				}
				if(root is uNodeMacro) {
					var macro = root as uNodeMacro;
					// VariableEditorUtility.DrawVariable(macro.variables, macro,
					// 	(v) => {
					// 		macro.variables = v;
					// 		uNodeEditorUtility.MarkDirty(macro);
					// 	}, null);
					VariableEditorUtility.DrawCustomList(
						macro.inputFlows,
						"Input Flows",
						(position, index, element) => {//Draw Element
							EditorGUI.LabelField(position, new GUIContent(element.GetName(), uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
						}, null, null);
					VariableEditorUtility.DrawCustomList(
						macro.outputFlows,
						"Output Flows",
						(position, index, element) => {//Draw Element
							EditorGUI.LabelField(position, new GUIContent(element.GetName(), uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
						}, null, null);
					VariableEditorUtility.DrawCustomList(
						macro.inputValues,
						"Input Values",
						(position, index, element) => {//Draw Element
							position.width -= EditorGUIUtility.labelWidth;
							EditorGUI.LabelField(position, new GUIContent(element.GetName(), uNodeEditorUtility.GetTypeIcon(element.ReturnType())));
							position.x += EditorGUIUtility.labelWidth;
							uNodeGUIUtility.EditValue(position, GUIContent.none, "type", element, element);
						}, null, null);
					VariableEditorUtility.DrawCustomList(
						macro.outputValues,
						"Output Values",
						(position, index, element) => {//Draw Element
							position.width -= EditorGUIUtility.labelWidth;
							EditorGUI.LabelField(position, new GUIContent(element.GetName(), uNodeEditorUtility.GetTypeIcon(element.ReturnType())));
							position.x += EditorGUIUtility.labelWidth;
							uNodeGUIUtility.EditValue(position, GUIContent.none, "type", element, element);
						}, null, null);
				}
				if(system.supportAttribute && root is IAttributeSystem) {
					VariableEditorUtility.DrawAttribute((root as IAttributeSystem).Attributes, root, (attributes) => {
						(root as IAttributeSystem).Attributes = attributes;
					}, root is uNodeStruct ? AttributeTargets.Struct : AttributeTargets.Class);
				}
				if(system.supportGeneric) {
					VariableEditorUtility.DrawGenericParameter((root as IGenericParameterSystem).GenericParameters, root, root, (p) => {
						(root as IGenericParameterSystem).GenericParameters = p;
					});
				}
				if(root is IInterfaceSystem) {
					var ifaceSystem = root as IInterfaceSystem;
					var ifaces = ifaceSystem.Interfaces;
					VariableEditorUtility.DrawInterfaces(ifaceSystem);
				}
				uNodeGUIUtility.EditValueLayouted(nameof(root.graphData.graphLayout), root.graphData, val => {
					root.graphData.graphLayout = (GraphLayout)val;
					UGraphView.ClearCache(root);
				});
				#region Error Checker
				if(usingNamespaces != null) {
					var namespaces = EditorReflectionUtility.GetNamespaces();
					foreach(var ns in usingNamespaces) {
						if(!namespaces.Contains(ns)) {
							EditorGUILayout.BeginVertical();
							EditorGUILayout.HelpBox($@"Using Namespace: '{ns}' was not found.", MessageType.Error);
							EditorGUILayout.EndVertical();
						}
					}
				}
				if(root is IInterfaceSystem) {
					var ifaceSystem = root as IInterfaceSystem;
					var ifaces = ifaceSystem.Interfaces;
					if(ifaces != null && !uNodeEditorUtility.IsPrefab(root)) {
						foreach(var iface in ifaces) {
							if(!iface.isAssigned)
								continue;
							var type = iface.startType;
							if(type == null)
								continue;
							var methods = type.GetMethods();
							foreach(var member in methods) {
								if(member.Name.StartsWith("get_", StringComparison.Ordinal) || member.Name.StartsWith("set_", StringComparison.Ordinal)) {
									continue;
								}
								if(!root.GetFunction(
									member.Name,
									member.GetGenericArguments().Length,
									member.GetParameters().Select(item => item.ParameterType).ToArray())) {
									var memberRect = EditorGUILayout.BeginVertical();
									EditorGUILayout.HelpBox(
$@"The graph does not implement interface method: 
'{ type.PrettyName() }' type: '{EditorReflectionUtility.GetOverloadingMethodNames(member)}'
[Click to implement it]", MessageType.Error);
									EditorGUILayout.EndVertical();
									if(Event.current.button == 0 && Event.current.type == EventType.MouseDown && memberRect.Contains(Event.current.mousePosition)) {
										NodeEditorUtility.AddNewFunction(root, member.Name, member.ReturnType,
											member.GetParameters().Select(item => item.Name).ToArray(),
											member.GetParameters().Select(item => item.ParameterType).ToArray(),
											member.GetGenericArguments().Select(item => item.Name).ToArray());
										uNodeGUIUtility.GUIChanged(root);
										uNodeEditor.window.Refresh();
									}
								}
							}
							var properties = type.GetProperties();
							foreach(var member in properties) {
								if(!root.GetPropertyData(member.Name)) {
									var memberRect = EditorGUILayout.BeginVertical();
									EditorGUILayout.HelpBox(
$@"The graph does not implement interface property: 
'{ type.PrettyName() }' type: '{member.PropertyType.PrettyName()}'
[Click to implement it]", MessageType.Error);
									EditorGUILayout.EndVertical();
									if(Event.current.button == 0 && Event.current.type == EventType.MouseDown && memberRect.Contains(Event.current.mousePosition)) {
										NodeEditorUtility.AddNewProperty(root, member.Name, (val) => {
											val.type = MemberData.CreateFromType(member.PropertyType);
										});
										uNodeGUIUtility.GUIChanged(root);
										uNodeEditor.window.Refresh();
									}
								}
							}
						}
					}
				}
				#endregion
			}
		}

		public static bool DrawNodeEditor(NodeComponent nodeComponent) {
			if(nodeComponent is BaseEventNode) {
				if(Event.current.type == EventType.Ignore)
					return false;
				BaseEventNode eventNode = nodeComponent as BaseEventNode;
				try {
					uNodeUtility.ClearEditorError(eventNode);
					eventNode.CheckError();
				}
				catch(System.Exception ex) {
					Debug.LogException(ex, nodeComponent);
				}
				Editor editor = GetEditor(nodeComponent);
				if(editor != null) {
					try {
						editor.OnInspectorGUI();
					}
					catch(Exception ex) {
						Debug.LogException(ex, nodeComponent);
					}
					var errors = uNodeUtility.GetEditorError(eventNode);
					if(errors != null && errors.Count > 0) {
						EditorGUILayout.Space();
						foreach(var e in errors) {
							EditorGUILayout.HelpBox(uNodeEditorUtility.RemoveHTMLTag(e.message), MessageType.Error);
						}
					}
				} else {
					return false;
				}
				if(GUI.changed) {
					uNodeGUIUtility.GUIChanged(nodeComponent);
				}
				return true;
			} 
			else 
			if(nodeComponent as Node != null) {
				Node node = nodeComponent as Node;
#if UseProfiler
				Profiler.BeginSample("Check Node Error");
#endif
				try {
					uNodeUtility.ClearEditorError(node);
					node.CheckError();
				}
				catch(System.Exception ex) {
					uNodeUtility.RegisterEditorError(node, ex.ToString());
				}
#if UseProfiler
				Profiler.EndSample();
#endif
				Editor editor = GetEditor(node);
				//Editor editor = Editor.CreateEditor(node);
				if(editor != null) {
					if(Event.current.type == EventType.Ignore || Event.current.type == EventType.Used)
						return false;
					try {
						editor.OnInspectorGUI();
						Action drawInput = null;
						var fields = EditorReflectionUtility.GetFields(node.GetType());
						try {
							bool isPrefab = uNodeEditorUtility.IsPrefab(node);
							if(isPrefab)
								EditorGUI.BeginChangeCheck();
							for(int f = 0; f < fields.Length; f++) {
								FieldInfo field = fields[f];
								if(field.IsDefinedAttribute(typeof(FieldConnectionAttribute))) {
									var attributes = EditorReflectionUtility.GetAttributes(field);
									var FCA = ReflectionUtils.GetAttribute<FieldConnectionAttribute>(attributes);
									if(FCA is FlowOutAttribute)
										continue;
									bool isFlow = node.IsFlowNode();
									if(FCA.hideOnFlowNode && isFlow) {
										continue;
									}
									if(FCA.hideOnNotFlowNode && !isFlow) {
										continue;
									}
									if(field.FieldType == typeof(MemberData)) {
										GUIContent content = FCA.label;
										if(content == null || content == GUIContent.none) {
											content = new GUIContent(field.Name);
										}
										MemberData member = field.GetValueOptimized(node) as MemberData;
										if(member == null) {
											member = MemberData.none;
											field.SetValueOptimized(node, member);
										}
										drawInput += () => {
											uNodeGUIUtility.ShowField(field, node, node);
											//uNodeEditorUtility.EditValue(uNodeEditorUtility.GetRect(),
											//	content,
											//	member,
											//	typeof(MemberData),
											//	delegate (object o) {
											//		member = o as MemberData;
											//		field.SetValue(node, member);
											//	}, node, attributes);
										};
									} else {
										var obj = field.GetValueOptimized(node);
										if(obj is MultipurposeMember) {
											drawInput += () => VariableEditorUtility.DrawMultipurposeMember(
												obj as MultipurposeMember,
												node,
												new GUIContent(field.Name));
										}
									}
								} else if(field.IsDefinedAttribute(typeof(FieldDrawerAttribute))) {
									var attributes = EditorReflectionUtility.GetAttributes(field);
									var FD = ReflectionUtils.GetAttribute<FieldDrawerAttribute>(attributes);
									if(FD.label == null) {
										FD.label = new GUIContent(field.Name);
									}
									var obj = field.GetValueOptimized(node);
									ReflectionUtils.TryCorrectingAttribute(node, ref attributes);
									uNodeGUIUtility.EditValue(uNodeGUIUtility.GetRect(), FD.label, obj, field.FieldType, delegate (object o) {
										uNodeEditorUtility.RegisterUndo(node, "Edit Field: " + field.Name);
										field.SetValueOptimized(node, o);
									}, node, attributes);
								}
							}
							if(isPrefab && EditorGUI.EndChangeCheck()) {
								uNodeEditorUtility.MarkDirty(node);
							}
						}
						catch(UnityEngine.ExitGUIException ex) {
							ex.ToString();
						}
						catch(System.Exception ex) {
							Debug.LogException(ex);
						}
						if(node is MultipurposeNode) {
							MultipurposeNode multipurposeNode = node as MultipurposeNode;
							EditorGUILayout.BeginVertical("Box");
							EditorGUILayout.LabelField("Inputs", EditorStyles.centeredGreyMiniLabel);
							uNodeGUIUtility.DrawInitializer(multipurposeNode.target, multipurposeNode);
							//if(drawInput != null) {
							//	drawInput();
							//}
							uNodeGUIUtility.DrawInputDescription(multipurposeNode.target, multipurposeNode);
							EditorGUILayout.EndVertical();
							uNodeGUIUtility.DrawOutputDescription(multipurposeNode);
						} else {
							if(drawInput != null) {
								drawInput();
							}
							uNodeGUIUtility.DrawInputOutputDescription(node);
						}
						var errors = uNodeUtility.GetEditorError(node);
						if(errors != null && errors.Count > 0) {
							EditorGUILayout.Space();
							foreach(var e in errors) {
								EditorGUILayout.HelpBox(uNodeEditorUtility.RemoveHTMLTag(e.message), MessageType.Error);
							}
						}
					}
					catch(System.Exception ex) {
						Debug.LogException(ex, node);
					}
					if(GUI.changed) {
						uNodeGUIUtility.GUIChanged(node);
					}
				} else {
					return false;
				}
				return true;
			}
			else 
			if(nodeComponent != null) {
				UnityEngine.Object uObject = nodeComponent as UnityEngine.Object;
				Editor editor = GetEditor(uObject);
				if(editor != null) {
					try {
						editor.OnInspectorGUI();
					}
					catch(Exception ex) {
						Debug.LogException(ex, nodeComponent);
					}
					if(GUI.changed) {
						uNodeGUIUtility.GUIChanged(uObject);
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Used to show inspector context menu.
		/// </summary>
		/// <param name="menu"></param>
		/// <param name="component"></param>
		public static void ShowInspectorMenu(GenericMenu menu, NodeComponent component) {
			MonoScript ms = uNodeEditorUtility.GetMonoScript(component);
			if(ms != null) {
				menu.AddItem(new GUIContent("Find Script"), false, delegate () {
					EditorGUIUtility.PingObject(ms);
				});
				menu.AddItem(new GUIContent("Edit Script"), false, delegate () {
					AssetDatabase.OpenAsset(ms);
				});
			}
			menu.AddItem(new GUIContent("Find GameObject"), false, delegate () {
				EditorGUIUtility.PingObject(component);
			});
			menu.ShowAsContext();
			Event.current.Use();
		}
	}

	[CustomEditor(typeof(CustomInspector))]
	public class CustomInspectorEditor : Editor {
		public override void OnInspectorGUI() {
			CustomInspector Target = (CustomInspector)target;

			if(Target.editorData != null) {
				EditorGUI.BeginChangeCheck();
				CustomInspector.ShowInspector(Target.editorData);
				if(EditorGUI.EndChangeCheck() || UnityEngine.GUI.changed) {
					uNodeGUIUtility.GUIChanged(Target);
					uNodeEditor.ForceRepaint();
				}
			}
		}
	}
}