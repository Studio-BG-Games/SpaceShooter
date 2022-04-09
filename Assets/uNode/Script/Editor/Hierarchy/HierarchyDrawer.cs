using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MaxyGames.uNode.Editors {
	public abstract class HierarchyDrawer {
		public GraphHierarchyTree manager;
		public virtual int order => 0;
		public abstract bool IsValid(Type type);

		#region Functions
		public virtual HierarchyNodeTree CreateNodeTree(NodeComponent nodeComponent) {
			var tree = new HierarchyNodeTree(nodeComponent, -1);
			tree.icon = uNodeEditorUtility.GetTypeIcon(nodeComponent.GetNodeIcon()) as Texture2D;
			return tree;
		}

		public static HierarchyFlowTree CreateFlowTree(NodeComponent owner, string fieldName, MemberData member, string displayName) {
			var tree = new HierarchyFlowTree(owner, member, uNodeEditorUtility.GetUIDFromString($"{owner.GetHashCode()}:F={fieldName}"), -1, displayName) {
				icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon)) as Texture2D
			};
			return tree;
		}

		protected void AddBlocks(EventData blocks, string uid, TreeViewItem parentTree, IList<TreeViewItem> rows) {
			if(blocks != null) {
				for(int i=0;i<blocks.blocks.Count;i++) {
					var block = blocks.blocks[i];
					if(block == null)
						continue;
					var tree = new HierarchyBlockTree(parentTree, block, uNodeEditorUtility.GetUIDFromString(parentTree.id + $"[BLOCK:{uid}-{i}]"));
					parentTree.AddChild(tree);
					rows.Add(tree);
				}
			}
		}

		public virtual void AddChildNodes(NodeComponent nodeComponent, TreeViewItem parentTree, IList<TreeViewItem> rows) {
			var fields = EditorReflectionUtility.GetFields(nodeComponent.GetType());
			foreach(var field in fields) {
				var fieldType = field.FieldType;
				if(field.IsDefinedAttribute(typeof(FieldConnectionAttribute))) {
					var attributes = EditorReflectionUtility.GetAttributes(field);
					var FCA = ReflectionUtils.GetAttribute<FieldConnectionAttribute>(attributes);
					bool isFlow = nodeComponent is Node && (nodeComponent as Node).IsFlowNode();
					if(FCA.hideOnFlowNode && isFlow) {
						continue;
					}
					if(FCA.hideOnNotFlowNode && !isFlow) {
						continue;
					}
					if(fieldType == typeof(MemberData)) {
						if(FCA is FlowOutAttribute flowOut) {
							MemberData member = field.GetValueOptimized(nodeComponent) as MemberData;
							if(member != null) {
								if(!flowOut.finishedFlow && field.Name != nameof(MultipurposeNode.onFinished)) {
									if(flowOut.displayFlowInHierarchy) {
										var flowItem = CreateFlowTree(
											nodeComponent,
											field.Name,
											member,
											FCA.label != null ? FCA.label.text : field.Name
										);
										parentTree.AddChild(flowItem);
										rows.Add(flowItem);
										manager.AddNodeTree(member, flowItem, rows);
									} else {
										manager.AddNodeTree(member, parentTree, rows);
									}
								} else {
									manager.AddNodeTree(member, parentTree, rows, false);
								}
							}
						}
					} else if(fieldType == typeof(List<MemberData>) && FCA is FlowOutAttribute) {
						List<MemberData> members = field.GetValueOptimized(nodeComponent) as List<MemberData>;
						if(members != null && members.Count > 0) {
							foreach(var member in members) {
								if(member != null && member.isAssigned && member.IsTargetingPortOrNode) {
									var n = member.GetTargetNode();
									if(n != null) {
										manager.AddNodes(n, parentTree, rows, isChildren: !(FCA as FlowOutAttribute).finishedFlow && field.Name != nameof(MultipurposeNode.onFinished));
									}
								}
							}
						}
					}
				} else if(fieldType == typeof(EventData)) {
					var blocks = field.GetValueOptimized(nodeComponent) as EventData;
					AddBlocks(blocks, field.Name, parentTree, rows);
				}
			}
		}
		#endregion

		#region Find Drawer
		private static List<HierarchyDrawer> _drawers;
		private static Dictionary<Type, HierarchyDrawer> drawerMaps = new Dictionary<Type, HierarchyDrawer>();
		public static List<HierarchyDrawer> FindHierarchyDrawer() {
			if(_drawers == null) {
				_drawers = EditorReflectionUtility.GetListOfType<HierarchyDrawer>();
				_drawers.Sort((x, y) => {
					return CompareUtility.Compare(x.order, y.order);
				});
			}
			return _drawers;
		}

		public static HierarchyDrawer FindDrawer(Type type) {
			if(type == null)
				return Default;
			if(drawerMaps.TryGetValue(type, out var drawer)) {
				return drawer;
			}
			var drawers = FindHierarchyDrawer();
			for(int i = 0; i < drawers.Count; i++) {
				if(drawers[i].IsValid(type)) {
					drawer = drawers[i];
					break;
				}
			}
			drawerMaps[type] = drawer;
			return drawer;
		}
		#endregion

		#region Default Drawer
		class DefaultDrawer : HierarchyDrawer {
			public override int order => int.MaxValue;
			public override bool IsValid(Type type) => true;
		}

		private static HierarchyDrawer _default;
		public static HierarchyDrawer Default {
			get {
				if(_default == null) {
					_default = new DefaultDrawer();
				}
				return _default;
			}
		}
		#endregion
	}

	#region Drawer
	class HierarchyEventDrawer : HierarchyDrawer {
		public override bool IsValid(Type type) {
			return type.IsSubclassOf(typeof(EventNode));
		}

		public override HierarchyNodeTree CreateNodeTree(NodeComponent nodeComponent) {
			var target = nodeComponent as EventNode;
			var result = base.CreateNodeTree(nodeComponent);
			result.displayName = target.eventType.ToString();
			return result;
		}

		public override void AddChildNodes(NodeComponent nodeComponent, TreeViewItem parentItem, IList<TreeViewItem> rows) {
			var node = nodeComponent as EventNode;
			var flows = node.GetFlows();
			if(flows != null) {
				for(int i=0;i<flows.Count;i++) {
					manager.AddNodeTree(flows[i], parentItem, rows);
				}
			}
		}
	}

	class HierarchyStateDrawer : HierarchyDrawer {
		public override bool IsValid(Type type) {
			return type == typeof(StateNode);
		}

		public override void AddChildNodes(NodeComponent nodeComponent, TreeViewItem parentItem, IList<TreeViewItem> rows) {
			var node = nodeComponent as StateNode;
			var flows = node.nestedFlowNodes;
			if(flows != null && flows.Count > 0) {
				var tree = new HierarchyFlowTree(nodeComponent, MemberData.none, uNodeEditorUtility.GetUIDFromString(nodeComponent.GetInstanceID() + "[EVENTS]"), -1, "Events");
				parentItem.AddChild(tree);
				rows.Add(tree);
				for(int i = 0; i < flows.Count; i++) {
					manager.AddNodes(flows[i], tree, rows);
				}
			}
			var transitions = node.TransitionEvents;
			if(transitions != null && transitions.Length > 0) {
				//var tree = new HierarchyFlowTree(nodeComponent, MemberData.none, uNodeEditorUtility.GetUIDFromString(nodeComponent.GetInstanceID() + "[TRANSITIONS]"), -1, "Transitions");
				//parentItem.AddChild(tree);
				//rows.Add(tree);
				for(int i = 0; i < transitions.Length; i++) {
					var transitionTree = new HierarchyTransitionTree(transitions[i], -1);
					manager.AddNodeTree(transitionTree, parentItem, rows);
					manager.AddNodeTree(transitions[i].target, transitionTree, rows);
				}
			}
		}
	}

	class HierarchyMacroDrawer : HierarchyDrawer {
		public override bool IsValid(Type type) {
			return type == typeof(Nodes.MacroPortNode);
		}

		public override HierarchyNodeTree CreateNodeTree(NodeComponent nodeComponent) {
			var target = nodeComponent as Nodes.MacroPortNode;
			var parentNode = target.parentComponent as NodeComponent;
			if(parentNode != null) {
				var tree = new HierarchyNodeTree(nodeComponent, -1);
				tree.icon = uNodeEditorUtility.GetTypeIcon(nodeComponent.GetNodeIcon()) as Texture2D;
				if(target.kind == PortKind.FlowInput) {
					tree.displayName = string.IsNullOrEmpty(tree.displayName) ? parentNode.GetNodeName() : $"{parentNode.GetNodeName()} ( {tree.displayName} )";
				}
				return tree;
			}
			return null;
		}

		public override void AddChildNodes(NodeComponent nodeComponent, TreeViewItem parentItem, IList<TreeViewItem> rows) {
			var macroPort = nodeComponent as Nodes.MacroPortNode;
			manager.AddNodeTree(macroPort.target, macroPort.kind == PortKind.FlowInput ? parentItem : parentItem.parent, rows, macroPort.kind == PortKind.FlowInput);
		}
	}
	#endregion
}