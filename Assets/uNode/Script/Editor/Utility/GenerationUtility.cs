using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MaxyGames.uNode.Editors {
	public class CompileResult {
		public Assembly assembly;

		public class CompileError {
			public string fileName;
			public bool isWarning;
			public string errorText;
			public string errorNumber;
			public int errorLine;
			public int errorColumn;

			public string errorMessage {
				get {
					if(string.IsNullOrEmpty(fileName)) {
						return $"({errorNumber}): {errorText}\nin line: {errorLine}:{errorColumn}";
					}
					string path = Directory.GetCurrentDirectory();
					if(fileName.StartsWith(path)) {
						return $"({errorNumber}): {errorText}\nin line: {errorLine}:{errorColumn} (at {fileName.Remove(0, path.Length + 1).Replace("\\", "/")}:{errorLine})";
					}
					return $"({errorNumber}): {errorText}\nin line: {errorLine}:{errorColumn}\nin file: {fileName}";
				}
			}
		}
		public IEnumerable<CompileError> errors;

		public void LogErrors() {
			if(errors != null) {
				foreach(var error in errors) {
					if(error.isWarning) {
						Debug.LogWarning(error.errorMessage);
					} else {
						Debug.LogError(error.errorMessage);
					}
				}
			}
		}

		public string GetErrorMessage() {
			if(errors != null) {
				System.Text.StringBuilder builder = new System.Text.StringBuilder();
				foreach(var error in errors) {
					if(!error.isWarning) {
						builder.AppendLine(error.errorMessage);
						builder.AppendLine();
					}
				}
				return builder.ToString();
			}
			return string.Empty;
		}
	}
	
	public static class GenerationUtility {
		#region Persistence Data
		[Serializable]
		public class Data {
			[SerializeField]
			public Dictionary<int, CachedScriptData> graphs = new Dictionary<int, CachedScriptData>();

			public CachedScriptData GetGraphData(GameObject graphAsset) {
				return GetGraphData(uNodeUtility.GetObjectID(graphAsset));
			}

			public CachedScriptData GetGraphData(int graphID) {
				if(!graphs.TryGetValue(graphID, out var scriptData)) {
					graphs[graphID] = scriptData = new CachedScriptData();
				}
				return scriptData;
			}
		}

		public class CachedScriptData : ISerializationCallbackReceiver {
			public string path;
			public int lastCompiledID;
			public int uniqueID;
			public string[] errors;
			public string generatedScript;

			[SerializeField]
			private string serializedCompiledHash;

			public Hash128 compiledHash;
			public bool isValid => errors == null || errors.Length == 0;

			public void MarkDirty() {
				path = null;
				compiledHash = default;
				lastCompiledID = 0;
			}

			public void OnAfterDeserialize() {
				compiledHash = Hash128.Parse(serializedCompiledHash);
			}

			public void OnBeforeSerialize() {
				serializedCompiledHash = compiledHash.ToString();
			}
		}

		public static Data persistenceData => GetData();

		public static Data _data;
		public static Data GetData() {
			if(_data == null) {
				_data = uNodeEditorUtility.LoadEditorData<Data>("GeneratorData");
				if(_data == null) {
					_data = new Data();
					SaveData();
				}
			}
			return _data;
		}

		public static void SaveData() {
			if(_data != null)
				uNodeEditorUtility.SaveEditorData(_data, "GeneratorData");
		}

		public static void MarkGraphDirty(GameObject graphAsset) {
			if(persistenceData.graphs.TryGetValue(uNodeUtility.GetObjectID(graphAsset), out var data)) {
				data.MarkDirty();
			}
		}

		public static void MarkGraphDirty(IEnumerable<GameObject> graphAssets) {
			foreach(var graph in graphAssets) {
				if(graph == null)
					continue;
				MarkGraphDirty(graph);
			}
		}

		public static bool IsGraphCompiled(GameObject graphAsset) {
			var scriptData = persistenceData.GetGraphData(graphAsset);
			if(scriptData.isValid) {
				if(uNodePreference.preferenceData.generatorData.compilationMethod == CompilationMethod.Roslyn) {
					return File.Exists(tempAssemblyPath);
				} else {
					return true;
				}
			}
			return false;
		}

		public static bool IsGraphUpToDate(GameObject graphAsset) {
			var scriptData = persistenceData.GetGraphData(graphAsset);
			if(scriptData.isValid && File.Exists(scriptData.path)) {
				var hash = AssetDatabase.GetAssetDependencyHash(AssetDatabase.GetAssetPath(graphAsset));
				return scriptData.compiledHash == hash;
			}
			return false;
		}
		#endregion

		static uNodePreference.PreferenceData _preferenceData;
		private static uNodePreference.PreferenceData preferenceData {
			get {
				if(_preferenceData != uNodePreference.GetPreference()) {
					_preferenceData = uNodePreference.GetPreference();
				}
				return _preferenceData;
			}
		}
		public const string tempFolder = "TempScript";
		public static string tempGeneratedFolder => tempFolder + Path.DirectorySeparatorChar + "Generated";
		public static string tempRoslynFolder => tempFolder + Path.DirectorySeparatorChar + "Scripts";
		public static string tempAssemblyPath => tempRoslynFolder + Path.DirectorySeparatorChar + "RuntimeAssembly.dll";
		public static string generatedPath => "Assets" + Path.DirectorySeparatorChar + "uNode.Generated";
		public static string resourcesPath => generatedPath + Path.DirectorySeparatorChar + "Resources";
		public static string projectScriptPath => generatedPath + Path.DirectorySeparatorChar + "Scripts";
		public static string projectSceneScriptPath => projectScriptPath + Path.DirectorySeparatorChar + "Scene";

		public static uNodeResourceDatabase GetDatabase() {
			var db = uNodeUtility.GetDatabase();
			if(db == null) {
				db = ScriptableObject.CreateInstance<uNodeResourceDatabase>();
				var dbDir = resourcesPath;
				Directory.CreateDirectory(dbDir);
				var path = dbDir + Path.DirectorySeparatorChar + "uNodeDatabase.asset";
				Debug.Log($"No database found, creating new database in: {path}");
				AssetDatabase.CreateAsset(db, path);
			}
			return db;
		}

		[MenuItem("Tools/uNode/Generate C# Scripts", false, 22)]
		public static void GenerateCSharpScript() {
			if(preferenceData.generatorData.compilationMethod == CompilationMethod.Unity) {
				CompileProjectGraphs();
			} else {
				if(Directory.Exists(projectScriptPath)) {
					Debug.LogWarning($"Warning: You're using Roslyn Compilation method but there's a generated script located on: {projectScriptPath} folder, please delete it to ensure script is working.\nIf the generated script in {projectScriptPath} folder still exist the graph will run with that script.");
				}
				if(preferenceData.generatorData.compileInBackground) {
					CompileGraphsInBackground();
				} else {
					CompileProjectGraphs(true, false);
				}
			}
		}

		/// <summary>
		/// Compile project graph.
		/// </summary>
		public static void CompileProjectGraphs(bool saveInTemporaryFolder = false, bool force = true) {
			try {
				var scripts = GenerationUtility.GenerateProjectScripts(force);
				var db = GetDatabase();
				EditorUtility.DisplayProgressBar("Saving Scripts", "", 1);
				string dir;
				List<string> scriptPaths = null;
				if(saveInTemporaryFolder) {
					dir = tempRoslynFolder;
					scriptPaths = new List<string>();
				} else {
					dir = projectScriptPath;
				}
				Directory.CreateDirectory(dir);
				var dateUID = DateTime.Now.GetTimeUID();
				foreach(var script in scripts) {
					var path = Path.GetFullPath(dir) + Path.DirectorySeparatorChar + script.fileName + ".cs";
					var assetPath = AssetDatabase.GetAssetPath(script.graphOwner);
					if(File.Exists(assetPath.RemoveLast(6).Add("cs"))) {
						//Skip when the graph has been compiled manually
						continue;
					}
					if(saveInTemporaryFolder)
						scriptPaths.Add(path);
					List<ScriptInformation> informations;
					var generatedScript = script.ToScript(out informations, true);
					using(StreamWriter sw = new StreamWriter(path)) {
						if(informations != null) {
							uNodeEditor.SavedData.RegisterGraphInfos(informations, script.graphOwner, path);
						}
						sw.Write(ConvertLineEnding(generatedScript, Application.platform != RuntimePlatform.WindowsEditor));
						sw.Close();
					}
					if(uNodeEditorUtility.IsPrefab(script.graphOwner)) {
						var scriptData = persistenceData.GetGraphData(script.graphOwner);
						scriptData.path = path;
						scriptData.compiledHash = AssetDatabase.GetAssetDependencyHash(AssetDatabase.GetAssetPath(script.graphOwner));
						scriptData.lastCompiledID = script.GetSettingUID();
						scriptData.generatedScript = generatedScript;
					}
					foreach(var root in script.graphs) {
						if(db.graphDatabases.Any(g => g.graph == root)) {
							continue;
						}
						db.graphDatabases.Add(new uNodeResourceDatabase.RuntimeGraphDatabase() {
							graph = root,
						});
						EditorUtility.SetDirty(db);
					}
				}
				if(saveInTemporaryFolder) {
					EditorUtility.DisplayProgressBar("Compiling Scripts", "", 1);
					var result = EditorBinding.RoslynCompileFileAndSave(Path.GetRandomFileName(), scriptPaths, Path.GetFullPath(dir) + Path.DirectorySeparatorChar + "RuntimeAssembly.dll", false);
					if(result.errors != null && result.errors.Any()) {
						Debug.LogError(result.GetErrorMessage());
					}
				} else {
					AssetDatabase.Refresh();
					AssetDatabase.SaveAssets();
				}
				db.ClearCache();
				Debug.Log("Successful generating project script, project graphs will run with native c#." +
				"\nRemember to compiles the graph again if you made a changes to a graphs to keep the script up to date." +
				"\nRemoving generated scripts will makes the graph to run with reflection again." +
				"\nGenerated project script can be found on: " + dir);
			}
			finally {
				EditorUtility.ClearProgressBar();
			}
		}

		[MenuItem("Tools/uNode/Generate C# including Scenes", false, 23)]
		public static void GenerateCSharpScriptIncludingSceneGraphs() {
			if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
				return;
			}
			CompileProjectGraphs();
			GenerateCSharpScriptForSceneGraphs();
		}

		public static void GenerateCSharpScriptForSceneGraphs() {
			DeleteGeneratedCSharpScriptForScenes();//Removing previous files so there's no outdated scripts
			var scenes = EditorBuildSettings.scenes;
			var dir = projectSceneScriptPath;
			// uNodeEditorUtility.FindAssetsByType<SceneAsset>();
			for (int i = 0; i < scenes.Length;i++) {
				var scene = scenes[i];
				var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
				if(sceneAsset == null || !scene.enabled) continue;
				EditorUtility.DisplayProgressBar($"Loading Scene: {sceneAsset.name} {i+1}-{scenes.Length}", "", 0);
				while(uNodeThreadUtility.IsNeedUpdate()) {
					uNodeThreadUtility.Update();
				}
				GraphUtility.DestroyTempGraph();
				var currentScene = EditorSceneManager.OpenScene(scene.path);
				var graphs = GameObject.FindObjectsOfType<uNodeComponentSystem>().Select(item => item.gameObject).Distinct().ToArray();
				var scripts = new List<CG.GeneratedData>();
				int count = 0;
				foreach(var graph in graphs) {
					count++;
					scripts.Add(GenerationUtility.GenerateCSharpScript(graph, true, (progress, info) => {
						EditorUtility.DisplayProgressBar($"Generating C# for: {sceneAsset.name} {i+1}-{scenes.Length} current: {count}-{graphs.Length}", info, progress);
					}));
				}
				while(uNodeThreadUtility.IsNeedUpdate()) {
					uNodeThreadUtility.Update();
				}
				GraphUtility.DestroyTempGraph();
				EditorSceneManager.SaveScene(currentScene);
				EditorUtility.DisplayProgressBar("Saving Scene Scripts", "", 1);
				Directory.CreateDirectory(dir);
				var startPath = Path.GetFullPath(dir) + Path.DirectorySeparatorChar;
				foreach(var script in scripts) {
					var path = startPath + currentScene.name + "_" + script.fileName + ".cs";
					int index = 1;
					while(File.Exists(path)) {//Ensure name to be unique
						path = startPath + currentScene.name + "_" + script.fileName + index + ".cs";
						index++;
					}
					using(StreamWriter sw = new StreamWriter(path)) {
						List<ScriptInformation> informations;
						var generatedScript = script.ToScript(out informations, true);
						if(informations != null) {
							uNodeEditor.SavedData.RegisterGraphInfos(informations, script.graphOwner, path);
						}
						sw.Write(GenerationUtility.ConvertLineEnding(generatedScript, false));
						sw.Close();
					}
				}
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.ClearProgressBar();
			Debug.Log("Successful generating scenes script, existing scenes graphs will run with native c#." +
			"\nRemember to compiles the graph again if you made a changes to a graphs to keep the script up to date." + 
			"\nRemoving generated scripts will makes the graph to run with reflection again." + 
			"\nGenerated scenes script can be found on: " + dir);
		}


		private static bool isGeneratingInBackground;
		/// <summary>
		/// Compile project runtime graphs in the background.
		/// </summary>
		public static void CompileGraphsInBackground() {
			if(!isGeneratingInBackground) {
				isGeneratingInBackground = true;
				uNodeThreadUtility.CreateThread(DoCompileGraphsInBackground).Start();
			}
		}

		/// <summary>
		/// Generate and compile all runtime graphs in background.
		/// Note: don't call it from main thread.
		/// </summary>
		public static void DoCompileGraphsInBackground() {
			try {
				uNodeThreadUtility.WaitOneFrame();
				var dir = tempRoslynFolder;
				var dirInfo = Directory.CreateDirectory(dir);
				var scripts = GenerationUtility.GenerateRuntimeGraphAsync(false);
				var scriptPaths = new List<string>();
				int skippedCount = 0;
				uNodeThreadUtility.QueueAndWait(() => {
					var db = GetDatabase();
					EditorProgressBar.ShowProgressBar("Saving Scripts", 1);
					foreach(var script in scripts) {
						if(script == null)
							continue;
						var path = Path.GetFullPath(dir) + Path.DirectorySeparatorChar + script.fileName + ".cs";
						//var assetPath = AssetDatabase.GetAssetPath(script.graphOwner);
						//if(File.Exists(assetPath.RemoveLast(6).Add("cs"))) {
						//	//Skip when the graph has been compiled manually
						//	continue;
						//}
						if(!script.isValid) {
							if(File.Exists(path)) {
								scriptPaths.Add(path);
								skippedCount++;
							}
							if(script.hasError) {
								foreach(var e in script.errors) {
									if(e is uNodeException) {
										uNodeDebug.LogException(e, (e as uNodeException).graphReference);
									} else {
										uNodeDebug.LogException(e, script.graphOwner);
									}
								}
							}
							continue;
						}
						List<ScriptInformation> informations;
						var generatedScript = script.ToScript(out informations, true);
						using(StreamWriter sw = new StreamWriter(path)) {
							if(informations != null) {
								uNodeEditor.SavedData.RegisterGraphInfos(informations, script.graphOwner, path);
							}
							sw.Write(ConvertLineEnding(generatedScript, Application.platform != RuntimePlatform.WindowsEditor));
							sw.Close();
						}
						scriptPaths.Add(path);
						if(uNodeEditorUtility.IsPrefab(script.graphOwner)) {
							var scriptData = persistenceData.GetGraphData(script.graphOwner);
							scriptData.path = path;
							scriptData.compiledHash = AssetDatabase.GetAssetDependencyHash(AssetDatabase.GetAssetPath(script.graphOwner));
							scriptData.lastCompiledID = script.GetSettingUID();
							if(scriptData.generatedScript != generatedScript) {
								scriptData.generatedScript = generatedScript;
							} else {
								skippedCount++;
							}
						}
						foreach(var root in script.graphs) {
							if(db.graphDatabases.Any(g => g.graph == root)) {
								continue;
							}
							db.graphDatabases.Add(new uNodeResourceDatabase.RuntimeGraphDatabase() {
								graph = root,
							});
							EditorUtility.SetDirty(db);
						}
					}
					db.ClearCache();
					//foreach(var file in dirInfo.EnumerateFiles()) {
					//	if(file.Extension == ".cs") {
					//		scriptPaths.Add(Path.GetFullPath(dir) + Path.DirectorySeparatorChar + file.Name);
					//	}
					//}
					//Debug.Log("Successful generating project script, project graphs will run with native c#." +
					//"\nRemember to compiles the graph again if you made a changes to a graphs to keep the script up to date." +
					//"\nRemoving generated scripts will makes the graph to run with reflection again." +
					//"\nGenerated project script can be found on: " + dir);
				});
				if(scriptPaths.Count != skippedCount || !File.Exists(tempAssemblyPath)) {
					uNodeThreadUtility.QueueAndWait(() => {
						EditorProgressBar.ShowProgressBar("Compiling Scripts", 1);
					});
					var result = EditorBinding.RoslynCompileFileAndSave(Path.GetRandomFileName(), scriptPaths, Path.GetFullPath(dir) + Path.DirectorySeparatorChar + "RuntimeAssembly.dll", false);
					if(result.errors != null && result.errors.Any()) {
						var map = persistenceData.graphs.ToList();
						foreach(var error in result.errors) {
							if(!string.IsNullOrEmpty(error.fileName)) {
								foreach(var pair in map) {
									if(pair.Value.path == error.fileName && !error.isWarning) {
										//Make sure to reset the hash for graph that has error message.
										pair.Value.compiledHash = default;
										pair.Value.generatedScript = string.Empty;
									}
								}
							}
							//uNodeDebug.LogError(error.errorMessage);
						}
						Debug.LogError(result.GetErrorMessage());
						//Debug.LogError($"Error compiling graphs. {uNodeLogger.uNodeConsoleWindow.KEY_OpenConsole}\n" + result.GetErrorMessage());
					}
				}
			}
			finally {
				isGeneratingInBackground = false;
				uNodeThreadUtility.Queue(() => {
					EditorProgressBar.ClearProgressBar();
				});
			}
		}

		#region Delete Generated Script
		[MenuItem("Tools/uNode/Delete Generated C# Scripts", false, 24)]
		public static void DeleteGeneratedCSharpScript() {
			EditorUtility.DisplayProgressBar("Deleting Generated C# Scripts", "", 1);
			if(Directory.Exists(projectScriptPath)) {
				Directory.Delete(projectScriptPath, true);
			}
			if(File.Exists(projectScriptPath + ".meta")) {
				File.Delete(projectScriptPath + ".meta");
			}
			if(File.Exists(tempAssemblyPath)) {
				File.Delete(tempAssemblyPath);
			}
			AssetDatabase.Refresh();
			EditorUtility.ClearProgressBar();
		}

		//public static void DeleteGeneratedCSharpScriptForProjects() {
		//	EditorUtility.DisplayProgressBar("Deleting Generated C# Scripts", "", 1);
		//	var dir = projectScriptPath;
		//	if(Directory.Exists(dir)) {
		//		Directory.Delete(dir, true);
		//	}
		//	if(File.Exists(dir + ".meta")) {
		//		File.Delete(dir + ".meta");
		//	}
		//	AssetDatabase.Refresh();
		//	EditorUtility.ClearProgressBar();
		//}

		public static void DeleteGeneratedCSharpScriptForScenes() {
			EditorUtility.DisplayProgressBar("Deleting Generated C# Scripts", "", 1);
			var dir = projectSceneScriptPath;
			if(Directory.Exists(dir)) {
				Directory.Delete(dir, true);
			}
			if(File.Exists(dir + ".meta")) {
				File.Delete(dir + ".meta");
			}
			AssetDatabase.Refresh();
			EditorUtility.ClearProgressBar();
		}
		#endregion

		public static void GenerateNativeGraphInProject(bool enableLogging = true) {
			try {
				System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
				watch.Start();
				var scripts = GenerateNativeProjectScripts(true);
				watch.Stop();
				if(enableLogging)
					Debug.LogFormat("Generating C# took {0,8:N3} s.", watch.Elapsed.TotalSeconds);
				var dir = "TempScript" + Path.DirectorySeparatorChar + "GeneratedCSharpGraph";
				Directory.CreateDirectory(dir);
				HashSet<string> fileNames = new HashSet<string>();
				List<string> paths = new List<string>();
				Action saveAction = null;
				foreach(var script in scripts) {
					var fileName = script.fileName;
					int index = 2;
					while(!fileNames.Add(fileName)) {
						fileName = script.fileName + index;
					}
					if(CanCompileScript()) {//Save to temp
						var path = Path.GetFullPath(dir) + Path.DirectorySeparatorChar + fileName + ".cs";
						using(StreamWriter sw = new StreamWriter(path)) {
							var generatedScript = script.ToScript(out var informations, true);
							if(informations != null) {
								uNodeEditor.SavedData.RegisterGraphInfos(informations, script.graphOwner, path);
							}
							sw.Write(preferenceData.generatorData.convertLineEnding ? ConvertLineEnding(generatedScript, Application.platform != RuntimePlatform.WindowsEditor) : generatedScript);
							sw.Close();
						}
						paths.Add(path);
					}
					{//Save to project
						saveAction += () => {
							var path = (Path.GetDirectoryName(AssetDatabase.GetAssetPath(script.graphOwner)) + Path.DirectorySeparatorChar + script.graphOwner.name + ".cs");
							using(StreamWriter sw = new StreamWriter(path)) {
								var generatedScript = script.ToScript(out var informations, true);
								if(informations != null) {
									uNodeEditor.SavedData.RegisterGraphInfos(informations, script.graphOwner, path);
								}
								sw.Write(preferenceData.generatorData.convertLineEnding ? ConvertLineEnding(generatedScript, Application.platform != RuntimePlatform.WindowsEditor) : generatedScript);
								sw.Close();
							}
						};
					}
				}
				if(CanCompileScript()) {
					watch.Restart();
					EditorUtility.DisplayProgressBar("Loading", "Compiling", 1);
					CompileFromFile(paths.ToArray());
					watch.Stop();
					if(enableLogging)
						Debug.LogFormat("Compiling script took {0,8:N3} s.", watch.Elapsed.TotalSeconds);
				}
				saveAction?.Invoke();
				AssetDatabase.Refresh();
				AssetDatabase.SaveAssets();
				Debug.Log("Successful generating script for C# Graphs in the project.");
			}
			finally {
				uNodeThreadUtility.QueueOnFrame(() => {
					EditorUtility.ClearProgressBar();
				});
			}
		}

		private static CG.GeneratedData[] GenerateNativeProjectScripts(
			bool force,
			string label = "Generating C# Scripts",
			bool clearProgressOnFinish = true) {
			try {
				int count = 0;
				var objects = GraphUtility.FindGraphPrefabs().Where(g => g.GetComponent<IIndependentGraph>() == null).ToList();
				var scripts = objects.Select(gameObject => {
					count++;
					return GenerateCSharpScript(gameObject, force, (progress, text) => {
						EditorUtility.DisplayProgressBar($"{label} {count}-{objects.Count}", text, progress);
					});
				}).Where(s => s != null);
				return scripts.ToArray();
			}
			finally {
				if(clearProgressOnFinish) {
					uNodeThreadUtility.QueueOnFrame(() => {
						EditorUtility.ClearProgressBar();
					});
				}
			}
		}

		public static void CompileNativeGraphInProject() {
			var graphs = GraphUtility.FindGraphPrefabs();
			CompileNativeGraph(graphs.Where(g => g.GetComponent<IIndependentGraph>() == null));
		}

		public static void CompileNativeGraph(IEnumerable<GameObject> graphs) {
			foreach(var graph in graphs) {
				CompileNativeGraph(graph);
			}
		}

		public static void CompileNativeGraph(GameObject graphObject, bool enableLogging = true) {
			string fileName = graphObject.name;
			GameObject prefabContent = null;
			var go = graphObject;
			if(uNodeEditorUtility.IsPrefab(graphObject)) {
				if(GraphUtility.HasTempGraphObject(graphObject)) {
					go = GraphUtility.GetTempGraphObject(graphObject);
				} else {
					prefabContent = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(graphObject));
					go = prefabContent;
				}
			} else if(GraphUtility.IsTempGraphObject(graphObject)) {
				graphObject = GraphUtility.GetOriginalObject(graphObject);
			}
			Directory.CreateDirectory(GenerationUtility.tempFolder);
			char separator = Path.DirectorySeparatorChar;
			string path = GenerationUtility.tempFolder + separator + fileName + ".cs";
			try {
				System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
				watch.Start();
				var script = GenerationUtility.GenerateCSharpScript(go, true, (progress, text) => {
                    EditorUtility.DisplayProgressBar("Loading", text, progress);
                });
				List<ScriptInformation> informations;
				var generatedScript = script.ToScript(out informations, true);
				if(preferenceData.generatorData.convertLineEnding) {
					generatedScript = GenerationUtility.ConvertLineEnding(generatedScript, Application.platform != RuntimePlatform.WindowsEditor);
				}
				if(preferenceData.generatorData != null && preferenceData.generatorData.analyzeScript && preferenceData.generatorData.formatScript) {
					var codeFormatter = TypeSerializer.Deserialize("MaxyGames.uNode.Editors.CSharpFormatter", false);
					if(codeFormatter != null) {
						var str = codeFormatter.
							GetMethod("FormatCode").
							Invoke(null, new object[] { generatedScript }) as string;
						generatedScript = str;
					}
				}
				using(StreamWriter sw = new StreamWriter(path)) {
					sw.Write(generatedScript);
					sw.Close();
				}
				watch.Stop();
				if(enableLogging)
					Debug.LogFormat("Generating C# took {0,8:N3} s.", watch.Elapsed.TotalSeconds);
				if(preferenceData.generatorData.compileScript) {
					bool isBecauseOfAccessibility = false;
					try {
						watch.Reset();
						watch.Start();
						EditorUtility.DisplayProgressBar("Loading", "Compiling", 1);
						var compileResult = CompileScript(generatedScript);
						if(compileResult.assembly == null) {
							isBecauseOfAccessibility = true;
							foreach(var error in compileResult.errors) {
								if(error.errorNumber != "CS0122") {
									isBecauseOfAccessibility = false;
									break;
								}
							}
							throw new Exception(compileResult.GetErrorMessage());
						}
						watch.Stop();
#if !NET_STANDARD_2_0
						if(enableLogging)
							Debug.LogFormat("Compiling script took {0,8:N3} s.", watch.Elapsed.TotalSeconds);		
#endif
					}
					catch (System.Exception ex) {
						watch.Stop();
						EditorUtility.ClearProgressBar();
						if(EditorUtility.DisplayDialog("Compile Errors", "Compile before save detect an error: \n" + ex.Message + "\n\n" +
							(isBecauseOfAccessibility ?
								"The initial errors may because of using a private class.\nWould you like to ignore the error and save it?" : 
								"Would you like to ignore the error and save it?"),
							"Ok, save it",
							"No, don't save")) {
							Debug.Log("Compile errors: " + ex.Message);
						} else {
							Debug.Log("Temp script saved to: " + Path.GetFullPath(path));
							throw ex;
						}
					}
				}
				if(EditorUtility.IsPersistent(graphObject)) {//For prefab and asset
					path = (Path.GetDirectoryName(AssetDatabase.GetAssetPath(graphObject)) + separator + fileName + ".cs");
					if(informations != null) {
						uNodeEditor.SavedData.RegisterGraphInfos(informations, script.graphOwner, path);
					}
					using(FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write)) {
						using(StreamWriter writer = new StreamWriter(stream)) {
							writer.Write(generatedScript);
							writer.Close();
						}
						stream.Close();
					}
				} else {//For the scene object.
					path = EditorUtility.SaveFilePanel("Save Script", "Assets", fileName + ".cs", "cs");
					if(informations != null) {
						uNodeEditor.SavedData.RegisterGraphInfos(informations, script.graphOwner, path);
					}
					using(FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write)) {
						using(StreamWriter writer = new StreamWriter(stream)) {
							writer.Write(generatedScript);
							writer.Close();
						}
						stream.Close();
					}
				}
				AssetDatabase.Refresh();
				Debug.Log("Script saved to: " + Path.GetFullPath(path));
				EditorUtility.ClearProgressBar();
			}
			catch {
				EditorUtility.ClearProgressBar();
				Debug.LogError("Aborting Generating C# Script because have error.");
				throw;
			} finally {
				if(prefabContent != null) {
					PrefabUtility.UnloadPrefabContents(prefabContent);
				}
			}
		}

		public static bool LoadRuntimeAssembly() {
			var pdbPath = Path.ChangeExtension(tempAssemblyPath, ".pdb");
			if(File.Exists(tempAssemblyPath)) {
				var rawAssembly = File.ReadAllBytes(tempAssemblyPath);
				Assembly assembly;
				if(pdbPath != null) {
					var pdb = File.ReadAllBytes(pdbPath);
					assembly = Assembly.Load(rawAssembly, pdb);
				} else {
					assembly = Assembly.Load(rawAssembly);
				}
				if(assembly != null) {
					ReflectionUtils.RegisterRuntimeAssembly(assembly);
					ReflectionUtils.UpdateAssemblies();
					ReflectionUtils.GetAssemblies();
					return true;
				}
			}
			return false;
		}

		public static void CompileProjectGraphsAnonymous() {
			if(CanCompileScript()) {
				var scripts = GenerateProjectScripts(true);
				var db = GetDatabase();
				var dir = "TempScript" + Path.DirectorySeparatorChar + "GeneratedOnPlay";
				Directory.CreateDirectory(dir);
				HashSet<string> fileNames = new HashSet<string>();
				List<string> paths = new List<string>();
				foreach(var script in scripts) {
					var fileName = script.fileName;
					int index = 2;
					while(!fileNames.Add(fileName)) {
						fileName = script.fileName + index;
					}
					var path = Path.GetFullPath(dir) + Path.DirectorySeparatorChar + script.fileName + ".cs";
					using(StreamWriter sw = new StreamWriter(path)) {
						var generatedScript = script.ToScript(out var informations, true);
						if(informations != null) {
							uNodeEditor.SavedData.RegisterGraphInfos(informations, script.graphOwner, path);
						}
						sw.Write(ConvertLineEnding(generatedScript, false));
						sw.Close();
					}
					foreach(var root in script.graphs) {
						if(db.graphDatabases.Any(g => g.graph == root)) {
							continue;
						}
						db.graphDatabases.Add(new uNodeResourceDatabase.RuntimeGraphDatabase() {
							graph = root,
						});
						EditorUtility.SetDirty(db);
					}
					paths.Add(path);
				}
				ReflectionUtils.RegisterRuntimeAssembly(CompileFromFile(paths.ToArray()));
				ReflectionUtils.UpdateAssemblies();
				ReflectionUtils.GetAssemblies();
			}
		}

		internal static void CompileAndPatchProjectGraphs() {
			if(CanCompileScript() && EditorBinding.patchType != null) {
				var scripts = GenerateProjectScripts(true);
				var db = GetDatabase();
				var dir = "TempScript" + Path.DirectorySeparatorChar + "GeneratedOnPlay";
				Directory.CreateDirectory(dir);
				List<Type> types = new List<Type>();
				HashSet<string> fileNames = new HashSet<string>();
				List<string> paths = new List<string>();
				foreach(var script in scripts) {
					var fileName = script.fileName;
					int index = 2;
					while(!fileNames.Add(fileName)) {
						fileName = script.fileName + index;
					}
					var path = Path.GetFullPath(dir) + Path.DirectorySeparatorChar + script.fileName + ".cs";
					using(StreamWriter sw = new StreamWriter(path)) {
						var generatedScript = script.ToScript(out var informations, true);
						if(informations != null) {
							uNodeEditor.SavedData.RegisterGraphInfos(informations, script.graphOwner, path);
						}
						sw.Write(ConvertLineEnding(generatedScript, false));
						sw.Close();
					}
					foreach(var root in script.graphs) {
						if(db.graphDatabases.Any(g => g.graph == root)) {
							continue;
						}
						db.graphDatabases.Add(new uNodeResourceDatabase.RuntimeGraphDatabase() {
							graph = root,
						});
						EditorUtility.SetDirty(db);
					}
					paths.Add(path);
					var ns = script.Namespace;
					foreach(var pair in script.classNames) {
						if(string.IsNullOrEmpty(ns)) {
							types.Add(pair.Value.ToType(false));
						} else {
							types.Add((ns + "." + pair.Value).ToType(false));
						}
					}
				}
				var assembly = CompileFromFile(paths.ToArray());
				if(assembly == null)
					return;
				for(int i=0;i<types.Count;i++) {
					if(types[i] == null)
						continue;
					var type = assembly.GetType(types[i].FullName);
					if(type != null) {
						EditorUtility.DisplayProgressBar("Patching", "Patch generated c# into existing script.", (float)i / types.Count);
						EditorBinding.patchType(types[i], type);
					}
				}
				ReflectionUtils.RegisterRuntimeAssembly(assembly);
				ReflectionUtils.UpdateAssemblies();
				ReflectionUtils.GetAssemblies();
			}
		}

		public static CG.GeneratedData[] GenerateProjectScripts(
			bool force, 
			string label = "Generating C# Scripts", 
			bool clearProgressOnFinish = true) {
			try {
				List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
				objects.AddRange(GraphUtility.FindGraphPrefabs());
				objects.AddRange(GraphUtility.FindGraphInterfaces());
				int count = 0;
				var scripts = objects.Select(g => {
					count++;
					if (g is GameObject gameObject) {
						var graphs = gameObject.GetComponents<uNodeRoot>();
						var targetData = gameObject.GetComponent<uNodeData>();
						if (targetData != null || 
							graphs.Select(gr => GraphUtility.GetGraphSystem(gr)).All(s => !s.allowAutoCompile) || 
							graphs.Any(gr => !gr.graphData.compileToScript)) {
							return null;
						}
						return GenerateCSharpScript(gameObject, force, (progress, text) => {
							EditorUtility.DisplayProgressBar($"{label} {count}-{objects.Count}", text, progress);
						});
					} else if(g is uNodeInterface iface) {
						EditorUtility.DisplayProgressBar($"Generating interface {iface.name} {count}-{objects.Count}", "", 1);
						return GenerateCSharpScript(iface);
					} else {
						throw new InvalidOperationException(g.GetType().FullName);
					}
				}).Where(s => s != null);
				return scripts.ToArray();
			} finally {
				if (clearProgressOnFinish) {
					uNodeThreadUtility.QueueOnFrame(() => {
						EditorUtility.ClearProgressBar();
					});
				}
			}
		}

		#region GenerateCSharpScript
		public static CG.GeneratedData GenerateCSharpScript(GameObject gameObject, bool force = true, Action<float, string> updateProgress = null) {
			if(uNodeEditorUtility.IsPrefab(gameObject)) {
				GameObject temp;
				if(GraphUtility.HasTempGraphObject(gameObject)) {
					temp = GraphUtility.GetTempGraphObject(gameObject);
				} else {
					GameObject goRoot = GameObject.Find("[@GRAPHS]");
					if(goRoot == null) {
						goRoot = new GameObject("[@GRAPHS]");
						goRoot.hideFlags = HideFlags.HideAndDontSave;
						uNodeThreadUtility.Queue(() => {
							if (goRoot != null) {
								GameObject.DestroyImmediate(goRoot);
							}
						});
					}
					temp = PrefabUtility.InstantiatePrefab(gameObject, goRoot.transform) as GameObject;
					temp.SetActive(false);
				}
				var roots = temp.GetComponents<uNodeRoot>();
				var targetData = temp.GetComponent<uNodeData>();
				return GenerateCSharpScript(roots, targetData, updateProgress);
			} else {
				var roots = gameObject.GetComponents<uNodeRoot>();
				var targetData = gameObject.GetComponent<uNodeData>();
				return GenerateCSharpScript(roots, targetData, updateProgress);
			}
		}

		public static CG.GeneratedData GenerateCSharpScript(uNodeInterface ifaceAsset) {
			return CG.Generate(new CG.GeneratorSetting(ifaceAsset) {
				fullTypeName = preferenceData.generatorData.fullTypeName,
				fullComment = preferenceData.generatorData.fullComment,
				generationMode = preferenceData.generatorData.generationMode,
			});
		}

		public static CG.GeneratedData GenerateCSharpScript(IList<uNodeRoot> roots, uNodeData targetData, Action<float, string> updateProgress = null) {
			if(roots.Count == 0 && targetData == null) {
				return null;
			}
			var generatorSettings = targetData?.generatorSettings;
			string nameSpace;
			IList<string> usingNamespace;
			bool debug, debugValue;
			if(generatorSettings != null) {
				nameSpace = generatorSettings.Namespace;
				usingNamespace = generatorSettings.usingNamespace;
				debug = generatorSettings.debug;
				debugValue = generatorSettings.debugValueNode;
			} else if(roots.Count == 1 && roots[0] is IIndependentGraph graph) {
				if(graph is IMacroGraph) return null; //Ensure to bypass macro graph since we don't support it yet.
				nameSpace = graph.Namespace;
				usingNamespace = graph.UsingNamespaces;
				debug = roots[0].graphData.debug;
				debugValue = roots[0].graphData.debugValueNode;
			} else if(roots.Count > 0) {
				var root = roots.FirstOrDefault();
				nameSpace = RuntimeType.RuntimeNamespace;
				usingNamespace = (root as INamespaceSystem)?.GetNamespaces().ToList();
				debug = false;
				debugValue = false;
			} else {
				throw new InvalidOperationException();
			}
			return CG.Generate(new CG.GeneratorSetting(roots, nameSpace, usingNamespace) {
				fullTypeName = preferenceData.generatorData.fullTypeName,
				fullComment = preferenceData.generatorData.fullComment,
				generationMode = preferenceData.generatorData.generationMode,
				runtimeOptimization = preferenceData.generatorData.optimizeRuntimeCode,
				debugScript = debug,
				debugValueNode = debugValue,
				targetData = targetData,
				updateProgress = updateProgress,
			});
		}
		#endregion

		#region GenerateCSharpAsync
		/// <summary>
		/// Generate Project Script in background.
		/// Note: don't call it from main thread.
		/// </summary>
		/// <param name="force"></param>
		/// <param name="label"></param>
		/// <param name="clearProgressOnFinish"></param>
		/// <returns></returns>
		public static CG.GeneratedData[] GenerateRuntimeGraphAsync(
			bool force,
			string label = "Generating C# Scripts",
			bool clearProgressOnFinish = true) {
			try {
				List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
				uNodeThreadUtility.QueueAndWait(() => {
					//Find graphs in main thread.
					objects.AddRange(GraphUtility.FindGraphPrefabs());
					objects.AddRange(GraphUtility.FindGraphInterfaces());
				});
				int count = 0;
				List<CG.GeneratedData> scripts = new List<CG.GeneratedData>();
				foreach(var g in objects) {
					count++;
					if(g is GameObject gameObject) {
						uNodeRoot[] graphs = null;
						uNodeData targetData = null;
						uNodeThreadUtility.QueueAndWait(() => {
							graphs = gameObject.GetComponents<uNodeRoot>();
							targetData = gameObject.GetComponent<uNodeData>();
							if(!force /*&& !GraphUtility.HasTempGraphObject(gameObject)*/ && IsGraphUpToDate(gameObject)) {
								var settings = new CG.GeneratorSetting(targetData, graphs) {
									fullTypeName = true /*preferenceData.generatorData.fullTypeName*/,
									fullComment = false /*preferenceData.generatorData.fullComment*/,
									generationMode = preferenceData.generatorData.generationMode,
									runtimeOptimization = preferenceData.generatorData.optimizeRuntimeCode,
									//debugScript = debug,
									//debugValueNode = debugValue,
									targetData = targetData,
								};
								var script = new CG.GeneratedData(null, settings);
								script.InitOwner();
								scripts.Add(script);
								graphs = null;
							};
						});
						if(targetData != null || graphs == null ||
							graphs.Select(gr => GraphUtility.GetGraphSystem(gr)).All(s => !s.allowAutoCompile) ||
							graphs.Any(gr => !gr.graphData.compileToScript)) {
							continue;
						}
						scripts.Add(GenerateCSharpAsync(gameObject, (progress, text) => {
							EditorProgressBar.ShowProgressBar($"{label} {count}-{objects.Count}", (float)count / (float)objects.Count);
						}));
					} else if(g is uNodeInterface iface) {
						EditorProgressBar.ShowProgressBar($"Generating interface {iface.name} {count}-{objects.Count}", (float)count / (float)objects.Count);
						uNodeThreadUtility.QueueAndWait(() => {
							scripts.Add(GenerateCSharpScript(iface));
						});
					} else {
						throw new InvalidOperationException(g.GetType().FullName);
					}
				}
				return scripts.ToArray();
			}
			finally {
				if(clearProgressOnFinish) {
					EditorProgressBar.ClearProgressBar();
				}
			}
		}

		/// <summary>
		/// Generate CSharp Script Async.
		/// Note: Don't call in main thread.
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="force"></param>
		/// <param name="updateProgress"></param>
		/// <returns></returns>
		public static CG.GeneratedData GenerateCSharpAsync(GameObject gameObject, Action<float, string> updateProgress = null) {
			uNodeRoot[] roots = null;
			uNodeData targetData = null;
			Action postAction = null;
			uNodeThreadUtility.QueueAndWait(() => {
				if(uNodeEditorUtility.IsPrefab(gameObject)) {
					GameObject temp;
					if(GraphUtility.HasTempGraphObject(gameObject)) {
						temp = GraphUtility.GetTempGraphObject(gameObject);
					} else {
						GameObject goRoot = GameObject.Find("[@GRAPHS]");
						if(goRoot == null) {
							goRoot = new GameObject("[@GRAPHS]");
							goRoot.hideFlags = HideFlags.HideAndDontSave;
							postAction += () => {
								if(goRoot != null) {
									GameObject.DestroyImmediate(goRoot);
								}
							};
						}
						temp = PrefabUtility.InstantiatePrefab(gameObject, goRoot.transform) as GameObject;
						temp.SetActive(false);
					}
					roots = temp.GetComponents<uNodeRoot>();
					targetData = temp.GetComponent<uNodeData>();
				} else {
					roots = gameObject.GetComponents<uNodeRoot>();
					targetData = gameObject.GetComponent<uNodeData>();
				}
			});
			if(roots == null)
				return null;
			var result = GenerateCSharpAsync(roots, targetData, updateProgress);
			if(postAction != null)
				uNodeThreadUtility.QueueAndWait(postAction);
			return result;
		}

		public static CG.GeneratedData GenerateCSharpAsync(IList<uNodeRoot> roots, uNodeData targetData, Action<float, string> updateProgress = null) {
			if(roots.Count == 0 && targetData == null) {
				return null;
			}
			var generatorSettings = targetData?.generatorSettings;
			string nameSpace;
			IList<string> usingNamespace;
			bool debug, debugValue;
			if(generatorSettings != null) {
				nameSpace = generatorSettings.Namespace;
				usingNamespace = generatorSettings.usingNamespace;
				debug = generatorSettings.debug;
				debugValue = generatorSettings.debugValueNode;
			} else if(roots.Count == 1 && roots[0] is IIndependentGraph graph) {
				if(graph is IMacroGraph)
					return null; //Ensure to bypass macro graph since we don't support it yet.
				nameSpace = graph.Namespace;
				usingNamespace = graph.UsingNamespaces;
				debug = roots[0].graphData.debug;
				debugValue = roots[0].graphData.debugValueNode;
			} else if(roots.Count > 0) {
				var root = roots.FirstOrDefault();
				nameSpace = RuntimeType.RuntimeNamespace;
				usingNamespace = (root as INamespaceSystem)?.GetNamespaces().ToList();
				debug = false;
				debugValue = false;
			} else {
				throw new InvalidOperationException();
			}
			var setting = new CG.GeneratorSetting(roots, nameSpace, usingNamespace) {
				fullTypeName = true /*preferenceData.generatorData.fullTypeName*/,
				fullComment = false /*preferenceData.generatorData.fullComment*/,
				generationMode = preferenceData.generatorData.generationMode,
				runtimeOptimization = preferenceData.generatorData.optimizeRuntimeCode,
				debugScript = debug,
				debugValueNode = debugValue,
				targetData = targetData,
				updateProgress = updateProgress,
				isAsync = true,
			};
			return CG.Generate(setting);
		}
		#endregion

		public static string ConvertLineEnding(string text, bool isUnixFormat) {
			var regex = new System.Text.RegularExpressions.Regex(@"(?<!\r)\n");
			const string LineEnd = "\r\n";

			string originalText = text;
			string changedText;
			changedText = regex.Replace(originalText, LineEnd);
			if(isUnixFormat) {
				changedText = changedText.Replace(LineEnd, "\n");
			}
			return changedText;
		}

		#region Compile
		public static CompileResult CompileScript(params string[] source) {
			var csharpParserType = EditorBinding.roslynUtilityType;
			if(csharpParserType != null) {
				var method = csharpParserType.GetMethod("CompileScript", new Type[] { typeof(IEnumerable<string>) });
				if(method != null) {
					return method.Invoke(null, new object[] { source as IEnumerable<string> }) as CompileResult;
				}
			}
#if !NET_STANDARD_2_0
			var provider = new Microsoft.CSharp.CSharpCodeProvider();
			var param = new System.CodeDom.Compiler.CompilerParameters();
			foreach (var assembly in EditorReflectionUtility.GetAssemblies()) {
				try {
					if (!string.IsNullOrEmpty(assembly.Location)) {
						param.ReferencedAssemblies.Add(assembly.Location);
					}
				} catch { continue; }
			}
			// Generate a dll in memory
			param.GenerateExecutable = false;
			param.GenerateInMemory = true;
			param.TreatWarningsAsErrors = false;
			param.IncludeDebugInformation = true;
			Directory.CreateDirectory(tempGeneratedFolder);
			param.TempFiles = new System.CodeDom.Compiler.TempFileCollection(tempGeneratedFolder, true);
			//No Waring
			//param.WarningLevel = 0;
			// Compile the source
			var result = provider.CompileAssemblyFromSource(param, source);

			List<CompileResult.CompileError> compileErrors = new List<CompileResult.CompileError>();
			if (result.Errors.Count > 0) {
				foreach (System.CodeDom.Compiler.CompilerError error in result.Errors) {
					compileErrors.Add(new CompileResult.CompileError() {
						fileName = error.FileName,
						isWarning = error.IsWarning,
						errorText = error.ErrorText,
						errorNumber = error.ErrorNumber,
						errorLine = error.Line,
						errorColumn = error.Column,
					});
				}
			}

			// Return the assembly
			return new CompileResult() {
				assembly = result.CompiledAssembly,
				errors = compileErrors,
			};
#else
			Debug.Log("Compiling script is disable due to unsupported in .NET Standard 2.0, change API compativility level to .NET 4.x to enable it or import CSharp Parser add-ons to compile with Roslyn instead.");
			return new CompileResult();
#endif
		}

		public static bool CanCompileScript() {
			if(EditorBinding.roslynUtilityType != null) {
				return true;
			}
#if NET_STANDARD_2_0
			return false;
#else
			return true;
#endif
		}

		public static Assembly Compile(params string[] source) {
			var csharpParserType = EditorBinding.roslynUtilityType;
			if(csharpParserType != null) {
				var method = csharpParserType.GetMethod("CompileScript", new Type[] { typeof(IEnumerable<string>) });
				if(method != null) {
					var compileResult = method.Invoke(null, new object[] { source as IEnumerable<string> }) as CompileResult;
					if(compileResult.assembly == null) {
						//compileResult.LogErrors();
						throw new Exception(compileResult.GetErrorMessage());
					}
					return compileResult.assembly;
				}
			}
#if !NET_STANDARD_2_0
			var provider = new Microsoft.CSharp.CSharpCodeProvider();
			var param = new System.CodeDom.Compiler.CompilerParameters();
			foreach (var assembly in EditorReflectionUtility.GetAssemblies()) {
				try {
					if (!string.IsNullOrEmpty(assembly.Location)) {
						param.ReferencedAssemblies.Add(assembly.Location);
					}
				} catch { continue; }
			}
			// Generate a dll in memory
			param.GenerateExecutable = false;
			param.GenerateInMemory = true;
			param.TreatWarningsAsErrors = false;
			param.IncludeDebugInformation = true;
			Directory.CreateDirectory(tempGeneratedFolder);
			param.TempFiles = new System.CodeDom.Compiler.TempFileCollection(tempGeneratedFolder, true);
			//No Waring
			//param.WarningLevel = 0;
			// Compile the source
			var result = provider.CompileAssemblyFromSource(param, source);

			if (result.Errors.Count > 0) {
				var msg = new System.Text.StringBuilder();
				bool hasError = false;
				foreach (System.CodeDom.Compiler.CompilerError error in result.Errors) {
					//Debug.LogError("Error (" + error.ErrorNumber + "): " + error.ErrorText + "\n");
					if (error.IsWarning) {
						Debug.LogWarningFormat("Warning ({0}): {1}\nin line: {2}",
						error.ErrorNumber, error.ErrorText, error.Line);
					} else {
						hasError = true;
						msg.AppendFormat("Error ({0}): {1}\nin line: {2}:{3}\nin file:{4}\n", error.ErrorNumber, error.ErrorText, error.Line, error.Column, error.FileName);
					}
				}
				if (hasError)
					throw new Exception(msg.ToString());
			}

			// Return the assembly
			return result.CompiledAssembly;
#else
			Debug.Log("Compiling script is disable due to unsupported in .NET Standard 2.0, change API compativility level to .NET 4.x to enable it or import CSharp Parser add-ons to compile with Roslyn instead.");
			return null;
#endif
		}

		public static Assembly CompileFromFile(params string[] files) {
			var csharpParserType = EditorBinding.roslynUtilityType;
			if(csharpParserType != null) {
				//var method = csharpParserType.GetMethod("CompileScript", new Type[] { typeof(IEnumerable<string>) });
				//if(method != null) {
				//	List<string> scripts = new List<string>();
				//	foreach(var file in files) {
				//		scripts.Add(File.ReadAllText(file));
				//	}
				//	var compileResult = method.Invoke(null, new object[] { scripts as IEnumerable<string> }) as CompileResult;
				//	if(compileResult.assembly == null) {
				//		//compileResult.LogErrors();
				//		throw new Exception(compileResult.GetErrorMessage());
				//	}
				//	return compileResult.assembly;
				//}
				var method = csharpParserType.GetMethod("CompileFiles", new Type[] { typeof(IEnumerable<string>) });
				if(method != null) {
					var compileResult = method.Invoke(null, new object[] { files as IEnumerable<string> }) as CompileResult;
					if(compileResult.assembly == null) {
						//compileResult.LogErrors();
						throw new Exception(compileResult.GetErrorMessage());
					}
					return compileResult.assembly;
				}
			}
#if !NET_STANDARD_2_0
			var provider = new Microsoft.CSharp.CSharpCodeProvider();
			var param = new System.CodeDom.Compiler.CompilerParameters();
			foreach (var assembly in EditorReflectionUtility.GetAssemblies()) {
				try {
					if (!string.IsNullOrEmpty(assembly.Location)) {
						param.ReferencedAssemblies.Add(assembly.Location);
					}
				} catch { continue; }
			}
			// Generate a dll in memory
			param.GenerateExecutable = false;
			param.GenerateInMemory = true;
			param.TreatWarningsAsErrors = false;
			param.IncludeDebugInformation = true;
			//No Waring
			//param.WarningLevel = 0;
			// Compile the source
			var result = provider.CompileAssemblyFromFile(param, files);
			if (result.Errors.Count > 0) {
				var msg = new System.Text.StringBuilder();
				bool hasError = false;
				foreach (System.CodeDom.Compiler.CompilerError error in result.Errors) {
					//Debug.LogError("Error (" + error.ErrorNumber + "): " + error.ErrorText + "\n");
					if (error.IsWarning) {
						Debug.LogWarningFormat("Warning ({0}): {1}\nin line: {2}",
						error.ErrorNumber, error.ErrorText, error.Line);
					} else {
						hasError = true;
						msg.AppendFormat("Error ({0}): {1}\nin line: {2}:{3}\nin file:{4}\n", error.ErrorNumber, error.ErrorText, error.Line, error.Column, error.FileName);
					}
				}
				if (hasError)
					throw new Exception(msg.ToString());
			}

			// Return the assembly
			return result.CompiledAssembly;
#else
			Debug.Log("Compiling script is disable due to unsupported in .NET Standard 2.0, change API compativility level to .NET 4.x to enable it or import CSharp Parser add-ons to compile with Roslyn instead.");
			return null;
#endif
		}
		#endregion
	}
}