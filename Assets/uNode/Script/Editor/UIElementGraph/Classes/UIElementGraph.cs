using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaxyGames.uNode.Editors {
	#region Classes
	public abstract class GroupCallback {
		public abstract void RegisterCallbacks(VisualElement target);
		public abstract void UnregisterCallbacks(VisualElement target);
	}

	public class DropableTargetEvent : GroupCallback {
		public Action<DragPerformEvent> onDragPerform;
		public Action<DragUpdatedEvent> onDragUpdate;
		public Action<DragEnterEvent> onDragEnter;
		public Action<DragLeaveEvent> onDragLeave;
		public Action<DragExitedEvent> onDragExited;

		public override void RegisterCallbacks(VisualElement target) {
			target.RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
			target.RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
			target.RegisterCallback<DragLeaveEvent>(OnDragLeaveEvent);
			target.RegisterCallback<DragEnterEvent>(OnDragEnterEvent);
			target.RegisterCallback<DragExitedEvent>(OnDragExitedEvent);
		}

		public override void UnregisterCallbacks(VisualElement target) {
			target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
			target.UnregisterCallback<DragPerformEvent>(OnDragPerformEvent);
			target.UnregisterCallback<DragLeaveEvent>(OnDragLeaveEvent);
			target.UnregisterCallback<DragEnterEvent>(OnDragEnterEvent);
			target.UnregisterCallback<DragExitedEvent>(OnDragExitedEvent);
		}

		private void OnDragPerformEvent(DragPerformEvent evt) {
			if(onDragPerform != null) {
				onDragPerform(evt);
				evt.StopImmediatePropagation();
			}
		}

		private void OnDragUpdatedEvent(DragUpdatedEvent evt) {
			if(onDragUpdate != null) {
				onDragUpdate(evt);
				evt.StopImmediatePropagation();
			}
		}

		private void OnDragEnterEvent(DragEnterEvent evt) {
			if(onDragEnter != null) {
				onDragEnter(evt);
				evt.StopImmediatePropagation();
			}
		}

		private void OnDragLeaveEvent(DragLeaveEvent evt) {
			if(onDragLeave != null) {
				onDragLeave(evt);
				evt.StopImmediatePropagation();
			}
		}

		private void OnDragExitedEvent(DragExitedEvent evt) {
			if(onDragExited != null) {
				onDragExited(evt);
				evt.StopImmediatePropagation();
			}
		}
	}

	public class DragableElementEvent : GroupCallback {
		public override void RegisterCallbacks(VisualElement target) {
			if(!(target is IDragManager)) {
				throw new Exception("Target must inherith of IDragManager interface");
			}
			target.RegisterCallback<MouseDownEvent>(OnMouseDown);
			target.RegisterCallback<MouseUpEvent>(OnMouseUp);
			target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
		}

		public override void UnregisterCallbacks(VisualElement target) {
			target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
			target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
			target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
		}

		private Vector2 m_MouseDownStartPos;
		private VisualElement currentClickedElement;

		private void OnMouseMove(MouseMoveEvent evt) {
			if(currentClickedElement != null && evt.imguiEvent != null && evt.imguiEvent.type == EventType.MouseDrag && (m_MouseDownStartPos - evt.localMousePosition).sqrMagnitude > 9.0) {
				StartDrag();
				currentClickedElement = null;
			}
		}

		private void OnMouseUp(MouseUpEvent evt) {
			currentClickedElement = null;
		}

		private void OnMouseDown(MouseDownEvent evt) {
			if(evt.button != 0)
				return;
			var ele = evt.currentTarget as VisualElement;
			if(ele != null && ele is IDragManager) {
				IDragManager manager = ele as IDragManager;
				var drag = manager.draggableElements.FirstOrDefault(item => item.ContainsPoint(ele.ChangeCoordinatesTo(item, evt.localMousePosition)));
				if(drag != null) {
					m_MouseDownStartPos = evt.localMousePosition;
					currentClickedElement = drag;
				}
			}
		}

		private void StartDrag() {
			var ele = currentClickedElement;
			if(ele != null) {
				DragAndDrop.activeControlID = ele.GetHashCode();
				DragAndDrop.PrepareStartDrag();
				DragAndDrop.SetGenericData("uNode", ele);
				DragAndDrop.StartDrag("Dragging Element");
				if(ele is IDragableElement) {
					(ele as IDragableElement).StartDrag();
				}
			}
		}
	}

	public class ClickableElement : VisualElement {
		public Clickable clickable { get; set; }
		public EventBase clickedEvent { get; protected set; }

		public Action onClick;
		public Label label;
		public Image icon;
		private Image breadcrumb;
		public DropdownMenu menu;

		public ClickableElement() {
			Init("");
		}

		public ClickableElement(Action onClick) {
			this.onClick = onClick;
			Init("");
		}

		public ClickableElement(string text) {
			Init(text);
		}

		public ClickableElement(string text, Action onClick) {
			this.onClick = onClick;
			Init(text);
		}

		protected void Init(string text) {
			clickable = new Clickable((evt) => {
				clickedEvent = evt;
				if(onClick != null) {
					onClick();
				}
				this.ShowMenu(menu);
			});
			this.AddManipulator(clickable);
			style.flexDirection = FlexDirection.Row;

			label = new Label(text);
			Add(label);
		}

		public void ShowIcon(Texture icon) {
			if(this.icon == null) {
				this.icon = new Image() {
					name = "icon"
				};
				Insert(0, this.icon);
			}
			if(icon != null)
				this.icon.image = icon;
		}

		public void EnableBreadcrumb(bool enable) {
			if(breadcrumb == null) {
				breadcrumb = new Image() {
					name = "breadcrumb"
				};
				Add(breadcrumb);
			}
			if(enable) {
				breadcrumb.ShowElement();
			} else {
				breadcrumb.HideElement();
			}
		}
	}
	#endregion

	public class UIElementGraph : NodeGraph {
		public VisualElement rootView;
		public UGraphView graphView;

		private IMGUIContainer iMGUIContainer;

		//graph visual properties
		public Vector3 position = Vector3.zero;
		public Vector3 scale = Vector3.one;

		public static bool richText {
			get {
				var theme = UIElementUtility.Theme;
				return theme != null ? theme.textSettings.useRichText : true;
			}
		}

		public override Vector2 canvasPosition {
			get {
				return new Vector2(0, 60);
			}
		}

		private bool hasInitialize;
		UnityEngine.Object lastSelected;

		public override void OnEnable() {
			InitializeRootView();
			InitializeGraph();
			iMGUIContainer = new IMGUIContainer(OnGUI);
			iMGUIContainer.StretchToParentSize();
			iMGUIContainer.pickingMode = PickingMode.Ignore;
			rootView.Add(iMGUIContainer);
		}

		void OnGUI() {
			if(editorData.currentCanvas is RootObject RO) {
				if(RO.startNode == null) {
					EditorGUILayout.HelpBox("No start node assigned, right click on 'flow node' and 'Set as start' to assign it.", MessageType.Warning);
				}
			} else if(editorData.currentCanvas is GroupNode GN) {
				if(GN.nodeToExecute == null) {
					EditorGUILayout.HelpBox("No start node assigned, right click on 'flow node' and 'Set as start' to assign it.", MessageType.Warning);
				}
			}
		}

		public override void OnDisable() {
			graphPanelContainer?.RemoveFromHierarchy();
			graphPanelContainer = null;
			tabbarContainer?.RemoveFromHierarchy();
			tabbarContainer = null;
			hasInitTabbar = false;
			graphView?.RemoveFromHierarchy();
			graphView = null;
			OnNoTarget();
		}

		public override void OnNoTarget() {
			if(rootView != null) {
				rootView.parent?.Remove(rootView);
				rootView = null;
			}
		}

		[NonSerialized]
		private bool hasInitTabbar;
		private VisualElement tabbarContainer;
		private float _tabPosition;

		public override void DrawTabbar(Vector2 position) {
			if(!hasInitTabbar) {
				hasInitTabbar = true;
				_tabPosition = position.x;
				ReloadTabbar();
			} else if(_tabPosition != position.x) {
				_tabPosition = position.x;
				ReloadTabbar();
			}
		}

		bool _isTabbarMarkedReload;
		public void MarkReloadTabbar() {
			if(!_isTabbarMarkedReload) {
				_isTabbarMarkedReload = true;
				uNodeThreadUtility.Queue(() => {
					_isTabbarMarkedReload = false;
					ReloadTabbar();
				});
			}
		}

		private void ReloadTabbar() {
			if(tabbarContainer != null) {
				tabbarContainer.RemoveFromHierarchy();
			}
			tabbarContainer = new VisualElement() {
				name = "tabbar-container"
			};
			tabbarContainer.style.left = _tabPosition;
			tabbarContainer.AddStyleSheet("uNodeStyles/Tabbar");
			tabbarContainer.AddStyleSheet(UIElementUtility.Theme.tabbarStyle);
			var tabbar = new VisualElement() {
				name = "tabbar"
			};
			{
				#region Main/Selection Tab
				if (window.mainGraph != null && window.mainGraph == window.selectedGraph && !window.mainGraph.selectedData.isValidGraph) {
					window.mainGraph.owner = null;
					window.mainGraph.graph = null;
					window.mainGraph.selectedData = new GraphEditorData();
				}
				var tabMainElement = new ClickableElement("\"Main\"") {
					name = "tab-element",
					onClick = () => {
						window.ChangeEditorTarget(null);
					},
				};
				tabMainElement.AddManipulator(new ContextualMenuManipulator((evt) => {
					evt.menu.AppendAction("Close All But This", (act) => {
						window.graphs.Clear();
						window.ChangeEditorTarget(null);
						ReloadTabbar();
					}, DropdownMenuAction.AlwaysEnabled);
					evt.menu.AppendSeparator("");
					evt.menu.AppendAction("Find Object", (act) => {
						EditorGUIUtility.PingObject(window.mainGraph.owner);
						ReloadTabbar();
					}, DropdownMenuAction.AlwaysEnabled);
					evt.menu.AppendAction("Select Object", (act) => {
						EditorGUIUtility.PingObject(window.mainGraph.owner);
						Selection.instanceIDs = new int[] { window.mainGraph.owner.GetInstanceID() };
						ReloadTabbar();
					}, DropdownMenuAction.AlwaysEnabled);
				}));
				if(window.selectedGraph == window.mainGraph) {
					tabMainElement.AddToClassList("tab-selected");
				}
				tabbar.Add(tabMainElement);
				#endregion

				for (int i = 0; i < window.graphs.Count; i++) {
					var graph = window.graphs[i];
					try {
						if (graph == null || graph.owner == null || !graph.selectedData.isValidGraph) {
							window.graphs.RemoveAt(i);
							i--;
							continue;
						}
					} catch {
						window.graphs.RemoveAt(i);
						i--;
						continue;
					}
					var tabElement = new ClickableElement(graph.displayName) {
						name = "tab-element",
						onClick = () => {
							window.ChangeEditorTarget(graph);
						},
					};
					tabElement.AddManipulator(new ContextualMenuManipulator((evt) => {
						evt.menu.AppendAction("Close", (act) => {
							var oldData = window.selectedGraph;
							window.graphs.Remove(graph);
							window.ChangeEditorTarget(oldData);
							ReloadTabbar();
						}, DropdownMenuAction.AlwaysEnabled);
						evt.menu.AppendAction("Close All", (act) => {
							window.graphs.Clear();
							window.ChangeEditorTarget(null);
							ReloadTabbar();
						}, DropdownMenuAction.AlwaysEnabled);
						evt.menu.AppendAction("Close Others", (act) => {
							var current = window.selectedGraph;
							window.graphs.Clear();
							window.graphs.Add(current);
							window.ChangeEditorTarget(current);
							ReloadTabbar();
						}, DropdownMenuAction.AlwaysEnabled);
						evt.menu.AppendSeparator("");
						evt.menu.AppendAction("Find Object", (act) => {
							EditorGUIUtility.PingObject(graph.owner);
							ReloadTabbar();
						}, DropdownMenuAction.AlwaysEnabled);
						evt.menu.AppendAction("Select Object", (act) => {
							EditorGUIUtility.PingObject(graph.owner);
							Selection.instanceIDs = new int[] { graph.owner.GetInstanceID() };
							ReloadTabbar();
						}, DropdownMenuAction.AlwaysEnabled);
					}));
					if(window.selectedGraph == graph) {
						tabElement.AddToClassList("tab-selected");
					}
					tabbar.Add(tabElement);
				}
				#region Plus Tab
				{
					var plusElement = new ClickableElement("+") {
						name = "tab-element",
					};
					{
						plusElement.menu = new DropdownMenu();
						plusElement.menu.AppendAction("Open...", (act) => {
							window.OpenNewGraphTab();
							ReloadTabbar();
						});

						#region Recent Files
						List<UnityEngine.Object> lastOpenedObjects = uNodeEditor.FindLastOpenedGraphs();
						for(int i = 0; i < lastOpenedObjects.Count; i++) {
							var obj = lastOpenedObjects[i];
							if(obj is uNodeRoot) {
								uNodeRoot root = obj as uNodeRoot;
								plusElement.menu.AppendAction("Open Recent/" + root.gameObject.name, (act) => {
									uNodeEditor.Open(root);
								});
							} else if(obj is uNodeData) {
								uNodeData data = obj as uNodeData;
								plusElement.menu.AppendAction("Open Recent/" + data.gameObject.name, (act) => {
									uNodeEditor.Open(data);
								});
							}
						}
						if(lastOpenedObjects.Count > 0) {
							plusElement.menu.AppendSeparator("Open Recent/");
							plusElement.menu.AppendAction("Open Recent/Clear Recent", (act) => {
								uNodeEditor.ClearLastOpenedGraphs();
							});
						}
						#endregion

						var graphSystem = GraphUtility.FindGraphSystemAttributes();
						int lastOrder = int.MinValue;
						for (int i = 0; i < graphSystem.Count;i++) {
							var g = graphSystem[i];
							if(i == 0 || Mathf.Abs(g.order - lastOrder) >= 10) {
								plusElement.menu.AppendSeparator("");
							}
							lastOrder = g.order;
							plusElement.menu.AppendAction(g.menu, (act) => {
								string path = EditorUtility.SaveFilePanelInProject("Create " + g.menu, "", "prefab", "");
								if (!string.IsNullOrEmpty(path)) {
									GameObject go = new GameObject();
									go.name = path.Split('/').Last().Split('.')[0];
									go.AddComponent(g.type);
									GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
									UnityEngine.Object.DestroyImmediate(go);
									AssetDatabase.SaveAssets();
									EditorUtility.FocusProjectWindow();
									uNodeEditor.Open(prefab.GetComponent(g.type) as uNodeRoot);
								}
							});
						}
					}
					tabbar.Add(plusElement);
				}
				#endregion
			}
			var pathbar = new VisualElement() {
				name = "pathbar"
			};
			if(editorData.graph != null) {
				var graph = new ClickableElement(editorData.graph.DisplayName) {
					name = "path-element"
				};
				graph.AddToClassList("path-graph");
				{
					graph.menu = new DropdownMenu();
					uNodeRoot[] graphs = editorData.graphs;
					if(graphs == null)
						return;
					for(int i = 0; i < graphs.Length; i++) {
						var g = graphs[i];
						if(g == null)
							continue;
						graph.menu.AppendAction(g.DisplayName, (act) => {
							if(g == editorData.graph) {
								window.ChangeEditorSelection(g);
							} else {
								uNodeEditor.Open(g);
							}
						}, (act) => {
							if(g == editorData.graph) {
								return DropdownMenuAction.Status.Checked;
							}
							return DropdownMenuAction.Status.Normal;
						});
					}
				}
				Type graphIcon = typeof(TypeIcons.GraphIcon);
				if(editorData.graph is IClass) {
					IClass classSystem = editorData.graph as IClass;
					graphIcon = classSystem.IsStruct ? typeof(TypeIcons.StructureIcon) : typeof(TypeIcons.ClassIcon);
				}
				graph.ShowIcon(uNodeEditorUtility.GetTypeIcon(graphIcon));
				graph.EnableBreadcrumb(true);
				pathbar.Add(graph);
				var root = window.selectedGraph.selectedData.selectedRoot;
				var function = new ClickableElement(root != null ? root.Name : editorData.graph is IStateGraph state && state.canCreateGraph ? "[State Graph]" : editorData.graph is IMacroGraph ? "[MACRO]" : "[NO ROOT]") {
					name = "path-element"
				};
				function.AddToClassList("path-function");
				{
					function.menu = new DropdownMenu();
					if(editorData.graph is IStateGraph stateGraph && stateGraph.canCreateGraph) {
						function.menu.AppendAction("[State Graph]", (act) => {
							if(editorData.selectedRoot != null || editorData.selectedGroup != null) {
								editorData.selected = null;
								editorData.selectedRoot = null;
								Refresh();
								UpdatePosition();
							}
							window.ChangeEditorSelection(null);
						}, (act) => {
							if(editorData.selectedRoot == null) {
								return DropdownMenuAction.Status.Checked;
							}
							return DropdownMenuAction.Status.Normal;
						});
					}

					List<RootObject> roots = new List<RootObject>();
					roots.AddRange(editorData.graph.Functions);
					roots.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
					for(int i = 0; i < roots.Count; i++) {
						var r = roots[i];
						if(r == null)
							continue;
						function.menu.AppendAction(r.Name, (act) => {
							if(editorData.currentCanvas == r) {
								window.ChangeEditorSelection(r);
							} else {
								editorData.selectedRoot = r;
								SelectionChanged();
								Refresh();
								UpdatePosition();
							}
						}, (act) => {
							if(r == editorData.selectedRoot) {
								return DropdownMenuAction.Status.Checked;
							}
							return DropdownMenuAction.Status.Normal;
						});
					}
				}
				function.ShowIcon(uNodeEditorUtility.GetTypeIcon(root == null ? typeof(TypeIcons.StateIcon) : typeof(TypeIcons.MethodIcon)));
				pathbar.Add(function);
				if(editorData.graph != null && editorData.selectedGroup && editorData.selectedGroup.owner == editorData.graph) {
					function.EnableBreadcrumb(true);
					List<Node> GN = new List<Node>();
					GN.Add(editorData.selectedGroup);
					NodeEditorUtility.FindParentNode(editorData.selectedGroup.transform, ref GN, editorData.graph);
					for(int i = GN.Count - 1; i >= 0; i--) {
						var nestedGraph = GN[i];
						var element = new ClickableElement(nestedGraph.GetNodeName()) {
							name = "path-element",
							onClick = () => {
								window.ChangeEditorSelection(nestedGraph);
								if(editorData.selectedGroup != nestedGraph) {
									editorData.selectedGroup = nestedGraph;
									Refresh();
									UpdatePosition();
								}
							}
						};
						element.AddToClassList("path-nested");
						element.ShowIcon(uNodeEditorUtility.GetTypeIcon(nestedGraph.GetNodeIcon()));
						pathbar.Add(element);
						if(i != 0) {
							element.EnableBreadcrumb(true);
						}
					}
				}
			} else {
				var graph = new ClickableElement("[NO GRAPH]") {
					name = "path-element"
				};
				pathbar.Add(graph);
			}
			tabbarContainer.Add(tabbar);
			tabbarContainer.Add(pathbar);
			window.rootVisualElement.Add(tabbarContainer);
			tabbarContainer.SendToBack();
		}

		public override void FrameGraph() {
			if(graphView != null) {
				graphView.FrameAll();
			}
		}

		private VisualElement graphPanelContainer;
		private bool isGraphPenelHidden;
		private GraphPanel panel;

		public override void DrawGraphPanel(Rect position) {
			bool flag = false;
			if(isGraphPenelHidden && graphPanelContainer != null) {
				graphPanelContainer.SetDisplay(true);
				graphPanelContainer.pickingMode = PickingMode.Position;
				isGraphPenelHidden = false;
				flag = true;
			}
			if(graphPanelContainer == null) {
				if(graphPanelContainer != null) {
					graphPanelContainer.RemoveFromHierarchy();
				}
				graphPanelContainer = new VisualElement() {
					name = "graph-panel-container"
				};
				//graphPanelContainer.styleSheets.Add(Resources.Load<StyleSheet>("uNodeStyles/Tabbar"));
				window.rootVisualElement.Add(graphPanelContainer);
				graphPanelContainer.SendToBack();
				if(panel != null) {
					panel.Dispose();
				}
				graphPanelContainer.Add(panel = new GraphPanel(this));
				flag = true;
			}
			if(flag || graphPanelContainer.resolvedStyle.width != position.width) {
				graphPanelContainer.StretchToParentSize();
				graphPanelContainer.style.left = position.x;
				graphPanelContainer.style.top = position.y;
				graphPanelContainer.style.width = position.width;
			}
		}

		public override void HideGraphPanel() {
			if(graphPanelContainer != null) {
				graphPanelContainer.SetDisplay(false);
				graphPanelContainer.pickingMode = PickingMode.Ignore;
			}
			isGraphPenelHidden = true;
		}

		public override void GUIChanged(object obj) {
			base.GUIChanged(obj);
			MarkReloadTabbar();
		}

		public override void ReloadView(bool fullReload) {
			if(rootView == null || graphView == null) {
				OnEnable();
			}
			MarkReloadTabbar();
			graphView.MarkRepaint(fullReload);
		}

		public override void OnSearchChanged(string search) {
			base.OnSearchChanged(search);
			if(graphView != null) {
				graphView.OnSearchChanged(searchQuery);
			}
		}

		public override void OnSearchNext() {
			base.OnSearchNext();
			if(graphView != null) {
				graphView.OnSearchNext();
			}
		}

		public override void OnSearchPrev() {
			base.OnSearchPrev();
			if(graphView != null) {
				graphView.OnSearchPrev();
			}
		}

		public void AddNode(NodeComponent node) {
			nodes.Add(node);
		}

		public void RemoveNode(NodeComponent node) {
			nodes.Remove(node);
		}

		public bool isGraphLoaded {
			get { return graphView != null && graphView.graph != null; }
		}

		void InitializeRootView() {
			if(rootView != null)
				rootView.RemoveFromHierarchy();
			var root = window.rootVisualElement;
			rootView = new VisualElement();
			rootView.name = "graphRootView";
			rootView.AddStyleSheet("uNodeStyles/NativeGraphStyle");
			rootView.AddStyleSheet("uNodeStyles/NativeControlStyle");
			rootView.AddStyleSheet(UIElementUtility.Theme.graphStyle);
			root.Add(rootView);
		}

		public void InitializeGraph() {
			if (graphView != null && !rootView.Contains(graphView)) {
				graphView.RemoveFromHierarchy();
			}
			if(graphView == null)
				graphView = new UGraphView();
			if(!rootView.Contains(graphView))
				rootView.Add(graphView);
			graphView.Initialize(this);
			ReloadTabbar();
		}

		public override void DrawCanvas(uNodeEditor window, GraphData graphData) {
			base.DrawCanvas(window, graphData);
			window.RemoveNotification();
			if(rootView == null) {
				OnEnable();
			}
			rootView.SetLayout(graphData.backgroundRect);
			if(!hasInitialize) {
				ReloadView();
				hasInitialize = true;
			}
			if (Event.current.type == EventType.Repaint && uNodeThreadUtility.frame % 2 == 0) {
				zoomScale = graphView.scale;
				if (position != Vector3.zero)
					editorData.position = position;
			}
			if(graphData.canvasRect.Contains(topMousePos)) {
				graphView.IMGUIEvent(Event.current);
			}
			_debugData = GetDebugData();
		}

		private GraphDebug.DebugData _debugData;
		public GraphDebug.DebugData GetDebugInfo() {
			if(_debugData == null) {
				return GetDebugData();
			}
			return _debugData;
		}

		public override void MoveCanvas(Vector2 position) {
			base.MoveCanvas(position);
			if(graphView != null) {
				graphView.UpdateViewTransform(-(position * graphView.scale), scale);
			}
			// ReloadView();
		}

		public override void HighlightNode(NodeComponent node) {
			if(graphView == null) return;
			graphView.HighlightNodes(new NodeComponent[] { node });
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class NodeCustomEditor : Attribute {
		public Type nodeType;

		public NodeCustomEditor(Type nodeType) {
			this.nodeType = nodeType;
		}
	}
}