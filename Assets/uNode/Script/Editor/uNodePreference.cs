#pragma warning disable
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MaxyGames.uNode.Editors {
	public class uNodePreference {
		public enum DefaultAccessor {
			Public,
			Private,
		}

		public class PreferenceData {
			public bool isDim = true,
				isLocked = false,
				carryNodes = false;

			[Tooltip("The default access modifier for new variable")]
			public DefaultAccessor newVariableAccessor;
			[Tooltip("The default access modifier for new function")]
			public DefaultAccessor newFunctionAccessor;

			//Node snapping
			public bool enableSnapping = true;
			public bool graphSnapping = true;
			public bool nodePortSnapping = true;
			public float portSnappingRange = 10;
			public bool gridSnapping = false;
			public bool spacingSnapping = false;

			public DisplayKind displayKind;

			public bool showGrid = false,
				inEditorDocumentation = true,
				hideChildObject = true,
				inspectorIntegration = true,
				hideNodeIcons,
				hideNodeComment,
				forceReloadGraph;
			[SerializeField]
			private int m_maxReloadMilis;
			public int maxReloadMilis {
				get {
					if(m_maxReloadMilis <= 0)
						m_maxReloadMilis = 50;
					return m_maxReloadMilis;
				}
				set {
					m_maxReloadMilis = value;
				}
			}
			public Color defaultValueColor = new Color(1, 0.95f, 0.6f);

			//Browser
			public bool coloredItem = false;
			public Color itemTypeColor = new Color(0, 0.65f, 0);
			public Color itemKeywordColor = new Color(0.1f, 0.33f, 0.6f);
			public Color itemInterfaceColor = new Color(0.6f, 0.4f, 0.13f);
			public Color itemEnumColor = new Color(0.7f, 0.7f, 0.04f);

			public float itemSelectorWidth = 380;
			public float itemSelectorHeight = 600;
			public int minDeepTypeSearch = 3;

			#region Type
			[SerializeField]
			List<string> _includedAssemblies;
			public List<string> includedAssemblies {
				get {
					if(_includedAssemblies == null) {
						_includedAssemblies = new List<string>() {
							"UnityEngine",
							"UnityEngine.CoreModule",
							"Assembly-CSharp",
							"Assembly-CSharp-firstpass",
							"mscorlib",
						};
					}
					return _includedAssemblies;
				}
			}

			[SerializeField]
			List<string> _nodeBrowserNamespaces;
			public List<string> nodeBrowserNamespaces {
				get {
					if(_nodeBrowserNamespaces == null) {
						_nodeBrowserNamespaces = new List<string>() {
							"System",
							"System.Collections",
							"System.Collections.Generic",
							"UnityEngine",
							"UnityEngine.AI",
							"UnityEngine.EventSystems",
							"UnityEngine.UI",
							"UnityEngine.UIElements",
						};
					}
					return _nodeBrowserNamespaces;
				}
				internal set {
					_nodeBrowserNamespaces = value;
				}
			}

			[SerializeField]
			List<string> _excludedNamespaces;
			public List<string> excludedNamespaces {
				get {
					if(_excludedNamespaces == null) {
						_excludedNamespaces = new List<string>() {
							"AOT",
							"MaxyGames*",
							"JetBrains.Annotations",
							"Mono.Security.Cryptography",
							"Microsoft*",
							"System.Buffers*",
							"System.CodeDom*",
							"System.Collections.Concurrent",
							"System.Collections.ObjectModel",
							"System.Collections.Specialized",
							"System.ComponentModel*",
							"System.Configuration",
							"System.Dynamic",
							"System.Diagnostics",
							"System.Globalization",
							"System.IO*",
							"System.Configuration.Assemblies",
							"System.Deployment.Internal",
							"System.Diagnostics*",
							"System.Media",
							"System.Management*",
							"System.Linq*",
							"System.Net*",
							"System.Reflection*",
							"System.Resources",
							"System.Text*",
							"System.Timers",
							"System.Threading*",
							"System.Web",
							"System.Windows*",
							"System.Security*",
							"System.Runtime*",
							"UnityEngineInternal*",
							"Unity.*",
							"UnityEditor.*",
							"UnityEditorInternal*",
							"UnityEngine.Apple*",
							"UnityEngine.Android",
							"UnityEngine.Animations",
							"UnityEngine.CrashReportHandler",
							"UnityEngine.Experimental*",
							"UnityEngine.Internal*",
							"UnityEngine.iOS*",
							"UnityEngine.SocialPlatforms*",
							"UnityEngine.VR*",
							"UnityEngine.Tizen*",
							"UnityEngine.Diagnostics",
							"UnityEditor.Experimental*",
							"UnityEngine.Assertions*",
							"UnityEngine.Rendering",
							"UnityEngine.Jobs",
							"UnityEngine.Lumin",
							"UnityEngine.Playables",
							"UnityEngine.Profiling*",
							"UnityEngine.tvOS",
							"UnityEngine.WSA",
							"UnityEngine.Sprites",
							"UnityEngine.Scripting*",
							"UnityEngine.Serialization",
							"UnityEngine.Networking.PlayerConnection",
							"UnityEngine.Windows*",
							"UnityEngine.U2D",
						};
					}
					return _excludedNamespaces;
				}
				set => _excludedNamespaces = value;
			}
			public List<string> excludedTypes;

			[SerializeField]
			Dictionary<Type, Color> _typeColors;
			public Dictionary<Type, Color> typeColors {
				get {
					if(_typeColors == null) {
						_typeColors = new Dictionary<Type, Color>() {
							{typeof(object), new Color(0.3f, 1, 0.5f) },
							{typeof(string), new Color(0.3f, 0.5f, 1) },
							{typeof(float), new Color(1, 0.5f, 0.4f) },
							{typeof(int), new Color(0.8f, 1, 0.25f) },
							{typeof(bool), new Color(1, 0.22f, 0.26f) },
							{typeof(Vector2), new Color(1, 0.7f, 0f) },
							{typeof(Vector3), new Color(1, 0.55f, 0f) },
							{typeof(UnityEngine.Object), new Color(0.67f, 0.22f, 1f) },
							{typeof(MonoBehaviour), new Color(0.48f, 0.34f, 0.8f) },
						};
					}
					return _typeColors;
				}
				set => _typeColors = value;
			}
			#endregion

			public bool showObsoleteItem, showStatusBar = true, enableErrorCheck = true, saveGraphPosition = true;
			public string editorTheme;

			[NonSerialized]
			public bool preventEditingPrefab = true;

			//Code Generation
			public GeneratorData generatorData = new GeneratorData();

			[System.Serializable]
			public class GeneratorData {
				[Tooltip(@"The Generation mode for every graphs. 
-Default is using individual graph setting and force to be pure when the graph use default.
-Performance: is forcing to generate pure c# and get better performance but it may give errors when other graph is not compiled into script.
-Compatibility: is forcing to generate c# script that's compatible with all graph even when other graph is not compiled into script.")]
				public GenerationKind generationMode = GenerationKind.Default;
				[Tooltip("If true, will auto generates scripts for project graphs on build to get native c# performance.")]
				public bool autoGenerateOnBuild = true;
				[Tooltip("If true, will include full type name with namespace.\nUse this when you have error regarding to type ambiguous.")]
				public bool fullTypeName = false;
				[Tooltip("If true will include comment for each event, node, action, and validation.")]
				public bool fullComment = false;
				[Tooltip("If true, will convert line ending this should fix line ending warning.")]
				public bool convertLineEnding = true;
				[Tooltip("If true, will optimize code for get / set variables and properties for Runtime Graph")]
				public bool optimizeRuntimeCode = false;

				[Header("Compilation")]
				[Tooltip("Compile script after generating code for check if have error or warning.")]
				public bool compileScript = true;
				[Tooltip(
@"The Runtime Graph compilation method:
-Unity: The generated scripts will be saved in 'Assets/uNode.Generated' folder and will be compiled by Unity, the assembly result is natively loaded.

-Roslyn: The generated scripts will be saved in temporary folder and will be compiled by Roslyn Compiler, the assembly result is automatic loaded on Play.
Note: Auto Generate on Buld will always using Unity method.")]
				public CompilationMethod compilationMethod;
				[Hide(nameof(compilationMethod), CompilationMethod.Unity, defaultValue =true)]
				[Tooltip("If true, will compile graph in background.\nNote: The generated script will using Full Type Name for performance reason.")]
				public bool compileInBackground;
				[Hide(nameof(compilationMethod), CompilationMethod.Unity)]
				[Hide(nameof(compileInBackground), false)]
				[Tooltip("If true, will auto generates script on save graph.")]
				public bool autoGenerateOnSave;

				[Header("Others")]
				[Tooltip("Analyze the generated script with CSharpParser ( Roslyn )")]
				public bool analyzeScript;
				[Hide(nameof(analyzeScript), false)]
				public bool formatScript = true;

				public bool IsAutoGenerateOnSave => compilationMethod == CompilationMethod.Roslyn && compileInBackground && autoGenerateOnSave;
			}

			#region Hidden Preferences
			public int m_itemSearchKind;
			public int m_itemDeepSearchKind;
			#endregion
		}

		class NoneGraph : NodeGraph {
			public override void DrawCanvas(uNodeEditor window, GraphData graphData) {
				GUIContent info = new GUIContent("There's no graph theme.");
				Vector2 size = EditorStyles.label.CalcSize(info);
				GUI.Label(new Rect(graphData.canvasRect.x + (graphData.canvasRect.width / 2) - (size.x / 2), graphData.canvasRect.y + (graphData.canvasRect.height / 2) - (size.y / 2), size.x, size.y), info);
			}

			public override void HighlightNode(NodeComponent node) {
				
			}
		}

		public static PreferenceData preferenceData {
			get {
				Initialize();
				return _preferenceData;
			}
		}

		private static NodeGraph _nodeGraph;
		public static NodeGraph nodeGraph {
			get {
				if(_nodeGraph == null) {
					if(!string.IsNullOrEmpty(preferenceData.editorTheme) && preferenceData.editorTheme.StartsWith("@")) {
						var customGraphs = uNodeEditorUtility.FindCustomGraph();
						foreach(var g in customGraphs) {
							if(string.IsNullOrEmpty(g.name))
								continue;
							if(preferenceData.editorTheme == "@" + g.name) {
								_nodeGraph = ReflectionUtils.CreateInstance(g.type) as NodeGraph;
								return _nodeGraph;
							}
						}
					}
					if(editorTheme != null) {
						var graphType = editorTheme.GetGraphType();
						if(graphType == null) {
							var customGraphs = uNodeEditorUtility.FindCustomGraph();
							if(customGraphs != null && customGraphs.Count > 0) {
								graphType = customGraphs[0]?.type;
							}
							if(graphType == null) {
								graphType = typeof(NoneGraph);
							}
						}
						_nodeGraph = System.Activator.CreateInstance(graphType) as NodeGraph;
					} else {
						 _nodeGraph = new NoneGraph();
					}
				}
				return _nodeGraph;
			}
		}

		private static PreferenceData _preferenceData;
		private static bool isInitialized = false, assemblyT, colorT, isDim, isLocked, advanced;
		private static Vector2 scrollPos;
		private static Assembly[] assemblies;
		private static Dictionary<Type, Color> _cachedColorMap = new Dictionary<Type, Color>();

		private static int selectedMenu = 0;

		[PreferenceItem("uNode")]
		public static void PreferencesGUI() {
			Initialize();
			selectedMenu = GUILayout.Toolbar(selectedMenu, new string[] { "Graph", "Editor", "Type", "Code Generation" });
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			EditorGUI.BeginChangeCheck();
			if(selectedMenu == 0) {//Graph
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(new GUIContent("Theme"));
				if(uNodeEditorGUI.Button(string.IsNullOrEmpty(preferenceData.editorTheme) ? "Default" : preferenceData.editorTheme, EditorStyles.popup)) {
					GenericMenu menu = new GenericMenu();
					menu.AddItem(new GUIContent("Default"), string.IsNullOrEmpty(preferenceData.editorTheme), () => {
						preferenceData.editorTheme = "";
						_editorTheme = null;
						nodeGraph.OnDisable();
						_nodeGraph = null;
						SavePreference();
					});
					menu.AddSeparator("");
					var theme = uNodeEditorUtility.FindAssetsByType<BaseEditorTheme>();
					foreach(var t in theme) {
						if(string.IsNullOrEmpty(t.themeName) || t.themeName.ToLower() == "[default]")
							continue;
						menu.AddItem(new GUIContent(t.themeName), preferenceData.editorTheme == t.themeName, () => {
							preferenceData.editorTheme = t.themeName;
							_editorTheme = null;
							nodeGraph.OnDisable();
							_nodeGraph = null;
							SavePreference();
						});
					}
					var customGraphs = uNodeEditorUtility.FindCustomGraph();
					if(customGraphs.Count > 0) {
						menu.AddSeparator("");
						foreach(var g in customGraphs) {
							if(string.IsNullOrEmpty(g.name))
								continue;
							menu.AddItem(new GUIContent(g.name), preferenceData.editorTheme == "@" + g.name, () => {
								preferenceData.editorTheme = "@" + g.name;
								_editorTheme = null;
								nodeGraph.OnDisable();
								_nodeGraph = null;
								SavePreference();
							});
						}
					}
					menu.ShowAsContext();
				}
				EditorGUILayout.EndHorizontal();
				if(!string.IsNullOrEmpty(preferenceData.editorTheme) && editorTheme != null && AssetDatabase.Contains(editorTheme)) {
					editorTheme.expanded = EditorGUILayout.Foldout(editorTheme.expanded, "Theme Settings");
					if(editorTheme.expanded) {
						Editor editor = CustomInspector.GetEditor(editorTheme);
						EditorGUI.indentLevel++;
						editor.OnInspectorGUI();
						EditorGUI.indentLevel--;
					}
					if(GUI.changed) {
						uNodeEditorUtility.MarkDirty(editorTheme);
					}
				}
				preferenceData.displayKind = (DisplayKind)EditorGUILayout.EnumPopup(new GUIContent("Display Kind"), preferenceData.displayKind);
				//preferenceData.isDim = EditorGUILayout.Toggle(new GUIContent("Dim Node"), preferenceData.isDim);
				bool showGrid = EditorGUILayout.Toggle(new GUIContent("Show Grid"), preferenceData.showGrid);
				if(showGrid != preferenceData.showGrid) {
					preferenceData.showGrid = showGrid;
					uNodeEditor.window?.Refresh(true);
				}
				preferenceData.showStatusBar = EditorGUILayout.Toggle(new GUIContent("Show Status Bar"), preferenceData.showStatusBar);
				preferenceData.maxReloadMilis = EditorGUILayout.IntField(new GUIContent("Max Reload Milis",
@"The max progress for loading a graph in milliseconds for each frame.
-Reducing value will make loading graph slower but less lag for Unity.
-Increase value will make loading graph faster but more lag for Unity.

Recommended value is between 10-100."), preferenceData.maxReloadMilis);

				EditorGUILayout.LabelField("Snapping", EditorStyles.boldLabel);
				uNodeGUIUtility.ShowField(nameof(preferenceData.enableSnapping), preferenceData);
				if(preferenceData.enableSnapping) {
					uNodeGUIUtility.ShowField(nameof(preferenceData.graphSnapping), preferenceData);
					uNodeGUIUtility.ShowField(nameof(preferenceData.nodePortSnapping), preferenceData);
					if(preferenceData.nodePortSnapping) {
						preferenceData.portSnappingRange = EditorGUILayout.Slider(new GUIContent("Port Snapping Range"), preferenceData.portSnappingRange, 10, 100);
					}
					uNodeGUIUtility.ShowField(nameof(preferenceData.gridSnapping), preferenceData);
					uNodeGUIUtility.ShowField(nameof(preferenceData.spacingSnapping), preferenceData);
				}
			} else if(selectedMenu == 1) {
				preferenceData.inspectorIntegration = EditorGUILayout.Toggle(new GUIContent("Inspector Integration"), preferenceData.inspectorIntegration);
				// preferenceData.isLocked = EditorGUILayout.Toggle(new GUIContent("Lock Selection"), preferenceData.isLocked);
				uNodeGUIUtility.ShowField(nameof(preferenceData.newVariableAccessor), preferenceData);
				uNodeGUIUtility.ShowField(nameof(preferenceData.newFunctionAccessor), preferenceData);

				EditorGUILayout.LabelField("Node Browser", EditorStyles.boldLabel);
				uNodeGUIUtility.ShowField(nameof(preferenceData.itemSelectorWidth), preferenceData);
				uNodeGUIUtility.ShowField(nameof(preferenceData.itemSelectorHeight), preferenceData);
				preferenceData.coloredItem = EditorGUILayout.Toggle(new GUIContent("Colored Item"), preferenceData.coloredItem);
				if(preferenceData.coloredItem) {
					uNodeGUIUtility.ShowField(new GUIContent("Type Color"), "itemTypeColor", preferenceData);
					uNodeGUIUtility.ShowField(new GUIContent("Keyword Color"), "itemKeywordColor", preferenceData);
					uNodeGUIUtility.ShowField(new GUIContent("Interface Color"), "itemInterfaceColor", preferenceData);
					uNodeGUIUtility.ShowField(new GUIContent("Enum Color"), "itemEnumColor", preferenceData);
				}

				advanced = EditorGUILayout.Foldout(advanced, "Advanced");
				if(advanced) {
					EditorGUI.indentLevel++;
					//preferenceData.hideNodeIcons = EditorGUILayout.Toggle(new GUIContent("Hide Node Icons"), preferenceData.hideNodeIcons);
					//preferenceData.hideNodeComment = EditorGUILayout.Toggle(new GUIContent("Hide Node Comment"), preferenceData.hideNodeComment);
					preferenceData.hideChildObject = EditorGUILayout.Toggle(new GUIContent("Hide Child Object"), preferenceData.hideChildObject);
					preferenceData.enableErrorCheck = EditorGUILayout.Toggle(new GUIContent("Enable Error Check", "Set to true to enable real time in editor error check."), preferenceData.enableErrorCheck);
					preferenceData.saveGraphPosition = EditorGUILayout.Toggle(new GUIContent("Save Graph Position", "If true, the opened graph will load last position."), preferenceData.saveGraphPosition);
					preferenceData.forceReloadGraph = EditorGUILayout.Toggle(new GUIContent("Force Reload Graph", "If true, the graph will be force reloaded so the graph will have persistence data but it will slow every refresing graph since it will recreate the whole graph."), preferenceData.forceReloadGraph);
					EditorGUI.indentLevel--;
				}
			} else if(selectedMenu == 2) {
				colorT = EditorGUILayout.Foldout(colorT, "Type Color");
				if(colorT) {
					EditorGUILayout.BeginVertical("Box");
					preferenceData.defaultValueColor = EditorGUILayout.ColorField(new GUIContent("Default Value Color"), preferenceData.defaultValueColor);
					uNodeGUIUtility.EditValueLayouted(new GUIContent("Type Colors"), preferenceData.typeColors, preferenceData.typeColors.GetType(), (val) => {
						preferenceData.typeColors = val as Dictionary<Type, Color>;
					});
					EditorGUILayout.EndVertical();
				}
				preferenceData.showObsoleteItem = EditorGUILayout.Toggle(new GUIContent("Show Obsolete Item"), preferenceData.showObsoleteItem);
				preferenceData.minDeepTypeSearch = EditorGUILayout.IntField(
					new GUIContent(
						"Min Word For Deep Search",
						"The minimal word for performing deep type search"),
					preferenceData.minDeepTypeSearch);
				VariableEditorUtility.DrawCustomList(
					preferenceData.includedAssemblies,
					"Included Assemblies",
					(position, index, value) => {
						EditorGUI.LabelField(position, value);
					},
					(position) => {
						List<ItemSelector.CustomItem> items = new List<ItemSelector.CustomItem>();
						var ass = EditorReflectionUtility.GetAssemblies();
						foreach(var assembly in ass) {
							if(assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
								continue;
							string assemblyName = assembly.GetName().Name;
							if(!string.IsNullOrEmpty(assemblyName) && !preferenceData.includedAssemblies.Contains(assemblyName)) {
								items.Add(ItemSelector.CustomItem.Create(assemblyName, (obj) => {
									preferenceData.includedAssemblies.Add(assemblyName);
									SavePreference();
									//XmlDoc.ReloadDocInBackground();
								}, "Namespaces"));
							}
						}
						items.Sort((x, y) => string.CompareOrdinal(x.name, y.name));
						ItemSelector.ShowWindow(null, null, null, false, items).ChangePosition(position.ToScreenRect()).displayDefaultItem = false;
					},
					(index) => {
						preferenceData.includedAssemblies.RemoveAt(index);
						SavePreference();
						//XmlDoc.ReloadDocInBackground();
					}
				);
				VariableEditorUtility.DrawNamespace("Browser Namespaces List", preferenceData.nodeBrowserNamespaces, null, (val) => {
					preferenceData.nodeBrowserNamespaces = val as List<string>;
					SavePreference();
				});
				uNodeGUIUtility.EditValue(
					uNodeGUIUtility.GetRect(),
					new GUIContent("Excluded Namespace"),
					preferenceData.excludedNamespaces,
					typeof(List<string>),
					(obj) => {
						preferenceData.excludedNamespaces = obj as List<string>;
						SavePreference();
					});
				uNodeGUIUtility.EditValue(
					uNodeGUIUtility.GetRect(),
					new GUIContent("Excluded Types"),
					preferenceData.excludedTypes,
					typeof(List<string>),
					(obj) => {
						preferenceData.excludedTypes = obj as List<string>;
						SavePreference();
					});
			} else if(selectedMenu == 3) {
				uNodeGUIUtility.EditValueLayouted(
					GUIContent.none,
					preferenceData.generatorData,
					(val) => {
						preferenceData.generatorData = val;
						SavePreference();
					});
			}
			if(GUILayout.Button("Set Default", EditorStyles.miniButton)) {
				SetDefault();
			}
			if(EditorGUI.EndChangeCheck()) {
				SavePreference();
			}
			EditorGUILayout.EndScrollView();
		}

		public static void ResetGraph() {
			_editorTheme = null;
			nodeGraph.OnDisable();
			_nodeGraph = null;
		}

		private static void SetDefault() {
			_preferenceData = new PreferenceData();
			_preferenceData.editorTheme = "";
			SavePreference();
		}

		private static void Initialize() {
			if(!isInitialized) {
				isInitialized = true;
				LoadPreference();
				assemblies = EditorReflectionUtility.GetAssemblies();
				List<Assembly> ass = new List<Assembly>();
				foreach(Assembly assembly in assemblies) {
					try {
						if(string.IsNullOrEmpty(assembly.Location))
							continue;
						ass.Add(assembly);
					} catch { continue; }
				}
				assemblies = ass.ToArray();
			}
		}

		private static BaseEditorTheme _editorTheme;
		public static BaseEditorTheme editorTheme {
			get {
				if(_editorTheme == null) {
					if(string.IsNullOrEmpty(preferenceData.editorTheme)) {
						var theme = uNodeEditorUtility.FindAssetsByType<BaseEditorTheme>();
						if(theme.Count > 0) {
							_editorTheme = theme.Find(t => t.themeName == "[default]");
							if(_editorTheme == null)
								_editorTheme = theme[0];
						}
					} else {
						var theme = uNodeEditorUtility.FindAssetsByType<BaseEditorTheme>();
						foreach(var t in theme) {
							if(string.IsNullOrEmpty(t.themeName))
								continue;
							if(t.themeName == preferenceData.editorTheme) {
								_editorTheme = t;
								break;
							}
						}
						if(_editorTheme == null && theme.Count > 0) {
							_editorTheme = theme.Find(t => t.themeName == "[default]");
							if(_editorTheme == null)
								_editorTheme = theme[0];
						}
					}
				}
				return _editorTheme;
			}
		}

		private static HashSet<string> _excludedNamespace;
		/// <summary>
		/// Get the excluded namespaces
		/// </summary>
		/// <returns></returns>
		public static HashSet<string> GetExcludedNamespace() {
			if(_excludedNamespace == null) {
				var namespaces = EditorReflectionUtility.GetNamespaces();
				var excluded = new HashSet<string>();
				foreach(var item in preferenceData.excludedNamespaces) {
					if(string.IsNullOrEmpty(item))
						continue;
					if(item.EndsWith("*")) {
						string name = item.RemoveLast();
						foreach(var ns in namespaces) {
							if(ns == null)
								continue;
							if(ns.StartsWith(name)) {
								excluded.Add(ns);
							}
						}
					} else {
						excluded.Add(item);
					}
				}
				_excludedNamespace = excluded;
			}
			return _excludedNamespace;
		}

		private static HashSet<string> _includedAssemblies;
		public static HashSet<string> GetIncludedAssemblies() {
			if(_includedAssemblies == null) {
				_includedAssemblies = new HashSet<string>(preferenceData.includedAssemblies);
			}
			return _includedAssemblies;
		}

		private static HashSet<string> _browserNamespaces;
		public static HashSet<string> GetBrowserNamespaceList() {
			if(_browserNamespaces == null) {
				_browserNamespaces = new HashSet<string>(preferenceData.nodeBrowserNamespaces);
			}
			return _browserNamespaces;
		}

		private static HashSet<string> _excludedTypes;
		/// <summary>
		/// Get the excluded types
		/// </summary>
		/// <returns></returns>
		public static HashSet<string> GetExcludedTypes() {
			if(_excludedTypes == null) {
				if(preferenceData.excludedTypes == null) {
					return new HashSet<string>();
				}
				_excludedTypes = new HashSet<string>();
				foreach(var item in preferenceData.excludedTypes) {
					if(string.IsNullOrEmpty(item))
						continue;
					_excludedTypes.Add(item);
				}
			}
			return _excludedTypes;
		}

		/// <summary>
		/// Get the preference data
		/// </summary>
		/// <returns></returns>
		public static PreferenceData GetPreference() {
			return preferenceData;
		}

		/// <summary>
		/// Add a new exluded type to preference and save it.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool AddExcludedType(Type type) {
			if(!preferenceData.excludedTypes.Contains(type.FullName)) {
				preferenceData.excludedTypes.Add(type.FullName);
				SavePreference();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Add a new exluded namespace to preference and save it.
		/// </summary>
		/// <param name="ns"></param>
		/// <returns></returns>
		public static bool AddExcludedNamespace(string ns) {
			if(!preferenceData.excludedNamespaces.Contains(ns)) {
				preferenceData.excludedNamespaces.Add(ns);
				SavePreference();
				return true;
			}
			return false;
		}

		public static Color GetColorForType(Type type) {
			if(type == null)
				return preferenceData.defaultValueColor;
			if(_cachedColorMap.ContainsKey(type)) {
				return _cachedColorMap[type];
			}
			if(preferenceData.typeColors.ContainsKey(type)) {
				_cachedColorMap[type] = preferenceData.typeColors[type];
				return preferenceData.typeColors[type];
			}
			foreach(var pair in preferenceData.typeColors) {
				if(pair.Key.IsCastableTo(type)) {
					_cachedColorMap[type] = pair.Value;
					return pair.Value;
				}
			}
			if(type.IsEnum) {
				_cachedColorMap[type] = uNodeUtility.richTextColor().enumColor;
				return uNodeUtility.richTextColor().enumColor;
			} else if(type.IsInterface) {
				_cachedColorMap[type] = uNodeUtility.richTextColor().interfaceColor;
				return uNodeUtility.richTextColor().enumColor;
			}
			_cachedColorMap[type] = preferenceData.defaultValueColor;
			return _cachedColorMap[type];
		}

		#region Save & Load
		public static void LoadPreference() {
			char separator = Path.DirectorySeparatorChar;
			string path = "uNode2Data" + separator + "PreferenceData" + ".byte";
			if(File.Exists(path)) {
				_preferenceData = SerializerUtility.Deserialize<PreferenceData>(File.ReadAllBytes(path));
			}
			if(_preferenceData == null) {
				SetDefault();
			}
		}

		public static void SavePreference() {
			if(_preferenceData == null)
				return;
			_excludedNamespace = null;
			_excludedTypes = null;
			_includedAssemblies = null;
			_browserNamespaces = null;
			_cachedColorMap = new Dictionary<Type, Color>();
			Directory.CreateDirectory("uNode2Data");
			char separator = Path.DirectorySeparatorChar;
			string path = "uNode2Data" + separator + "PreferenceData" + ".byte";
			File.WriteAllBytes(path, SerializerUtility.Serialize(_preferenceData));
		}
		#endregion
	}
}