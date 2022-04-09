//#define UseProfiler
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.IMGUI.Controls;
#if UNITY_5_5_OR_NEWER && UseProfiler
using UnityEngine.Profiling;
#endif

namespace MaxyGames.uNode.Editors {
	/// <summary>
	/// The main editor window for editing uNode.
	/// </summary>
	public partial class uNodeEditor : EditorWindow {
		#region Const
		public const string Key_Recent_Item = "unode_show_recent_item";
		private const string MESSAGE_PATCH_WARNING = "Patching should be done in 'Compatibility' generation mode or there will be visible/invisible errors, please use 'Compatibility' mode when trying to make live changes to compiled code.";
		#endregion

		#region Variable & Function
		/// <summary>
		/// The uNode editor instance
		/// </summary>
		public static uNodeEditor window;
		private static HashSet<NodeComponent> dimmedNode;

		#region Private Fields
		private Event currentEvent;

		private int limitMultiEdit = 10, dimRefreshTime, errorRefreshTime;
		private float oldCanvasWidth;
		[NonSerialized]
		private Rect canvasArea = new Rect(0, 57, 35000, 35000);
		private Vector2 scrollPos2,
			clickPos;
		private Rect inspectorRect;
		private GraphExplorerTree explorerTree;
		private SearchField explorerSearch;
		private IMGUIContainer mainGUI;
		#endregion

		#region Properties
		/// <summary>
		/// The editor data
		/// </summary>
		public GraphEditorData editorData {
			get {
				return selectedGraph.selectedData;
			}
		}

		/// <summary>
		/// The graph editor.
		/// </summary>
		public NodeGraph graphEditor {
			get {
				return uNodePreference.nodeGraph;
			}
		}

		public static Dictionary<UnityEngine.Object, List<uNodeUtility.ErrorMessage>> editorErrors {
			get {
				return uNodeUtility.editorErrorMap;
			}
		}

		public bool isZoom {
			get {
				return graphEditor.zoomScale != 1;
			}
		}
		
		/// <summary>
		/// The debug object.
		/// </summary>
		public object debugObject {
			get {
				return editorData.debugTarget;
			}
			set {
				editorData.debugTarget = value;
			}
		}

		static uNodePreference.PreferenceData _preferenceData;
		/// <summary>
		/// The preference data.
		/// </summary>
		public static uNodePreference.PreferenceData preferenceData {
			get {
				if(_preferenceData != uNodePreference.GetPreference()) {
					_preferenceData = uNodePreference.GetPreference();
				}
				return _preferenceData;
			}
		}
		/// <summary>
		/// Are the main selection is locked?
		/// </summary>
		public static bool isLocked {
			get {
				return preferenceData.isLocked;
			}
			set {
				if(preferenceData.isLocked != value) {
					preferenceData.isLocked = value;
					uNodePreference.SavePreference();
				}
			}
		}
		/// <summary>
		/// Are the node is dimmed?
		/// </summary>
		public static bool isDim {
			get {
				return preferenceData.isDim;
			}
			set {
				if(preferenceData.isDim != value) {
					preferenceData.isDim = value;
					uNodePreference.SavePreference();
				}
			}
		}

		
		private EditorInteraction interaction { get; set; }

		private bool haveInteraction {
			get {
				return interaction != null;
			}
		}

		/// <summary>
		/// The height of the status bar.
		/// </summary>
		public float statusBarHeight {
			get {
				if(preferenceData.showStatusBar) {
					return 18;
				}
				return 0;
			}
		}
		#endregion

		#region EditorData
		[SerializeField]
		public GraphData mainGraph = new GraphData();
		[SerializeField]
		public List<GraphData> graphs = new List<GraphData>();

		public GraphData selectedGraph {
			get {
				if(_selectedDataIndex > 0 && graphs.Count >= _selectedDataIndex) {
					return graphs[_selectedDataIndex - 1];
				} else {
					_selectedDataIndex = 0;
				}
				return mainGraph;
			}
			set {
				if(value != null && value != mainGraph && graphs.Contains(value)) {
					_selectedDataIndex = graphs.IndexOf(value) + 1;
				} else {
					_selectedDataIndex = 0;
				}
			}
		}
		#endregion

		/// <summary>
		/// An event to be called on GUIChanged.
		/// </summary>
		public static event Action onChanged;
		/// <summary>
		/// An event to be called on Selection is changed.
		/// </summary>
		public static event Action<GraphEditorData> onSelectionChanged;

		static uNodeEditor() {
			Undo.undoRedoPerformed -= UndoRedoCallback;
			Undo.undoRedoPerformed += UndoRedoCallback;
		}

		/// <summary>
		/// Show the uNodeEditor.
		/// </summary>
		[MenuItem("Tools/uNode/uNode Editor", false, 0)]
		public static void ShowWindow() {
			window = (uNodeEditor)GetWindow(typeof(uNodeEditor), false);
			window.minSize = new Vector2(300, 250);
			window.autoRepaintOnSceneChange = true;
			window.wantsMouseMove = true;
			window.titleContent = new GUIContent("uNode Editor"/*, Resources.Load<Texture2D>("uNODE_Logo")*/);
			window.Show();
		}

		#endregion

		#region UnityEvent
		private GameObject oldTarget = null;
		void Update() {
			if(!EditorApplication.isPaused) {
				GraphDebug.debugLinesTimer = Mathf.Repeat(GraphDebug.debugLinesTimer += 0.03f, 1f);
			}
			if(Selection.activeGameObject != null && (oldTarget != Selection.activeGameObject)) {
				OnSelectionChange();
				oldTarget = Selection.activeGameObject;
			}
			if(preferenceData.enableErrorCheck) {
				int nowSecond = System.DateTime.Now.Second;
				//if(nowSecond % 2 != 0 && nowSecond != errorRefreshTime)
				if(nowSecond != errorRefreshTime) {
					CheckErrors();
					errorRefreshTime = nowSecond;
					Repaint();
				}
			}
			if(selectedGraph != null && selectedGraph.graph != null) {
				if(selectedGraph.owner != null) {
					//
				}
			}
			InitGraph();
		}

