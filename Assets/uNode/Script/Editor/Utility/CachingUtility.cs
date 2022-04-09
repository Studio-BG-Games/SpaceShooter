using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode.Editors {
	public static class CachingUtility {
		#region Classes
		class CachedData {
			public List<string> projectGraphPaths;
			[NonSerialized]
			public GameObject[] projectGraphs;
		}
		#endregion

		#region Fields
		private static CachedData _cachedData;
		private static CachedData cachedData {
			get {
				if(_cachedData == null) {
					_cachedData = LoadCache();
				}
				return _cachedData;
			}
		}
		#endregion

		static void SaveCache() {
			uNodeEditorUtility.SaveEditorData(_cachedData, "CachedData");
		}

		static CachedData LoadCache() {
			_cachedData = uNodeEditorUtility.LoadEditorData<CachedData>("CachedData");
			if(_cachedData == null) {
				_cachedData = new CachedData();
				SaveCache();
			}
			return _cachedData;
		}

		public static void MarkDirtyGraph() {
			bool flag = cachedData.projectGraphPaths != null;
			cachedData.projectGraphs = null;
			cachedData.projectGraphPaths = null;
			if(flag) {
				SaveCache();
			}
		}

		public static GameObject[] FindGraphsInProject() {
			if(cachedData.projectGraphs == null) {
				if(cachedData.projectGraphPaths == null) {
					UnityEngine.Profiling.Profiler.BeginSample("uNode.Caching.FindGraphAssets");
					cachedData.projectGraphs = uNodeEditorUtility.FindPrefabsOfType<uNodeComponentSystem>().ToArray();
					cachedData.projectGraphPaths = cachedData.projectGraphs.Select(g => AssetDatabase.GetAssetPath(g)).ToList();
					SaveCache();
					UnityEngine.Profiling.Profiler.EndSample();
				} else {
					UnityEngine.Profiling.Profiler.BeginSample("uNode.Caching.LoadGraphAssets");
					List<GameObject> prefabs = new List<GameObject>();
					for(int i=0;i< cachedData.projectGraphPaths.Count;i++) {
						var path = cachedData.projectGraphPaths[i];
						var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
						if(prefab == null) {
							cachedData.projectGraphPaths.RemoveAt(i);
							i--;
							continue;
						}
						prefabs.Add(prefab);
					}
					cachedData.projectGraphs = prefabs.ToArray();
					UnityEngine.Profiling.Profiler.EndSample();
				}
			}
			return cachedData.projectGraphs;
		}
	}
}