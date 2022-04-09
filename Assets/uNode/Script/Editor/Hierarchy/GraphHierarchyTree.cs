using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MaxyGames.uNode.Editors {
	#region TreeViews
	internal class HiearchyNamespaceTree : TreeViewItem {
		public HiearchyNamespaceTree() {

		}

		public HiearchyNamespaceTree(string @namespace, int depth) : base(uNodeEditorUtility.GetUIDFromString("[NAMESPACES]" + @namespace), depth, @namespace) {
			icon = uNodeEditorUtility.GetTypeIcon(typeof(uNode.TypeIcons.NamespaceIcon)) as Texture2D;
		}
	}

	public class HierarchyGraphTree : TreeViewItem {
		public uNodeRoot graph;

		public HierarchyGraphTree() {

		}

		public HierarchyGraphTree(uNodeRoot graph, int depth) : base(graph.GetHashCode(), depth, graph.DisplayName) {
			this.graph = graph;
			icon = uNodeEditorUtility.GetTypeIcon(graph) as Texture2D;
		}
	}

	internal class HierarchySummaryTree : TreeViewItem {
		public TreeViewItem owner;

		public HierarchySummaryTree() {

		}

		public HierarchySummaryTree(string displayName, TreeViewItem owner, int depth = -1) : base(uNodeEditorUtility.GetUIDFromString(owner.id.ToString() + "[SUMMARY]"), depth, displayName) {
			this.owner = owner;
		}
	}

	internal class HierarchyVariableSystemTree : TreeViewItem {
		public IVariableSystem variableSystem;

		public HierarchyVariableSystemTree() {

		}

		public HierarchyVariableSystemTree(IVariableSystem variableSystem, int id, int depth) : base(id, depth, "Variables") {
			this.variableSystem = variableSystem;
			icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FieldIcon)) as Texture2D;
		}
	}

	internal class HierarchyVariableTree : TreeViewItem {
		public VariableData variable;

		public HierarchyVariableTree() {

		}

		public HierarchyVariableTree(VariableData variable, int id, int depth) : base(id, depth, $"{variable.Name} : {variable.type.DisplayName(false, false)}") {
			this.variable = variable;
			icon = uNodeEditorUtility.GetTypeIcon(variable.type) as Texture2D;
		}
	}

	internal class HierarchyPropertySystemTree : TreeViewItem {
		public IPropertySystem propertySystem;

		public HierarchyPropertySystemTree() {

		}

		public HierarchyPropertySystemTree(IPropertySystem propertySystem, int id, int depth) : base(id, depth, "Properties") {
			this.propertySystem = propertySystem;
			icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.PropertyIcon)) as Texture2D;
		}
	}

	internal class HierarchyPropertyTree : TreeViewItem {
		public uNodeProperty property;

		public HierarchyPropertyTree() {

		}

		public HierarchyPropertyTree(uNodeProperty property, int depth) : base(property.GetHashCode(), depth, $"{property.Name} : {property.type.DisplayName(false, false)}") {
			this.property = property;
			icon = uNodeEditorUtility.GetTypeIcon(property.type) as Texture2D;
		}
	}

	internal class HierarchyFunctionSystemTree : TreeViewItem {
		public IFunctionSystem functionSystem;

		public HierarchyFunctionSystemTree() {

		}

		public HierarchyFunctionSystemTree(IFunctionSystem functionSystem, int id, int depth) : base(id, depth, "Functions") {
			this.functionSystem = functionSystem;
			icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.MethodIcon)) as Texture2D;
		}
	}

	internal class HierarchyFunctionTree : TreeViewItem {
		public uNodeFunction function;

		public HierarchyFunctionTree() {

		}

		public HierarchyFunctionTree(uNodeFunction function, int depth) : base(function.GetHashCode(), depth, $"{function.Name}({ string.Join(", ", function.Parameters.Select(p => p.type.DisplayName(false, false))) }) : {function.returnType.DisplayName(false, false)}") {
			this.function = function;
			icon = uNodeEditorUtility.GetTypeIcon(function.returnType) as Texture2D;
		}
	}

	public class HierarchyNodeTree : TreeViewItem {
		public NodeComponent node;

		public HierarchyNodeTree() {

		}

		public HierarchyNodeTree(NodeComponent node, int depth) : base(node.GetHashCode(), depth, node.GetRichName()) {
			this.node = node;
		}
	}

	public class HierarchyTransitionTree : TreeViewItem {
		public TransitionEvent transition;

		public HierarchyTransitionTree() {

		}

		public HierarchyTransitionTree(TransitionEvent transition, int depth) : base(transition.GetHashCode(), depth, transition.Name) {
			this.transition = transition;
		}
	}

	public class HierarchyBlockTree : TreeViewItem {
		public TreeViewItem owner;
		public EventActionData block;

		public HierarchyBlockTree() {

		}

		public HierarchyBlockTree(TreeViewItem owner, EventActionData block, int id, int depth = -1) : base(id, depth, block.GetRichName()) {
			this.owner = owner;
			this.block = block;
		}
	}

	public class HierarchyRefNodeTree : TreeViewItem {
		public HierarchyNodeTree tree;

		public HierarchyRefNodeTree() {

		}

		public HierarchyRefNodeTree(HierarchyNodeTree tree, int depth) : base(tree.id, depth, tree.displayName) {
			this.tree = tree;
			icon = tree.icon;
		}
	}

	public class HierarchyPortTree : TreeViewItem {
		public FlowInput port;
		public NodeComponent node;

		public HierarchyPortTree() {

		}

		public HierarchyPortTree(NodeComponent node, FlowInput port, int id, int depth, string displayName) : base(id, depth, displayName) {
			this.node = node;
			this.port = port;
		}
	}

	public class HierarchyFlowTree : TreeViewItem {
		public NodeComponent owner;
		public MemberData flow;

		public HierarchyFlowTree() {

		}

		public HierarchyFlowTree(NodeComponent owner, MemberData flow, int id, int depth, string displayName) : base(id, depth, displayName) {
			this.flow = flow;
			this.owner = owner;
		}
	}
	#endregion

	public class GraphHierarchyTree : TreeView {
		public uNodeEditor graphEditor;
		public GraphEditorData editorData => graphEditor?.editorData;

		private NodeComponent refSelectedTree;
		private uNodeRoot graph;
		private HashSet<int> expandStates;
		private Dictionary<NodeComponent, HierarchyNodeTree> nodeTreesMap = new Dictionary<NodeComponent, HierarchyNodeTree>();
		private Dictionary<FlowInput, HierarchyPortTree> flowPortsMap = new Dictionary<FlowInput, HierarchyPortTree>();

		public GraphHierarchyTree(TreeViewState state) : base(state) {
			graphEditor = uNodeEditor.window;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			Reload(true);
		}

		public void Reload(bool initReload) {
			if(initReload) {
				expandStates = new HashSet<int>(GetExpanded());
				Reload();
				expandStates = null;
			} else {
				Reload();
			}
		}

		protected override TreeViewItem BuildRoot() {
			return new TreeViewItem { id = 0, depth = -1 };
		}

		protected override IList<TreeViewItem> BuildRows(TreeViewItem root) {
			graphEditor = uNodeEditor.window;
			var rows = GetRows() ?? new List<TreeViewItem>();
			rows.Clear();
			nodeTreesMap.Clear();
			flowPortsMap.Clear();
			if(graphEditor == null) {
				return rows;
			}
			if(editorData.graph != null) {
				var graph = editorData.graph;
				this.graph = graph;
				CreateGraphTree(graph, root, rows);
				//int prevCount = rows.Count;
				//var item = new HierarchyGraphTree(graph, -1);
				//if(expandStates != null && !expandStates.Contains(item.id)) {
				//	SetExpanded(item.id, true);
				//}
				//if(IsExpanded(item.id)) {
				//	AddChildren(item, rows);
				//} else {
				//	item.children = CreateChildListForCollapsedParent();
				//}
				//root.AddChild(item);
				//rows.Insert(rows.Count - (rows.Count - prevCount), item);
			}
			SetupDepthsFromParentsAndChildren(root);
			return rows;
		}

		private void AddSummary(string summary, TreeViewItem owner, TreeViewItem parent, IList<TreeViewItem> rows) {
			if(string.IsNullOrEmpty(summary))
				return;
			var strs = summary.Split('\n');
			for(int i=0; i< strs.Length;i++) {
				if(string.IsNullOrEmpty(strs[i]))
					continue;
				var tree = new HierarchySummaryTree(strs[i], owner);
				parent.AddChild(tree);
				rows.Add(tree);
			}
		}

		private void CreateGraphTree(uNodeRoot graph, TreeViewItem parent, IList<TreeViewItem> rows) {
			{//Variables
				int prevCount = rows.Count;
				var childItem = new HierarchyVariableSystemTree(graph, uNodeEditorUtility.GetUIDFromString(graph.GetHashCode() + ":V"), -1);
				if(expandStates != null && !expandStates.Contains(childItem.id)) {
					SetExpanded(childItem.id, true);
				}
				var variables = graph.Variables;
				if(variables?.Count > 0) {
					if(IsExpanded(childItem.id)) {
						AddChildren(childItem, rows);
					} else {
						childItem.children = CreateChildListForCollapsedParent();
					}
				}
				parent.AddChild(childItem);
				rows.Insert(rows.Count - (rows.Count - prevCount), childItem);
			}
			{//Properties
				int prevCount = rows.Count;
				var childItem = new HierarchyPropertySystemTree(graph, uNodeEditorUtility.GetUIDFromString(graph.GetHashCode() + ":P"), -1);
				if(expandStates != null && !expandStates.Contains(childItem.id)) {
					SetExpanded(childItem.id, true);
				}
				var properties = graph.Properties;
				if(properties?.Count > 0) {
					if(IsExpanded(childItem.id)) {
						AddChildren(childItem, rows);
					} else {
						childItem.children = CreateChildListForCollapsedParent();
					}
				}
				parent.AddChild(childItem);
				rows.Insert(rows.Count - (rows.Count - prevCount), childItem);
			}
			{//Functions
				int prevCount = rows.Count;
				var childItem = new HierarchyFunctionSystemTree(graph, uNodeEditorUtility.GetUIDFromString(graph.GetHashCode() + ":F"), -1);
				if(expandStates != null && !expandStates.Contains(childItem.id)) {
					SetExpanded(childItem.id, true);
				}
				var functions = graph.Functions;
				if(functions?.Count > 0) {
					if(IsExpanded(childItem.id)) {
						if(graph is IStateGraph) {
							var tree = new TreeViewItem(uNodeEditorUtility.GetUIDFromString(graph.GetHashCode() + "[StateGraph]"), -1, "[STATE GRAPH]") {
								icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.StateIcon)) as Texture2D,
							};
							childItem.AddChild(tree);
							rows.Add(tree);
							if(expandStates != null && !expandStates.Contains(tree.id)) {
								SetExpanded(tree.id, true);
							}
							if(IsExpanded(tree.id)) {
								var root = graph as IStateGraph;
								var flows = root.eventNodes;
								if(flows != null) {
									for(int i = 0; i < flows.Count; i++) {
										AddNodes(flows[i], tree, rows);
									}
								}
							} else {
								tree.children = CreateChildListForCollapsedParent();
							}
						} else if(graph is uNodeMacro) {
							var tree = new TreeViewItem(uNodeEditorUtility.GetUIDFromString(graph.GetHashCode() + "[Macro]"), -1, "[MACRO]") {
								icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.StateIcon)) as Texture2D,
							};
							childItem.AddChild(tree);
							rows.Add(tree);
							if(expandStates != null && !expandStates.Contains(tree.id)) {
								SetExpanded(tree.id, true);
							}
							if(IsExpanded(tree.id)) {
								var root = graph as uNodeMacro;
								var flows = root.inputFlows;
								if(flows != null) {
									for(int i = 0; i < flows.Count; i++) {
										AddNodes(flows[i], tree, rows);
									}
								}
							} else {
								tree.children = CreateChildListForCollapsedParent();
							}
						}
						AddChildren(childItem, rows);
					} else {
						childItem.children = CreateChildListForCollapsedParent();
					}
				}
				parent.AddChild(childItem);
				rows.Insert(rows.Count - (rows.Count - prevCount), childItem);
			}
		}

		private void AddChildren(TreeViewItem item, IList<TreeViewItem> rows) {
			if(item == null)
				throw new ArgumentNullException(nameof(item));
			if(item.children == null)
				item.children = new List<TreeViewItem>();
			if(item is HierarchyVariableSystemTree variableSystemTree) {
				var variables = variableSystemTree.variableSystem.Variables;
				if(variables != null && variables.Count > 0) {
					for(int i = 0; i < variables.Count; ++i) {
						var variable = variables[i];
						var childItem = new HierarchyVariableTree(variable, uNodeEditorUtility.GetUIDFromString($"V:{i}"), -1);
						AddSummary(variable.summary, childItem, item, rows);
						item.AddChild(childItem);
						rows.Add(childItem);
					}
				}
			} else if(item is HierarchyPropertySystemTree propertySystemTree) {
				var properties = propertySystemTree.propertySystem.Properties;
				if(properties != null && properties.Count > 0) {
					for(int i = 0; i < properties.Count; ++i) {
						var property = properties[i];
						var childItem = new HierarchyPropertyTree(property, -1);
						AddSummary(property.summary, childItem, item, rows);
						item.AddChild(childItem);
						rows.Add(childItem);
					}
				}
			} else if(item is HierarchyFunctionSystemTree functionSystemTree) {
				var functions = functionSystemTree.functionSystem.Functions;
				if(functions != null && functions.Count > 0) {
					for(int i = 0; i < functions.Count; ++i) {
						var function = functions[i];
						var childItem = new HierarchyFunctionTree(function, -1);
						AddSummary(function.summary, childItem, item, rows);
						item.AddChild(childItem);
						rows.Add(childItem);
						if(expandStates != null && !expandStates.Contains(childItem.id)) {
							SetExpanded(childItem.id, true);
						}
						if(IsExpanded(childItem.id)) {
							AddChildren(childItem, rows);
						} else {
							childItem.children = CreateChildListForCollapsedParent();
						}
					}
				}
			} else if(item is HierarchyFunctionTree functionTree) {
				var function = functionTree.function;
				if(function != null && function.startNode != null) {
					AddNodes(function.startNode, functionTree, rows);
				}
			} else if(item is HierarchyGraphTree graphTree) {
				var graph = graphTree.graph;
				CreateGraphTree(graph, item, rows);
			} else {
				throw new NotImplementedException(item.GetType().ToString());
			}
		}

		protected override void SelectionChanged(IList<int> selectedIds) {
			bool flag = true;
			if(selectedIds?.Count > 0) {
				int firstSelection = selectedIds[0];
				var rows = GetRows();
				for(int i = 0; i < rows.Count; i++) {
					if(rows[i]?.id == firstSelection) {
						var tree = rows[i];
						if(tree is HierarchyFlowTree flowTree) {
							refSelectedTree = flowTree.owner;
							flag = false;
						}
						break;
					}
				}
			}
			if(flag) {
				refSelectedTree = null;
			}
			base.SelectionChanged(selectedIds);
		}

		protected override bool CanChangeExpandedState(TreeViewItem item) {
			if(!string.IsNullOrEmpty(searchString) || item is HierarchyFlowTree || item is HierarchyNodeTree || item is HierarchyPortTree || item is HierarchyTransitionTree) {
				return false;
			}
			return item.hasChildren;
		}

		protected override bool CanMultiSelect(TreeViewItem item) {
			return false;
		}

		protected override void RowGUI(RowGUIArgs args) {
			Event evt = Event.current;
			if(args.rowRect.Contains(evt.mousePosition)) {
				if(evt.type == EventType.ContextClick) {
					ContextClick(args.item, evt);
				} else if(evt.type == EventType.MouseDown) {
					if(evt.clickCount == 2 && evt.button == 0) {//Double click
						HighlightTree(args.item);
					} else if(evt.modifiers == EventModifiers.Shift && evt.button == 0) {//Left click + Shift
						Inspect(args.item, GUIUtility.GUIToScreenPoint(evt.mousePosition));
					}
				}
			}
			if(evt.type == EventType.Repaint) {
				#region Debug
				if(args.item is HierarchyNodeTree || args.item is HierarchyRefNodeTree) {
					NodeComponent node = null;
					if(args.item is HierarchyNodeTree) {
						node = (args.item as HierarchyNodeTree).node;
					} else if(args.item is HierarchyRefNodeTree) {
						node = (args.item as HierarchyRefNodeTree).tree.node;
					}
					//bool hasBreakpoint = uNodeUtility.HasBreakpoint(uNodeUtility.GetObjectID(node));
					if(node != null && Application.isPlaying && GraphDebug.useDebug) {
						GraphDebug.DebugData.NodeDebug nodeDebug = null;
						var debugData = GetDebugInfo();
						if(debugData != null && debugData.nodeDebug.ContainsKey(uNodeUtility.GetObjectID(node))) {
							nodeDebug = debugData.nodeDebug[uNodeUtility.GetObjectID(node)];
						}
						if(nodeDebug != null) {
							var oldColor = GUI.color;
							switch(nodeDebug.nodeState) {
								case StateType.Success:
									GUI.color = UIElementUtility.Theme.nodeSuccessColor;
									break;
								case StateType.Failure:
									GUI.color = UIElementUtility.Theme.nodeFailureColor;
									break;
								case StateType.Running:
									GUI.color = UIElementUtility.Theme.nodeRunningColor;
									break;
							}
							GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.2f);
							Rect debugRect = args.rowRect;
							debugRect.width = GetContentIndent(args.item);
							GUI.DrawTexture(new Rect(debugRect.x + debugRect.width - 10, debugRect.y, 10, debugRect.height), Texture2D.whiteTexture);
							GUI.color = Color.Lerp(GUI.color, Color.clear, (Time.unscaledTime - nodeDebug.calledTime) * 2);
							GUI.DrawTexture(new Rect(debugRect.x, debugRect.y, debugRect.width - 10, debugRect.height), Texture2D.whiteTexture);
							GUI.color = oldColor;
						}
					}
				} else if(args.item is HierarchyFlowTree) {
					var tree = args.item as HierarchyFlowTree;
					var flow = tree.flow;
					var node = (tree.parent as HierarchyNodeTree)?.node ?? (tree.parent as HierarchyPortTree)?.node;
					if(node != null && flow != null && Application.isPlaying && GraphDebug.useDebug) {
						var debugData = GetDebugInfo();
						if(debugData != null) {
							var oldColor = GUI.color;
							float times = -1;
							if(flow.targetType == MemberData.TargetType.FlowInput) {
								int ID = uNodeUtility.GetObjectID(flow.startTarget as MonoBehaviour);
								if(debugData != null && debugData.flowInputDebug.ContainsKey(ID)) {
									if(debugData.flowInputDebug[ID].ContainsKey(flow.startName)) {
										times = Time.unscaledTime - debugData.flowInputDebug[ID][flow.startName];
									}
								}
							} else {
								int ID = uNodeUtility.GetObjectID(flow.startTarget as MonoBehaviour);
								if(debugData != null && debugData.flowTransitionDebug.ContainsKey(ID)) {
									if(debugData.flowTransitionDebug[ID].ContainsKey(int.Parse(flow.startName))) {
										times = Time.unscaledTime - debugData.flowTransitionDebug[ID][int.Parse(flow.startName)];
									}
								}
							}
							if(times >= 0) {
								GUI.color = UIElementUtility.Theme.nodeSuccessColor;
								GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.2f);
								Rect debugRect = args.rowRect;
								debugRect.width = GetContentIndent(args.item);
								GUI.DrawTexture(new Rect(debugRect.x + debugRect.width - 10, debugRect.y, 10, debugRect.height), Texture2D.whiteTexture);
								GUI.color = Color.Lerp(GUI.color, Color.clear, times * 2);
								GUI.DrawTexture(new Rect(debugRect.x, debugRect.y, debugRect.width - 10, debugRect.height), Texture2D.whiteTexture);
								GUI.color = oldColor;
							}
						}
					}
				}
				#endregion

				#region Draw Row
				Rect labelRect = args.rowRect;
				labelRect.x += GetContentIndent(args.item);
				//if(args.selected) {
				//	uNodeGUIStyle.itemStatic.Draw(labelRect, new GUIContent(args.label, icon), false, false, false, false);
				//} else {
				//	uNodeGUIStyle.itemNormal.Draw(labelRect, new GUIContent(args.label, icon), false, false, false, false);
				//}
				if(args.item is HierarchySummaryTree) {
					uNodeGUIStyle.itemNormal.Draw(labelRect, new GUIContent(uNodeUtility.WrapTextWithColor("//" + args.label, uNodeUtility.GetRichTextSetting().summaryColor), args.item.icon), false, false, false, false);
				} else {
					if(!args.selected && refSelectedTree != null) {
						DrawHighlightedBackground(args.item, args.rowRect);
					}
					if(args.item is HierarchyNodeTree) {
						NodeComponent node = (args.item as HierarchyNodeTree).node;
						if(GraphDebug.HasBreakpoint(uNodeUtility.GetObjectID(node))) {
							var oldColor = GUI.color;
							GUI.color = Color.red;
							GUI.DrawTexture(new Rect(args.rowRect.x, args.rowRect.y, 16, 16), uNodeUtility.DebugPoint);
							GUI.color = oldColor;
						}
					}
					uNodeGUIStyle.itemNormal.Draw(labelRect, new GUIContent(args.label, args.item.icon), false, false, false, false);
				}
				#endregion
			}
			//base.RowGUI(args);
		}

		#region Private Functions
		private void ContextClick(TreeViewItem tree, Event evt) {
			if(tree is HierarchyNodeTree nodeTree) {
				var node = nodeTree.node;
				var mPOS = GUIUtility.GUIToScreenPoint(evt.mousePosition);
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Inspect..."), false, () => {
					Inspect(nodeTree, mPOS);
				});
				menu.AddItem(new GUIContent("Hightlight Node"), false, () => {
					uNodeEditor.HighlightNode(node);
				});
				MonoScript ms = uNodeEditorUtility.GetMonoScript(node);
				if(ms != null) {
					menu.AddSeparator("");
					menu.AddItem(new GUIContent("References/Find Script"), false, delegate () {
						EditorGUIUtility.PingObject(ms);
					});
					menu.AddItem(new GUIContent("References/Edit Script"), false, delegate () {
						AssetDatabase.OpenAsset(ms);
					});
				}
				if(!GraphDebug.HasBreakpoint(uNodeUtility.GetObjectID(node))) {
					menu.AddItem(new GUIContent("Add Breakpoint"), false, delegate () {
						GraphDebug.AddBreakpoint(uNodeUtility.GetObjectID(node));
						uNodeGUIUtility.GUIChanged(node);
					});
				} else {
					menu.AddItem(new GUIContent("Remove Breakpoint"), false, delegate () {
						GraphDebug.RemoveBreakpoint(uNodeUtility.GetObjectID(node));
						uNodeGUIUtility.GUIChanged(node);
					});
				}
				menu.ShowAsContext();
			} else if(tree is HierarchyRefNodeTree) {
				ContextClick((tree as HierarchyRefNodeTree).tree, evt);
			} else if(tree is HierarchySummaryTree) {
				ContextClick((tree as HierarchySummaryTree).owner, evt);
			}
		}

		private void DrawHighlightedBackground(TreeViewItem tree, Rect position) {
			if(tree is HierarchyNodeTree) {
				var nTree = tree as HierarchyNodeTree;
				if(nTree.node == refSelectedTree) {
					var oldColor = GUI.color;
					var color = Color.yellow;
					color.a = 0.2f;
					GUI.color = color;
					GUI.DrawTexture(position, Texture2D.whiteTexture);
					GUI.color = oldColor;
				}
			} else if(tree is HierarchyRefNodeTree) {
				DrawHighlightedBackground((tree as HierarchyRefNodeTree).tree, position);
			}
		}

		private bool HighlightTree(TreeViewItem tree) {
			if(tree is HierarchyNodeTree) {
				var node = (tree as HierarchyNodeTree).node;
				uNodeEditor.HighlightNode(node);
				return true;
			} else if(tree is HierarchyPortTree) {
				uNodeEditor.HighlightNode((tree as HierarchyPortTree).node);
				return true;
			} else if(tree is HierarchyFlowTree) {
				uNodeEditor.HighlightNode((tree as HierarchyFlowTree).owner);
				return true;
			} else if(tree is HierarchyRefNodeTree) {
				return HighlightTree((tree as HierarchyRefNodeTree).tree);
			} else if(tree is HierarchySummaryTree) {
				return HighlightTree((tree as HierarchySummaryTree).owner);
			} else if(tree is HierarchyBlockTree) {
				return HighlightTree((tree as HierarchyBlockTree).owner);
			}
			return false;
		}

		private GraphDebug.DebugData GetDebugInfo() {
			if(editorData.graph != graph)
				return null;
			object debugObject = editorData.debugTarget;
			if(Application.isPlaying && debugObject != null) {
				if(debugObject is GraphDebug.DebugData) {
					return debugObject as GraphDebug.DebugData;
				} else if(GraphDebug.debugData.TryGetValue(uNodeUtility.GetObjectID(editorData.graph), out var debugMap) &&
					debugMap.ContainsKey(debugObject)) {
					return debugMap[debugObject];
				}
			}
			return null;
		}
		#endregion

		#region Functions
		private void Inspect(TreeViewItem treeView, Vector2 position) {
			if(treeView is HierarchyNodeTree nodeTree) {
				ActionPopupWindow.ShowWindow(Vector2.zero, () => {
					CustomInspector.ShowInspector(new GraphEditorData(graph) { selected = nodeTree.node });
				}, 300, 300).ChangePosition(position);
			} else if(treeView is HierarchyFunctionTree functionTree) {
				ActionPopupWindow.ShowWindow(Vector2.zero, () => {
					CustomInspector.ShowInspector(new GraphEditorData(graph) { selected = functionTree.function });
				}, 300, 300).ChangePosition(position);
			} else if(treeView is HierarchyPropertyTree propertyTree) {
				ActionPopupWindow.ShowWindow(Vector2.zero, () => {
					CustomInspector.ShowInspector(new GraphEditorData(graph) { selected = propertyTree.property });
				}, 300, 300).ChangePosition(position);
			} else if(treeView is HierarchyVariableTree variableTree) {
				ActionPopupWindow.ShowWindow(Vector2.zero, () => {
					CustomInspector.ShowInspector(new GraphEditorData(graph) { selected = variableTree.variable });
				}, 300, 300).ChangePosition(position);
			}
		}

		public bool AddNodeTree(TreeViewItem tree, TreeViewItem parentTree, IList<TreeViewItem> rows, bool isChildren = true) {
			if(tree == null || parentTree == null)
				return false;
			if(isChildren) {
				parentTree.AddChild(tree);
			} else {
				parentTree.parent.AddChild(tree);
			}
			rows.Add(tree);
			return true;
		}

		public bool CanAddTree(MemberData member) {
			if(member.isAssigned && member.IsTargetingPortOrNode) {
				if(member.targetType == MemberData.TargetType.FlowNode) {
					return member.GetTargetNode() != null;
				} else if(member.targetType == MemberData.TargetType.FlowInput) {
					return member.GetTargetNode() != null && member.Invoke(null) as FlowInput != null;
				}
			}
			return false;
		}

		public bool AddNodeTree(MemberData member, TreeViewItem parentTree, IList<TreeViewItem> rows, bool isChildren = true) {
			if(member.isAssigned && member.IsTargetingPortOrNode) {
				if(member.targetType == MemberData.TargetType.FlowNode) {
					var n = member.GetTargetNode();
					if(n != null) {
						AddNodes(n, parentTree, rows, isChildren);
						return true;
					}
				} else if(member.targetType == MemberData.TargetType.FlowInput) {
					var n = member.GetTargetNode();
					var flow = member.Invoke(null) as FlowInput;
					if(n != null && flow != null) {
						bool flag = false;
						if(!flowPortsMap.TryGetValue(flow, out var flowTree)) {
							flag = true;
							flowTree = new HierarchyPortTree(
								n,
								flow,
								uNodeEditorUtility.GetUIDFromString($"{n.GetHashCode()}:FI={member.startName}"),
								-1,
								$"{n.GetNodeName()} ( {ObjectNames.NicifyVariableName(flow.name)} )") {
								icon = uNodeEditorUtility.GetTypeIcon(n.GetNodeIcon()) as Texture2D
							};
							flowPortsMap[flow] = flowTree;
						}
						if(isChildren) {
							parentTree.AddChild(flowTree);
						} else {
							parentTree.parent.AddChild(flowTree);
						}
						rows.Add(flowTree);
						if(flag) {
							var drawer = HierarchyDrawer.FindDrawer(n.GetType());
							drawer.manager = this;
							drawer.AddChildNodes(n, flowTree, rows);
						}
						return true;
					}
				}
			}
			return false;
		}

		public void AddNodes(NodeComponent nodeComponent, TreeViewItem parentItem, IList<TreeViewItem> rows, bool isChildren = true) {
			if(nodeTreesMap.TryGetValue(nodeComponent, out var childItem)) {
				var tree = new HierarchyRefNodeTree(childItem, -1);
				if(isChildren) {
					parentItem.AddChild(tree);
				} else {
					parentItem.parent.AddChild(tree);
				}
				rows.Add(tree);
				return;
			}
			var drawer = HierarchyDrawer.FindDrawer(nodeComponent.GetType());
			drawer.manager = this;
			//Create Node Tree
			childItem = drawer.CreateNodeTree(nodeComponent);
			if(isChildren) {
				AddSummary(nodeComponent.comment, childItem, parentItem, rows);
				parentItem.AddChild(childItem);
			} else {
				AddSummary(nodeComponent.comment, childItem, parentItem.parent, rows);
				parentItem.parent.AddChild(childItem);
			}
			rows.Add(childItem);
			nodeTreesMap[nodeComponent] = childItem;
			drawer.AddChildNodes(nodeComponent, childItem, rows);
		}
		#endregion
	}
}