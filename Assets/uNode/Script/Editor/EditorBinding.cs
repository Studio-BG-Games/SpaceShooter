using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Reflection;
using System.Threading;
//using UnityEngine.Events;
//using UnityEngine.SceneManagement;

namespace MaxyGames.uNode.Editors {
	public static class EditorBinding {
		public static Action<Type, Type> patchType;
		public static Action<GameObject, UnityEngine.Object> savePrefabAsset;

		public static event EditorSceneManager.NewSceneCreatedCallback onNewSceneCreated;
		public static event EditorSceneManager.SceneClosingCallback onSceneClosing;
		//public static UnityAction<Scene, Scene> onSceneChanged;
		public static event EditorSceneManager.SceneSavingCallback onSceneSaving;
		public static event EditorSceneManager.SceneSavedCallback onSceneSaved;
		public static event EditorSceneManager.SceneOpeningCallback onSceneOpening;
		public static event EditorSceneManager.SceneOpenedCallback onSceneOpened;
		public static event Action onFinishCompiling;

		public static Type roslynUtilityType => "MaxyGames.uNode.Editors.RoslynUtility".ToType(false);
		private static MethodInfo _compileFileAndSave;
		private static Thread mainThread;

		public static bool IsInMainThread() {
			return mainThread == Thread.CurrentThread;
		}

		public static CompileResult RoslynCompileFileAndSave(string assemblyName, IEnumerable<string> files, string savePath, bool loadAssembly) {
			if(roslynUtilityType != null) {
				if(_compileFileAndSave == null)
					_compileFileAndSave = roslynUtilityType.GetMethod("CompileFilesAndSave");
				return _compileFileAndSave.InvokeOptimized(null, assemblyName, files, savePath, loadAssembly) as CompileResult;
			}
			return null;
		}

		[InitializeOnLoadMethod]
		internal static void OnInitialize() {
			GraphUtility.Initialize();
			EditorSceneManager.newSceneCreated += onNewSceneCreated;
			EditorSceneManager.sceneClosing += onSceneClosing;
			EditorSceneManager.sceneSaving += onSceneSaving;
			EditorSceneManager.sceneSaved += onSceneSaved;
			EditorSceneManager.sceneOpening += onSceneOpening;
			EditorSceneManager.sceneOpened += onSceneOpened;
			mainThread = Thread.CurrentThread;
		}

		[UnityEditor.Callbacks.DidReloadScripts]
		internal static void OnScriptReloaded() {
			uNodeEditor.OnFinishedCompiling();
			if(onFinishCompiling != null)
				onFinishCompiling();
		}
	}
}