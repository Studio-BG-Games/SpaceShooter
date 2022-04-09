using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode.Editors {
    [System.Serializable]
	public class GraphEditorData {
		[SerializeField]
		private GameObject gameObject;

		#region Constructors
		public GraphEditorData() {

		}

		public GraphEditorData(GraphEditorData editorData) {
			if(editorData == null)
				return;
			gameObject = editorData.gameObject;
			_graphData = editorData._graphData;
			_graph = editorData._graph;
			_selectedRoot = editorData._selectedRoot;
			_selectedGroup = editorData._selectedGroup;
			_selectedObject = editorData._selectedObject;
			_selected = editorData._selected;
			debugAnyScript = editorData.debugAnyScript;
			debugObject = editorData.debugObject;
			debugSelf = editorData.debugSelf;
			selectedNodes = editorData.selectedNodes;
		}

		public GraphEditorData(GameObject gameObject) {
			this.gameObject = gameObject;
		}

		public GraphEditorData(uNodeComponentSystem graph) {
			if(graph is uNodeData data) {
				gameObject = data.gameObject;
				_graphData = data;
			} else if(graph is uNodeRoot root) {
				_graph = root;
				if(root) {
					gameObject = root.gameObject;
				}
			} else {
				_graph = null;
			}
		}
		#endregion

		public void SetOwner(UnityEngine.Object owner) {
			if(owner is GameObject) {
				gameObject = owner as GameObject;
			} else if(owner is uNodeRoot) {
				_graph = owner;
				gameObject = (owner as uNodeRoot).gameObject;
			} else if(owner is uNodeData) {
				_graphData = owner;
				gameObject = (owner as uNodeData).gameObject;
			}
		}

		/// <summary>
		/// The GameObject of uNode
		/// </summary>
		public GameObject owner {
			get {
				if(!gameObject && graph) {
					return graph.gameObject;
				}
				return gameObject;
			}
		}

		[SerializeField]
		private UnityEngine.Object _graphData;
		public uNodeData graphData {
			get {
				if((!_graphData || !(_graphData is uNodeData)) && gameObject && gameObject != null) {
					try {
						_graphData = gameObject.GetComponent<uNodeData>();
					}
					catch {
						gameObject = null;
					}
				}
				return _graphData as uNodeData;
			}
		}

		[SerializeField]
		private UnityEngine.Object _graph;
		/// <summary>
		/// The selected target root.
		/// </summary>
		public uNodeRoot graph {
			get {
				return _graph as uNodeRoot;
			}
		}

		public bool isValidGraph => graph != null || graphData != null;

		public GraphSystemAttribute graphSystem => GraphUtility.GetGraphSystem(graph);

		/// <summary>
		/// All of uNode components
		/// </summary>
		public uNodeRoot[] graphs {
			get {
				if(graph != null) {
					return graph.GetComponents<uNodeRoot>();
				} else if(owner != null) {
					return owner.GetComponents<uNodeRoot>();
				}
				return null;
			}
		}

		public IStateGraph targetStateGraph {
			get {
				return graph as IStateGraph;
			}
		}

		/// <summary>
		/// The current scope of graph editing.
		/// A value must be a group node, a graph root (function, property, constructor), or a graph itseft.
		/// </summary>
		/// <value></value>
		public UnityEngine.Object currentCanvas {
			get{
				if(selectedGroup != null) {
					return selectedGroup;
				}
				if(selectedRoot != null) {
					return selectedRoot;
				}
				return graph;
			}
		}

		public Transform currentRoot {
			get {
				if(selectedGroup != null) {
					return selectedGroup.transform;
				}
				if(selectedRoot != null) {
					return selectedRoot.transform;
				}
				return graph.RootObject.transform;
			}
		}

		public bool canAddNode => selectedRoot != null || graph is IMacroGraph || graph is IStateGraph state && state.canCreateGraph;

		public bool supportCoroutine {
			get {
				if(selectedRoot == null && (graph is IMacroGraph || graph is IStateGraph state && state.canCreateGraph)) {
					return true;
				} else if(selectedGroup is ISuperNode) {
					ISuperNode superNode = selectedGroup as ISuperNode;
					if(superNode.AcceptCoroutine()) {
						return true;
					}
				}
				if(selectedRoot != null) {
					return selectedRoot.CanHaveCoroutine();
				}
				return false;
			}
		}

		#region Selection
		[SerializeField]
		private UnityEngine.Object _selectedRoot;
		/// <summary>
		/// The selected uNode root
		/// </summary>
		public RootObject selectedRoot {
			get {
				return _selectedRoot as RootObject;
			}
			set {
				selectedGroup = null;
				_selectedRoot = value;
			}
		}

		[SerializeField]
		private UnityEngine.Object _selectedGroup;
		public Node selectedGroup {
			get {
				return _selectedGroup as Node;
			}
			set {
				if(value == null || value is ISuperNode) {
					_selectedGroup = value;
				}
			}
		}

		public List<NodeComponent> selectedNodes = new List<NodeComponent>();

		[SerializeField]
		private UnityEngine.Object _selectedObject;
		private object _selected;
		/// <summary>
		/// The selected object.
		/// </summary>
		public object selected {
			get {
				if(_selected != null) {
					return _selected;
				} else if(selectedNodes.Count > 0) {
					return selectedNodes;
				} else if(_selectedObject != null) {
					return _selectedObject;
				}
				return _selected;
			}
			set {
				_selected = value;
				_selectedObject = value as UnityEngine.Object;
				if(value is List<NodeComponent>) {
					
				} else {
					selectedNodes.Clear();
				}
			}
		}
		#endregion

		#region Debug
		private object _debugObject;
		private uNodeRoot oldTargetDebug;
		/// <summary>
		/// The debug object.
		/// </summary>
		public object debugTarget {
			get {
				if(oldTargetDebug != graph) {
					_debugObject = null;
					oldTargetDebug = graph;
				}
				if(_debugObject != null) {
					if(_debugObject is UnityEngine.Object o && o == null) {
						_debugObject = null;
					} else {
						return _debugObject;
					}
				}
				if(debugObject != null) {
					return debugObject;
				} else if(debugAnyScript) {
					if(Application.isPlaying && graph != null) {
						UnityEngine.Object obj = graph.GetPersistenceObject();
						if(GraphDebug.debugData.ContainsKey(uNodeUtility.GetObjectID(obj))) {
							Dictionary<object, GraphDebug.DebugData> debugMap = null;
							debugMap = GraphDebug.debugData[uNodeUtility.GetObjectID(obj)];
							if(debugMap.Count > 0) {
								foreach(KeyValuePair<object, GraphDebug.DebugData> pair in debugMap) {
									if(pair.Key != null && pair.Key as uNodeRoot != obj) {
										if(pair.Key is UnityEngine.Object o && o == null) {
											continue;
										}
										_debugObject = pair.Value;
										break;
									}
								}
							}
						}
					}
					return _debugObject;
				}
				if(debugSelf) {
					return graph;
				}
				return null;
			}
			set {
				if(value is UnityEngine.Object) {
					debugObject = value as UnityEngine.Object;
					debugAnyScript = false;
				} else if(value is bool) {
					debugAnyScript = (bool)value;
				} else {
					debugAnyScript = false;
				}
				_debugObject = value;
				if(value == null) {
					debugObject = null;
				}
				debugSelf = true;
			}
		}

		public UnityEngine.Object debugObject;
		public bool debugAnyScript = true, debugSelf = true;
		#endregion

		public List<NodeComponent> nodes = new List<NodeComponent>();
		public List<NodeRegion> regions = new List<NodeRegion>();

		/// <summary>
		/// True if the owner can be edited by graph editor.
		/// </summary>
		public bool isGraphOpen {
			get {
				return selectedRoot != null || selectedGroup != null || graph is IMacroGraph || graph is IStateGraph state && state.canCreateGraph;
			}
		}

		[System.Serializable]
		public class GraphCanvas {
			public UnityEngine.Object owner;
			public Vector2 position /*= new Vector2(14000, 14000)*/;
			public float zoomScale = 1;
			public bool hasFocused;
		}
		[SerializeField]
		private List<GraphCanvas> canvasDatas = new List<GraphCanvas>();

		public Vector2 GetPosition(UnityEngine.Object obj) {
			return GetGraphPosition(obj).position;
		}

		public void SetPosition(UnityEngine.Object obj, Vector2 position) {
			var data = GetGraphPosition(obj);
			if(data.position != Vector2.zero)
				data.position = position;
		}

		public GraphCanvas GetCurrentCanvasData() {
			return GetGraphPosition(currentCanvas, false);
		}
		
		private GraphCanvas GetGraphPosition(UnityEngine.Object obj, bool focusCanvas = true) {
			GraphCanvas graphCanvas = canvasDatas.FirstOrDefault(p => p.owner == obj);
			if(graphCanvas != null) {
				if (graphCanvas.position != Vector2.zero) {
					return graphCanvas;
				}
			} else {
				graphCanvas = new GraphCanvas() {
					owner = obj,
				};
			}
			canvasDatas.Add(graphCanvas);
			if(!focusCanvas) {
				return graphCanvas;
			}
			if(obj is RootObject) {
				RootObject root = obj as RootObject;
				if(root.startNode != null) {
					graphCanvas.position = new Vector2(root.startNode.editorRect.x - 200, root.startNode.editorRect.y - 200);
				}
			} else if(obj is ISuperNode) {
				ISuperNode superNode = obj as ISuperNode;
				foreach(var n in superNode.nestedFlowNodes) {
					if(n != null) {
						graphCanvas.position = new Vector2(n.editorRect.x - 200, n.editorRect.y - 200);
						break;
					}
				}
			} else if(obj is IStateGraph) {
				IStateGraph stateGraph = obj as IStateGraph;
				foreach(var n in stateGraph.eventNodes) {
					if(n != null) {
					graphCanvas.position = new Vector2(n.editorRect.x - 200, n.editorRect.y - 200);
						break;
					}
				}
			} else if(obj is NodeComponent) {
				var n = obj as NodeComponent;
				graphCanvas.position = new Vector2(n.editorRect.x - 200, n.editorRect.y - 200);
			}
			if(graphCanvas.position == Vector2.zero) {
				Component comp = obj as Component;
				if(obj is GameObject gameObject) {
					comp = gameObject.GetComponent<uNodeComponent>();
				}
				if(comp != null) {
					List<NodeComponent> nodes = new List<NodeComponent>();
					if(comp is uNodeRoot graph && graph.RootObject != null) {
						foreach(Transform t in graph.RootObject.transform) {
							nodes.Add(t.GetComponent<NodeComponent>());
						}
					} else {
						foreach(Transform t in comp.transform) {
							nodes.Add(t.GetComponent<NodeComponent>());
						}
					}
					var n = nodes.FirstOrDefault(nod => nod != null);
					if (n != null) {
						graphCanvas.position = new Vector2(n.editorRect.x - 200, n.editorRect.y - 200);
					} else {
						n = nodes.FirstOrDefault();
						if (n != null) {
							graphCanvas.position = new Vector2(n.editorRect.x - 200, n.editorRect.y - 200);
						} else {
							graphCanvas.position = Vector2.zero;
						}
					}
				}
			}
			return graphCanvas;
		}

		public bool HasPosition(UnityEngine.Object obj) {
			return canvasDatas.Any(p => p.owner == obj);
		}

		public Vector2 position {
			get {
				return GetPosition(currentCanvas);
			}
			set {
				SetPosition(currentCanvas, value);
			}
		}

		/// <summary>
		/// Refresh editor data.
		/// </summary>
		public void Refresh() {
			if(graph == null)
				return;
			graph.Refresh();
			nodes.Clear();
			this.regions.Clear();
			List<NodeRegion> regions = new List<NodeRegion>();
			if(graph.RootObject != null) {
				graph.RootObject.GetComponentsInChildren<NodeRegion>(true, regions);
			}
			if(selectedGroup) {//In Grouped
				if(selectedGroup is IRefreshable) {
					(selectedGroup as IRefreshable).Refresh();
				}
				if(selectedRoot != null && selectedRoot is IRefreshable) {
					(selectedRoot as IRefreshable).Refresh();
				}
				foreach(Node node in graph.nodes) {
					if(node == null || node.transform.parent != selectedGroup.transform)
						continue;
					nodes.Add(node);
					if(node is IRefreshable) {
						(node as IRefreshable).Refresh();
					}
				}
				foreach(NodeRegion region in regions) {
					if(region == null || region.transform.parent != selectedGroup.transform)
						continue;
					this.regions.Add(region);
				}
			} else if(selectedRoot) {//Inside RootObject
				if(selectedRoot is IRefreshable) {
					(selectedRoot as IRefreshable).Refresh();
				}
				foreach(Node node in graph.nodes) {
					if(node == null || node.transform.parent != selectedRoot.transform)
						continue;
					nodes.Add(node);
					if(node is IRefreshable) {
						(node as IRefreshable).Refresh();
					}
				}
				foreach(NodeRegion region in regions) {
					if(region == null || region.transform.parent != selectedRoot.transform)
						continue;
					this.regions.Add(region);
				}
			} else {//Inside StateMachine
				foreach(Node node in graph.nodes) {
					if(node == null || node.transform.parent.gameObject != graph.RootObject)
						continue;
					nodes.Add(node);
					if(node is IRefreshable) {
						(node as IRefreshable).Refresh();
					}
				}
				foreach(NodeRegion region in regions) {
					if(region == null || region.transform.parent.gameObject != graph.RootObject)
						continue;
					this.regions.Add(region);
				}
			}
		}

		public HashSet<string> GetNamespaces() {
			if(graph != null) {
				var ns = graph.GetNamespaces();
				if(ns != null) {
					return ns.ToHashSet();
				}
			}
			if(graphData != null) {
				return new HashSet<string>(graphData.GetNamespaces());
			}
			return new HashSet<string>();
		}

		public void ResetPositionData() {
			canvasDatas.Clear();
		}
	}
}