using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaxyGames.uNode.Editors {
	/// <summary>
	/// The main editor window for editing uNode.
	/// </summary>
	public partial class uNodeEditor {
		#region Save & Load Setting
		private static uNodeEditorData _savedData;
		public static uNodeEditorData SavedData {
			get {
				if(_savedData == null) {
					LoadOptions();
					if(_savedData == null) {
						_savedData = new uNodeEditorData();
					}
				}
				return _savedData;
			}
			set {
				_savedData = value;
			}
		}

		/// <summary>
		/// Save the current graph
		/// Note: this function will not work on playmode use SaveCurrentGraph if need to save either on editor or in playmode.
		/// </summary>
		public static void AutoSaveCurrentGraph() {
			if(Application.isPlaying)
				return;
			SaveCurrentGraph();
		}

		/// <summary>
		/// Save the current graph
		/// </summary>
		public static void SaveCurrentGraph() {
			if(window == null)
				return;
			if(window.selectedGraph != null && window.selectedGraph.graph != null && window.editorData.owner != null) {
				GraphUtility.SaveGraph(window.editorData.owner);
			} else if(Application.isPlaying && window.editorData.graph is uNodeRuntime runtime && runtime.originalGraph != null) {
				GraphUtility.SaveRuntimeGraph(runtime);
			}
		}

		public static void SaveOptions() {
			EditorDataSerializer.Save(_savedData, "EditorData");
		}

		public static void LoadOptions() {
			_savedData = EditorDataSerializer.Load<uNodeEditorData>("EditorData");
			if(_savedData == null) {
				_savedData = new uNodeEditorData();
				SaveOptions();
			}
		}
		#endregion

		#region Useful Function
		/// <summary>
		/// Is the editor are not allowed to edit
		/// </summary>
		/// <returns></returns>
		public bool IsDisableEdit() {
			return (!Application.isPlaying || uNodePreference.GetPreference().preventEditingPrefab) && uNodeEditorUtility.IsPrefab(editorData.owner);
		}

		public void OpenNewGraphTab() {
			string path = EditorUtility.OpenFilePanelWithFilters("Open uNode", "Assets", new string[] { "uNode files", "prefab,asset" });
			if(path.StartsWith(Application.dataPath)) {
				path = "Assets" + path.Substring(Application.dataPath.Length);
			} else {
				path = null;
			}
			if(!string.IsNullOrEmpty(path)) {
				if(path.EndsWith(".prefab")) {
					GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
					if(go != null) {
						var root = go.GetComponent<uNodeRoot>();
						if(root != null) {
							Open(root);
							RegisterOpenedFile(path);
						} else {
							var data = go.GetComponent<uNodeData>();
							if(data != null) {
								Open(data);
								RegisterOpenedFile(path);
							}
						}
					}
				}
			}
		}

		public static List<UnityEngine.Object> FindLastOpenedGraphs() {
			List<UnityEngine.Object> lastOpenedObjects = new List<UnityEngine.Object>();
			if(SavedData.lastOpenedFile == null) {
				return lastOpenedObjects;
			}
			for(int i = 0; i < SavedData.lastOpenedFile.Count; i++) {
				string path = SavedData.lastOpenedFile[i];
				if(!File.Exists(path))
					continue;
				if(path.EndsWith(".prefab")) {
					GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
					if(go != null) {
						var root = go.GetComponent<uNodeRoot>();
						if(root != null) {
							lastOpenedObjects.Add(root);
						} else {
							var data = go.GetComponent<uNodeData>();
							if(data != null) {
								lastOpenedObjects.Add(data);
							}
						}
					}
				}
				if(lastOpenedObjects.Count >= 10) {
					break;
				}
			}
			return lastOpenedObjects;
		}

		public static void ClearLastOpenedGraphs() {
			SavedData.lastOpenedFile = null;
			SaveOptions();
		}

		/// <summary>
		/// Clear the graph cached data so the graph will have fresh datas
		/// </summary>
		public static void ClearGraphCache() {
			UGraphView.ClearCache();
		}

		/// <summary>
		/// Check the editor errors.
		/// </summary>
		public void CheckErrors() {
			if(editorData == null)
				return;
#if UseProfiler
			Profiler.BeginSample("Check Errors");
#endif
			if(editorErrors != null) {
				var map = new Dictionary<UnityEngine.Object, List<uNodeUtility.ErrorMessage>>(editorErrors);
				editorErrors.Clear();
				foreach(var pair in map) {
					if(pair.Key != null && pair.Key as UnityEngine.Object) {
						editorErrors.Add(pair.Key, pair.Value);
					}
				}
			}
			var roots = editorData.graphs;
			if(roots != null) {
				for(int i = 0; i < roots.Length; i++) {
					uNodeUtility.ClearEditorError(roots[i]);
					var nodes = NodeEditorUtility.FindAllNode(roots[i]);
					if(nodes != null) {
						foreach(var node in nodes) {
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
						}
					}
					if(roots[i].RootObject != null) {
						FindMissingScripts(roots[i].RootObject.transform, roots[i]);
					}
				}
			}
#if UseProfiler
			Profiler.EndSample();
#endif
		}

		private void FindMissingScripts(Transform transform, uNodeRoot owner) {
			var comps = transform.GetComponents<MonoBehaviour>();
			for(int i = 0; i < comps.Length; i++) {
				if(comps[i] == null) {
					uNodeUtility.RegisterEditorError(owner, transform.gameObject, "Missing script found on object: " + transform.gameObject.name, (position) => {
						GenericMenu menu = new GenericMenu();
						menu.AddItem(new GUIContent("Remove object"), false, () => {
							NodeEditorUtility.RemoveObject(transform.gameObject);
						});
						menu.ShowAsContext();
					});
					break;
				}
			}
			if(transform.childCount > 0) {
				foreach(Transform tr in transform) {
					FindMissingScripts(tr, owner);
				}
			}

		}
		#endregion

		#region EventHandler
		private void DragToVariableHandler(Rect rect, List<VariableData> variables, UnityEngine.Object owner) {
			if(rect.Contains(currentEvent.mousePosition)) {
				if((currentEvent.type == EventType.DragPerform || currentEvent.type == EventType.DragUpdated)) {
					bool isPrefab = uNodeEditorUtility.IsPrefab(editorData.owner);
					if(DragAndDrop.GetGenericData("uNode") == null &&
						DragAndDrop.visualMode == DragAndDropVisualMode.None &&
						DragAndDrop.objectReferences.Length == 1) {
						if(isPrefab) {
							if(uNodeEditorUtility.IsSceneObject(DragAndDrop.objectReferences[0])) {
								DragAndDrop.visualMode = DragAndDropVisualMode.None;
								return;
							}
						}
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					}
					if(currentEvent.type == EventType.DragPerform) {
						DragAndDrop.AcceptDrag();
						if(DragAndDrop.GetGenericData("uNode") != null) {
							//Nothing todo.
						} else if(DragAndDrop.objectReferences.Length == 1) {//Dragging UnityObject
							var dragObject = DragAndDrop.objectReferences[0];
							//rightClickPos = currentEvent.mousePosition;
							//var iPOS = GetMousePositionForMenu();
							GenericMenu menu = new GenericMenu();
							menu.AddDisabledItem(new GUIContent("Add variable"));
							menu.AddSeparator("");
							menu.AddItem(new GUIContent(dragObject.GetType().Name), false, () => {
								var variable = uNodeEditorUtility.AddVariable(dragObject.GetType(), variables, owner);
								variable.variable = dragObject;
								variable.Serialize();
							});
							menu.AddSeparator("");
							if(dragObject is GameObject) {
								Component[] components = (dragObject as GameObject).GetComponents<Component>();
								foreach(var c in components) {
									menu.AddItem(new GUIContent(c.GetType().Name), false, (comp) => {
										var variable = uNodeEditorUtility.AddVariable(comp.GetType(), variables, owner);
										variable.variable = comp;
										variable.Serialize();
									}, c);
								}
							} else if(dragObject is Component) {
								menu.AddItem(new GUIContent("GameObject"), false, () => {
									var variable = uNodeEditorUtility.AddVariable(dragObject.GetType(), variables, owner);
									variable.variable = (dragObject as Component).gameObject;
									variable.Serialize();
								});
								Component[] components = (dragObject as Component).GetComponents<Component>();
								foreach(var c in components) {
									menu.AddItem(new GUIContent(c.GetType().Name), false, (comp) => {
										var variable = uNodeEditorUtility.AddVariable(comp.GetType(), variables, owner);
										variable.variable = comp;
										variable.Serialize();
									}, c);
								}
							}
							menu.ShowAsContext();
						}
						Event.current.Use();
					}
				}
			} else if((currentEvent.type == EventType.DragPerform ||
				currentEvent.type == EventType.DragUpdated) &&
				DragAndDrop.GetGenericData("uNode") != null) {
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
			}
		}

		private void TopEventHandler() {
			var e = Event.current;
			if(e.type == EventType.KeyUp) {
				if(e.keyCode == KeyCode.F10) {
					if(editorData.graphSystem == null || editorData.graphSystem.allowCompileToScript) {
						GenerateSource();
					}
				} else if(e.keyCode == KeyCode.F9) {
					if(editorData.graphSystem == null || editorData.graphSystem.allowPreviewScript) {
						PreviewSource();
					}
				} else if(e.keyCode == KeyCode.F5) {
					Refresh(true);
				}
			}
		}
		#endregion

		#region Menu Functions
		static void ShowAddNewRootMenu(GameObject owner, Action<uNodeRoot> onAdded = null) {
			GenericMenu menu = new GenericMenu();
			var graphSystem = GraphUtility.FindGraphSystemAttributes();
			int lastOrder = int.MinValue;
			for(int i = 0; i < graphSystem.Count; i++) {
				var g = graphSystem[i];
				if(!g.allowCreateInScene)
					continue;
				if(lastOrder != int.MinValue && Mathf.Abs(g.order - lastOrder) >= 10) {
					menu.AddSeparator("");
				}
				lastOrder = g.order;
				menu.AddItem(new GUIContent(g.menu), false, delegate () {
					var comp = owner.AddComponent(g.type);
					if(onAdded != null)
						onAdded(comp as uNodeRoot);
				});
			}
			menu.AddSeparator("");
			var templates = uNodeEditorUtility.FindAssetsByType<uNodeTemplate>();
			if(templates != null && templates.Count > 0) {
				menu.AddSeparator("");
				foreach(var t in templates) {
					string path = t.name;
					if(!string.IsNullOrEmpty(t.path)) {
						path = t.path;
					}
					menu.AddItem(new GUIContent(path), false, (temp) => {
						var tmp = temp as uNodeTemplate;
						Serializer.Serializer.Deserialize(tmp.serializedData, owner);
						var comp = owner.GetComponents<uNodeRoot>();
						if(comp.Length > 0) {
							if(onAdded != null)
								onAdded(comp.Last());
						}
					}, t);
				}
			}
			menu.ShowAsContext();
		}

		void RegisterOpenedFile(string path) {
			if(SavedData.lastOpenedFile == null) {
				SavedData.lastOpenedFile = new List<string>();
			}
			if(SavedData.lastOpenedFile.Contains(path)) {
				SavedData.lastOpenedFile.Remove(path);
			}
			SavedData.lastOpenedFile.Add(path);
			SaveOptions();
		}
		#endregion

		#region Others
		/// <summary>
		/// Change the editor target.
		/// </summary>
		/// <param name="data"></param>
		public void ChangeEditorTarget(GraphData data) {
			bool needRefresh = data == null || selectedGraph != data || selectedGraph.selectedData.currentCanvas != data.selectedData.currentCanvas;
			selectedGraph = data;
			OnMainTargetChange();
			if(needRefresh) {
				Refresh();
			}
			UpdatePosition();
		}

		/// <summary>
		/// Change the editor main target.
		/// </summary>
		/// <param name="target"></param>
		internal static void ChangeMainTarget(uNodeComponentSystem target, GameObject graph = null, bool forceSelect = false) {
			if(window == null) {
				ShowWindow();
			}
			if(graph == null) {
				if(uNodeEditorUtility.IsPrefab(target)) {
					if(target is uNodeRuntime) {
						return;
					}
					graph = target.gameObject;
					target = LoadTempGraph(target);
				} else {
					graph = GraphUtility.GetOriginalObject(target.gameObject);
					if(graph == null && target is uNodeRuntime runtime && runtime.originalGraph != null) {
						graph = runtime.originalGraph.gameObject;
					}
				}
			}
			bool isGraph = target as uNodeRoot;
			for(int i = 0; i < window.mainGraph.data.Count; i++) {
				var d = window.mainGraph.data[i];
				if(isGraph ? d.graph == target : d.graph == null && d.graphData == target) {
					if(isGraph ? !d.graph : !d.graphData) {
						window.mainGraph.data.RemoveAt(i);
						i--;
						continue;
					}
					bool needRefresh = window.selectedGraph == window.mainGraph && window.selectedGraph.selectedData.currentCanvas != d.currentCanvas;
					window.mainGraph.selectedData = d;
					if(forceSelect) {
						window.ChangeEditorTarget(window.mainGraph);
					} else {
						window.OnMainTargetChange();
					}
					if(needRefresh) {
						window.Refresh();
					}
					return;
				}
			}
			window.mainGraph.selectedData = new GraphEditorData(target);
			window.mainGraph.graph = graph;
			if(forceSelect) {
				window.ChangeEditorTarget(window.mainGraph);
			} else {
				window.OnMainTargetChange();
			}
			if(window.selectedGraph == window.mainGraph)
				window.Refresh();
		}

		/// <summary>
		/// Change the editor target.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="graph"></param>
		public static void Open(uNodeRoot target, GameObject graph = null) {
			if(window == null) {
				ShowWindow();
			}
			if(graph == null) {
				if(uNodeEditorUtility.IsPrefab(target)) {
					if(target is uNodeRuntime) {
						return;
					}
					graph = target.gameObject;
					target = LoadTempGraph(target);
				} else {
					graph = GraphUtility.GetOriginalObject(target.gameObject);
					if(graph == null && target is uNodeRuntime runtime && runtime.originalGraph != null) {
						graph = runtime.originalGraph.gameObject;
					}
				}
			}
			if(graph == null) {
				ChangeMainTarget(target);
				window.selectedGraph = window.mainGraph;
				//window.selectedGraph.graph = graph;
				window.ChangeEditorSelection(window.editorData.graph);
				return;
			}
			foreach(var data in window.graphs) {
				if(data == null)
					continue;
				if(data.owner == target.gameObject || graph != null && data.graph == graph) {
					for(int i = 0; i < data.data.Count; i++) {
						var d = data.data[i];
						if(d.graph == target) {
							if(!d.graph) {
								data.data.RemoveAt(i);
								i--;
								continue;
							}
							window.selectedGraph = data;
							window.selectedGraph.selectedData = d;
							window.ChangeEditorSelection(window.editorData.graph);
							window.Refresh();
							window.UpdatePosition();
							return;
						}
					}
					var ED = new GraphEditorData(target);
					data.data.Add(ED);
					data.selectedData = ED;
					window.selectedGraph = data;
					window.ChangeEditorSelection(window.editorData.graph);
					window.Refresh();
					window.UpdatePosition();
					return;
				}
			}
			window.graphs.Add(new GraphData() { data = new List<GraphEditorData> { new GraphEditorData(target) } });
			window.selectedGraph = window.graphs.Last();
			window.selectedGraph.graph = graph;
			window.ChangeEditorSelection(window.editorData.graph);
			window.Refresh();
			window.UpdatePosition();
		}

		/// <summary>
		/// Change the editor target.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="graph"></param>
		public static void Open(uNodeData target, GameObject graph = null) {
			if(window == null) {
				ShowWindow();
			}
			if(graph == null) {
				if(uNodeEditorUtility.IsPrefab(target)) {
					graph = target.gameObject;
					target = LoadTempGraph(target);
				} else {
					graph = GraphUtility.GetOriginalObject(target.gameObject);
				}
			}
			if(graph == null) {
				ChangeMainTarget(target);
				window.selectedGraph = window.mainGraph;
				window.UpdatePosition();
				return;
			}
			foreach(var data in window.graphs) {
				if(data == null)
					continue;
				if(data.owner == target.gameObject || graph != null && data.graph == graph) {
					for(int i = 0; i < data.data.Count; i++) {
						var d = data.data[i];
						if(d.graph == null && d.graphData == target) {
							if(!d.graphData) {
								data.data.RemoveAt(i);
								i--;
								continue;
							}
							window.selectedGraph = data;
							window.selectedGraph.selectedData = d;
							window.Refresh();
							window.UpdatePosition();
							return;
						}
					}
					var ED = new GraphEditorData(target);
					data.data.Add(ED);
					data.selectedData = ED;
					window.selectedGraph = data;
					window.Refresh();
					window.UpdatePosition();
					return;
				}
			}
			window.graphs.Add(new GraphData() { data = new List<GraphEditorData> { new GraphEditorData(target) } });
			window.selectedGraph = window.graphs.Last();
			window.selectedGraph.graph = graph;
			window.Refresh();
			window.UpdatePosition();
		}

		public static void Open(INode<uNodeRoot> nodeComponent, GameObject graph = null) {
			if(window == null) {
				ShowWindow();
			}
			var target = nodeComponent.GetOwner();
			if(nodeComponent is Component component && component.GetComponentsInParent<uNodeRoot>(true).FirstOrDefault() is uNodeMacro macro && macro != target) {
				var temp = macro.GetComponent<TemporaryGraph>();
				if(temp != null && temp.prefab != null) {
					var obj = uNodeEditorUtility.GetPrefabTransform(component.transform, macro.transform, temp.prefab.transform);
					if(obj != null) {
						target = temp.prefab.GetComponent<uNodeRoot>();
						nodeComponent = obj.GetComponent<INode<uNodeRoot>>();
					}
				}
			}
			if(graph == null) {
				if(uNodeEditorUtility.IsPrefab(target)) {
					if(target is uNodeRuntime) {
						return;
					}
					graph = target.gameObject;
					nodeComponent = LoadTempGraphNode(nodeComponent);
					if(nodeComponent == null)
						return;
					target = nodeComponent.GetOwner();
					if(target == null)
						return;
				} else {
					graph = GraphUtility.GetOriginalObject(target.gameObject);
					if(graph == null && target is uNodeRuntime runtime && runtime.originalGraph != null) {
						graph = runtime.originalGraph.gameObject;
					}
				}
			}
			if(graph == null) {
				ChangeMainTarget(target);
				Open(nodeComponent, window.mainGraph, window);
				window.UpdatePosition();
				return;
			}
			foreach(var data in window.graphs) {
				if(data == null)
					continue;
				if(data.owner == target.gameObject || graph != null && data.graph == graph) {
					for(int i = 0; i < data.data.Count; i++) {
						var d = data.data[i];
						if(d.graph == null && d.graphData == target) {
							if(!d.graphData) {
								data.data.RemoveAt(i);
								i--;
								continue;
							}
							window.selectedGraph = data;
							window.selectedGraph.selectedData = d;
							Open(nodeComponent, data, window);
							window.UpdatePosition();
							return;
						}
					}
					var ED = new GraphEditorData(target);
					data.data.Add(ED);
					data.selectedData = ED;
					window.selectedGraph = data;
					Open(nodeComponent, data, window);
					window.UpdatePosition();
					return;
				}
			}
			window.graphs.Add(new GraphData() { data = new List<GraphEditorData> { new GraphEditorData(target) } });
			window.selectedGraph = window.graphs.Last();
			window.selectedGraph.graph = graph;
			Open(nodeComponent, window.selectedGraph, window);
			window.UpdatePosition();
		}

		private static void Open(INode<uNodeRoot> nodeComponent, GraphData data, uNodeEditor editor) {
			if(nodeComponent != null && data != null) {
				data.selectedData.selected = nodeComponent;
				data.selectedData.selectedNodes.Clear();
				if(nodeComponent is RootObject) {
					data.selectedData.selectedRoot = nodeComponent as RootObject;
				} else if(nodeComponent is NodeComponent) {
					data.selectedData.selectedNodes.Add(nodeComponent as NodeComponent);
					data.selectedData.selectedRoot = (nodeComponent as NodeComponent).rootObject;
					data.selectedData.selectedGroup = (nodeComponent as NodeComponent).parentComponent as Node;
					//if(nodeComponent is ISuperNode) {
					//	data.selectedData.selectedGroup = nodeComponent as Node;
					//}
				}
				editor.graphEditor.MoveCanvas(data.selectedData.GetPosition(nodeComponent as UnityEngine.Object));
				editor.EditorSelectionChanged();
			}
		}

		private static void OnSelectionChanged(NodeComponent component) {
			if(component == null)
				return;
			if(component.owner != null && component.owner != window.mainGraph.selectedData.graph) {
				window.mainGraph.graph = component.owner.gameObject;
			}
			if(component.transform.parent != null) {
				if(component.transform.parent.gameObject == window.mainGraph.selectedData.graph.RootObject) {
					window.mainGraph.selectedData.selectedGroup = null;
					window.mainGraph.selectedData.selectedRoot = null;
					window.mainGraph.selectedData.GetPosition(window.mainGraph.selectedData.graph);
				} else {
					RootObject root = uNodeHelper.GetComponentInParent<RootObject>(component.transform);
					if(root != null) {
						window.mainGraph.selectedData.selectedRoot = root;
						window.mainGraph.selectedData.GetPosition(root);
					} else {
						window.mainGraph.selectedData.selectedRoot = null;
					}
					NodeComponent parentComp = component.transform.parent.GetComponent<NodeComponent>();
					if(parentComp is ISuperNode) {
						window.mainGraph.selectedData.selectedGroup = parentComp as Node;
						window.mainGraph.selectedData.GetPosition(parentComp);
					}
				}
			}
			if(window.selectedGraph == window.mainGraph) {
				NodeEditorUtility.SelectNode(window.mainGraph, component);
				window.graphEditor.MoveCanvas(new Vector2(component.editorRect.x - 200, component.editorRect.y - 200));
			}
		}

		private static void OnSelectionChanged(RootObject rootObject) {
			if(rootObject != null) {
				if(rootObject.transform.parent != null && window.mainGraph.selectedData.graph != null) {
					if(rootObject.transform.parent.gameObject == window.mainGraph.selectedData.graph.RootObject) {
						NodeEditorUtility.SelectRoot(window.mainGraph, rootObject);
					}
				}
				window.mainGraph.selectedData.GetPosition(rootObject);
			}
		}

		private static GameObject LoadTempGraphObject(UnityEngine.Object graph) {
			GameObject go = null;
			if(graph is GameObject) {
				go = GraphUtility.GetTempGraphObject(graph as GameObject);
			} else if(graph is uNodeRoot) {
				go = GraphUtility.GetTempGraphObject((graph as uNodeRoot).gameObject);
			} else if(graph is INode<uNodeRoot>) {
				go = GraphUtility.GetTempGraphObject((graph as INode<uNodeRoot>).GetOwner().gameObject);
			}
			return go;
		}

		private static T LoadTempGraph<T>(T graph) where T : uNodeComponentSystem {
			var go = GraphUtility.GetTempGraphObject(graph);
			return go;
		}

		private static INode<T> LoadTempGraphNode<T>(INode<T> node) where T : uNodeRoot {
			var go = GraphUtility.GetTempGraphObject(node);
			return go;
		}

		/// <summary>
		/// Change the editor selection.
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="forceChange"></param>
		private static void ChangeMainSelection(GameObject gameObject, bool forceChange = false) {
			if(window == null) {
				ShowWindow();
			}
			if(uNodeEditorUtility.IsPrefab(gameObject)) {
				return;
			}
			window.mainGraph.graph = GraphUtility.GetOriginalObject(gameObject);
			window.UpdateMainSelection(gameObject);
			bool needRefresh = window.selectedGraph == window.mainGraph;
			if(forceChange) {
				window.ChangeEditorTarget(window.mainGraph);
			}
			if(!isLocked && window.mainGraph.selectedData.graph != null) {
				NodeComponent comp = gameObject.GetComponent<NodeComponent>();
				if(comp != null) {
					OnSelectionChanged(comp);
					if(forceChange) {
						window.ChangeEditorSelection(comp);
					}
				} else {
					RootObject root = gameObject.GetComponent<RootObject>();
					if(root != null) {
						OnSelectionChanged(root);
						if(forceChange) {
							window.ChangeEditorSelection(root);
						}
					}
				}
			}
			window.OnMainTargetChange();
			if(needRefresh)
				window.Refresh();
		}

		/// <summary>
		/// Called by uNodeInitializer on compiling script.
		/// </summary>
		public static void OnCompiling() {
			if(window != null) {
				window._isCompiling = false;
				window.SaveEditorData();
				window._isCompiling = true;
			}
		}

		/// <summary>
		/// Called by uNodeInitializer on finished compiling script.
		/// </summary>
		public static void OnFinishedCompiling() {
			if(window != null) {
				window._isCompiling = false;
				window.LoadEditorData();
				GUIChanged();
			}
		}

		private void OnMainTargetChange() {
			if(editorData == mainGraph.selectedData) {
				if(editorData.graph != null) {
					UpdatePosition();
					// editorData.selectedNodes.Clear();
				}
			}
		}

		/// <summary>
		/// Refresh uNode Editor
		/// </summary>
		public void Refresh() {
			Refresh(false);
		}

		/// <summary>
		/// Refresh uNode Editor
		/// </summary>
		public void Refresh(bool fullRefresh) {
			graphEditor.window = this;
			editorData.Refresh();
			RefreshDimmedNode();
			graphEditor.ReloadView(fullRefresh);
			if(prevShowGraph != null && !prevShowGraph.Value) {
				explorerTree?.Reload();
			}
			GUIChanged();
		}

		#region Highlight
		/// <summary>
		/// Highlight the node for a second.
		/// </summary>
		/// <param name="node"></param>
		public static void HighlightNode(NodeComponent node) {
			ShowWindow();
			if(uNodeEditorUtility.IsPrefab(node)) {
				node = GraphUtility.GetTempGraphObject(node) as NodeComponent;
			}
			Open(node);
			window.Refresh();
			window.graphEditor.HighlightNode(node);
		}

		/// <summary>
		/// Highlight the node from a Script Information data with the given line number and column number
		/// </summary>
		/// <param name="scriptInfo"></param>
		/// <param name="line"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public static bool HighlightNode(EditorScriptInfo scriptInfo, int line, int column = -1) {
			if(scriptInfo == null)
				return false;
			if(scriptInfo.informations == null)
				return false;
			var path = AssetDatabase.GUIDToAssetPath(scriptInfo.guid);
			if(string.IsNullOrEmpty(path))
				return false;
			var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
			if(asset is GameObject gameObject) {
				return HighlightNode(gameObject, scriptInfo.informations, line, column);
			}
			return false;
		}

		/// <summary>
		/// Highlight the node from a Script Information data with the given line number and column number
		/// </summary>
		/// <param name="informations"></param>
		/// <param name="line"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public static bool HighlightNode(IEnumerable<ScriptInformation> informations, int line, int column = -1) {
			return HighlightNode(null, informations, line, column);
		}

		/// <summary>
		/// Highlight the node from a Script Information data with the given line number and column number
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="informations"></param>
		/// <param name="line"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public static bool HighlightNode(GameObject graph, IEnumerable<ScriptInformation> informations, int line, int column = -1) {
			if(informations == null)
				return false;
			List<ScriptInformation> information = new List<ScriptInformation>();
			foreach(var info in informations) {
				if(info == null)
					continue;
				if(info.startLine <= line && info.endLine >= line) {
					information.Add(info);
				}
			}
			if(column > 0) {
				information.Sort((x, y) => {
					int result = CompareUtility.Compare(x.lineRange, y.lineRange);
					if(result == 0) {
						int xColumn = int.MaxValue;
						if(x.startColumn <= column && x.endColumn >= column) {
							xColumn = x.columnRange;
						}
						int yColumn = int.MaxValue;
						if(y.startColumn <= column && y.endColumn >= column) {
							yColumn = y.columnRange;
						}
						return CompareUtility.Compare(xColumn, yColumn);
					}
					return result;
				});
			} else {
				information.Sort((x, y) => {
					int result = CompareUtility.Compare(x.lineRange, y.lineRange);
					if(result == 0) {
						return CompareUtility.Compare(y.columnRange, x.columnRange);
					}
					return result;
				});
			}
			foreach(var info in information) {
				if(info != null) {
					// Debug.Log(line + ":" + column);
					// Debug.Log(info.startLine + "-" + info.endLine);
					// Debug.Log(info.startColumn + "-" + info.endColumn);
					// Debug.Log(info.lineRange + ":" + info.columnRange);
					if(int.TryParse(info.id, out var id)) {
						UnityEngine.Object obj;
						if(graph != null) {
							obj = uNodeEditorUtility.FindObjectByUniqueIdentifier(graph.transform, id);
							if(obj == null) {
								obj = EditorUtility.InstanceIDToObject(id);
								if(obj == null && int.TryParse(info.ghostID, out var gID)) {
									obj = EditorUtility.InstanceIDToObject(gID);
								}
							}
						} else {
							obj = EditorUtility.InstanceIDToObject(id);
						}
						if(obj is GameObject) {
							GameObject go = obj as GameObject;
							obj = go.GetComponent<NodeComponent>();
							if(obj == null) {
								obj = go.GetComponent<RootObject>();
							}
						} else if(obj == null) {
							obj = EditorUtility.InstanceIDToObject(id);
						}
						if(obj is NodeComponent) {
							HighlightNode(obj as NodeComponent);
							return true;
						} else if(obj is RootObject) {
							var root = obj as RootObject;
							if(root.startNode != null) {
								HighlightNode(root.startNode);
							} else {
								Open(root);
							}
							return true;
						}
					} else if(info.id.StartsWith(CG.KEY_INFORMATION_VARIABLE)) {

					}
				}
			}
			return false;
		}

		public static bool CanHighlightNode(EditorScriptInfo scriptInfo, int line, int column = -1) {
			if(scriptInfo == null)
				return false;
			if(scriptInfo.informations == null)
				return false;
			var path = AssetDatabase.GUIDToAssetPath(scriptInfo.guid);
			if(string.IsNullOrEmpty(path))
				return false;
			var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
			if(asset is GameObject gameObject) {
				return CanHighlightNode(gameObject, scriptInfo.informations, line, column);
			}
			return false;
		}

		public static bool CanHighlightNode(GameObject graph, IEnumerable<ScriptInformation> informations, int line, int column = -1) {
			if(informations == null)
				return false;
			List<ScriptInformation> information = new List<ScriptInformation>();
			foreach(var info in informations) {
				if(info == null)
					continue;
				if(info.startLine <= line && info.endLine >= line) {
					information.Add(info);
				}
			}
			if(column > 0) {
				information.Sort((x, y) => {
					int result = CompareUtility.Compare(x.lineRange, y.lineRange);
					if(result == 0) {
						int xColumn = int.MaxValue;
						if(x.startColumn <= column && x.endColumn >= column) {
							xColumn = x.columnRange;
						}
						int yColumn = int.MaxValue;
						if(y.startColumn <= column && y.endColumn >= column) {
							yColumn = y.columnRange;
						}
						return CompareUtility.Compare(xColumn, yColumn);
					}
					return result;
				});
			} else {
				information.Sort((x, y) => {
					int result = CompareUtility.Compare(x.lineRange, y.lineRange);
					if(result == 0) {
						return CompareUtility.Compare(y.columnRange, x.columnRange);
					}
					return result;
				});
			}
			foreach(var info in information) {
				if(info != null) {
					if(int.TryParse(info.id, out var id)) {
						UnityEngine.Object obj;
						if(graph != null) {
							obj = uNodeEditorUtility.FindObjectByUniqueIdentifier(graph.transform, id);
							if(obj == null) {
								obj = EditorUtility.InstanceIDToObject(id);
								if(obj == null && int.TryParse(info.ghostID, out var gID)) {
									obj = EditorUtility.InstanceIDToObject(gID);
								}
							}
						} else {
							obj = EditorUtility.InstanceIDToObject(id);
						}
						if(obj is GameObject) {
							GameObject go = obj as GameObject;
							obj = go.GetComponent<NodeComponent>();
							if(obj == null) {
								obj = go.GetComponent<RootObject>();
							}
						} else if(obj == null) {
							obj = EditorUtility.InstanceIDToObject(id);
						}
						if(obj is NodeComponent) {
							return true;
						} else if(obj is RootObject) {
							return true;
						}
					} else if(info.id.StartsWith(CG.KEY_INFORMATION_VARIABLE)) {

					}
				}
			}
			return false;
		}
		#endregion

		void RefreshDimmedNode() {
			dimmedNode = new HashSet<NodeComponent>();
			//var nodes = new HashSet<NodeComponent>();
			//if(editorData.selectedGroup is ISuperNode) {
			//	ISuperNode superNode = editorData.selectedGroup as ISuperNode;
			//	foreach(var n in superNode.nestedFlowNodes) {
			//		if(n == null)
			//			continue;
			//		if(!nodes.Contains(n))
			//			nodes.Add(n);
			//		NodeEditorUtility.FindConnectedNode(n, nodes);
			//	}
			//} else if(editorData.selectedRoot) {
			//	if(editorData.selectedRoot.startNode) {
			//		if(!nodes.Contains(editorData.selectedRoot.startNode))
			//			nodes.Add(editorData.selectedRoot.startNode);
			//		NodeEditorUtility.FindConnectedNode(editorData.selectedRoot.startNode, nodes);
			//	}
			//} else if(graphEditor.eventNodes != null) {
			//	foreach(var method in graphEditor.eventNodes) {
			//		if(method == null)
			//			continue;
			//		NodeEditorUtility.FindConnectedNode(method, nodes);
			//	}
			//}
			//foreach(var n in editorData.nodes) {
			//	if(!nodes.Contains(n)) {
			//		dimmedNode.Add(n);
			//	}
			//}
		}

		[Serializable]
		private class TempEditorData {
			public List<GraphData> editorsData;
			public GraphData mainEditorData;
			public int selectionIndex;
		}

		public void SaveEditorData() {
			//if(_isCompiling)
			//	return;
			//TempEditorData temp = new TempEditorData();
			//temp.editorsData = editorsData;
			//temp.mainEditorData = mainEditorData;
			//temp.selectionIndex = _selectedDataIndex;
			//uNodeEditorUtility.SaveEditorData(temp, "EditorTabData", out tempObjects);
			try {
				if(_isCompiling)
					return;
				TempEditorData temp = new TempEditorData();
				temp.editorsData = graphs;
				temp.mainEditorData = mainGraph;
				temp.selectionIndex = _selectedDataIndex;
				string json = JsonUtility.ToJson(temp, false);
				if(_serializedJSON != json) {
					_serializedJSON = json;
					uNodeEditorUtility.SaveEditorData(_serializedJSON, "EditorTabData");
				}
			}
			catch { }
		}

		void LoadEditorData() {
			//if(tempObjects == null || tempObjects.Count == 0)
			//	return;
			//var loadedData = uNodeEditorUtility.LoadEditorData<TempEditorData>("EditorTabData", tempObjects);
			//if(loadedData != null) {
			//	editorsData = loadedData.editorsData;
			//	mainEditorData = loadedData.mainEditorData;
			//	_selectedDataIndex = loadedData.selectionIndex;
			//	ChangeEditorTarget(selectedData);
			//}
			//_isCompiling = false;
			var loadedData = JsonUtility.FromJson<TempEditorData>(!string.IsNullOrEmpty(_serializedJSON) ? _serializedJSON : uNodeEditorUtility.LoadEditorData<string>("EditorTabData"));
			if(loadedData != null) {
				graphs = loadedData.editorsData;
				mainGraph = loadedData.mainEditorData;
				_selectedDataIndex = loadedData.selectionIndex;
				if(!preferenceData.saveGraphPosition) {
					if(graphs != null) {
						for(int i = 0; i < graphs.Count; i++) {
							var g = graphs[i];
							if(g != null && g.data != null && g.owner != null) {
								foreach(var d in g.data) {
									d.ResetPositionData();
								}
							} else {
								graphs.RemoveAt(i);
								i--;
							}
						}
					}
					if(mainGraph != null && mainGraph.data != null) {
						foreach(var d in mainGraph.data) {
							d.ResetPositionData();
						}
					}
				}
				ChangeEditorTarget(selectedGraph);
			}
			_isCompiling = false;
		}

		int _selectedDataIndex;
		[SerializeField]
		string _serializedJSON;
		bool _useDebug, _isCompiling;
		void OnPlaymodeStateChanged(PlayModeStateChange state) {
			switch(state) {
				case PlayModeStateChange.EnteredPlayMode:
					LoadEditorData();
					GraphDebug.useDebug = _useDebug;
					Refresh();
					break;
				case PlayModeStateChange.EnteredEditMode:
					CustomInspector.ResetEditor();
					LoadEditorData();
					GraphDebug.useDebug = _useDebug;
					break;
				case PlayModeStateChange.ExitingEditMode:
				case PlayModeStateChange.ExitingPlayMode:
					_isCompiling = false;
					_useDebug = GraphDebug.useDebug;
					SaveEditorData();
					break;
			}
		}

		public void UpdatePosition() {
			graphEditor.MoveCanvas(editorData.GetPosition(editorData.currentCanvas));
		}

		private GameObject targetSelection;
		void UpdateMainSelection(GameObject gameObject) {
			if(selectedGraph == mainGraph && mainGraph.selectedData.graph != null && isLocked)
				return;
			if(gameObject != null && targetSelection != gameObject) {
				targetSelection = gameObject;
				uNodeComponentSystem root = null;
				if(gameObject.GetComponent<uNodeComponentSystem>() != null) {
					root = gameObject.GetComponent<uNodeComponentSystem>();
				} else if(gameObject.GetComponent<NodeComponent>() != null) {
					root = gameObject.GetComponent<NodeComponent>().owner;
				} else if(gameObject.GetComponent<RootObject>() != null) {
					root = gameObject.GetComponent<RootObject>().owner;
				} else {
					targetSelection = null;
				}
				if(root != null) {
					if(root is uNodeRoot && mainGraph.selectedData.graph != root) {
						mainGraph.selectedData = null;
					} else if(root is uNodeData && mainGraph.selectedData.graphData != root) {
						mainGraph.selectedData = null;
					}
					ChangeMainTarget(root);
				} else {
					mainGraph.selectedData = null;
				}
			}
		}

		void UpdateMainSelection(Component component) {
			if(selectedGraph == mainGraph && mainGraph.selectedData.graph != null && isLocked)
				return;
			if(component != null && targetSelection != component.gameObject) {
				targetSelection = component.gameObject;
				uNodeRoot root = null;
				if(component is uNodeRoot) {
					root = component as uNodeRoot;
				} else if(component is NodeComponent) {
					root = (component as NodeComponent).owner;
				} else if(component is RootObject) {
					root = (component as RootObject).owner;
				} else if(component.GetComponent<uNodeRoot>() != null) {
					root = component.GetComponent<uNodeRoot>();
				} else if(component.GetComponent<NodeComponent>() != null) {
					root = component.GetComponent<NodeComponent>().owner;
				} else if(component.GetComponent<uNodeData>() != null) {
					ChangeMainTarget(component.GetComponent<uNodeData>());
				} else if(component.GetComponent<RootObject>() != null) {
					root = component.GetComponent<RootObject>().owner;
				} else {
					targetSelection = null;
				}
				if(root != null) {
					ChangeMainTarget(root);
				}
			}
		}

		static void UndoRedoCallback() {
			if(window == null)
				return;
			window.Refresh(true);
			window.Repaint();
		}

		public static void ForceRepaint() {
			if(window != null) {
				window.Repaint();
				EditorApplication.RepaintHierarchyWindow();
				GUIChanged();
			}
		}
		#endregion
	}
}