		private bool? prevShowGraph;
		void OnGUI() {
			if(Event.current.type != EventType.Repaint && Event.current.type != EventType.Repaint) {
				prevShowGraph = editorData.graphData || editorData.owner;
			}
			if(prevShowGraph == null) return;
			bool showGraph = prevShowGraph.Value;
			window = this;
			graphEditor.window = this;
			GUI.color = Color.white;

			#region Validation
			try {
				if(!showGraph && mainGraph == selectedGraph) {
					if(mainGUI == null || mainGUI.parent == null) {
						if(explorerTree == null) {
							explorerTree = new GraphExplorerTree();
						}
						if(explorerSearch == null) {
							explorerSearch = new SearchField();
						}
						mainGUI = new IMGUIContainer(() => {
							var areaRect = new Rect(0, 0, mainGUI.layout.width, mainGUI.layout.height);
							var explorerRect = areaRect;
							if(explorerRect.width > 400) {
								var rect = new Rect(400, 0, explorerRect.width - 400, explorerRect.height);
								explorerRect.width = 400;
								if(rect.width > 300) {
									explorerRect.width += rect.width - 300;
									rect.x += rect.width - 300;
									rect.width = 300;
								}
								GUILayout.BeginArea(rect);
								if(GUILayout.Button(new GUIContent("New Graph"))) {
									GraphCreatorWindow.ShowWindow();
									Event.current.Use();
								}
								if(GUILayout.Button(new GUIContent("Open Graph"))) {
									OpenNewGraphTab();
								}
								if(Selection.activeGameObject == null) {
									EditorGUILayout.HelpBox("Double Click a uNode Graph Assets to edit the graph or click 'New Graph' to create a new graph.\n" +
										"Or select a GameObject in hierarchy to create a new Scene Graph", MessageType.Info);
								} else {
									if(uNodeEditorUtility.IsPrefab(Selection.activeGameObject)) {
										var comp = Selection.activeGameObject.GetComponent<uNodeComponentSystem>();
										if(comp is uNodeRuntime) {
											EditorGUILayout.HelpBox(string.Format("To edit graph or create a new uNode graph with \'{0}\', please open the prefab", Selection.activeGameObject.name), MessageType.Info);
										} else {
											if(GUILayout.Button(new GUIContent("Edit Graph"))) {
												if(comp is uNodeRoot) {
													Open(comp as uNodeRoot);
												} else if(comp is uNodeData) {
													Open(comp as uNodeData);
												}
												Event.current.Use();
											}
											EditorGUILayout.HelpBox(string.Format("To edit \'{0}\' graph, please edit graph in new Tab", Selection.activeGameObject.name), MessageType.Info);
										}
									} else {
										if(GUILayout.Button(new GUIContent("Create from Selection"))) {
											ShowAddNewRootMenu(Selection.activeGameObject, (root) => {
												Open(root);
												Refresh();
											});
											Event.current.Use();
										}
										EditorGUILayout.HelpBox(string.Format("To begin a new uNode graph with \'{0}\', create a uNode component", Selection.activeGameObject.name), MessageType.Info);
									}
								}
								GUILayout.EndArea();
							}
							var search = explorerSearch.OnGUI(new Rect(0, 0, explorerRect.width, 16), explorerTree.searchString);
							if(search != explorerTree.searchString) {
								explorerTree.searchString = search;
								explorerTree.Reload();
							}
							explorerRect.height -= 16;
							explorerRect.y += 16;
							explorerTree.OnGUI(explorerRect);
						});
						mainGUI.style.flexGrow = 1;
						rootVisualElement.Add(mainGUI);
					}
				} else {
					mainGUI?.RemoveFromHierarchy();
				}
			}
			catch {
				if(selectedGraph == mainGraph) {
					//selectedData.data = new EditorData();
				} else {
					graphs.Remove(selectedGraph);
					ChangeEditorTarget(null);
				}
				ChangeEditorTarget(mainGraph);
			}
			#endregion

			DrawToolbar();

			#region Init
			bool displayInspectorPanel = SavedData.rightVisibility;
			canvasArea.position = graphEditor.canvasPosition;
			Rect areaZoom = canvasArea;
			areaZoom.x += SavedData.leftPanelWidth;
			Rect canvasRect = new Rect(SavedData.leftPanelWidth, areaZoom.y, position.width - SavedData.leftPanelWidth, position.height - areaZoom.y - statusBarHeight);
			inspectorRect = Rect.zero;
			if(displayInspectorPanel) {
				inspectorRect = new Rect((canvasRect.x + canvasRect.width) - SavedData.rightPanelWidth, canvasRect.y, SavedData.rightPanelWidth, canvasRect.height);
				canvasRect.width -= SavedData.rightPanelWidth;
			}
			#endregion

			#region Tabbar
			graphEditor.DrawTabbar(new Vector2(showGraph ? SavedData.leftPanelWidth : 0, 20));
			#endregion

			#region StatusBar
			if(showGraph && statusBarHeight > 0) {//Status bar
				DrawStatusBar(new Rect(
					showGraph ? areaZoom.x : 0,
					position.height - statusBarHeight,
					(showGraph ? 0 : areaZoom.x) + canvasRect.width + inspectorRect.width,
					statusBarHeight));
			}
			#endregion

			#region DrawCanvas
			if(showGraph) {
				if(isDim && System.DateTime.Now.Second != dimRefreshTime) {
					RefreshDimmedNode();
					dimRefreshTime = System.DateTime.Now.Second;
				}
				Rect backgroundRect = new Rect(
					areaZoom.x, 
					areaZoom.y + 1, 
					position.width - SavedData.leftPanelWidth - (displayInspectorPanel ? SavedData.rightPanelWidth : 0), 
					position.height - areaZoom.y - statusBarHeight
				);
				if(editorData.graph != null) {
					TopEventHandler();
				}
				graphEditor.DrawCanvas(this, new Editors.GraphData() {
					editorData = editorData,
					canvasArea = areaZoom,
					canvasRect = canvasRect,
					backgroundRect = backgroundRect,
					dimmedNodes = dimmedNode,
					isDim = isDim,
					isDisableEdit = IsDisableEdit(),
				});
			} else {
				graphEditor.OnNoTarget();
			}
			#endregion

			if(haveInteraction &&
					(interaction.interactionKind == EditorInteraction.InteractionKind.ClickOrDrag ||
					interaction.interactionKind == EditorInteraction.InteractionKind.Click) &&
					(Event.current.type == EventType.MouseUp ||
					Event.current.type == EventType.Ignore)) {

				if(interaction.onClick != null) {
					interaction.onClick();
				}
				interaction = null;
			} else if(haveInteraction &&
				(interaction.interactionKind == EditorInteraction.InteractionKind.ClickOrDrag ||
				interaction.interactionKind == EditorInteraction.InteractionKind.Drag) &&
				Event.current.type == EventType.MouseDrag) {
				if(interaction.onDrag != null) {
					interaction.onDrag();
				}
				interaction.hasDragged = true;
			}
			if(haveInteraction && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore)) {
				interaction = null;
			}
			if(showGraph && SavedData.leftVisibility) {
				DrawLeftPanel(canvasRect);
			} else {
				graphEditor.HideGraphPanel();
			}
			if(showGraph && displayInspectorPanel) {
				DrawRightPanel(inspectorRect);
			}
			if(GUI.changed) {
				GUIChanged();
			}
		}

		public static void GUIChanged(object obj) {
			if(window != null && window.graphEditor != null) {
				window.graphEditor.GUIChanged(obj);
			}
			GUIChanged();
		}

		public static void GUIChanged() {
			if(window != null) {
				try {
					EditorUtility.SetDirty(window);
					if(window.selectedGraph != null && window.selectedGraph.owner != null)
						EditorUtility.SetDirty(window.selectedGraph.owner);
				}
				catch {
					if(window.selectedGraph != null) {
						window.selectedGraph.owner = null;
					}
				}
			}
			if(onChanged != null) {
				onChanged();
			}
		}

		[NonSerialized]
		NodeGraph initedGraph;
		void InitGraph() {
			if(initedGraph != graphEditor) {
				graphEditor.window = this;
				graphEditor.OnEnable();
				initedGraph = graphEditor;
			}
		}

		void OnEnable() {
			uNodeGUIUtility.onGUIChanged -= GUIChanged;
			uNodeGUIUtility.onGUIChanged += GUIChanged;
			EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;

			LoadEditorData();
			InitGraph();
			if(selectedGraph != null && selectedGraph.graph != null) {
				if(selectedGraph.owner != null) {
					//
				}
			}
			Refresh();
			if(!hasLoad) {
				LoadOptions();
				hasLoad = true;
			}
		}

		bool hasLoad = false;
		void OnDisable() {
			uNodeGUIUtility.onGUIChanged -= GUIChanged;
			EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
			SaveEditorData();
			if(!hasLoad) {
				LoadOptions();
				hasLoad = true;
			}
			SaveOptions();
			graphEditor.OnDisable();
			initedGraph = null;
		}

		void OnSelectionChange() {
			if(mainGraph.selectedData.graph != null && (isLocked || mainGraph.selectedData.graph.gameObject == Selection.activeGameObject))
				return;
			if(Selection.activeGameObject != null) {
				//var selected = Selection.activeObject;
				ChangeMainSelection(Selection.activeGameObject);
				//Selection.instanceIDs = new int[] { selected.GetInstanceID() };
			}
			if (selectedGraph == mainGraph) {
				GUIChanged();
			}
		}
		#endregion

		#region GUI
		#region Panels
		private void DrawLeftPanel(Rect ScrollRect) {
			Rect resizeLRect = new Rect(SavedData.leftPanelWidth - 3, ScrollRect.y, 5, ScrollRect.height);
			EditorGUIUtility.AddCursorRect(resizeLRect, MouseCursor.ResizeHorizontal);
			if(currentEvent.button == 0 && currentEvent.type == EventType.MouseDown && resizeLRect.Contains(currentEvent.mousePosition)) {
				if(!haveInteraction) {
					interaction = new EditorInteraction("ResizeLeftCanvas");
					clickPos = currentEvent.mousePosition;
					oldCanvasWidth = SavedData.leftPanelWidth;
					currentEvent.Use();
				}
			}
			if(interaction == "ResizeLeftCanvas") {
				SavedData.leftPanelWidth = oldCanvasWidth - (clickPos - currentEvent.mousePosition).x;
				if(SavedData.leftPanelWidth > 400) {
					SavedData.leftPanelWidth = 400;
				}
				if(SavedData.leftPanelWidth < 150) {
					SavedData.leftPanelWidth = 150;
				}
				SaveOptions();
				Repaint();
			}
			graphEditor.DrawGraphPanel(new Rect(0, 18, resizeLRect.x, 0));
		}

