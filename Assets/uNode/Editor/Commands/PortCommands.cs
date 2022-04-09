using System;
using UnityEngine;
using UnityEditor;
using MaxyGames.uNode.Nodes;
using System.Collections.Generic;

namespace MaxyGames.uNode.Editors.Commands {
	#region Macro Commands
	class CreateMacroFlowInputPortCommand : PortMenuCommand {
		public override string name {
			get {
				return "New Macro Port";
			}
		}
		public override bool onlyContextMenu => true;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, delegate (MacroPortNode n) {
				n.target = data.getConnection();
				n.gameObject.name = data.portName;
				n.kind = uNode.PortKind.FlowInput;
				n.editorRect = source.editorRect;
				n.editorRect.y -= 100;
			});
			graph.Refresh();
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != uNode.PortKind.FlowInput)
				return false;
			return source.parentComponent is IMacro || source.owner is IMacroGraph && source.IsInRoot;
		}
	}

	class CreateMacroValueInputPortCommand : PortMenuCommand {
		public override string name {
			get {
				return "New Macro Port";
			}
		}
		public override bool onlyContextMenu => true;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, delegate (MacroPortNode n) {
				data.member.CopyFrom(MemberData.ValueOutput(n));
				n.type = new MemberData(data.portType);
				n.gameObject.name = data.portName;
				n.kind = uNode.PortKind.ValueInput;
				n.editorRect = source.editorRect;
				n.editorRect.x -= 100;
			});
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != uNode.PortKind.ValueInput)
				return false;
			return source.parentComponent is IMacro || source.owner is IMacroGraph && source.IsInRoot;
		}
	}

	class CreateMacroFlowOutputPortCommand : PortMenuCommand {
		public override string name {
			get {
				return "New Macro Port";
			}
		}
		public override bool onlyContextMenu => true;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, delegate (MacroPortNode n) {
				data.member.CopyFrom(MemberData.FlowInput(n));
				n.gameObject.name = data.portName;
				n.kind = uNode.PortKind.FlowOutput;
				n.editorRect = source.editorRect;
				n.editorRect.y += source.editorRect.height + 100;
			});
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != uNode.PortKind.FlowOutput)
				return false;
			return source.parentComponent is IMacro || source.owner is IMacroGraph && source.IsInRoot;
		}
	}

	class CreateMacroValueOutputPortCommand : PortMenuCommand {
		public override string name {
			get {
				return "New Macro Port";
			}
		}
		public override bool onlyContextMenu => true;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, delegate (MacroPortNode n) {
				n.type = new MemberData(data.portType);
				n.target = data.getConnection();
				n.gameObject.name = data.portName;
				n.kind = uNode.PortKind.ValueOutput;
				n.editorRect = source.editorRect;
				n.editorRect.x += source.editorRect.width + 100;
			});
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != uNode.PortKind.ValueOutput)
				return false;
			return source.parentComponent is IMacro || source.owner is IMacroGraph && source.IsInRoot;
		}
	}
	#endregion

	class GetInstanceOutputPortCommand : PortMenuCommand {
		public override string name {
			get {
				return "Get instance";
			}
		}
		public override bool onlyContextMenu => true;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			Type type = data.portType;
			FilterAttribute filter = new FilterAttribute {
				VoidType = true,
				MaxMethodParam = int.MaxValue,
				Public = true,
				Instance = true,
				Static = false,
				UnityReference = false,
				InvalidTargetType = MemberData.TargetType.Null | MemberData.TargetType.Values,
				// DisplayDefaultStaticType = false
			};
			List<ItemSelector.CustomItem> customItems;
			if(type is RuntimeType) {
				customItems = ItemSelector.MakeCustomItems((type as RuntimeType).GetRuntimeMembers(), filter);
				if(type.BaseType != null)
					customItems.AddRange(ItemSelector.MakeCustomItems(type.BaseType, filter, "Inherit Member"));
			} else {
				customItems = ItemSelector.MakeCustomItems(type, filter);
			}
			var usingNamespaces = source.owner.GetNamespaces().ToHashSet();
			if(usingNamespaces != null && usingNamespaces.Count > 0) {
				customItems.AddRange(ItemSelector.MakeExtensionItems(type, usingNamespaces, filter, "Extensions"));
			}
			{//Custom input port items.
				if(customItems == null) {
					customItems = new System.Collections.Generic.List<ItemSelector.CustomItem>();
				}
				var customInputItems = NodeEditorUtility.FindCustomInputPortItems();
				if(customInputItems != null && customInputItems.Count > 0) {
					var mData = data.getConnection();
					foreach(var c in customInputItems) {
						c.graph = graph;
						c.mousePositionOnCanvas = mousePositionOnCanvas;
						if(c.IsValidPort(type, PortAccessibility.OnlyGet)) {
							var items = c.GetItems(source as Node, mData, type);
							if(items != null) {
								customItems.AddRange(items);
							}
						}
					}
				}
			}
			if(customItems != null) {
				filter.Static = true;
				customItems.Sort((x, y) => {
					if(x.category != y.category) {
						return string.Compare(x.category, y.category, StringComparison.OrdinalIgnoreCase);
					}
					return string.Compare(x.name, y.name, StringComparison.OrdinalIgnoreCase);
				});
				ItemSelector w = ItemSelector.ShowWindow(source, MemberData.none, filter, (MemberData mData) => {
					bool flag = mData.targetType == MemberData.TargetType.Method && !type.IsCastableTo(mData.startType);
					if(!flag && !mData.isStatic) {
						mData.instance = data.getConnection();
					}
					NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (MultipurposeNode nod) => {
						if(nod.target == null) {
							nod.target = new MultipurposeMember();
						}
						nod.target.target = mData;
						MemberDataUtility.UpdateMultipurposeMember(nod.target);
						if(flag) {
							var pTypes = mData.ParameterTypes;
							if(pTypes != null) {
								int paramIndex = 0;
								MemberData param = null;
								for (int i = 0; i < pTypes.Length;i++){
									var types = pTypes[i];
									if(types != null) {
										for (int y = 0; y < types.Length;y++) {
											if(type.IsCastableTo(types[y])) {
												param = data.getConnection();
												break;
											}
											paramIndex++;
										}
										if(param != null) break;
									}
								}
								if(nod.target.parameters.Length > paramIndex && param != null) {
									nod.target.parameters[paramIndex] = param;
								}
							}
						}
						graph.Refresh();
					});
				}, customItems).ChangePosition(GUIUtility.GUIToScreenPoint(mousePosition));
				w.displayRecentItem = false;
				w.displayNoneOption = false;
			}
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != PortKind.ValueOutput)
				return false;
			return data.portName != UGraphView.SelfPortID || source is Node node && node.CanGetValue();
		}
	}

	class SetInstanceOutputPortCommand : PortMenuCommand {
		public override string name {
			get {
				return "Set instance";
			}
		}

		public override bool onlyContextMenu => true;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			Type type = data.portType;
			if(type.IsSubclassOf(typeof(System.MulticastDelegate))) {
				NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, delegate (EventHook n) {
					n.target = new MemberData(source, MemberData.TargetType.ValueNode);
				});
			} else {
				NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, delegate (NodeSetValue n) {
					n.target = data.getConnection();
					if (type.IsSubclassOf(typeof(System.MulticastDelegate))) {
						NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, delegate (NodeLambda node) {
							n.value = new MemberData(node, MemberData.TargetType.ValueNode);
							n.setType = SetType.Add;
							node.delegateType = MemberData.CreateFromType(type);
						});
					} else {
						n.value = MemberData.CreateValueFromType(type);
					}
				});
			}
			graph.Refresh();
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != PortKind.ValueOutput)
				return false;
			return data.portName == UGraphView.SelfPortID && source is Node node && node.CanSetValue();
		}
	}

	class SetInstanceFieldOutputPortCommand : PortMenuCommand {
		public override string name {
			get {
				return "Set instance field";
			}
		}
		public override bool onlyContextMenu => true;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			Type type = data.portType;
			FilterAttribute filter = new FilterAttribute();
			filter.HideTypes.Add(typeof(void));
			filter.MaxMethodParam = int.MaxValue;
			filter.SetMember = true;
			filter.Public = true;
			filter.Instance = true;
			filter.Static = false;
			filter.DisplayDefaultStaticType = false;
			var customItems = ItemSelector.MakeCustomItems(type, filter, type.PrettyName());
			if(customItems != null) {
				ItemSelector w = ItemSelector.ShowWindow(source, MemberData.none, filter, delegate (MemberData mData) {
					mData.instance = data.getConnection();
					NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (MultipurposeNode n) => {
						if(n.target == null) {
							n.target = new MultipurposeMember();
						}
						n.target.target = mData;
						MemberDataUtility.UpdateMultipurposeMember(n.target);
						NodeEditorUtility.AddNewNode(graph.editorData, null, null,
							new Vector2(mousePositionOnCanvas.x + n.editorRect.width + 150, mousePositionOnCanvas.y),
							(NodeSetValue SV) => {
								SV.target = new MemberData(n, MemberData.TargetType.ValueNode);
							});
					});
					graph.Refresh();
				}, customItems).ChangePosition(GUIUtility.GUIToScreenPoint(mousePosition));
				w.displayDefaultItem = false;
			}
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != PortKind.ValueOutput)
				return false;
			return data.portName != UGraphView.SelfPortID || source is Node node && node.CanGetValue();
		}
	}

	class PromoteToNodeInputPortCommand : PortMenuCommand {
		public override string name {
			get {
				return "Promote to node";
			}
		}
		public override bool onlyContextMenu => true;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			if(source.owner) {
				Undo.SetCurrentGroupName("Promote to node");
				Undo.RegisterFullObjectHierarchyUndo(source.owner, "Promote to node");
			}
			MemberData m = data.member;
			if(data.portType != null && (!m.isAssigned || m.type == null)) {
				m.CopyFrom(MemberData.CreateValueFromType(data.portType));
			}
			NodeEditorUtility.AddNewNode<MultipurposeNode>(graph.editorData, null, null, new Vector2(source.editorRect.x - 100, source.editorRect.y), (node) => {
				node.target.target = new MemberData(m);
				MemberDataUtility.UpdateMultipurposeMember(node.target);
				m.CopyFrom(new MemberData(node, MemberData.TargetType.ValueNode));
			});
			graph.Refresh();
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != PortKind.ValueInput)
				return false;
			var member = data.member;
			if(member != null && member.isTargeted && member.targetType != MemberData.TargetType.ValueNode) {
				if(data.portType != null && data.portType.IsByRef && member.targetType.IsTargetingValue()) {
					return false;
				}
				return true;
			}
			return false;
		}
	}

	class ToNodeInputPortCommand : PortMenuCommand {
		public override string name {
			get {
				return "Assign to value node";
			}
		}
		public override bool onlyContextMenu => true;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			if(source.owner) {
				Undo.SetCurrentGroupName("To value node");
				Undo.RegisterFullObjectHierarchyUndo(source.owner, "To value node");
			}
			var member = data.member;
			NodeEditorUtility.AddNewNode<MultipurposeNode>(graph.editorData, null, null, new Vector2(source.editorRect.x - 100, source.editorRect.y), (node) => {
				node.target.target = MemberData.CreateFromValue(ReflectionUtils.CreateInstance(filter.Types[0]), filter.Types[0]);
				MemberDataUtility.UpdateMultipurposeMember(node.target);
				member.CopyFrom(MemberData.ValueOutput(node));
			});
			graph.Refresh();
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != PortKind.ValueInput)
				return false;
			var member = data.member;
			if(member == null || member.isAssigned || filter != null && filter.SetMember) {
				return false;
			} else if(filter != null && filter.Types != null && filter.Types.Count > 0) {
				Type t = filter.GetActualType();
				if(t != null && !t.IsAbstract && !t.IsInterface && !t.IsByRef) {
					return true;
				}
			}
			return false;
		}
	}

	class ToConstructorInputPortCommand : PortMenuCommand {
		public override string name {
			get {
				return "Assign to contructor node";
			}
		}
		public override bool onlyContextMenu => true;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			if(source.owner) {
				Undo.SetCurrentGroupName("Assign to contructor node");
				Undo.RegisterFullObjectHierarchyUndo(source.owner, "Assign to contructor node");
			}
			var member = data.member;
			NodeEditorUtility.AddNewNode<MultipurposeNode>(graph.editorData, null, null, new Vector2(source.editorRect.x - 100, source.editorRect.y), (node) => {
				var type = filter.GetActualType();
				var ctors = type.GetConstructors();
				if(ctors != null && ctors.Length > 0) {
					System.Reflection.ConstructorInfo ctor = null;
					foreach(var c in ctors) {
						if(ctor == null) {
							ctor = c;
						} else if(ctor.GetParameters().Length < c.GetParameters().Length) {
							ctor = c;
						}
					}
					node.target.target = new MemberData(ctor);
				} else {
					node.target.target = new MemberData(type.Name + ".ctor", type, MemberData.TargetType.Constructor);
				}
				MemberDataUtility.UpdateMultipurposeMember(node.target);
				member.CopyFrom(new MemberData(node, MemberData.TargetType.ValueNode));
			});
			graph.Refresh();
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != PortKind.ValueInput)
				return false;
			var member = data.member;
			if(member == null) {
				return false;
			} else if(filter != null && filter.Types != null && filter.Types.Count > 0) {
				Type t = filter.GetActualType();
				if(ReflectionUtils.CanCreateInstance(t) && !t.IsPrimitive && t != typeof(string)) {
					return true;
				}
			}
			return false;
		}
	}

	class PromoteToVariableInputPortCommand : PortMenuCommand {
		Type type;

		public override string name {
			get {
				return "Promote to variable";
			}
		}
		public override bool onlyContextMenu => true;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			if(source.owner) {
				Undo.SetCurrentGroupName("Promote to variable");
				Undo.RegisterFullObjectHierarchyUndo(source.owner, "Promote to variable");
			}
			MemberData m = data.member;
			var root = graph.editorData.graph;
			VariableData var = uNodeEditorUtility.AddVariable(type, root.Variables, root);
			if(data.portType != null) {
				if(m.isAssigned && !data.portType.IsByRef) {
					var.Set(m.Get());
				} else if(data.portType.IsByRef) {
					var.modifier.SetPrivate();
				}
			}
			m.CopyFrom(new MemberData(var, root));
			uNodeGUIUtility.GUIChanged(source.owner);
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != PortKind.ValueInput)
				return false;
			var member = data.member;
			type = filter != null ? filter.GetActualType() : typeof(object);
			if(member != null && (!member.isAssigned || member.targetType == MemberData.TargetType.Values) && type != null) {
				if(type.IsByRef) {
					type = type.GetElementType();
				}
				return true;
			}
			return false;
		}
	}

	class PromoteToVariableNodeInputPortCommand : PortMenuCommand {
		Type type;

		public override string name {
			get {
				return "Promote to variable node";
			}
		}
		public override bool onlyContextMenu => true;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			if(source.owner) {
				Undo.SetCurrentGroupName("Promote to variable node");
				Undo.RegisterFullObjectHierarchyUndo(source.owner, "Promote to variable node");
			}
			MemberData m = data.member;
			var root = graph.editorData.graph;
			VariableData var = uNodeEditorUtility.AddVariable(type, root.Variables, root);
			if(data.portType != null) {
				if(m.isAssigned && !data.portType.IsByRef) {
					var.Set(m.Get());
				} else if(data.portType.IsByRef) {
					var.modifier.SetPrivate();
				}
			}
			m.CopyFrom(MemberData.CreateFromValue(var, root));
			NodeEditorUtility.AddNewNode<MultipurposeNode>(graph.editorData, null, null, new Vector2(source.editorRect.x - 100, source.editorRect.y), (node) => {
				node.target.target = new MemberData(m);
				MemberDataUtility.UpdateMultipurposeMember(node.target);
				m.CopyFrom(new MemberData(node, MemberData.TargetType.ValueNode));
			});
			graph.Refresh();
			uNodeGUIUtility.GUIChanged(source.owner);
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != PortKind.ValueInput)
				return false;
			var member = data.member;
			type = filter != null ? filter.GetActualType() : typeof(object);
			if(member != null && (!member.isAssigned || member.targetType == MemberData.TargetType.Values) && type != null) {
				if(type.IsByRef) {
					type = type.GetElementType();
				}
				return true;
			}
			return false;
		}
	}

	class AssignToDelegateInputPortCommand : PortMenuCommand {
		Type type;

		public override string name {
			get {
				return "Assign to anonymous function";
			}
		}

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			if(source.owner) {
				Undo.SetCurrentGroupName("Assign to anonymous function");
				Undo.RegisterFullObjectHierarchyUndo(source.owner, "Assign to anonymous function");
			}
			MemberData m = data.member;
			var pos = mousePositionOnCanvas != Vector2.zero ? mousePositionOnCanvas : new Vector2(source.editorRect.x - 100, source.editorRect.y);
			NodeEditorUtility.AddNewNode<NodeAnonymousFunction>(graph.editorData, null, null,
				pos,
				(node) => {
					var method = type.GetMethod("Invoke");
					if(method != null) {
						node.returnType = new MemberData(method.ReturnType);
						foreach(var p in method.GetParameters()) {
							node.parameterTypes.Add(new MemberData(p.ParameterType));
						}
					}
					m.CopyFrom(new MemberData(node, MemberData.TargetType.ValueNode));
					graph.Refresh();
				});
			graph.Refresh();
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != uNode.PortKind.ValueInput)
				return false;
			var member = data.member;
			type = filter != null ? filter.GetActualType() : typeof(object);
			if(member != null && type != null && type.IsCastableTo(typeof(Delegate))) {
				if(filter != null && filter.SetMember) {
					return false;
				}
				return true;
			}
			return false;
		}
	}

	class AssignToLambdaInputPortCommand : PortMenuCommand {
		Type type;

		public override string name {
			get {
				return "Assign to lambda";
			}
		}

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			if(source.owner) {
				Undo.SetCurrentGroupName("Assign to lambda");
				Undo.RegisterFullObjectHierarchyUndo(source.owner, "Assign to lambda");
			}
			MemberData m = data.member;
			var pos = mousePositionOnCanvas != Vector2.zero ? mousePositionOnCanvas : new Vector2(source.editorRect.x - 100, source.editorRect.y);
			NodeEditorUtility.AddNewNode<NodeLambda>(graph.editorData, null, null,
				pos,
				(node) => {
					node.delegateType = MemberData.CreateFromType(type);
					m.CopyFrom(new MemberData(node, MemberData.TargetType.ValueNode));
					graph.Refresh();
				});
			graph.Refresh();
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != PortKind.ValueInput)
				return false;
			var member = data.member;
			type = filter != null ? filter.GetActualType() : typeof(object);
			if(member != null && type != null && type.IsCastableTo(typeof(Delegate))) {
				if(filter != null && filter.SetMember) {
					return false;
				}
				return true;
			}
			return false;
		}
	}

	class PromoteToLocalVariableInputPortCommand : PortMenuCommand {
		Type type;

		public override string name {
			get {
				return "Promote to local variable";
			}
		}
		public override bool onlyContextMenu => true;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			if(source.owner) {
				Undo.SetCurrentGroupName("Promote to local variable");
				Undo.RegisterFullObjectHierarchyUndo(source.owner, "Promote to local variable");
			}
			MemberData m = data.member;
			var root = graph.editorData.selectedRoot;
			VariableData var = uNodeEditorUtility.AddVariable(type, root.localVariable, root);
			if(m.isAssigned && data.portType != null && !data.portType.IsByRef) {
				var.Set(m.Get());
			}
			m.CopyFrom(MemberData.CreateFromValue(var, root));
			uNodeGUIUtility.GUIChanged(source.owner);
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != PortKind.ValueInput)
				return false;
			if(graph.editorData.selectedRoot) {
				var member = data.member;
				type = filter != null ? filter.GetActualType() : typeof(object);
				if(member != null && (!member.isAssigned || member.targetType == MemberData.TargetType.Values) && type != null) {
					if(type.IsByRef) {
						type = type.GetElementType();
					}
					return true;
				}
			}
			return false;
		}
	}

	class CacheInstanceOutputPortCommand : PortMenuCommand {
		public override string name {
			get {
				return "Cache output";
			}
		}

		public override int order => 100;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (CacheNode n) => {
				n.target = data.getConnection();
			});
			graph.Refresh();
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind != PortKind.ValueOutput)
				return false;
			return data.portName != UGraphView.SelfPortID || source is Node node && node.CanGetValue();
		}
	}

	class ExposeOutputPortCommand : PortMenuCommand {
		public override string name {
			get {
				return "Expose output";
			}
		}
		public override int order => 100;

		public override void OnClick(NodeComponent source, PortCommandData data, Vector2 mousePosition) {
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (ExposedNode n) => {
				n.value = data.getConnection();
				n.Refresh(true);
			});
			graph.Refresh();
		}

		public override bool IsValidPort(NodeComponent source, PortCommandData data) {
			if(data.portKind == PortKind.ValueOutput) {
				return data.portName != UGraphView.SelfPortID || source is Node node && node.CanGetValue();
			}
			return false;
		}
	}
}