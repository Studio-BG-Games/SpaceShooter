using System;
using System.Collections.Generic;
using UnityEngine;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors.Commands {
	public class CustomSetItem : CustomInputPortItem {
		public override IList<ItemSelector.CustomItem> GetItems(Node source, MemberData data, System.Type type) {
			var items = new List<ItemSelector.CustomItem>();
			items.Add(ItemSelector.CustomItem.Create("Set Value", () => {
				NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (NodeSetValue n) => {
					n.target = data;
					if(type != null) {
						if(type.IsSubclassOf(typeof(System.MulticastDelegate))) {
							NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas - new Vector2(100, 0), delegate (NodeLambda node) {
								n.value = new MemberData(node, MemberData.TargetType.ValueNode);
								n.setType = SetType.Add;
								node.delegateType = MemberData.CreateFromType(type);
							});
						} else {
							n.value = MemberData.CreateValueFromType(type);
						}
					}
					graph.Refresh();
				});
			}, "Flows", icon: uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
			return items;
		}
		
		public override bool IsValidPort(Type type, PortAccessibility accessibility) {
			return accessibility.CanSet();
		}
	}

	public class CustomInputEventHookItem : CustomInputPortItem {
		public override IList<ItemSelector.CustomItem> GetItems(Node source, MemberData data, System.Type type) {
			var items = new List<ItemSelector.CustomItem>();
			items.Add(ItemSelector.CustomItem.Create("Event Hook", () => {
				NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (EventHook n) => {
					n.target = data;
					graph.Refresh();
				});
			}, "Flows", icon: uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
			return items;
		}

		public override bool IsValidPort(Type type, PortAccessibility accessibility) {
			return type.IsCastableTo(typeof(Delegate)) || type.IsCastableTo(typeof(UnityEngine.Events.UnityEventBase));
		}
	}

	public class CustomInputArithmaticItem : CustomInputPortItem {
		public override IList<ItemSelector.CustomItem> GetItems(Node source, MemberData data, System.Type type) {
			var items = new List<ItemSelector.CustomItem>();
			if(type.IsPrimitive && type != typeof(bool) && type != typeof(char)) {
				string typeName = type.PrettyName();
				items.Add(ItemSelector.CustomItem.Create(string.Format("Add ({0}, {0})", typeName), () => {
					NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (MultiArithmeticNode n) => {
						n.targets[0] = data;
						n.targets[1] = new MemberData(ReflectionUtils.CreateInstance(type));
						n.operatorType = ArithmeticType.Add;
						graph.Refresh();
					});
				}, "Operator", icon: uNodeEditorUtility.GetTypeIcon(type)));
				items.Add(ItemSelector.CustomItem.Create(string.Format("Subtract ({0}, {0})", typeName), () => {
					NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (MultiArithmeticNode n) => {
						n.targets[0] = data;
						n.targets[1] = new MemberData(ReflectionUtils.CreateInstance(type));
						n.operatorType = ArithmeticType.Subtract;
						graph.Refresh();
					});
				}, "Operator", icon: uNodeEditorUtility.GetTypeIcon(type)));
				items.Add(ItemSelector.CustomItem.Create(string.Format("Divide ({0}, {0})", typeName), () => {
					NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (MultiArithmeticNode n) => {
						n.targets[0] = data;
						n.targets[1] = new MemberData(ReflectionUtils.CreateInstance(type));
						n.operatorType = ArithmeticType.Divide;
						graph.Refresh();
					});
				}, "Operator", icon: uNodeEditorUtility.GetTypeIcon(type)));
				items.Add(ItemSelector.CustomItem.Create(string.Format("Multiply ({0}, {0})", typeName), () => {
					NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (MultiArithmeticNode n) => {
						n.targets[0] = data;
						n.targets[1] = new MemberData(ReflectionUtils.CreateInstance(type));
						n.operatorType = ArithmeticType.Multiply;
						graph.Refresh();
					});
				}, "Operator", icon: uNodeEditorUtility.GetTypeIcon(type)));
				items.Add(ItemSelector.CustomItem.Create(string.Format("Modulo ({0}, {0})", typeName), () => {
					NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (MultiArithmeticNode n) => {
						n.targets[0] = data;
						n.targets[1] = new MemberData(ReflectionUtils.CreateInstance(type));
						n.operatorType = ArithmeticType.Modulo;
						graph.Refresh();
					});
				}, "Operator", icon: uNodeEditorUtility.GetTypeIcon(type)));
			}

			var preference = uNodePreference.GetPreference();
			var assemblies = EditorReflectionUtility.GetAssemblies();
			var includedAssemblies = uNodePreference.GetIncludedAssemblies();
			var ns = graph.editorData.GetNamespaces();
			foreach(var assembly in assemblies) {
				if(!includedAssemblies.Contains(assembly.GetName().Name)) {
					continue;
				}
				var operators = EditorReflectionUtility.GetOperators(assembly, (op) => {
					return ns.Contains(op.DeclaringType.Namespace);
				});
				if(operators.Count > 0) {
					foreach(var op in operators) {
						switch(op.Name) {
							case "op_Addition": {
								var parameters = op.GetParameters();
								if(parameters[0].ParameterType != type && parameters[1].ParameterType != type)
									break;
								items.Add(GetItem(type, parameters[0].ParameterType, parameters[1].ParameterType, op.ReturnType, data, ArithmeticType.Add));
								break;
							}
							case "op_Subtraction": {
								var parameters = op.GetParameters();
								if(parameters[0].ParameterType != type && parameters[1].ParameterType != type)
									break;
								items.Add(GetItem(type, parameters[0].ParameterType, parameters[1].ParameterType, op.ReturnType, data, ArithmeticType.Subtract));
								break;
							}
							case "op_Division": {
								var parameters = op.GetParameters();
								if(parameters[0].ParameterType != type && parameters[1].ParameterType != type)
									break;
								items.Add(GetItem(type, parameters[0].ParameterType, parameters[1].ParameterType, op.ReturnType, data, ArithmeticType.Divide));
								break;
							}
							case "op_Multiply": {
								var parameters = op.GetParameters();
								if(parameters[0].ParameterType != type && parameters[1].ParameterType != type)
									break;
								items.Add(GetItem(type, parameters[0].ParameterType, parameters[1].ParameterType, op.ReturnType, data, ArithmeticType.Multiply));
								break;
							}
							case "op_Modulus": {
								var parameters = op.GetParameters();
								if(parameters[0].ParameterType != type && parameters[1].ParameterType != type)
									break;
								items.Add(GetItem(type, parameters[0].ParameterType, parameters[1].ParameterType, op.ReturnType, data, ArithmeticType.Modulo));
								break;
							}
						}
					}
				}
			}
			items.Sort((x, y) => string.Compare(x.name, y.name, StringComparison.OrdinalIgnoreCase));
			return items;
		}

		private ItemSelector.CustomItem GetItem(Type type, Type param1, Type param2, Type returnType, MemberData data, ArithmeticType operatorType) {
			return ItemSelector.CustomItem.Create(string.Format(operatorType.ToString() + " ({0}, {1})", param1.PrettyName(), param2.PrettyName()), () => {
				NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (MultiArithmeticNode n) => {
					if(param1.IsCastableTo(type)) {
						n.targets[0] = data;
						n.targets[1] = new MemberData(ReflectionUtils.CreateInstance(param2));
					} else {
						n.targets[0] = new MemberData(ReflectionUtils.CreateInstance(param1));
						n.targets[1] = data;
					}
					n.operatorType = operatorType;
					graph.Refresh();
				});
			}, "Operator", icon: uNodeEditorUtility.GetTypeIcon(returnType));
		}

		public override bool IsValidPort(Type type, PortAccessibility accessibility) {
			return true;
		}
	}

    public class CustomInputComparisonItem : CustomInputPortItem {
		public override IList<ItemSelector.CustomItem> GetItems(Node source, MemberData data, System.Type type) {
			var items = new List<ItemSelector.CustomItem>();
			if(type.IsPrimitive && type != typeof(bool) && type != typeof(char)) {//Primitives
				items.AddRange(GetCustomItemForPrimitives(type, data));
			} else {
				items.Add(GetItem(type, type, type, typeof(bool), data, ComparisonType.Equal));
				items.Add(GetItem(type, type, type, typeof(bool), data, ComparisonType.NotEqual));
			}
			var preference = uNodePreference.GetPreference();
			var assemblies = EditorReflectionUtility.GetAssemblies();
			var includedAssemblies = uNodePreference.GetIncludedAssemblies();
			var ns = graph.editorData.GetNamespaces();
			foreach(var assembly in assemblies) {
				if(!includedAssemblies.Contains(assembly.GetName().Name)) {
					continue;
				}
				var operators = EditorReflectionUtility.GetOperators(assembly, (op) => {
					return ns.Contains(op.DeclaringType.Namespace);
				});
				if(operators.Count > 0) {
					foreach(var op in operators) {
						switch(op.Name) {
							//case "op_Equality": {
							//	var parameters = op.GetParameters();
							//	if(parameters[0].ParameterType != type && parameters[1].ParameterType != type)
							//		break;
							//	items.Add(GetItem(type, parameters[0].ParameterType, parameters[1].ParameterType, op.ReturnType, data, ComparisonType.Equal));
							//	break;
							//}
							//case "op_Inequality": {
							//	var parameters = op.GetParameters();
							//	if(parameters[0].ParameterType != type && parameters[1].ParameterType != type)
							//		break;
							//	items.Add(GetItem(type, parameters[0].ParameterType, parameters[1].ParameterType, op.ReturnType, data, ComparisonType.NotEqual));
							//	break;
							//}
							case "op_LessThan": {
								var parameters = op.GetParameters();
								if(parameters[0].ParameterType != type && parameters[1].ParameterType != type)
									break;
								items.Add(GetItem(type, parameters[0].ParameterType, parameters[1].ParameterType, op.ReturnType, data, ComparisonType.LessThan));
								break;
							}
							case "op_GreaterThan": {
								var parameters = op.GetParameters();
								if(parameters[0].ParameterType != type && parameters[1].ParameterType != type)
									break;
								items.Add(GetItem(type, parameters[0].ParameterType, parameters[1].ParameterType, op.ReturnType, data, ComparisonType.GreaterThan));
								break;
							}
							case "op_LessThanOrEqual": {
								var parameters = op.GetParameters();
								if(parameters[0].ParameterType != type && parameters[1].ParameterType != type)
									break;
								items.Add(GetItem(type, parameters[0].ParameterType, parameters[1].ParameterType, op.ReturnType, data, ComparisonType.LessThanOrEqual));
								break;
							}
							case "op_GreaterThanOrEqual": {
								var parameters = op.GetParameters();
								if(parameters[0].ParameterType != type && parameters[1].ParameterType != type)
									break;
								items.Add(GetItem(type, parameters[0].ParameterType, parameters[1].ParameterType, op.ReturnType, data, ComparisonType.GreaterThanOrEqual));
								break;
							}
						}
					}
				}
			}
			items.Sort((x, y) => string.Compare(x.name, y.name, StringComparison.OrdinalIgnoreCase));
			return items;
		}

		private List<ItemSelector.CustomItem> GetCustomItemForPrimitives(Type type, MemberData data) {
			var items = new List<ItemSelector.CustomItem>();
			items.Add(GetItem(type, type, type, typeof(bool), data, ComparisonType.Equal));
			items.Add(GetItem(type, type, type, typeof(bool), data, ComparisonType.GreaterThan));
			items.Add(GetItem(type, type, type, typeof(bool), data, ComparisonType.GreaterThanOrEqual));
			items.Add(GetItem(type, type, type, typeof(bool), data, ComparisonType.LessThan));
			items.Add(GetItem(type, type, type, typeof(bool), data, ComparisonType.LessThanOrEqual));
			items.Add(GetItem(type, type, type, typeof(bool), data, ComparisonType.NotEqual));
			return items;
		}

		private ItemSelector.CustomItem GetItem(Type type, Type param1, Type param2, Type returnType, MemberData data, ComparisonType operatorType) {
			return ItemSelector.CustomItem.Create(string.Format(operatorType.ToString() + " ({0}, {1})", param1.PrettyName(), param2.PrettyName()), () => {
				NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (ComparisonNode n) => {
					if(param1.IsCastableTo(type)) {
						n.targetA = data;
						n.targetB = new MemberData(ReflectionUtils.CreateInstance(param1));
					} else {
						n.targetA = new MemberData(ReflectionUtils.CreateInstance(param2));
						n.targetB = data;
					}
					n.operatorType = operatorType;
					graph.Refresh();
				});
			}, "Operator", icon: uNodeEditorUtility.GetTypeIcon(returnType));
		}

		public override bool IsValidPort(Type type) {
			return true;
		}
	}
}