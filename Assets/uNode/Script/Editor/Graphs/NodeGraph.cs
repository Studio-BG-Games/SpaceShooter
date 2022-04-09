using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode.Editors {
	[Serializable]
	public class GraphSearchQuery {
		public enum SearchType {
			None,
			Node,
			Port,
			NodeType,
		}

		public List<string> query = new List<string>();
		public SearchType type = SearchType.None;

		public static HashSet<string> csharpKeyword = new HashSet<string>() {
			"false",
			"true",
			"null",
			"bool",
			"byte",
			"char",
			"decimal",
			"double",
			"float",
			"int",
			"long",
			"object",
			"sbyte",
			"short",
			"string",
			"uint",
			"ulong",
		};
	}

	public abstract class NodeGraph {
		#region Static
		public List<NodeComponent> nodeToCopy = new List<NodeComponent>();
		public static NodeGraph openedGraph;

		private static char[] numberChar = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

		public static IEnumerable<string> GetGraphUsingNamespaces(INode<uNodeRoot> node) {
			return GetGraphUsingNamespaces(node.GetOwner());
		}

		public static IEnumerable<string> GetGraphUsingNamespaces(uNodeRoot graph) {
			return graph.GetNamespaces();
		}

		public static HashSet<string> GetOpenedGraphUsingNamespaces() {
			HashSet<string> ns = null;
			if(openedGraph != null) {
				ns = openedGraph.editorData.GetNamespaces();
			}
			return ns;
		}

		/// <summary>
		/// Refresh opened graph.
		/// </summary>
		public static void RefreshOpenedGraph() {
			if(openedGraph != null) {
				openedGraph.Refresh();
			}
		}
		#endregion

		#region Event
		public abstract void HighlightNode(NodeComponent node);

		public void UpdatePosition() {
			window?.UpdatePosition();
		}

		public void SelectionChanged() {
			window?.EditorSelectionChanged();
		}

		public void GUIChanged() {
			uNodeEditor.GUIChanged();
		}

		public virtual void GUIChanged(object obj) {

		}

		/// <summary>
		/// Refresh the graph.
		/// </summary>
		public void Refresh() {
			window?.Refresh();
			GUIChanged();
		}

		/// <summary>
		/// Repaint the graph.
		/// </summary>
		public void Repaint() {
			window?.Repaint();
		}

		public GraphSearchQuery searchQuery = new GraphSearchQuery();
		public virtual void OnSearchChanged(string search) {
			if(search == null || string.IsNullOrEmpty(search.Trim())) {
				searchQuery = new GraphSearchQuery();
			} else {
				searchQuery.query.Clear();
				var strs = search.Split(':');
				if(strs.Length == 2) {
					if(string.IsNullOrEmpty(strs[1].Trim())) {
						searchQuery.type = GraphSearchQuery.SearchType.None;
						return;
					}
					searchQuery.query.AddRange(strs[1].Split('&').Where(s => !string.IsNullOrEmpty(s.Trim())).Select(s => s.Trim()));
					switch(strs[0].Trim()) {
						case "p":
						case "port":
							searchQuery.type = GraphSearchQuery.SearchType.Port;
							break;
						case "t":
						case "type":
							searchQuery.type = GraphSearchQuery.SearchType.NodeType;
							break;
						case "n":
						case "node":
							searchQuery.type = GraphSearchQuery.SearchType.Node;
							break;
					}
				} else {
					searchQuery.query.AddRange(strs[0].Split('&').Where(s => !string.IsNullOrEmpty(s.Trim())).Select(s => s.Trim()));
					searchQuery.type = GraphSearchQuery.SearchType.Node;
				}
			}
		}

		public virtual void OnSearchNext() {

		}

		public virtual void OnSearchPrev() {

		}
		#endregion

		#region Properties
		public float zoomScale {
			get {
				return editorData.GetCurrentCanvasData().zoomScale;
			}
			protected set {
				editorData.GetCurrentCanvasData().zoomScale = value;
			}
		}

		public bool isZoom {
			get {
				return zoomScale != 1;
			}
		}

		public float loadingProgress { get; set; }

		public virtual Vector2 canvasPosition {
			get {
				return new Vector2(0, 57);
			}
		}

		public GraphEditorData editorData => window?.editorData;

		public List<NodeComponent> nodes {
			get {
				return editorData != null ? editorData.nodes : null;
			}
		}

		public List<NodeRegion> regions {
			get {
				return editorData != null ? editorData.regions : null;
			}
		}

		uNodePreference.PreferenceData _preferenceData;
		/// <summary>
		/// The preference data.
		/// </summary>
		public uNodePreference.PreferenceData preferenceData {
			get {
				if(_preferenceData != uNodePreference.GetPreference()) {
					_preferenceData = uNodePreference.GetPreference();
				}
				return _preferenceData;
			}
		}
		#endregion

		#region Variables
		public uNodeEditor window;
		public List<BaseEventNode> eventNodes = new List<BaseEventNode>();

		protected Vector2 scrollView = new Vector2(30000, 30000);
		public GraphData graphData { get; protected set; }
		public Vector2 topMousePos, zoomMousePos;
		protected bool isSelection, isDragging;
		#endregion

		#region CopyPaste
		public void PasteNode(Vector3 position) {
			if(nodeToCopy == null || nodeToCopy.Count == 0)
				return;
			if(uNodeEditorUtility.IsPrefab(editorData.graph.gameObject)) {
				throw new Exception("Editing graph prefab dirrectly is not supported.");
			}
			uNodeEditorUtility.RegisterUndo(editorData.graph, "Paste Node");
			uNodeRoot UNR = editorData.graph;
			float progress = 0;
			int loopIndex = 0;
			if(nodeToCopy.Count > 5) {
				EditorUtility.DisplayProgressBar("Loading", "", progress);
			}
			Vector2 center = Vector2.zero;
			int centerLength = 0;
			Dictionary<uNodeComponent, uNodeComponent> CopyedObjectMap = new Dictionary<uNodeComponent, uNodeComponent>(EqualityComparer<uNodeComponent>.Default);
			foreach(uNodeComponent comp in nodeToCopy) {
				if(comp == null || comp is BaseGraphEvent && editorData.selectedGroup != null)
					continue;
				Node node = comp as Node;
				if(!CopyedObjectMap.ContainsKey(comp)) {
					uNodeComponent com = Object.Instantiate(comp);
					com.gameObject.name = com.gameObject.name.Replace("(Clone)", "");
					if(editorData.selectedGroup == null) {
						if(editorData.selectedRoot) {
							com.transform.parent = editorData.selectedRoot.transform;
						} else {
							com.transform.parent = NodeEditorUtility.GetNodeRoot(UNR).transform;
						}
					} else {
						com.transform.parent = editorData.selectedGroup.transform;
					}
					int index = 0;
					string nm = com.gameObject.name.TrimEnd(numberChar);
					while(true) {
						bool flag = false;
						string gName = com.gameObject.name;
						foreach(Transform t in com.transform.parent) {
							if(t != com.transform) {
								if(t.gameObject.name.Equals(gName)) {
									flag = true;
									break;
								}
							}
						}
						if(flag) {
							com.gameObject.name = nm + index.ToString();
							index++;
							continue;
						}
						break;
					}
					CopyedObjectMap.Add(comp, com);
					if(comp is IMacro || comp is ISuperNode) {
						var fields = EditorReflectionUtility.GetFields(comp.GetType());
						foreach(var field in fields) {
							if(field.FieldType == typeof(List<Nodes.MacroPortNode>)) {
								var value = field.GetValueOptimized(comp) as List<Nodes.MacroPortNode>;
								if(value != null) {
									var sourceValue = field.GetValueOptimized(com) as List<Nodes.MacroPortNode>;
									for(int i = 0; i < value.Count; i++) {
										if(value[i] == null)
											continue;
										CopyedObjectMap.Add(value[i], sourceValue[i]);
									}
								}
							}
						}
					}
				}
				if(node != null) {
					center.x += node.editorRect.x;
					center.y += node.editorRect.y;
					centerLength++;
				} else {
					BaseEventNode met = comp as BaseEventNode;
					if(met != null) {
						center.x += met.editorRect.x;
						center.y += met.editorRect.y;
						centerLength++;
					}
				}
				loopIndex++;
				progress = (float)loopIndex / (float)nodeToCopy.Count;
				if(nodeToCopy.Count > 5) {
					EditorUtility.DisplayProgressBar("Loading", "", progress);
				}
			}
			progress = 0;
			center /= centerLength;
			HashSet<uNodeComponent> needReflectedComponent = new HashSet<uNodeComponent>();
			uNodeRoot compEvent = null;
			foreach(uNodeComponent com in nodeToCopy) {
				uNodeComponent comp = null;
				if(CopyedObjectMap.ContainsKey(com)) {
					comp = CopyedObjectMap[com];
					if(comp == null) {
						loopIndex++;
						progress = (float)loopIndex / (float)nodeToCopy.Count;
						if(nodeToCopy.Count > 5) {
							EditorUtility.DisplayProgressBar("Loading", "", progress);
						}
						continue;
					}
					if(comp as Node) {
						Node node = comp as Node;
						Func<object, bool> validation = delegate (object o) {
							if(o is MemberData) {
								MemberData member = o as MemberData;
								if(member.IsTargetingPortOrNode) {
									NodeComponent n = member.GetInstance() as NodeComponent;
									if(n && n is uNodeComponent) {
										if(CopyedObjectMap.ContainsKey(n)) {
											member.instance = CopyedObjectMap[n] as NodeComponent;
											n.owner = UNR;
											return true;
										} else if(n.owner != UNR || n.transform.parent != node.transform.parent && n.transform.parent != node.transform) {
											member.instance = null;
											n.owner = UNR;
											return true;
										}
										//return true;
									}
								}
							}
							return false;
						};
						if(node as StateNode) {
							StateNode eventNode = node as StateNode;
							TransitionEvent[] TE = eventNode.GetTransitions();
							foreach(TransitionEvent n in TE) {
								var tn = n.GetTargetNode();
								if(tn == null)
									continue;
								if(CopyedObjectMap.ContainsKey(tn)) {
									n.target = MemberData.FlowInput(CopyedObjectMap[tn] as Node);
									n.owner = UNR;
								} else if(n.owner != UNR || tn != null && tn.owner != UNR
									|| tn != null && tn.transform.parent != node.transform.parent) {
									n.target = MemberData.none;
									n.owner = UNR;
								}
							}
						} else if(node is IMacro || node is ISuperNode) {
							var fields = EditorReflectionUtility.GetFields(comp.GetType());
							foreach(var field in fields) {
								if(field.FieldType == typeof(List<Nodes.MacroPortNode>)) {
									var value = field.GetValueOptimized(comp) as List<Nodes.MacroPortNode>;
									if(value != null) {
										foreach(var v in value) {
											AnalizerUtility.AnalizeObject(v, validation);
										}
									}
								}
							}
						}
						AnalizerUtility.AnalizeObject(node, validation);
						node.editorRect = new Rect(node.editorRect.x - center.x + position.x, node.editorRect.y - center.y + position.y, node.editorRect.width, node.editorRect.height);
						if(node.owner != UNR) {
							node.owner = UNR;
						}
					} else if(comp is BaseEventNode) {
						BaseEventNode method = comp as BaseEventNode;
						var flows = method.GetFlows();
						for(int i = 0; i < flows.Count; i++) {
							var tn = flows[i].GetTargetNode();
							if(tn != null && CopyedObjectMap.ContainsKey(tn)) {
								flows[i] = new MemberData(CopyedObjectMap[flows[i].GetTargetNode()], MemberData.TargetType.FlowNode);
							} else if(method.owner != UNR) {
								flows[i] = MemberData.none;
							}
						}
						method.owner = UNR;
						method.editorRect = new Rect(method.editorRect.x - center.x + position.x, method.editorRect.y - center.y + position.y, method.editorRect.width, method.editorRect.height);
					}
				}
				loopIndex++;
				progress = (float)loopIndex / (float)nodeToCopy.Count;
				if(nodeToCopy.Count > 5) {
					EditorUtility.DisplayProgressBar("Loading", "", progress);
				}
			}
			if(nodeToCopy.Count > 5) {
				EditorUtility.ClearProgressBar();
			}
			if(needReflectedComponent.Count > 0) {
				NodeEditorUtility.PerformReflectComponent(needReflectedComponent.ToList(), compEvent, UNR);
			}
			foreach(KeyValuePair<uNodeComponent, uNodeComponent> keys in CopyedObjectMap) {
				if(keys.Value != null) {
					Undo.RegisterCreatedObjectUndo(keys.Value.gameObject, "Paste Node");
				}
			}
			if(uNodeEditorUtility.IsPrefabInstance(editorData.graph)) {
				PrefabUtility.ApplyPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(editorData.graph), InteractionMode.UserAction);
			}
			//allCopyedEvent.Clear();
			Refresh();
		}

		public void CopyNodes(params NodeComponent[] nodes) {
			CopyNodes(nodes);
		}

		public virtual void CopyNodes(IEnumerable<NodeComponent> nodes) {
			nodeToCopy.Clear();
			foreach(var comp in nodes) {
				nodeToCopy.Add(comp);
			}
		}
		#endregion

		#region Menu
		public Vector2 GetMenuPosition() {
			return window.GetMousePositionForMenu(topMousePos);
		}

		public static bool CreateNodeProcessor(MemberData member, GraphEditorData editorData, Vector2 position, Action<Node> onCreated) {
			var members = member.GetMembers(false);
			if(members != null && members.Length > 0 && members[members.Length - 1] is MethodInfo method && method.Name.StartsWith("op_", StringComparison.Ordinal)) {
				string name = method.Name;
				switch(name) {
					case "op_Addition":
					case "op_Subtraction":
					case "op_Multiply":
					case "op_Division":
						NodeEditorUtility.AddNewNode<Nodes.MultiArithmeticNode>(editorData, null, null, position, n => {
							switch(name) {
								case "op_Addition":
									n.operatorType = ArithmeticType.Add;
									break;
								case "op_Subtraction":
									n.operatorType = ArithmeticType.Subtract;
									break;
								case "op_Multiply":
									n.operatorType = ArithmeticType.Multiply;
									break;
								case "op_Division":
									n.operatorType = ArithmeticType.Divide;
									break;
							}
							var param = method.GetParameters();
							n.targets = new List<MemberData>() {
								MemberData.CreateValueFromType(param[0].ParameterType),
								MemberData.CreateValueFromType(param[1].ParameterType),
							};
							onCreated?.Invoke(n);
						});
						return true;
				}
			}
			NodeEditorUtility.AddNewNode<MultipurposeNode>(editorData, null, null, position, delegate (MultipurposeNode n) {
				if(n.target == null) {
					n.target = new MultipurposeMember();
				}
				n.target.target = member;
				MemberDataUtility.UpdateMultipurposeMember(n.target);
				onCreated?.Invoke(n);
			});
			return false;
		}

		public void ShowFavoriteMenu(Vector2 position,
			FilterAttribute filter = null,
			Action<Node> onAddNode = null,
			bool flowNodes = true) {
			var valueMenuPos = GetMenuPosition();
			if(filter == null) {
				filter = new FilterAttribute();
				//filter.CanSelectType = true;
				//filter.HideTypes.Add(typeof(void));
			} else {
				filter = new FilterAttribute(filter);
			}
			filter.DisplayInstanceOnStatic = false;
			filter.MaxMethodParam = int.MaxValue;
			filter.Public = true;
			filter.Instance = true;
			if(flowNodes) {
				filter.VoidType = true;
			}
			var customItems = ItemSelector.MakeFavoriteTrees(() => {
				var favoriteItems = new List<ItemSelector.CustomItem>();
				foreach(var menuItem in NodeEditorUtility.FindNodeMenu()) {
					if(!uNodeEditor.SavedData.HasFavorite("NODES", menuItem.type.FullName))
						continue;
					if(filter.OnlyGetType && menuItem.type != typeof(Type)) {
						continue;
					}
					bool isFlowNode = !menuItem.type.IsSubclassOf(typeof(ValueNode));
					if(editorData.selectedRoot && menuItem.HideOnFlow || !flowNodes && isFlowNode)
						continue;
					if(isFlowNode && filter.SetMember || !filter.IsValidTarget(MemberData.TargetType.FlowNode))
						continue;
					if(!isFlowNode && !filter.IsValidTarget(MemberData.TargetType.ValueNode))
						continue;
					if(editorData.selectedGroup && (menuItem.HideOnGroup))
						continue;
					if(menuItem.HideOnStateMachine && !editorData.selectedRoot && !editorData.selectedGroup)
						continue;
					if(menuItem.returnType != null && menuItem.returnType != typeof(object) && !filter.IsValidType(menuItem.returnType)) {
						continue;
					}
					if(menuItem.IsCoroutine && !editorData.supportCoroutine) {
						continue;
					}
					favoriteItems.Add(ItemSelector.CustomItem.Create(
						menuItem,
						() => {
							NodeEditorUtility.AddNewNode<Node>(editorData, menuItem.name.Split(' ')[0], menuItem.type, position, onAddNode);
							Refresh();
						},
						icon: isFlowNode ? uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon)) : null,
						category: "Nodes"));
				}
				return favoriteItems;
			}, filter);
			ItemSelector w = ItemSelector.ShowWindow(
				null,
				new MemberData(
					editorData.selectedGroup ??
					editorData.selectedRoot as UnityEngine.Object ??
					editorData.graph,
					MemberData.TargetType.SelfTarget),
				filter,
				delegate (MemberData value) {
					CreateNodeProcessor(value, editorData, position, (n) => {
						if(onAddNode != null) {
							onAddNode(n);
						}
						Refresh();
					});
				}).ChangePosition(valueMenuPos);
			w.displayDefaultItem = false;
			w.CustomTrees = customItems;
		}

		public void ShowNodeMenu(Vector2 position, 
			FilterAttribute filter = null, 
			Action<Node> onAddNode = null, 
			bool flowNodes = true, 
			bool expandItems = true, 
			List<ItemSelector.CustomItem> additionalItems = null) {
			var valueMenuPos = GetMenuPosition();
			if(filter == null) {
				filter = new FilterAttribute();
				//filter.CanSelectType = true;
				//filter.HideTypes.Add(typeof(void));
			} else {
				filter = new FilterAttribute(filter);
			}
			filter.DisplayInstanceOnStatic = true;
			filter.MaxMethodParam = int.MaxValue;
			filter.Public = true;
			filter.Instance = true;
			if(flowNodes) {
				filter.VoidType = true;
			}
			ItemSelector w = ItemSelector.ShowWindow(
				editorData.selectedGroup ?? 
				editorData.selectedRoot as UnityEngine.Object ?? 
				editorData.graph, 
				new MemberData(
					editorData.selectedGroup ?? 
					editorData.selectedRoot as UnityEngine.Object ?? 
					editorData.graph, 
					MemberData.TargetType.SelfTarget), 
				filter, 
				delegate (MemberData value) {
					CreateNodeProcessor(value, editorData, position, (n) => {
						if(onAddNode != null) {
							onAddNode(n);
						}
						Refresh();
					});
				}).ChangePosition(valueMenuPos);
			w.favoriteHandler = () => {
				var favoriteItems = new List<ItemSelector.CustomItem>();
				foreach(var menuItem in NodeEditorUtility.FindNodeMenu()) {
					if(!uNodeEditor.SavedData.HasFavorite("NODES", menuItem.type.FullName))
						continue;
					if(filter.OnlyGetType && menuItem.type != typeof(Type)) {
						continue;
					}
					bool isFlowNode = !menuItem.type.IsSubclassOf(typeof(ValueNode));
					if(editorData.selectedRoot && menuItem.HideOnFlow || !flowNodes && isFlowNode)
						continue;
					if(isFlowNode && filter.SetMember || !filter.IsValidTarget(MemberData.TargetType.FlowNode))
						continue;
					if(!isFlowNode && !filter.IsValidTarget(MemberData.TargetType.ValueNode))
						continue;
					if(editorData.selectedGroup && (menuItem.HideOnGroup))
						continue;
					if(menuItem.HideOnStateMachine && !editorData.selectedRoot && !editorData.selectedGroup)
						continue;
					if(menuItem.returnType != null && menuItem.returnType != typeof(object) && !filter.IsValidType(menuItem.returnType)) {
						continue;
					}
					if(menuItem.IsCoroutine && !editorData.supportCoroutine) {
						continue;
					}
					favoriteItems.Add(ItemSelector.CustomItem.Create(
						menuItem,
						() => {
							NodeEditorUtility.AddNewNode<Node>(editorData, menuItem.name.Split(' ')[0], menuItem.type, position, onAddNode);
							Refresh();
						},
						icon: isFlowNode ? uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon)) : null,
						category: "Nodes"));
				}
				return favoriteItems;
			};
			w.displayNoneOption = false;
			w.displayCustomVariable = false;
			w.customItemDefaultExpandState = expandItems;
			if(filter.SetMember)
				return;//Return on set member is true.
			List<ItemSelector.CustomItem> customItems = new List<ItemSelector.CustomItem>();
			if(additionalItems != null) {
				customItems.AddRange(additionalItems);
			}
			foreach(var menuItem in NodeEditorUtility.FindNodeMenu()) {
				if(filter.OnlyGetType && menuItem.type != typeof(Type)) {
					continue;
				}
				bool isFlowNode = !menuItem.type.IsSubclassOf(typeof(ValueNode));
				if(editorData.selectedRoot && menuItem.HideOnFlow || !flowNodes && isFlowNode)
					continue;
				if(isFlowNode && filter.SetMember || !filter.IsValidTarget(MemberData.TargetType.FlowNode))
					continue;
				if(!isFlowNode && !filter.IsValidTarget(MemberData.TargetType.ValueNode))
					continue;
				if(editorData.selectedGroup && (menuItem.HideOnGroup))
					continue;
				if(menuItem.HideOnStateMachine && !editorData.selectedRoot && !editorData.selectedGroup)
					continue;
				if(menuItem.returnType != null && menuItem.returnType != typeof(object) && !filter.IsValidType(menuItem.returnType)) {
					continue;
				}
				if(menuItem.IsCoroutine && !editorData.supportCoroutine) {
					continue;
				}
				customItems.Add(ItemSelector.CustomItem.Create(
					menuItem, 
					() => {
						NodeEditorUtility.AddNewNode<Node>(editorData, menuItem.name.Split(' ')[0], menuItem.type, position, onAddNode);
						Refresh();
					}, 
					icon: isFlowNode ? uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon)) : null));
			}

			#region Flow
			if(flowNodes && !filter.SetMember && filter.IsValidType(typeof(void))) {
				if(!(!editorData.selectedRoot && !editorData.selectedGroup)) {
					customItems.Add(ItemSelector.CustomItem.Create("Continue", delegate () {
						NodeEditorUtility.AddNewNode<NodeJumpStatement>(
							editorData,
							"Continue",
							position,
							delegate (NodeJumpStatement n) {
								n.statementType = JumpStatementType.Continue;
							});
						Refresh();
					}, "JumpStatement", icon: uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
					customItems.Add(ItemSelector.CustomItem.Create("Break", delegate () {
						NodeEditorUtility.AddNewNode<NodeJumpStatement>(
							editorData,
							"Break",
							position,
							delegate (NodeJumpStatement n) {
								n.statementType = JumpStatementType.Break;
								if(onAddNode != null) {
									onAddNode(n);
								}
							});
						Refresh();
					}, "JumpStatement", icon: uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
				}
				if(editorData.selectedRoot) {
					customItems.Add(ItemSelector.CustomItem.Create("Return", delegate () {
						NodeEditorUtility.AddNewNode<NodeReturn>(
							editorData,
							"Return",
							position,
							delegate (NodeReturn n) {
								if(onAddNode != null) {
									onAddNode(n);
								}
							});
						Refresh();
					}, "Return", icon: uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
				}
			}
			#endregion

			if(filter.IsValidTarget(MemberData.TargetType.ValueNode)) {
				if(filter.IsValidType(typeof(Type))) {
					customItems.Add(ItemSelector.CustomItem.Create("typeof()", delegate () {
						var win = TypeSelectorWindow.ShowWindow(Vector2.zero, new FilterAttribute() { OnlyGetType = true, DisplayRuntimeType = false }, delegate (MemberData[] types) {
							NodeEditorUtility.AddNewNode<MultipurposeNode>(
								editorData,
								position,
								delegate (MultipurposeNode n) {
									if(n.target == null) {
										n.target = new MultipurposeMember();
									}
									n.target.target = types[0];
									if(onAddNode != null) {
										onAddNode(n);
									}
									Refresh();
									w.Close();
								});
						});
						win.targetObject = editorData.selectedGroup ?? editorData.selectedRoot as UnityEngine.Object ?? editorData.graph;
						win.ChangePosition(valueMenuPos);
						GUIUtility.ExitGUI();
					}, "Data"));
				}
				var nodeMenuItems = NodeEditorUtility.FindCreateNodeCommands();
				foreach(var n in nodeMenuItems) {
					n.graph = this;
					n.filter = filter;
					if(!n.IsValid()) {
						continue;
					}
					customItems.Add(ItemSelector.CustomItem.Create(n.name, () => {
						var createdNode = n.Setup(position);
						if(onAddNode != null) {
							onAddNode(createdNode);
						}
					}, n.category, icon: uNodeEditorUtility.GetTypeIcon(n.icon)));
				}
			}
			w.customItems = customItems;
		}
		#endregion

		#region Functions
		public void RefreshEventNodes() {
			eventNodes = null;
			if(!editorData.selectedGroup && !editorData.selectedRoot && editorData.targetStateGraph != null || editorData.selectedGroup as StateNode) {
				if(!editorData.selectedGroup && !editorData.selectedRoot && editorData.targetStateGraph != null) {
					eventNodes = new List<BaseEventNode>();
					foreach(var n in editorData.targetStateGraph.eventNodes) {
						eventNodes.Add(n);
					}
				} else if(editorData.selectedGroup as StateNode) {
					eventNodes = new List<BaseEventNode>(NodeEditorUtility.FindChildNode<StateEventNode>(editorData.selectedGroup.transform).Select(item => item as BaseEventNode));
				}
			}
		}

		public virtual void FrameGraph() {

		}

		public void ReloadView() {
			ReloadView(false);
		}

		public virtual void ReloadView(bool fullReload) {

		}

		public virtual void OnEnable() {

		}

		public virtual void OnDisable() {

		}

		public virtual void OnNoTarget() {

		}
		public virtual void DrawGraphPanel(Rect position) {

		}

		public virtual void HideGraphPanel() {

		}

		public virtual void DrawTabbar(Vector2 position) {

		}

		public virtual void DrawCanvas(uNodeEditor window, GraphData graphData) {
			this.graphData = graphData;
			this.window = window;
			openedGraph = this;
			topMousePos = Event.current.mousePosition;
		}
		#endregion

		#region Utility
		protected GraphDebug.DebugData GetDebugData() {
			object debugObject = editorData.debugTarget;
			if(Application.isPlaying && debugObject != null) {
				if(debugObject is GraphDebug.DebugData) {
					return debugObject as GraphDebug.DebugData;
				} else if (GraphDebug.debugData.TryGetValue(uNodeUtility.GetObjectID(editorData.graph), out var debugMap) && 
					debugMap.ContainsKey(debugObject)) {
					return debugMap[debugObject];
				}
				//var db = uNodeUtility.debugData;
				//var id = uNodeUtility.GetObjectID(editorData.graph);
			}
			return null;
		}

		public Vector2 GetMousePosition() {
			return topMousePos;
		}

		public void ClearSelection() {
			window.ChangeEditorSelection(null);
		}

		public void SelectNode(NodeComponent node, bool clearSelectedNodes = true) {
			if(clearSelectedNodes)
				editorData.selectedNodes.Clear();
			editorData.selectedNodes.Add(node);
			editorData.selected = editorData.selectedNodes;
			SelectionChanged();
		}

		public void Select(object value) {
			editorData.selectedNodes.Clear();
			editorData.selected = value;
			SelectionChanged();
		}

		public void UnselectNode(NodeComponent node) {
			if(editorData.selectedNodes.Contains(node)) {
				editorData.selectedNodes.Remove(node);
			}
		}

		public void SelectNode(IList<NodeComponent> nodes) {
			ClearSelection();
			editorData.selectedNodes.AddRange(nodes);
			editorData.selected = editorData.selectedNodes;
			SelectionChanged();
			Refresh();
		}

		public void SelectRoot(RootObject root) {
			editorData.GetPosition(root);
			editorData.selected = root;
			editorData.selectedRoot = root;
			editorData.selectedGroup = null;
			SelectionChanged();
			Refresh();
		}

		public void SelectConnectedNode(Component from, bool allConnection = false, Action<NodeComponent> callbackAction = null) {
			if(from == null)
				return;
			if(from is BaseEventNode) {
				BaseEventNode method = from as BaseEventNode;
				var flows = method.GetFlows();
				foreach(var n in flows) {
					if(n == null || n.GetTargetNode() == null)
						continue;
					if(!editorData.selectedNodes.Contains(n.GetTargetNode())) {
						editorData.selectedNodes.Add(n.GetTargetNode());
						editorData.selected = editorData.selectedNodes;
						if(allConnection) {
							SelectConnectedNode(n.GetTargetNode(), allConnection, callbackAction);
						}
						SelectionChanged();
						if(callbackAction != null) {
							callbackAction(n.GetTargetNode());
						}
					}
				}
			} else if(from is Node) {
				Node node = from as Node;
				if(node as StateNode) {
					StateNode eventNode = node as StateNode;
					TransitionEvent[] TE = eventNode.GetTransitions();
					foreach(TransitionEvent T in TE) {
						if(T.GetTargetNode() != null) {
							if(!editorData.selectedNodes.Contains(T.GetTargetNode())) {
								editorData.selectedNodes.Add(T.GetTargetNode());
								editorData.selected = editorData.selectedNodes;
								if(allConnection) {
									SelectConnectedNode(T.GetTargetNode(), allConnection, callbackAction);
								}
								SelectionChanged();
								if(callbackAction != null) {
									callbackAction(T.GetTargetNode());
								}
							}
						}
					}
				}
				Func<object, bool> validation = delegate (object o) {
					if(o is MemberData) {
						MemberData member = o as MemberData;
						if(member.targetType == MemberData.TargetType.FlowNode ||
							member.targetType == MemberData.TargetType.ValueNode) {
							Node n = member.GetInstance() as Node;
							if(member.isAssigned && member.GetInstance() is Node) {
								if(!editorData.selectedNodes.Contains(n)) {
									editorData.selectedNodes.Add(n);
									editorData.selected = editorData.selectedNodes;
									if(allConnection) {
										SelectConnectedNode(n, allConnection, callbackAction);
									}
									SelectionChanged();
									if(callbackAction != null) {
										callbackAction(n);
									}
								}
								//return true;
							}
						}
					}
					return false;
				};
				AnalizerUtility.AnalizeObject(node, validation);
			}
		}

		/// <summary>
		/// Move the canvas to the position
		/// </summary>
		/// <param name="position"></param>
		public virtual void MoveCanvas(Vector2 position) {
			if(editorData == null)
				return;
			editorData.position = position;
			// if(editorData.position.x < 0) {
			// 	editorData.position = new Vector2(0, editorData.position.y);
			// }
			// if(editorData.position.y < 0) {
			// 	editorData.position = new Vector2(editorData.position.x, 0);
			// }
		}
		#endregion
	}
}