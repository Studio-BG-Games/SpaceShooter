using UnityEngine;
using UnityEditor;
using MaxyGames.Events;
using MaxyGames.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor.Callbacks;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using MaxyGames.OdinSerializer.Editor;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode.Editors {
	internal static class uNodeEditorMenu {
		internal class MyCustomBuildProcessor : UnityEditor.Build.IPreprocessBuildWithReport, UnityEditor.Build.IPostprocessBuildWithReport {
			public int callbackOrder => 0;

			public void OnPreprocessBuild(BuildReport report) {
				uNodeEditorInitializer.OnPreprocessBuild();
			}

			public void OnPostprocessBuild(BuildReport report) {
				uNodeEditorInitializer.OnPostprocessBuild();
			}
		}

#if UNODE_DEBUG
		[MenuItem("Tools/uNode/Advanced/Scan AOT Type", false, 1000010)]
		public static void ScanAOTType() {
			uNodeEditorInitializer.AOTScan(out var types);
			Debug.Log(types.Count);
			foreach(var t in types) {
				Debug.Log(t);
			}
		}
#endif

		private static void MigrateSerialization(object data) {
			if(data == null)
				return;
			if(data is MemberData member) {
				if(!member.isStatic) {
					object value = member.instance;
					if(!(value is Object)) {
						MigrateSerialization(value);
					}
					member.instance = value;
				}
				if(member.targetType == MemberData.TargetType.Values) {
					member = new MemberData(member.Get());
				}
				return;
			}
			AnalizerUtility.AnalizeObject(data, (obj) => {
				if(obj is MemberData) {
					MigrateSerialization(obj);
					return true;
				} else if(obj is ISerializationCallbackReceiver serializationCallback) {
					if(serializationCallback is EventActionData EAD) {
						MigrateSerialization(EAD.block);
					}
					serializationCallback.OnBeforeSerialize();
					serializationCallback.OnAfterDeserialize();
					return true;
				}
				return false;
			}, (instance, field, type, value) => {
				field.SetValueOptimized(instance, value);
			});
		}

		[MenuItem("Tools/uNode/Update Graph Database", false, 2)]
		public static void UpdateDatabase() {
			var db = uNodeUtility.GetDatabase();
			if(db == null && EditorUtility.DisplayDialog("No graph database", "There's no graph database found in the project, do you want to create new?", "Ok", "Cancel")) {
				while(true) {
					var path = EditorUtility.SaveFolderPanel("Select resources folder to save database to", "Assets", "").Replace('/', Path.DirectorySeparatorChar);
					if(!string.IsNullOrEmpty(path)) {
						if(path.StartsWith(Directory.GetCurrentDirectory(), StringComparison.Ordinal) && path.ToLower().EndsWith("resources")) {
							db = ScriptableObject.CreateInstance<uNodeResourceDatabase>();
							path = path.Remove(0, Directory.GetCurrentDirectory().Length + 1) + Path.DirectorySeparatorChar + "uNodeDatabase.asset";
							AssetDatabase.CreateAsset(db, path);
						} else {
							uNodeEditorUtility.DisplayErrorMessage("Please select 'Resources' folder in project");
							continue;
						}
					}
					break;
				}
			}
			if(db != null) {
				var graphs = uNodeEditorUtility.FindComponentInPrefabs<uNodeRoot>();
				foreach(var root in graphs) {
					if(db.graphDatabases.Any(g => g.graph == root)) {
						continue;
					}
					db.graphDatabases.Add(new uNodeResourceDatabase.RuntimeGraphDatabase() {
						graph = root,
					});
					EditorUtility.SetDirty(db);
				}
			}
		}

		[MenuItem("Tools/uNode/Update/Migrate Project Serialization", false, 100001)]
		public static void MigrateSerializationData() {
			var prefabs = uNodeEditorUtility.FindPrefabsOfType<uNodeRoot>();
			foreach(var prefab in prefabs) {
				if(GraphUtility.HasTempGraphObject(prefab)) {
					var graph = GraphUtility.GetTempGraphObject(prefab);
					var scripts = graph.GetComponentsInChildren<MonoBehaviour>(true);
					foreach(var behavior in scripts) {
						MigrateSerialization(behavior);
						uNodeEditorUtility.MarkDirty(behavior);
					}
					GraphUtility.SaveGraph(graph);
				} else {
					var graph = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
					var scripts = graph.GetComponentsInChildren<MonoBehaviour>(true);
					foreach(var behavior in scripts) {
						MigrateSerialization(behavior);
						uNodeEditorUtility.MarkDirty(behavior);
					}
					PrefabUtility.SaveAsPrefabAsset(graph, AssetDatabase.GetAssetPath(prefab));
					PrefabUtility.UnloadPrefabContents(graph);
				}
				uNodeEditorUtility.MarkDirty(prefab);
			}
			var assets = uNodeEditorUtility.FindAssetsByType<ScriptableObject>();
			foreach(var asset in assets) {
				MigrateSerialization(asset);
				uNodeEditorUtility.MarkDirty(asset);
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

#if UNODE_COMPILE_ON_PLAY
		[MenuItem("Tools/uNode/Advanced/Compile On Play: Enabled", false, 10001)]
#else
		[MenuItem("Tools/uNode/Advanced/Compile On Play: Disabled", false, 10001)]
#endif
		private static void AdvancedCompileOnPlay() {
#if UNODE_COMPILE_ON_PLAY
			uNodeEditorUtility.RemoveDefineSymbols(new string[] { "UNODE_COMPILE_ON_PLAY" });
#else
			if(EditorBinding.roslynUtilityType != null) {
				uNodeEditorUtility.AddDefineSymbols(new string[] { "UNODE_COMPILE_ON_PLAY" });
			} else {
#if NET_STANDARD_2_0
				EditorUtility.DisplayDialog("Cannot enable Compile On Play", "Cannot enable compile graphs on play because unsupported compiling scripts in .NET Standard 2.0, change API compativility level to .NET 4.x to enable it or import CSharp Parser add-ons.", "Ok");
#else
				uNodeEditorUtility.AddDefineSymbols(new string[] { "UNODE_COMPILE_ON_PLAY" });
#endif
			}
#endif
		}

		[MenuItem("Assets/Create/Create Asset Instance", false, 19)]
		public static void CreateAssetInstance() {
			if(Selection.activeObject is GameObject gameObject && gameObject.GetComponent<uNodeClassAsset>() != null) {
				var graph = gameObject.GetComponent<uNodeClassAsset>();
				var classAsset = ScriptableObject.CreateInstance<uNodeAssetInstance>();
				classAsset.target = graph;
				ProjectWindowUtil.CreateAsset(classAsset, $"New_{graph.DisplayName}.asset");
			} else {
				var items = ItemSelector.MakeCustomItemsForInstancedType(new System.Type[] { typeof(uNodeClassAsset) }, (val) => {
					var graph = val as uNodeClassAsset;
					var classAsset = ScriptableObject.CreateInstance<uNodeAssetInstance>();
					classAsset.target = graph;
					ProjectWindowUtil.CreateAsset(classAsset, $"New_{graph.DisplayName}.asset");
				}, false);
				var pos = EditorWindow.mouseOverWindow?.position ?? EditorWindow.focusedWindow?.position ?? Rect.zero;
				ItemSelector.ShowCustomItem(items).ChangePosition(pos);
			}
		}

		static List<string> GetObjDependencies(Object obj, HashSet<Object> scannedObjs) {
			List<string> result = new List<string>();
			if(!scannedObjs.Add(obj)) {
				return result;
			}
			result.AddRange(AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(obj), true));
			if(obj is MonoScript) {
				var monoScript = obj as MonoScript;
				var path = AssetDatabase.GetAssetPath(monoScript);
				if(path.EndsWith(".cs")) {
					var graphPath = path.RemoveLast(3).Add(".prefab");
					if(File.Exists(graphPath)) {
						var graphObj = AssetDatabase.LoadAssetAtPath<GameObject>(graphPath);
						if(graphObj != null && graphObj.GetComponent<uNodeComponentSystem>() != null) {
							result.AddRange(GetObjDependencies(graphObj, scannedObjs));
						}
					}
				}
				return result;
			}
			Func<object, bool> func = (val) => {
				if(val is MemberData) {
					var member = val as MemberData;
					var references = member.GetUnityReferences();
					foreach(var r in references) {
						var mainAsset = r;
						if(r is Component comp) {
							mainAsset = comp.gameObject;
						}
						if(AssetDatabase.IsMainAsset(mainAsset) && scannedObjs.Add(mainAsset)) {
							result.Add(AssetDatabase.GetAssetPath(mainAsset));
							result.AddRange(GetObjDependencies(mainAsset, scannedObjs));
						}
					}
					if(member.isAssigned) {
						var type = member.startType;
						if(type != null && !type.IsRuntimeType()) {
							var monoScript = uNodeEditorUtility.GetMonoScript(type);
							if(monoScript != null) {
								result.AddRange(GetObjDependencies(monoScript, scannedObjs));
							} else {
								var loc = type.Assembly.Location;
								if(!string.IsNullOrEmpty(type.Assembly.Location)) {
									var fileName = Path.GetFileName(loc);
									if(scriptsMaps.TryGetValue(fileName, out var monoScripts)) {
										foreach(var script in monoScripts) {
											result.AddRange(GetObjDependencies(script, scannedObjs));
										}
									}
								}
							}
						}
					}
				} else if(val is Object) {
					var mainAsset = val as Object;
					if(mainAsset is Component comp && comp != null) {
						mainAsset = comp.gameObject;
					}
					if(AssetDatabase.IsMainAsset(mainAsset) && scannedObjs.Add(mainAsset)) {
						result.Add(AssetDatabase.GetAssetPath(mainAsset));
						result.AddRange(GetObjDependencies(mainAsset, scannedObjs));
					}
				}
				return false;
			};
			if(obj is GameObject go) {
				var comps = go.GetComponentsInChildren<MonoBehaviour>(true);
				foreach(var c in comps) {
					if(scannedObjs.Add(c)) {
						AnalizerUtility.AnalizeObject(c, func);
						if(c is Nodes.LinkedMacroNode macro && macro.macroAsset != null) {
							result.Add(AssetDatabase.GetAssetPath(macro.macroAsset));
							result.AddRange(GetObjDependencies(macro.macroAsset.gameObject, scannedObjs));
						}
						if(c is IVariableSystem) {
							var varSystem = c as IVariableSystem;
							AnalizerUtility.AnalizeObject(varSystem.Variables, func);
						}
					}
				}
			} else {
				AnalizerUtility.AnalizeObject(obj, func);
			}
			return result;
		}

		static Dictionary<string, HashSet<MonoScript>> scriptsMaps;

		static void UpdateScriptMap() {
			scriptsMaps = new Dictionary<string, HashSet<MonoScript>>();
			var unodePath = uNodeEditorUtility.GetUNodePath();
			string[] assetPaths = AssetDatabase.GetAllAssetPaths();
			foreach(string assetPath in assetPaths) {
				if(assetPath.EndsWith(".cs") && !assetPath.StartsWith(unodePath + "/", StringComparison.Ordinal)) {
					var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
					var type = monoScript.GetType();
					var assName = type.GetMethod("GetAssemblyName", MemberData.flags).InvokeOptimized(monoScript) as string;
					if(!scriptsMaps.TryGetValue(assName, out var monoScripts)) {
						monoScripts = new HashSet<MonoScript>();
						scriptsMaps[assName] = monoScripts;
					}
					monoScripts.Add(monoScript);
				}
			}
		}

		[MenuItem("Assets/Export uNode Graphs", false, 30)]
		public static void ExportSelectedGraphs() {
			EditorUtility.DisplayProgressBar("Finding Graphs Dependencies", "", 0);
			UpdateScriptMap();
			var guids = Selection.assetGUIDs;
			List<string> exportPaths = new List<string>();
			var hash = new HashSet<Object>();
			foreach(var guid in guids) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
				if(AssetDatabase.IsValidFolder(path)) {//Skip if folder or unknow type.
					var paths = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith(path + "/", StringComparison.Ordinal));
					foreach(var subPath in paths) {
						var subAsset = AssetDatabase.LoadAssetAtPath<Object>(subPath);
						exportPaths.Add(subPath);
						exportPaths.AddRange(GetObjDependencies(subAsset, hash));
					}
					continue;
				}
				exportPaths.Add(path);
				exportPaths.AddRange(GetObjDependencies(obj, hash));
			}
			var unodePath = uNodeEditorUtility.GetUNodePath();
			var projectDir = Directory.GetCurrentDirectory();
			for(int i = 0; i < exportPaths.Count; i++) {
				var path = exportPaths[i];
				if(path.StartsWith(unodePath + "/", StringComparison.Ordinal) || path == unodePath || !path.StartsWith("Assets", StringComparison.Ordinal) && !path.StartsWith("ProjectSettings", StringComparison.Ordinal)) {
					exportPaths.RemoveAt(i);
					i--;
					continue;
				}
			}
			EditorUtility.ClearProgressBar();
			ExportGraphWindow.Show(exportPaths.Distinct().OrderBy(p => p).ToArray());
		}

		class ExportGraphWindow : EditorWindow {
			[Serializable]
			class ExportData {
				public string path;
				public bool enable;
			}
			ExportData[] exportPaths;
			Vector2 scroll;

			static ExportGraphWindow window;

			public static ExportGraphWindow Show(string[] exportedPath) {
				window = GetWindow<ExportGraphWindow>(true);
				window.exportPaths = exportedPath.Select(p => new ExportData() { path = p, enable = true }).ToArray();
				window.minSize = new Vector2(300, 250);
				window.titleContent = new GUIContent("Export Graphs");
				window.Show();
				return window;
			}

			private void OnGUI() {
				if(exportPaths.Length == 0) {
					EditorGUILayout.HelpBox("Nothing to export", MessageType.Info);
					return;
				}
				GUILayout.BeginVertical();
				scroll = EditorGUILayout.BeginScrollView(scroll);
				for(int i = 0; i < exportPaths.Length; i++) {
					var data = exportPaths[i];
					var obj = AssetDatabase.LoadAssetAtPath<Object>(data.path);
					if(obj == null)
						continue;
					using(new GUILayout.HorizontalScope()) {
						data.enable = EditorGUILayout.Toggle(data.enable, GUILayout.Width(EditorGUIUtility.singleLineHeight));
						Texture icon = uNodeEditorUtility.GetTypeIcon(obj);
						if(obj is GameObject go) {
							var customIcon = go.GetComponent<ICustomIcon>();
							if(customIcon != null) {
								icon = uNodeEditorUtility.GetTypeIcon(customIcon);
							} else {
								var unode = go.GetComponent<uNodeComponentSystem>();
								if(unode != null) {
									icon = uNodeEditorUtility.GetTypeIcon(unode);
								}
							}
						}
						EditorGUILayout.LabelField(new GUIContent(icon), GUILayout.Width(EditorGUIUtility.singleLineHeight));
						EditorGUILayout.LabelField(new GUIContent(data.path));
					}
				}
				EditorGUILayout.EndScrollView();
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("Export")) {
					var savePath = EditorUtility.SaveFilePanel("Export Graphs", "", "", "unitypackage");
					if(!string.IsNullOrEmpty(savePath)) {
						AssetDatabase.ExportPackage(exportPaths.Where(p => p.enable).Select(p => p.path).ToArray(), savePath);
						Close();
					}
				}
				GUILayout.EndVertical();
			}
		}

		// [MenuItem("Assets/Create Instance", false, 19)]
		// public static void CreateUNodeAssetInstance()
		// {
		// 	var graph = (Selection.activeObject as GameObject).GetComponent<uNodeClassAsset>();
		// 	var classAsset = ScriptableObject.CreateInstance<uNodeAssetInstance>();
		// 	classAsset.target = graph;
		// 	ProjectWindowUtil.CreateAsset(classAsset, $"New_{graph.DisplayName}.asset");
		// }

		// [MenuItem("Assets/Create Instance", true, 19)]
		// public static bool CanCreateUNodeAssetInstance()
		// {
		// 	var gameObject = Selection.activeObject as GameObject;
		// 	if(gameObject != null) {
		// 		var asset = gameObject.GetComponent<uNodeClassAsset>();
		// 		if(asset != null) {
		// 			return true;
		// 		}
		// 	}
		// 	return false;
		// }

		[MenuItem("Assets/Create/uNode/Class Component", false, -10000)]
		private static void CreateClassComponent() {
			GraphCreatorWindow.ShowWindow(typeof(uNodeClassComponent));
			//CreatePrefabWithComponent<uNodeClassComponent>("ClassComponent");
		}

		[MenuItem("Assets/Create/uNode/Class Asset", false, -10001)]
		private static void CreateClassAsset() {
			GraphCreatorWindow.ShowWindow(typeof(uNodeClassAsset));
			//CreatePrefabWithComponent<uNodeClassAsset>("ClassAsset");
		}

		[MenuItem("Assets/Create/uNode/C# Class", false, -900)]
		private static void CreateUNodeClass() {
			GraphCreatorWindow.ShowWindow(typeof(uNodeClass));
			//CreatePrefabWithComponent<uNodeClass>("Class");
		}

		[MenuItem("Assets/Create/uNode/C# Struct", false, -900)]
		private static void CreateUNodeStruct() {
			GraphCreatorWindow.ShowWindow(typeof(uNodeStruct));
			//CreatePrefabWithComponent<uNodeStruct>("Struct");
		}

		[MenuItem("Assets/Create/uNode/Macro", false, -80)]
		private static void CreateUNodeMacro() {
			//GraphCreatorWindow.ShowWindow(typeof(uNodeMacro));
			CreatePrefabWithComponent<uNodeMacro>("Macro");
		}

		//[MenuItem("Assets/Create/uNode/Runtime", false, -70)]
		//private static void CreateUNodeRuntime() {
		//	GraphCreatorWindow.ShowWindow(typeof(uNodeRuntime));
		//	CreatePrefabWithComponent<uNodeRuntime>("uNodeRuntime");
		//}

		[MenuItem("Assets/Create/uNode/Component Singleton", false, 101)]
		private static void CreateComponentSingleton() {
			GraphCreatorWindow.ShowWindow(typeof(uNodeComponentSingleton));
			//CreatePrefabWithComponent<uNodeComponentSingleton>("ComponentSingletonGraph");
		}

		[MenuItem("Assets/Create/uNode/Graph Interface", false, 102)]
		private static void CreateGraphInterface() {
			var classAsset = ScriptableObject.CreateInstance<uNodeInterface>();
			ProjectWindowUtil.CreateAsset(classAsset, $"New_Interface.asset");
		}

		// [MenuItem("Assets/Create/uNode/Asset Singleton", false, 100)]
		// private static void CreateAssetSingleton() {
		// 	CreatePrefabWithComponent<uNodeAssetSingleton>("AssetSingletonGraph");
		// }

		// [MenuItem("Assets/Create/uNode/Asset/GlobalVariable")]
		// private static void CreateMap() {
		// 	CustomAssetUtility.CreateAsset<GlobalVariable>();
		// }

		[MenuItem("Assets/Create/uNode/Editor/Graph Theme")]
		private static void CreateTheme() {
			CustomAssetUtility.CreateAsset<VerticalEditorTheme>((theme) => {
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				EditorUtility.FocusProjectWindow();
			});
		}

		private static void CreatePrefabWithComponent<T>(string name) where T : Component {
			GameObject go = new GameObject(name);
			go.AddComponent<T>();
			string path = CustomAssetUtility.GetCurrentPath() + "/New_" + go.name + ".prefab";
			int index = 0;
			while(File.Exists(path)) {
				index++;
				path = CustomAssetUtility.GetCurrentPath() + "/New_" + go.name + index + ".prefab";
			}
#if UNITY_2018_3_OR_NEWER
			GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
#else
			GameObject prefab = PrefabUtility.CreatePrefab(path, go);
#endif
			Object.DestroyImmediate(go);
			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = prefab;
		}
	}
}