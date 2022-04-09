using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors {
	[CustomEditor(typeof(MultiArithmeticNode), true)]
	class MultiArithmeticNodeEditor : Editor {
		FilterAttribute filter = new FilterAttribute(typeof(object));

		public override void OnInspectorGUI() {
			MultiArithmeticNode node = target as MultiArithmeticNode;
			DrawDefaultInspector();
			VariableEditorUtility.DrawCustomList(node.targets, "Values", 
				drawElement: (position, index, value) => {
					position.height = EditorGUIUtility.singleLineHeight;
					var pos = EditorGUI.PrefixLabel(position, new GUIContent("Value " + index));
					pos.height = EditorGUIUtility.singleLineHeight;
					EditorReflectionUtility.ShowGUI(pos, node.targets[index], filter, node, (obj) => {
						node.targets[index] = obj;
					});
					position.y += EditorGUIUtility.singleLineHeight;
					pos = EditorGUI.PrefixLabel(position, new GUIContent("Type"));
					uNodeGUIUtility.DrawTypeDrawer(pos, node.targetTypes[index], new ObjectTypeAttribute(typeof(object)), GUIContent.none, (type) => {
						node.targetTypes[index] = type;
					});
				},
				elementHeight: (index) => EditorGUIUtility.singleLineHeight * 2,
				add: (position) => {
					ItemSelector.ShowType(node, null, member => {
						uNodeEditorUtility.RegisterUndo(node);
						var type = member.Get<Type>();
						node.targetTypes.Add(type);
						node.targets.Add(MemberData.CreateValueFromType(type));
						uNodeGUIUtility.GUIChanged(node);
					}).ChangePosition(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
				}, 
				remove: (index) => {
					node.targets.RemoveAt(index);
					node.targetTypes.RemoveAt(index);
				},
				reorder: (list, oldIndex, newIndex) => {
					uNodeUtility.ReorderList(node.targets, newIndex, oldIndex);
					uNodeEditorUtility.RegisterUndo(node);
					uNodeUtility.ReorderList(node.targets, oldIndex, newIndex);
					uNodeUtility.ReorderList(node.targetTypes, oldIndex, newIndex);
				});
			if(GUILayout.Button(new GUIContent("Change Operator"))) {
				var customItems = new List<ItemSelector.CustomItem>();
				{//Primitives
					customItems.AddRange(GetCustomItemForPrimitives(node, typeof(int)));
					customItems.AddRange(GetCustomItemForPrimitives(node, typeof(float)));
				}
				var ns = NodeGraph.GetGraphUsingNamespaces(node);
				var preference = uNodePreference.GetPreference();
				var assemblies = EditorReflectionUtility.GetAssemblies();
				var includedAssemblies = uNodePreference.GetIncludedAssemblies();
				foreach(var assembly in assemblies) {
					if(!includedAssemblies.Contains(assembly.GetName().Name)) {
						continue;
					}
					var operators = EditorReflectionUtility.GetOperators(assembly, (op) => {
						return ns == null || ns.Contains(op.DeclaringType.Namespace);
					});
					if(operators.Count > 0) {
						foreach(var op in operators) {
							switch(op.Name) {
								case "op_Addition": {
									var parameters = op.GetParameters();
									customItems.Add(GetCustomItem(node, parameters[0].ParameterType, parameters[1].ParameterType, op.DeclaringType, op.ReturnType, ArithmeticType.Add));
									break;
								}
								case "op_Subtraction": {
									var parameters = op.GetParameters();
									customItems.Add(GetCustomItem(node, parameters[0].ParameterType, parameters[1].ParameterType, op.DeclaringType, op.ReturnType, ArithmeticType.Subtract));
									break;
								}
								case "op_Division": {
									var parameters = op.GetParameters();
									customItems.Add(GetCustomItem(node, parameters[0].ParameterType, parameters[1].ParameterType, op.DeclaringType, op.ReturnType, ArithmeticType.Divide));
									break;
								}
								case "op_Multiply": {
									var parameters = op.GetParameters();
									customItems.Add(GetCustomItem(node, parameters[0].ParameterType, parameters[1].ParameterType, op.DeclaringType, op.ReturnType, ArithmeticType.Multiply));
									break;
								}
								case "op_Modulus": {
									var parameters = op.GetParameters();
									customItems.Add(GetCustomItem(node, parameters[0].ParameterType, parameters[1].ParameterType, op.DeclaringType, op.ReturnType, ArithmeticType.Modulo));
									break;
								}
							}
						}
					}
				}
				customItems.Sort((x, y) => {
					if(x.category == y.category) {
						return string.Compare(x.name, y.name, StringComparison.OrdinalIgnoreCase);
					}
					return string.Compare(x.category, y.category, StringComparison.OrdinalIgnoreCase);
				});
				if(customItems.Count > 0) {
					ItemSelector.ShowWindow(null, null, null, false, customItems).
						ChangePosition(
							GUIUtility.GUIToScreenRect(GUILayoutUtility.GetLastRect())
						).displayDefaultItem = false;
				}
			}
		}

		private static List<ItemSelector.CustomItem> GetCustomItemForPrimitives(MultiArithmeticNode source, Type type) {
			var items = new List<ItemSelector.CustomItem>();
			items.Add(GetCustomItem(source, type, type, type, type, ArithmeticType.Add));
			items.Add(GetCustomItem(source, type, type, type, type, ArithmeticType.Divide));
			items.Add(GetCustomItem(source, type, type, type, type, ArithmeticType.Modulo));
			items.Add(GetCustomItem(source, type, type, type, type, ArithmeticType.Multiply));
			items.Add(GetCustomItem(source, type, type, type, type, ArithmeticType.Subtract));
			return items;
		}

		private static ItemSelector.CustomItem GetCustomItem(MultiArithmeticNode source, Type param1, Type param2, Type declaredType, Type returnType, ArithmeticType operatorType) {
			return ItemSelector.CustomItem.Create(string.Format(operatorType.ToString() + " ({0}, {1})", param1.PrettyName(), param2.PrettyName()), () => {
				uNodeEditorUtility.RegisterUndo(source);
				source.operatorType = operatorType;
				while(source.targets.Count > 2) {
					source.targets.RemoveAt(source.targets.Count - 1);
				}
				source.targets[0].CopyFrom(new MemberData(ReflectionUtils.CreateInstance(param1)));
				source.targets[1].CopyFrom(new MemberData(ReflectionUtils.CreateInstance(param2)));
				source.targetTypes.Clear();
				source.targetTypes.Add(param1);
				source.targetTypes.Add(param2);
				uNodeGUIUtility.GUIChanged(source);
			}, declaredType.PrettyName() + " : Operator", icon: uNodeEditorUtility.GetTypeIcon(returnType));
		}
	}
}