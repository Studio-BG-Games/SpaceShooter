using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MaxyGames.uNode;

namespace MaxyGames {
	public static class uNodeSingleton {
		private static Dictionary<Type, MonoBehaviour> instanceMaps = new Dictionary<Type, MonoBehaviour>();
		/// <summary>
		/// Get the singleton instance
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetInstance<T>() where T : MonoBehaviour {
			MonoBehaviour instance;
			if(!instanceMaps.TryGetValue(typeof(T), out instance) || instance == null) {
				var objs = GameObject.FindObjectsOfType<T>();
                T result = objs.FirstOrDefault();
                if(result == null) {
					var db = uNodeUtility.GetDatabase()?.GetGraphDatabase<T>();
					if(db != null && db.graph != null) {
						if(db.graph is uNodeComponentSingleton singleton) {
							singleton.EnsureInitialized();
							if(singleton.runtimeBehaviour is T) {
								instanceMaps[typeof(T)] = singleton.runtimeBehaviour;
								return singleton.runtimeBehaviour as T;
							} else if(singleton.runtimeInstance is T) {
								instanceMaps[typeof(T)] = singleton.runtimeInstance;
								return singleton.runtimeInstance as T;
							}
						} else {
							throw new Exception("The graph is not singleton, cannot get the singleton instance.");
						}
					} else {
						throw new NullReferenceException($"The graph database:{typeof(T).FullName} was not found.  Please update database in menu 'Tools > uNode > Update Graph Database' to fix this.");
					}
				}
                instanceMaps[typeof(T)] = result;
                return result;
			}
			return instance as T;
		}

		/// <summary>
		/// Get the graph singleton instance by its unique identifier
		/// </summary>
		/// <param name="uniqueIdentifier"></param>
		/// <returns></returns>
		public static IRuntimeClass GetGraphInstance(string uniqueIdentifier) {
			var db = uNodeUtility.GetDatabase()?.GetGraphDatabase(uniqueIdentifier);
			if(db != null) {
				var graph = db.graph as uNodeComponentSingleton;
				if(graph != null) {
					graph.EnsureInitialized();
					return graph.RuntimeClass;
				} else {
					Debug.LogError("The graph reference is missing. Please update database in menu 'Tools > uNode > Update Graph Database' to fix this.");
				}
			} else {
				Debug.LogError("The graph database was not found. Please update database in menu 'Tools > uNode > Update Graph Database' to fix this.");
			}
			return null;
		}

		internal static Dictionary<uNodeRoot, uNodeRuntime> graphSingletons = new Dictionary<uNodeRoot, uNodeRuntime>(EqualityComparer<uNodeRoot>.Default);
		/// <summary>
		/// Get the graph singleton instance
		/// </summary>
		/// <param name="graphSingleton"></param>
		/// <returns></returns>
		internal static uNodeRuntime GetRuntimeGraph(uNodeComponentSingleton graphSingleton) {
			uNodeRuntime instance;
			if(!graphSingletons.TryGetValue(graphSingleton, out instance) || instance == null) {
				var objs = GameObject.FindObjectsOfType<uNodeRuntime>();
				instance = objs.FirstOrDefault(g => g.GraphName == graphSingleton.GraphName);
				if(instance == null) {
					GameObject mainObject = new GameObject($"[Singleton:{graphSingleton.GraphName}]");
					if(graphSingleton.IsPersistence) {
						GameObject.DontDestroyOnLoad(mainObject);
					}
					uNodeRoot graph = UnityEngine.Object.Instantiate(graphSingleton);
					uNodeRuntime main = mainObject.AddComponent<uNodeRuntime>();
					main.originalGraph = graphSingleton;
					main.Name = graphSingleton.GraphName;//This will ensure the uid is valid
					main.Variables = graph.Variables;
					main.RootObject = graph.RootObject;
					main.RootObject.transform.SetParent(mainObject.transform);
					AnalizerUtility.RetargetNodeOwner(graph, main, main.RootObject.GetComponentsInChildren<MonoBehaviour>(true));
					main.Refresh();
					UnityEngine.Object.Destroy(graph.gameObject);
					instance = main;
					graphSingletons[instance] = instance;
				}
			}
			return instance;
		}

		internal static Dictionary<uNodeRoot, RuntimeBehaviour> nativeGraphSingletons = new Dictionary<uNodeRoot, RuntimeBehaviour>(EqualityComparer<uNodeRoot>.Default);
		internal static RuntimeBehaviour GetNativeGraph(uNodeComponentSingleton graphSingleton) {
			RuntimeBehaviour instance;
			if (!nativeGraphSingletons.TryGetValue(graphSingleton, out instance) || instance == null) {
				var type = graphSingleton.GeneratedTypeName.ToType(false);
				if(type == null) return null;
				var objs = GameObject.FindObjectsOfType(type);
				instance = objs.FirstOrDefault() as RuntimeBehaviour;
				if (instance == null) {
					GameObject mainObject = new GameObject($"[Singleton:{graphSingleton.GraphName}]");
					if(graphSingleton.IsPersistence) {
						GameObject.DontDestroyOnLoad(mainObject);
					}
					instance = mainObject.AddComponent(type) as RuntimeBehaviour;
					//Initialize the references
					var references = graphSingleton.graphData.unityObjects;
					for (int i = 0; i < references.Count;i++) {
						instance.SetVariable(references[i].name, references[i].value);
					}
					//Initialize the variable
					for(int i = 0; i < graphSingleton.Variables.Count; i++) {
						VariableData var = graphSingleton.Variables[i];
						instance.SetVariable(var.Name, var.value);
					}
				}
				nativeGraphSingletons[graphSingleton] = instance;
			}
			return instance;
		}

		// private static Dictionary<Type, ScriptableObject> assetInstanceMaps = new Dictionary<Type, ScriptableObject>();
		// public static T GetAssetInstance<T>() where T : ScriptableObject {
		// 	ScriptableObject instance;
		// 	if(!assetInstanceMaps.TryGetValue(typeof(T), out instance) || instance == null) {
		// 		T result = ScriptableObject.CreateInstance<T>();
		// 		assetInstanceMaps[typeof(T)] = result;
        //         return result;
		// 	}
		// 	return instance as T;
		// }
	}
}