		private void DrawRightPanel(Rect areaRect) {
			Rect resizeRRect = new Rect(areaRect.x - 2, areaRect.y, 6, areaRect.height);
			EditorGUIUtility.AddCursorRect(resizeRRect, MouseCursor.ResizeHorizontal);
			if((currentEvent.button == 0 && currentEvent.type == EventType.MouseDown) && resizeRRect.Contains(currentEvent.mousePosition)) {
				if(!haveInteraction) {
					interaction = "ResizeRightCanvas";
					clickPos = currentEvent.mousePosition;
					oldCanvasWidth = SavedData.rightPanelWidth;
					//isDragging = true;
				}
			}
			GUILayout.BeginArea(areaRect, "", "Box");
			scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2);
#if UseProfiler
			Profiler.BeginSample("Draw Inspector");
#endif
			CustomInspector.ShowInspector(editorData, limitMultiEdit);
#if UseProfiler
			Profiler.EndSample();
#endif
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
			if(interaction == "ResizeRightCanvas") {
				SavedData.rightPanelWidth = oldCanvasWidth + (clickPos - currentEvent.mousePosition).x;
				if(SavedData.rightPanelWidth > 400) {
					SavedData.rightPanelWidth = 400;
				}
				if(SavedData.rightPanelWidth < 250) {
					SavedData.rightPanelWidth = 250;
				}
				SaveOptions();
				Repaint();
			}
		}
		#endregion

		public void ChangeEditorSelection(object value) {
			editorData.selected = value;
			if(value != editorData.selectedNodes)
				editorData.selectedNodes.Clear();
			if(value is NodeComponent) {
				editorData.selectedNodes.Add(value as NodeComponent);
				editorData.selected = editorData.selectedNodes;
			}
			EditorSelectionChanged();
		}

		CustomInspector inspectorWrapper;
		internal void EditorSelectionChanged() {
			if(onSelectionChanged != null) {
				onSelectionChanged(editorData);
			}
			if(editorData.selected == null || !uNodePreference.GetPreference().inspectorIntegration)
				return;
			inspectorWrapper = CustomInspector.Default;
			inspectorWrapper.editorData = editorData;
			if(Selection.activeObject == inspectorWrapper) {
				CustomInspector.GetEditor(inspectorWrapper).Repaint();
				//if(CustomInspector.Editors.TryGetValue(inspectorWrapper.GetInstanceID(), out var editor)) {
				//	editor.Repaint();
				//} else if(inspectorWrapper.editorData.selected != inspectorWrapper.editorData.selectedNodes) {
				//	RepaintInspector(typeof(CustomInspectorEditor));
				//}
			} else {
				Selection.instanceIDs = new int[] { inspectorWrapper.GetInstanceID() };
			}
		}

		static void RepaintInspector(System.Type t) {
			var editors = Resources.FindObjectsOfTypeAll<Editor>();
			for(int i = 0; i < editors.Length; i++) {
				if(editors[i].GetType() == t) {
					editors[i].Repaint();
					return;
				}
			}
		}

		public void ResetView() {
			UpdatePosition();
		}

		//#region Progress Bar
		//class ProgressBar {
		//	public string title;
		//	public string info;
		//	public float progress;

		//	public bool display => container != null;

		//	public IMGUIContainer container;
		//}
		//private ProgressBar progressBar = new ProgressBar();

		//public void DisplayProgressBar(string title, string info, float progress) {
		//	progressBar.title = title;
		//	progressBar.info = info;
		//	progressBar.progress = progress;
		//	if(!progressBar.display) {
		//		progressBar.container = new IMGUIContainer(() => {
		//			GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none);
		//			GUILayout.FlexibleSpace();
		//			EditorGUI.ProgressBar(uNodeGUIUtility.GetRect(), progressBar.progress, progressBar.info);
		//			GUILayout.FlexibleSpace();
		//		});
		//		rootVisualElement.Add(progressBar.container);
		//		progressBar.container.StretchToParentSize();
		//	} else {
		//		progressBar.container?.BringToFront();
		//	}
		//}

		//public void ClearProgressBar() {
		//	if(progressBar.container != null) {
		//		progressBar.container.RemoveFromHierarchy();
		//		progressBar.container = null;
		//	}
		//}
		//#endregion

		public void DrawToolbar() {
			currentEvent = Event.current;
			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(100));
			{
				Rect leftVisibility = GUILayoutUtility.GetRect(13.4f, 20);
				GUI.DrawTexture(leftVisibility, uNodeGUIStyle.GetVisiblilityTexture(SavedData.leftVisibility), ScaleMode.ScaleToFit);
				if(currentEvent.button == 0 && currentEvent.type == EventType.MouseUp && leftVisibility.Contains(currentEvent.mousePosition)) {
					SavedData.leftVisibility = !SavedData.leftVisibility;
					Repaint();
					currentEvent.Use();
				}
			}
			if(editorData.graph != null) {//Debug
				string names = "None";
				if(!GraphDebug.useDebug) {
					names = "Disable";
				} else if(debugObject != null) {
					if(debugObject is UnityEngine.Object) {
						if(debugObject as UnityEngine.Object == editorData.graph) {
							if(editorData.graph is uNodeRuntime) {
								names = "self";
							}
						} else if(debugObject as MonoBehaviour && (debugObject as MonoBehaviour).gameObject) {
							names = (debugObject as MonoBehaviour).gameObject.name;
						} else {
							names = debugObject.GetType().Name;
						}
					} else if(editorData.debugAnyScript) {
						names = "Any";
					} else {
						names = debugObject.GetType().Name;
					}
				} else if(editorData.debugAnyScript) {
					if(editorData.graph is uNodeRuntime runtime && runtime.originalGraph == null) {
						names = "self";
						if(runtime.runtimeBehaviour == null && Application.isPlaying) {
							debugObject = runtime;
						}
					} else {
						names = "Any";
					}
				}
				if(editorData.graph is uNodeRuntime && (editorData.graph as uNodeRuntime).originalGraph != null) {
					names = "@" + editorData.graph.DisplayName;
				}
				GUIContent content = new GUIContent("Debug : " + names, "");
				Vector2 size = ((GUIStyle)"Button").CalcSize(content);
				if(GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(size.x), GUILayout.Height(15))) {
					GenericMenu menu = new GenericMenu();
					menu.AddDisabledItem(new GUIContent("Script Debugger"), false);
					menu.AddSeparator("");
					if(editorData.graph is IIndependentGraph graph) {
						menu.AddItem(new GUIContent("Debug Mode " + (editorData.graph.graphData.debug ? " (Enabled) " : " (Disabled) ")), editorData.graph.graphData.debug, delegate () {
							editorData.graph.graphData.debug = !editorData.graph.graphData.debug;
						});
						if(editorData.graph.graphData.debug) {
							menu.AddItem(new GUIContent("Debug Value" + (editorData.graph.graphData.debugValueNode ? " (Enabled) " : " (Disabled) ")), editorData.graph.graphData.debugValueNode, delegate () {
								editorData.graph.graphData.debugValueNode = !editorData.graph.graphData.debugValueNode;
							});
						}
					} else {
						menu.AddItem(new GUIContent("Debug Mode " + (generatorSettings.debug ? " (Enabled) " : " (Disabled) ")), generatorSettings.debug, delegate () {
							generatorSettings.debug = !generatorSettings.debug;
						});
						if(generatorSettings.debug) {
							menu.AddItem(new GUIContent("Debug Value" + (generatorSettings.debugValueNode ? " (Enabled) " : " (Disabled) ")), generatorSettings.debugValueNode, delegate () {
								generatorSettings.debugValueNode = !generatorSettings.debugValueNode;
							});
						}
					}
					menu.AddSeparator("");
					menu.AddDisabledItem(new GUIContent("Debug References"), false);
					menu.AddSeparator("");
					menu.AddItem(new GUIContent("None"), false, delegate () {
						debugObject = null;
						editorData.debugSelf = false;
						editorData.debugAnyScript = false;
						GraphDebug.useDebug = true;
					});
					menu.AddItem(new GUIContent("Disable"), false, delegate () {
						GraphDebug.useDebug = false;
					});
					if(editorData.graph is uNodeRuntime) {
						menu.AddItem(new GUIContent("Self"), false, delegate () {
							debugObject = null;
							GraphDebug.useDebug = true;
							if(editorData.graph is uNodeRuntime && (editorData.graph as uNodeRuntime).originalGraph != null) {
								Open((editorData.graph as uNodeRuntime).originalGraph);
							}
						});
					} else {
						menu.AddItem(new GUIContent("Any Script Instance"), false, delegate () {
							debugObject = null;
							editorData.debugAnyScript = true;
							GraphDebug.useDebug = true;
						});
					}
					Type type = uNodeEditorUtility.GetFullScriptName(editorData.graph).ToType(false);
					if(type != null && type.IsSubclassOf(typeof(MonoBehaviour))) {
						UnityEngine.Object[] obj = FindObjectsOfType(type);
						if(obj.Length == 0) {
							List<UnityEngine.Object> objs = new List<UnityEngine.Object>();
							var assemblies = ReflectionUtils.GetRuntimeAssembly();
							for(int i=assemblies.Length-1;i>0;i--) {
								var t = assemblies[i].GetType(type.FullName, false);
								if(t != null) {
									objs.AddRange(FindObjectsOfType(t));
								}
							}
							obj = objs.ToArray();
						}
						for(int i = 0; i < obj.Length; i++) {
							if(obj[i] is MonoBehaviour) {
								menu.AddItem(new GUIContent("Object/" + i + "-" + type.PrettyName()), debugObject == obj[i] as object, delegate (object reference) {
									GraphDebug.useDebug = true;
									UnityEngine.Object o = reference as UnityEngine.Object;
									EditorGUIUtility.PingObject(o);
									debugObject = o;
								}, obj[i]);
							}
							if(i > 250) {
								break;
							}
						}
					}
					if(Application.isPlaying && editorData.graph != null) {
						if(GraphDebug.debugData.ContainsKey(uNodeUtility.GetObjectID(editorData.graph))) {
							Dictionary<object, GraphDebug.DebugData> debugMap = null;
							debugMap = GraphDebug.debugData[uNodeUtility.GetObjectID(editorData.graph)];
							if(debugMap.Count > 0) {
								int count = 0;
								foreach(KeyValuePair<object, GraphDebug.DebugData> pair in debugMap) {
									if(count > 250)
										break;
									if(pair.Key != null && pair.Key as uNodeRoot != editorData.graph.GetPersistenceObject()) {
										if(pair.Key is UnityEngine.Object && (pair.Key as UnityEngine.Object) == null)
											continue;
										menu.AddItem(new GUIContent("Script/" + count + "-" + pair.Key.GetType().PrettyName()), debugObject == pair.Key, delegate (object reference) {
											KeyValuePair<object, GraphDebug.DebugData> objPair = (KeyValuePair<object, GraphDebug.DebugData>)reference;
											UnityEngine.Object o = objPair.Key as UnityEngine.Object;
											if(o != null) {
												EditorGUIUtility.PingObject(o);
											}
											debugObject = objPair.Value;
											GraphDebug.useDebug = true;
										}, pair);
										count++;
									}
								}
							}
						}
						var objs = GameObject.FindObjectsOfType<uNodeRuntime>();
						int counts = 0;
						foreach(var obj in objs) {
							if(counts > 250)
								break;
							if(obj.originalGraph != null && obj.originalGraph.gameObject == selectedGraph.graph) {
								menu.AddItem(new GUIContent("Runtime/" + counts + "-" + obj.gameObject.name), false, (reference) => {
									uNodeRoot o = reference as uNodeRoot;
									if(o != null) {
										EditorGUIUtility.PingObject(o);
									}
									debugObject = null;
									GraphDebug.useDebug = true;
									ChangeMainTarget(o, null, true);
								}, obj);
								counts++;
							}
						}
					}
					menu.ShowAsContext();
				}
			}
			if(editorData.isGraphOpen && graphEditor != null) {
				if(GUILayout.Button(new GUIContent("Frame Graph", "Frame the graph\nHotkey: F"), EditorStyles.toolbarButton)) {
					graphEditor.FrameGraph();
				}
			}
			if(editorData.graph != null || editorData.graphData != null) {
				//Handle graph asset.
				if(selectedGraph != null && (selectedGraph.graph != null || Application.isPlaying && editorData.graph is uNodeRuntime runtime && runtime.originalGraph != null)) {
					if(GUILayout.Button(new GUIContent("Save"), EditorStyles.toolbarButton, GUILayout.Height(15))) {
						SaveCurrentGraph();
						if(preferenceData.generatorData.IsAutoGenerateOnSave) {
							GenerationUtility.GenerateCSharpScript();
						}
						if(Application.isPlaying) {
							EditorPrefs.SetBool("unode_graph_saved_in_playmode", true);
						}
					}
				}
			}
			GUILayout.FlexibleSpace();
			if(editorData.graph != null || editorData.graphData != null) {
				if(editorData.graph == null || editorData.graph is IClass) {
					if (editorData.graphSystem == null || editorData.graphSystem.allowPreviewScript) {
						if (GUILayout.Button(new GUIContent("Preview", "Preview C# Script\nHotkey: F9"), EditorStyles.toolbarButton, GUILayout.Width(55), GUILayout.Height(15))) {
							if (selectedGraph != null && selectedGraph.graph != null) {
								AutoSaveCurrentGraph();
							}
							PreviewSource();
							EditorUtility.ClearProgressBar();
						}
					}
					if (editorData.graphSystem == null || editorData.graphSystem.allowCompileToScript) {
						if(GUILayout.Button(new GUIContent("Compile", "Generate C# Script\nHotkey: F10 ( compile current graph )"), EditorStyles.toolbarDropDown, GUILayout.Width(60), GUILayout.Height(15))) {
							GenericMenu menu = new GenericMenu();
							if(Application.isPlaying && EditorBinding.patchType != null) {
								if(editorData.graph != null) {
									var type = TypeSerializer.Deserialize(uNodeEditorUtility.GetFullScriptName(editorData.graph), false);
									if(type != null) {
										menu.AddItem(new GUIContent("Patch Current Graph"), false, () => {
											if(preferenceData.generatorData.generationMode != GenerationKind.Compatibility) {
												if(EditorUtility.DisplayDialog(
													"Warning!",
													MESSAGE_PATCH_WARNING + $"\n\nDo you want to ignore and patch in '{preferenceData.generatorData.generationMode}' mode?",
													"Yes", "No")) {
													PatchScript(type);
													EditorUtility.ClearProgressBar();
												}
											} else {
												PatchScript(type);
												EditorUtility.ClearProgressBar();
											}
										});
									}
									//menu.AddItem(new GUIContent("Patch Project Graphs"), false, () => {
									//	GenerationUtility.CompileAndPatchProjectGraphs();
									//});
									menu.AddSeparator("");
								}
							}
							menu.AddSeparator("");
							if(Application.isPlaying) {
								menu.AddDisabledItem(new GUIContent("Compile Current Graph"), false);
								menu.AddDisabledItem(new GUIContent("Compile All C# Graph"), false);
								menu.AddSeparator("");
								menu.AddDisabledItem(new GUIContent("Compile Graphs (Project)"), false);
								menu.AddDisabledItem(new GUIContent("Compile Graphs (Project + Scenes)"), false);
							} else {
								menu.AddItem(new GUIContent("Compile Current Graph"), false, () => {
									GenerateSource();
									EditorUtility.ClearProgressBar();
								});
								menu.AddItem(new GUIContent("Compile All C# Graph"), false, () => {
									if(Application.isPlaying) {
										uNodeEditorUtility.DisplayErrorMessage("Cannot compile all graph on playmode");
										return;
									}
									AutoSaveCurrentGraph();
									GenerationUtility.GenerateNativeGraphInProject();
								});
								menu.AddSeparator("");
								menu.AddItem(new GUIContent("Compile Graphs (Project)"), false, () => {
									AutoSaveCurrentGraph();
									GenerationUtility.GenerateCSharpScript();
								});
								menu.AddItem(new GUIContent("Compile Graphs (Project + Scenes)"), false, () => {
									AutoSaveCurrentGraph();
									GenerationUtility.GenerateCSharpScriptIncludingSceneGraphs();
								});
							}
							menu.ShowAsContext();
							if (selectedGraph != null && selectedGraph.graph != null) {
								AutoSaveCurrentGraph();
							}
						}
					} else if(editorData.graphSystem.allowAutoCompile) {
						if(GUILayout.Button(new GUIContent("Compile", "Generate C# Script"), 
							preferenceData.generatorData.compilationMethod == CompilationMethod.Unity && !Application.isPlaying ? EditorStyles.toolbarDropDown : EditorStyles.toolbarButton, GUILayout.Width(60), GUILayout.Height(15))) {
							//if(Application.isPlaying) {
							//	uNodeEditorUtility.DisplayErrorMessage("Cannot generate project scripts on playmode");
							//	return;
							//}
							GenericMenu menu = new GenericMenu();
							if(Application.isPlaying) {
								if(editorData.graph != null && EditorBinding.patchType != null) {
									var type = TypeSerializer.Deserialize(uNodeEditorUtility.GetFullScriptName(editorData.graph), false);
									if(type != null) {
										menu.AddItem(new GUIContent("Patch Current Graph"), false, () => {
											if(preferenceData.generatorData.generationMode != GenerationKind.Compatibility) {
												if(EditorUtility.DisplayDialog(
													"Warning!",
													MESSAGE_PATCH_WARNING + $"\n\nDo you want to ignore and patch in '{preferenceData.generatorData.generationMode}' mode?",
													"Yes", "No")) {
													PatchScript(type);
													EditorUtility.ClearProgressBar();
												}
											} else {
												PatchScript(type);
												EditorUtility.ClearProgressBar();
											}
										});
									}
									menu.AddItem(new GUIContent("Patch Project Graphs"), false, () => {
										if(preferenceData.generatorData.generationMode != GenerationKind.Compatibility) {
											if(EditorUtility.DisplayDialog(
												"Warning!",
												MESSAGE_PATCH_WARNING + $"\n\nDo you want to ignore and patch in '{preferenceData.generatorData.generationMode}' mode?",
												"Yes", "No")) {
												GenerationUtility.CompileAndPatchProjectGraphs();
											}
										} else {
											GenerationUtility.CompileAndPatchProjectGraphs();
										}
									});
								}
								menu.AddDisabledItem(new GUIContent("Compile Graphs (Project)"), false);
								menu.AddDisabledItem(new GUIContent("Compile Graphs (Project + Scenes)"), false);
							} else {
								if(preferenceData.generatorData.compilationMethod == CompilationMethod.Unity) {
									menu.AddItem(new GUIContent("Compile Graphs (Project)"), false, () => {
										AutoSaveCurrentGraph();
										GenerationUtility.GenerateCSharpScript();
									});
									menu.AddItem(new GUIContent("Compile Graphs (Project + Scenes)"), false, () => {
										AutoSaveCurrentGraph();
										GenerationUtility.GenerateCSharpScriptIncludingSceneGraphs();
									});
								} else {
									AutoSaveCurrentGraph();
									GenerationUtility.GenerateCSharpScript();
									return;
								}
							}
							//var str = preferenceData.generatorData.compileScript ? "Enabled" : "Disabled";
							//menu.AddItem(new GUIContent($"Compile Options/Pre Compile Before Save ({str})"), false, () => {
							//	preferenceData.generatorData.compileScript = !preferenceData.generatorData.compileScript;
							//	uNodePreference.SavePreference();
							//});
							//menu.AddItem(new GUIContent("Compile Options/Compile for Runtime Graphs ( Class Component, Class Asset, etc )"), false, () => {

							//});
							//menu.AddItem(new GUIContent("Compile Options/Compile for C# Graphs ( C# Class, C# Struct, etc )"), false, () => {

							//});
							menu.ShowAsContext();
						}
					}
				}
				if(GUILayout.Button(new GUIContent("Select", "Select uNode GameObject"), EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(15))) {
					if(selectedGraph.graph != null) {
						EditorGUIUtility.PingObject(selectedGraph.graph);
					} else {
						EditorGUIUtility.PingObject(editorData.graph);
						Selection.instanceIDs = new int[] { editorData.graph.gameObject.GetInstanceID() };
					}
				}
			}
			//isDim = GUILayout.Toggle(isDim, new GUIContent("Dim"), EditorStyles.toolbarButton, GUILayout.Width(30), GUILayout.Height(15));
			if(selectedGraph == mainGraph) {
				isLocked = GUILayout.Toggle(isLocked, new GUIContent("Lock", "Keep this object selected."), EditorStyles.toolbarButton, GUILayout.Width(35), GUILayout.Height(15));
			}
			if(GUILayout.Button(new GUIContent("Refresh", "Refresh the graph.\nHotkey: F5"), EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(15))) {
				if(editorData.graph != null) {
					Refresh(true);
					CheckErrors();
				}
			}
			GUILayout.Space(5);
			SavedData.rightVisibility = GUILayout.Toggle(SavedData.rightVisibility, new GUIContent("Inspector", "View to edit selected node, method or transition"), EditorStyles.toolbarButton);
			if(GUILayout.Button("~", EditorStyles.toolbarButton)) {
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Preference Editor"), false, () => {
					ActionWindow.ShowWindow(() => {
						uNodePreference.PreferencesGUI();
					});
				});
				menu.AddItem(new GUIContent("Node Display/Default"), uNodePreference.preferenceData.displayKind == DisplayKind.Default, () => {
					uNodePreference.preferenceData.displayKind = DisplayKind.Default;
					uNodePreference.SavePreference();
					UGraphView.ClearCache();
				});
				menu.AddItem(new GUIContent("Node Display/Partial"), uNodePreference.preferenceData.displayKind == DisplayKind.Partial, () => {
					uNodePreference.preferenceData.displayKind = DisplayKind.Partial;
					uNodePreference.SavePreference();
					UGraphView.ClearCache();
				});
				menu.AddItem(new GUIContent("Node Display/Full"), uNodePreference.preferenceData.displayKind == DisplayKind.Full, () => {
					uNodePreference.preferenceData.displayKind = DisplayKind.Full;
					uNodePreference.SavePreference();
					UGraphView.ClearCache();
				});
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Code Generator Options"), false, () => {
					ActionWindow.ShowWindow(() => {
						ShowGenerateCSharpGUI();
					});
				});
				menu.AddItem(new GUIContent("Graph Explorer"), false, () => {
					ExplorerWindow.ShowWindow();
				});
				menu.AddItem(new GUIContent("Graph Hierarchy"), false, () => {
					GraphHierarchy.ShowWindow();
				});
				menu.AddItem(new GUIContent("Node Browser"), false, () => {
					NodeBrowserWindow.ShowWindow();
				});
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Import"), false, () => {
					ActionWindow.ShowWindow(() => {
						ShowImportGUI();
					});
				});
				menu.AddItem(new GUIContent("Export"), false, () => {
					ActionWindow.ShowWindow(() => {
						ShowExportGUI();
					});
				});
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Fix missing members"), false, () => {
					RefactorWindow.Refactor(editorData.graph);
				});
				menu.AddItem(new GUIContent("Refresh All Graphs"), false, () => {
					UGraphView.ClearCache();
				});
				menu.AddItem(new GUIContent("Check All Graph Errors"), false, () => {
					GraphUtility.CheckGraphErrors();
				});
				menu.ShowAsContext();
			}
			GUILayout.EndHorizontal();
		}
		
		#region StatusBar
		[SerializeField]
		private string searchNode = "";
		private void DrawStatusBar(Rect rect) {
			currentEvent = Event.current;
			if(graphEditor.loadingProgress > 0) {
				EditorGUI.ProgressBar(rect, graphEditor.loadingProgress, "Loading...");
				return;
			}
			GUILayout.BeginArea(rect, EditorStyles.toolbar);
			GUILayout.BeginHorizontal();
			//if(preferenceData.enableErrorCheck) 
			{
				int errorCount = ErrorsCount();
				if(GUILayout.Button(new GUIContent(errorCount + " errors", errorCount > 0 ? uNodeGUIStyle.errorIcon : null), EditorStyles.toolbarButton)) {
					ErrorCheckWindow.Init();
				}
			}
			bool enableSnapping = preferenceData.enableSnapping && (preferenceData.graphSnapping || preferenceData.gridSnapping || preferenceData.spacingSnapping || preferenceData.nodePortSnapping);
			if(GUILayout.Toggle(enableSnapping, new GUIContent("Snap", "Snap the node to the port or grid"), EditorStyles.toolbarButton) != enableSnapping) {
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Enable Snapping"), preferenceData.enableSnapping, () => {
					preferenceData.enableSnapping = !preferenceData.enableSnapping;
					uNodePreference.SavePreference();
				});
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Graph Snapping"), preferenceData.graphSnapping, () => {
					preferenceData.graphSnapping = !preferenceData.graphSnapping;
					uNodePreference.SavePreference();
				});
				menu.AddItem(new GUIContent("Node Port Snapping"), preferenceData.nodePortSnapping, () => {
					preferenceData.nodePortSnapping = !preferenceData.nodePortSnapping;
					uNodePreference.SavePreference();
				});
				menu.AddItem(new GUIContent("Grid Snapping"), preferenceData.gridSnapping, () => {
					preferenceData.gridSnapping = !preferenceData.gridSnapping;
					uNodePreference.SavePreference();
				});
				menu.AddItem(new GUIContent("Spacing Snapping"), preferenceData.spacingSnapping, () => {
					preferenceData.spacingSnapping = !preferenceData.spacingSnapping;
					uNodePreference.SavePreference();
				});
				menu.ShowAsContext();
			}
			bool carry = GUILayout.Toggle(preferenceData.carryNodes, new GUIContent("Carry", "Carry the connected node when moving (CTRL)"), EditorStyles.toolbarButton);
			if(carry != preferenceData.carryNodes) {
				preferenceData.carryNodes = carry;
				uNodePreference.SavePreference();
			}
			GUILayout.FlexibleSpace();
			GUILayout.Label("Zoom : " + graphEditor.zoomScale.ToString("F2"));
			string search = uNodeGUIUtility.DrawSearchBar(searchNode, GUIContent.none, "uNodeSearchBar", GUILayout.MaxWidth(150));
			if(!search.Equals(searchNode) || currentEvent.isKey && currentEvent.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "uNodeSearchBar") {
				searchNode = search;
				graphEditor.OnSearchChanged(searchNode);
			}
			EditorGUI.BeginDisabledGroup(searchNode == null || string.IsNullOrEmpty(searchNode.Trim()));
			if(GUILayout.Button("Prev", EditorStyles.toolbarButton)) {
				graphEditor.OnSearchPrev();
			}
			if(GUILayout.Button("Next", EditorStyles.toolbarButton)) {
				graphEditor.OnSearchNext();
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		private int ErrorsCount() {
			if(editorErrors != null) {
				int count = 0;
				foreach(var pair in editorErrors) {
					if(pair.Key != null && pair.Value != null) {
						if(ErrorCheckWindow.onlySelectedUNode) {
							var obj = pair.Key;
							if(obj as NodeComponent) {
								var owner = (obj as NodeComponent).owner;
								if(owner?.gameObject == editorData.graph?.gameObject) {
									count += pair.Value.Count;
								}
							} else if(obj as uNodeRoot) {
								if((obj as uNodeRoot).gameObject == editorData.graph?.gameObject) {
									count += pair.Value.Count;
								}
							}
						} else {
							count += pair.Value.Count;
						}
					}
				}
				return count;
			}
			return 0;
		}
		#endregion
		#endregion

		#region Tools
		public class ToolSettings {
			public GlobalVariable target_GlobalVariable;
			public uNodeRoot target_uNodeObject;
			public GameObject target_GameObject;
			public int i_oType = 0;
			public int exportTo = 0;
			public bool toChild;
			public bool includeOtherComponent;
			public bool overwrite = true;
		}
		private ToolSettings toolSetting = new ToolSettings();
		private uNodeData.GeneratorSettings generatorSettings {
			get {
				if(editorData.graph) {
					var obj = editorData.owner.GetComponent<uNodeData>();
					if(obj) {
						return obj.generatorSettings;
					} else if(!IsDisableEdit() && !(editorData.graph is IIndependentGraph)) {
						return editorData.owner.AddComponent<uNodeData>().generatorSettings;
					}
				} else if(editorData.graphData) {
					return editorData.graphData.generatorSettings;
				}
				return null;
			}
		}

		public void PreviewSource() {
			//string nameSpace;
			//IList<string> usingNamespace;
			//bool debug, debugValue;
			//if(generatorSettings != null) {
			//	nameSpace = generatorSettings.Namespace;
			//	usingNamespace = generatorSettings.usingNamespace;
			//	debug = generatorSettings.debug;
			//	debugValue = generatorSettings.debugValueNode;
			//} else if(editorData.graph is IIndependentGraph graph) {
			//	nameSpace = graph.Namespace;
			//	usingNamespace = graph.UsingNamespaces;
			//	debug = editorData.graph.graphData.debug;
			//	debugValue = editorData.graph.graphData.debugValueNode;
			//} else {
			//	throw new InvalidOperationException();
			//}
			Directory.CreateDirectory(GenerationUtility.tempFolder);
			char separator = Path.DirectorySeparatorChar;
			try {
				System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
				watch.Start();
				//var script = CodeGenerator.Generate(new CodeGenerator.GeneratorSetting(editorData.graphs, nameSpace, usingNamespace) {
				//	forceGenerateAllNode = preferenceData.generatorData.forceGenerateAllNode,
				//	resolveUnityObject = preferenceData.generatorData.resolveUnityObject,
				//	fullTypeName = preferenceData.generatorData.fullTypeName,
				//	fullComment = preferenceData.generatorData.fullComment,
				//	enableOptimization = preferenceData.generatorData.enableOptimization,
				//	generateTwice = preferenceData.generatorData.generateNodeTwice,
				//	//debugScript = true,
				//	//debugValueNode = true,
				//	debugScript = debug,
				//	debugValueNode = debugValue,
				//	debugPreprocessor = false,
				//	includeGraphInformation = true,
				//	targetData = editorData.graphData,
				//	generationMode = preferenceData.generatorData.generationMode,
				//	updateProgress = (progress, text) => {
				//		EditorUtility.DisplayProgressBar("Loading", text, progress);
				//	},
				//});
				var script = GenerationUtility.GenerateCSharpScript(editorData.graph != null ? new uNodeRoot[] { editorData.graph } : new uNodeRoot[0], editorData.graphData, (progress, text) => {
					EditorUtility.DisplayProgressBar($"Generating C# Scripts", text, progress);
				});
				var generatedScript = script.ToScript(out var informations);
				string path = GenerationUtility.tempFolder + separator + script.fileName + ".cs";
				using(StreamWriter sw = new StreamWriter(path)) {
					sw.Write(generatedScript);
					sw.Close();
				}
				watch.Stop();
				string originalScript = generatedScript;
				EditorUtility.DisplayProgressBar($"Generating C# Scripts", "Analizing Generated C# Script", 1);
				//Debug.LogFormat("Generating C# took {0,8:N3} s.", watch.Elapsed.TotalSeconds);
				if(preferenceData.generatorData != null && preferenceData.generatorData.analyzeScript && preferenceData.generatorData.formatScript) {
					var codeFormatter = TypeSerializer.Deserialize("MaxyGames.uNode.Editors.CSharpFormatter", false);
					if(codeFormatter != null) {
						var str = codeFormatter.
							GetMethod("FormatCode").
							Invoke(null, new object[] { originalScript }) as string;
						originalScript = str;
						generatedScript = str;
					}
				}
				var syntaxHighlighter = TypeSerializer.Deserialize("MaxyGames.SyntaxHighlighter.CSharpSyntaxHighlighter", false);
				if(syntaxHighlighter != null) {
					string highlight = syntaxHighlighter.GetMethod("GetRichText").Invoke(null, new object[] { generatedScript }) as string;
					if(!string.IsNullOrEmpty(highlight)) {
						generatedScript = highlight;
					}
				}
				PreviewSourceWindow.ShowWindow(generatedScript, originalScript).informations = informations?.ToArray();
				EditorUtility.ClearProgressBar();
#if UNODE_DEBUG
				uNodeEditorUtility.CopyToClipboard(script.ToRawScript());
#endif
			}
			catch (Exception ex) {
				EditorUtility.ClearProgressBar();
				Debug.LogError("Aborting Generating C# Script because of errors.\nErrors: " + ex.ToString());
				throw ex;
			}
		}

		private void PatchScript(Type scriptType) {
			if(editorData.graph == null || EditorBinding.patchType == null)
				return;
			try {
				System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
				watch.Start();
				var script = GenerationUtility.GenerateCSharpScript(editorData.graph != null ? new uNodeRoot[] { editorData.graph } : new uNodeRoot[0], editorData.graphData, (progress, text) => {
					EditorUtility.DisplayProgressBar($"Generating C# Scripts", text, progress);
				});
				//var script = CodeGenerator.Generate(new CodeGenerator.GeneratorSetting(new uNodeRoot[] { editorData.graph }, nameSpace, usingNamespace) {
				//	forceGenerateAllNode = preferenceData.generatorData.forceGenerateAllNode,
				//	resolveUnityObject = preferenceData.generatorData.resolveUnityObject,
				//	enableOptimization = preferenceData.generatorData.enableOptimization,
				//	fullTypeName = true,
				//	fullComment = false,
				//	generateTwice = preferenceData.generatorData.generateNodeTwice,
				//	debugScript = true, //Enable debug on patching script.
				//	debugValueNode = true, //Enable debug on patching script.
				//	debugPreprocessor = false, //Prevent debug preprocessor to be included in generated code.
				//	includeGraphInformation = false, //Don't include graph information as we don't need it.
				//	targetData = editorData.graphData,
				//	generationMode = preferenceData.generatorData.generationMode,
				//	updateProgress = (progress, text) => {
				//		EditorUtility.DisplayProgressBar("Loading", text, progress);
				//	},
				//});
				var dir = "TempScript" + Path.DirectorySeparatorChar + "Patched";
				Directory.CreateDirectory(dir);
				var path = Path.GetFullPath(dir) + Path.DirectorySeparatorChar + script.fileName + ".cs";
				using(StreamWriter sw = new StreamWriter(path)) {
					var generatedScript = script.ToScript(out var informations, true);
					SavedData.UnregisterGraphInfo(script.graphOwner);
					if(informations != null) {
						SavedData.RegisterGraphInfos(informations, script.graphOwner, path);
					}
					sw.Write(generatedScript);
					sw.Close();
				}
				var db = GenerationUtility.GetDatabase();
				foreach(var root in script.graphs) {
					if(db.graphDatabases.Any(g => g.graph == root)) {
						continue;
					}
					db.graphDatabases.Add(new uNodeResourceDatabase.RuntimeGraphDatabase() {
						graph = root,
					});
					EditorUtility.SetDirty(db);
				}
				//if(generatorSettings.convertLineEnding) {
				//	generatedScript = ConvertLineEnding(generatedScript,
				//		Application.platform != RuntimePlatform.WindowsEditor);
				//}
				EditorUtility.DisplayProgressBar("Compiling", "", 1);
				var assembly = GenerationUtility.CompileFromFile(path);
				if (assembly != null) {
					string typeName;
					if(string.IsNullOrEmpty(script.Namespace)) {
						typeName = script.classNames.First().Value;
					} else {
						typeName = script.Namespace + "." + script.classNames.First().Value;
					}
					var type = assembly.GetType(typeName);
					if (type != null) {
						EditorUtility.DisplayProgressBar("Patching", "Patch generated c# into existing script.", 1);
						EditorBinding.patchType(scriptType, type);
						//ReflectionUtils.RegisterRuntimeAssembly(assembly);
						//ReflectionUtils.UpdateAssemblies();
						//ReflectionUtils.GetAssemblies();
						watch.Stop();
						Debug.LogFormat("Generating & Patching type: {1} took {0,8:N3} s.", watch.Elapsed.TotalSeconds, scriptType);
					} else {
						watch.Stop();
						Debug.LogError($"Error on patching script because type: {typeName} is not found.");
					}
				}
				EditorUtility.ClearProgressBar();
			}
			catch {
				EditorUtility.ClearProgressBar();
				Debug.LogError("Aborting Generating C# Script because have error.");
				throw;
			}
		}

		public void GenerateSource() {
			GenerationUtility.CompileNativeGraph(editorData.owner);
		}

		private void ShowGenerateCSharpGUI(){
			if(!(editorData.graph is IIndependentGraph) && (generatorSettings == null || editorData.graph == null)) {
				return;
			}
			if(editorData.graph is IIndependentGraph graph) {
				VariableEditorUtility.DrawNamespace("Using Namespaces", graph.UsingNamespaces, editorData.graphData, (arr) => {
					graph.UsingNamespaces = arr as List<string> ?? arr.ToList();
					uNodeEditorUtility.MarkDirty(editorData.graphData);
				});
				uNodeGUIUtility.ShowField("debug", editorData.graph.graphData, null);
				if(editorData.graph.graphData.debug)
					uNodeGUIUtility.ShowField("debugValueNode", editorData.graph.graphData, null);
			} else {
				EditorGUI.BeginChangeCheck();
				uNodeGUIUtility.ShowField("Namespace", generatorSettings, null);
				VariableEditorUtility.DrawNamespace("Using Namespaces", generatorSettings.usingNamespace.ToList(), editorData.graphData, (arr) => {
					generatorSettings.usingNamespace = arr.ToArray();
					uNodeEditorUtility.MarkDirty(editorData.graphData);
				});
				uNodeGUIUtility.ShowField("debug", generatorSettings, null);
				if(generatorSettings.debug)
					uNodeGUIUtility.ShowField("debugValueNode", generatorSettings, null);
				if(EditorGUI.EndChangeCheck()) {
					SaveOptions();
				}
			}
			if (editorData.graphSystem == null || editorData.graphSystem.allowCompileToScript) {
				if (GUILayout.Button(new GUIContent("Generate"))) {
					GenerateSource();
				}
			}
		}

		private void ShowImportGUI(){
			toolSetting.i_oType = EditorGUILayout.Popup("Import Type", toolSetting.i_oType, new string[] {
				"Variable and Node",
				"Node",
				"Variable"});
			if(toolSetting.i_oType != 1) {
				toolSetting.exportTo = EditorGUILayout.Popup("Import From", toolSetting.exportTo, new string[] {
				"uNodeRoot",
				"Global Variable"});
				if(toolSetting.exportTo == 0) {
					toolSetting.target_uNodeObject = EditorGUILayout.ObjectField(new GUIContent("Target uNode"), toolSetting.target_uNodeObject, typeof(uNodeRoot), true) as uNodeRoot;
					toolSetting.overwrite = EditorGUILayout.Toggle(new GUIContent("Overwrite"), toolSetting.overwrite);
				} else {
					toolSetting.target_GlobalVariable = EditorGUILayout.ObjectField(new GUIContent("Target Global Variable"), toolSetting.target_GlobalVariable, typeof(GlobalVariable), true) as GlobalVariable;
				}
			} else {
				toolSetting.target_uNodeObject = EditorGUILayout.ObjectField(new GUIContent("Target uNode"), toolSetting.target_uNodeObject, typeof(uNodeRoot), true) as uNodeRoot;
			}
			if(GUILayout.Button(new GUIContent("Import"))) {
				if(toolSetting.exportTo == 0) {
					if(toolSetting.target_uNodeObject != null &&
						EditorUtility.DisplayDialog("Are you sure?", "Do you wish to import it\nNote: this can't be undone", "Import", "Cancel")) {
						if(toolSetting.i_oType == 2) {
							if(toolSetting.overwrite) {
								editorData.graph.Variables.Clear();
							}
							foreach(VariableData variable in toolSetting.target_uNodeObject.Variables) {
								editorData.graph.Variables.Add(new VariableData(variable));
							}
							return;
						}
						GameObject go = Instantiate(NodeEditorUtility.GetNodeRoot(toolSetting.target_uNodeObject), Vector3.zero, Quaternion.identity) as GameObject;
						uNodeEditorUtility.UnlockPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(go));
						if(toolSetting.overwrite) {
							if(toolSetting.i_oType != 1)
								editorData.graph.Variables.Clear();
							if(toolSetting.i_oType != 2)
								DestroyImmediate(NodeEditorUtility.GetNodeRoot(editorData.graph));
						}
						if(toolSetting.i_oType != 1) {
							foreach(VariableData variable in toolSetting.target_uNodeObject.Variables) {
								editorData.graph.Variables.Add(new VariableData(variable));
							}
						}
						if(toolSetting.i_oType != 2) {
							uNodeRoot root = editorData.graph;
							if(uNodeEditorUtility.IsPrefab(root)) {
								root = PrefabUtility.InstantiatePrefab(root) as uNodeRoot;
								uNodeEditorUtility.UnlockPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(go));
							}
							List<uNodeComponent> needReflectedComponent = new List<uNodeComponent>();
							for(int i = 0; i < go.transform.childCount; i++) {
								uNodeComponent comp = go.transform.GetChild(i).GetComponent<uNodeComponent>();
								if(comp != null) {
									if(comp is NodeComponent && root is IClass cls && 
										(cls.IsStruct || typeof(MonoBehaviour).IsAssignableFrom(cls.GetInheritType()))) {
										continue;
									}
									uNodeComponent[] comps = comp.GetComponentsInChildren<uNodeComponent>(true);
									for(int x = 0; x < comps.Length; x++) {
										if(NodeEditorUtility.NeedReflectComponent(comps[x], toolSetting.target_uNodeObject, root)) {
											needReflectedComponent.Add(comps[x]);
										}
									}
									uNodeEditorUtility.SetParent(comp.transform, NodeEditorUtility.GetNodeRoot(root).transform);
									i--;
								}
							}
							if(needReflectedComponent.Count > 0) {
								NodeEditorUtility.PerformReflectComponent(needReflectedComponent, toolSetting.target_uNodeObject, root);
							}
							if(uNodeEditorUtility.IsPrefab(editorData.graph)) {
								uNodeEditorUtility.SavePrefabAsset(root.transform.root.gameObject, editorData.graph.transform.root.gameObject);
								DestroyImmediate(root.transform.root.gameObject);
							}
						}
						DestroyImmediate(go);
						Refresh();
					} else if(toolSetting.target_uNodeObject == null) {
						Debug.LogError("Target uNode must exist");
					}
				} else if(toolSetting.exportTo == 1) {
					if(toolSetting.target_GlobalVariable != null &&
						EditorUtility.DisplayDialog("Are you sure?", "Do you wish to import it\nNote: this can't be undone", "Import", "Cancel")) {
						foreach(VariableData variable in toolSetting.target_GlobalVariable.variable) {
							editorData.graph.Variables.Add(new VariableData(variable));
						}
					} else if(toolSetting.target_GlobalVariable == null) {
						Debug.LogError("Target GlobalVariable must exist");
					}
				}
			}
		}

		private void ShowExportGUI() {
			toolSetting.i_oType = EditorGUILayout.Popup("Export Type", toolSetting.i_oType, new string[] {
				"Variable and Node",
				"Node",
				"Variable"});
			toolSetting.exportTo = EditorGUILayout.Popup("Export To", toolSetting.exportTo, new string[] {
				"Prefab",
				"Game Object",
				"Exist uNodeRoot",
				"Global Variable",
				"Asset"});
			if(toolSetting.exportTo == 1) {
				toolSetting.target_GameObject = EditorGUILayout.ObjectField(new GUIContent("Target GameObject"), toolSetting.target_GameObject, typeof(GameObject), true) as GameObject;
				toolSetting.toChild = EditorGUILayout.Toggle(new GUIContent("Export To Child"), toolSetting.toChild);
				if(toolSetting.toChild) {
					toolSetting.includeOtherComponent = EditorGUILayout.Toggle(new GUIContent("Include Other Component"), toolSetting.includeOtherComponent);
				}
			} else if(toolSetting.exportTo == 2) {
				toolSetting.target_uNodeObject = EditorGUILayout.ObjectField(new GUIContent("Target uNode"), toolSetting.target_uNodeObject, typeof(uNodeRoot), true) as uNodeRoot;
				toolSetting.overwrite = EditorGUILayout.Toggle(new GUIContent("Overwrite"), toolSetting.overwrite);
			}
			if(GUILayout.Button(new GUIContent("Export"))) {
				if(toolSetting.exportTo == 0) {//Export To Prefab
					string path = EditorUtility.SaveFilePanelInProject("Export uNode to Prefab",
						editorData.graph.gameObject.name + ".prefab",
						"prefab",
						"Please enter a file name to save the prefab to");
					if(path.Length != 0) {
						GameObject go = Instantiate(editorData.graph.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
						uNodeEditorUtility.UnlockPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(go));
						go.name = "uNode";
						go.transform.parent = null;
						foreach(Component comp in go.GetComponents<Component>()) {
							if(comp is Transform || comp is uNodeRoot)
								continue;
							DestroyImmediate(comp);
						}
						uNodeRoot UNR = go.GetComponent<uNodeRoot>();
						for(int i = 0; i < go.transform.childCount; i++) {
							Transform t = go.transform.GetChild(i);
							if(t.gameObject != NodeEditorUtility.GetNodeRoot(UNR)) {
								DestroyImmediate(t.gameObject);
								i--;
							}
						}
						if(toolSetting.i_oType == 1) {
							UNR.Variables.Clear();
						}
						if(toolSetting.i_oType == 2) {
							DestroyImmediate(NodeEditorUtility.GetNodeRoot(UNR));
							UNR.Refresh();
						}
						PrefabUtility.SaveAsPrefabAsset(go, path);
						DestroyImmediate(go);
					}
				} else if(toolSetting.exportTo == 1) {//Export To Game Object
					if(toolSetting.target_GameObject != null &&
						EditorUtility.DisplayDialog("Are you sure?", "Do you wish to export the uNode\nNote: this can't be undone", "Export", "Cancel")) {
						if(toolSetting.i_oType == 2) {
							uNodeRoot ES = toolSetting.target_GameObject.AddComponent<uNodeRoot>();
							foreach(VariableData variable in editorData.graph.Variables) {
								ES.Variables.Add(new VariableData(variable));
							}
							return;
						}
						if(!toolSetting.toChild) {
							GameObject go = Instantiate(NodeEditorUtility.GetNodeRoot(editorData.graph), Vector3.zero, Quaternion.identity) as GameObject;
							string s = go.name.Replace("(Clone)", "");
							go.name = s;
							uNodeRoot ES = NodeEditorUtility.CopyComponent(editorData.graph, toolSetting.target_GameObject) as uNodeRoot;
							if(toolSetting.i_oType != 1) {
								foreach(VariableData variable in editorData.graph.Variables) {
									ES.Variables.Add(new VariableData(variable));
								}
							}
							if(toolSetting.i_oType != 2) {
								ES.RootObject = go;
								List<uNodeComponent> needReflectedComponent = new List<uNodeComponent>();
								uNodeComponent[] comps = go.GetComponentsInChildren<uNodeComponent>(true);
								for(int i = 0; i < comps.Length; i++) {
									uNodeComponent comp = comps[i];
									if(comp != null) {
										if(NodeEditorUtility.NeedReflectComponent(comp, editorData.graph, ES)) {
											needReflectedComponent.Add(comp);
										}
									}
								}
								if(needReflectedComponent.Count > 0) {
									NodeEditorUtility.PerformReflectComponent(needReflectedComponent, editorData.graph, ES);
								}
								uNodeEditorUtility.SetParent(go.transform, toolSetting.target_GameObject.transform);
							}
							ES.Refresh();
						} else {
							GameObject go = Instantiate(editorData.graph.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
							string s = go.name.Replace("(Clone)", "");
							go.name = s;
							uNodeEditorUtility.UnlockPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(go));
							if(!toolSetting.includeOtherComponent) {
								foreach(Component comp in go.GetComponents<Component>()) {
									if(comp is Transform || comp is uNodeRoot)
										continue;
									DestroyImmediate(comp);
								}
							}
							uNodeRoot UNR = go.GetComponent<uNodeRoot>();
							for(int i = 0; i < go.transform.childCount; i++) {
								Transform t = go.transform.GetChild(i);
								if(t.gameObject != NodeEditorUtility.GetNodeRoot(UNR)) {
									DestroyImmediate(t.gameObject);
									i--;
								}
							}
							if(toolSetting.i_oType == 1) {
								UNR.Variables.Clear();
							}
							uNodeEditorUtility.SetParent(go.transform, toolSetting.target_GameObject.transform);
						}
					} else if(toolSetting.target_GameObject == null) {
						Debug.LogError("Target GameObject must exist");
					}
				} else if(toolSetting.exportTo == 2) {//Export To uNode
					if(toolSetting.target_uNodeObject != null &&
						EditorUtility.DisplayDialog("Are you sure?", "Do you wish to export the uNode\nNote: this can't be undone", "Export", "Cancel")) {
						if(toolSetting.i_oType == 2) {
							if(toolSetting.overwrite) {
								toolSetting.target_uNodeObject.Variables.Clear();
							}
							foreach(VariableData variable in editorData.graph.Variables) {
								toolSetting.target_uNodeObject.Variables.Add(new VariableData(variable));
							}
							return;
						}
						GameObject go = Instantiate(NodeEditorUtility.GetNodeRoot(editorData.graph), Vector3.zero, Quaternion.identity) as GameObject;
						string s = go.name.Replace("(Clone)", "");
						go.name = s;
						if(toolSetting.overwrite) {
							if(toolSetting.i_oType != 1)
								toolSetting.target_uNodeObject.Variables.Clear();
							if(toolSetting.i_oType != 2)
								DestroyImmediate(NodeEditorUtility.GetNodeRoot(toolSetting.target_uNodeObject));
						}
						if(toolSetting.i_oType != 1) {
							foreach(VariableData variable in editorData.graph.Variables) {
								toolSetting.target_uNodeObject.Variables.Add(new VariableData(variable));
							}
						}
						if(toolSetting.i_oType != 2) {
							uNodeRoot root = toolSetting.target_uNodeObject;
							if(uNodeEditorUtility.IsPrefab(root)) {
								root = PrefabUtility.InstantiatePrefab(root) as uNodeRoot;
								uNodeEditorUtility.UnlockPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(go));
							}
							List<uNodeComponent> needReflectedComponent = new List<uNodeComponent>();
							for(int i = 0; i < go.transform.childCount; i++) {
								uNodeComponent comp = go.transform.GetChild(i).GetComponent<uNodeComponent>();
								if(comp != null) {
									uNodeComponent[] comps = comp.GetComponentsInChildren<uNodeComponent>(true);
									for(int x = 0; x < comps.Length; x++) {
										if(NodeEditorUtility.NeedReflectComponent(comps[x], editorData.graph, root)) {
											needReflectedComponent.Add(comps[x]);
										}
									}
									comp.transform.parent = NodeEditorUtility.GetNodeRoot(root).transform;
									i--;
								}
							}
							if(needReflectedComponent.Count > 0) {
								NodeEditorUtility.PerformReflectComponent(needReflectedComponent, editorData.graph, root);
							}
							if(uNodeEditorUtility.IsPrefab(toolSetting.target_uNodeObject)) {
								uNodeEditorUtility.SavePrefabAsset(root.transform.root.gameObject, toolSetting.target_uNodeObject.transform.root.gameObject);
								DestroyImmediate(root.transform.root.gameObject);
							}
						}
						DestroyImmediate(go);
						toolSetting.target_uNodeObject.Refresh();
					} else if(toolSetting.target_uNodeObject == null) {
						Debug.LogError("Target uNode must exist");
					}
				} else if(toolSetting.exportTo == 3) {//Export To Global Variable
					string path = EditorUtility.SaveFilePanelInProject("Export uNode to GlobalVariable",
						editorData.graph.gameObject.name + ".asset",
						"asset",
						"Please enter a file name to save the variable to");
					if(path.Length != 0) {
						GlobalVariable asset = GlobalVariable.CreateInstance(typeof(GlobalVariable)) as GlobalVariable;
						foreach(VariableData variable in editorData.graph.Variables) {
							asset.variable.Add(new VariableData(variable));
						}
						AssetDatabase.CreateAsset(asset, path);
						AssetDatabase.SaveAssets();
					}
				}
			}
		}
		#endregion
	}
}