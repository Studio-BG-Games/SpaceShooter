using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode {
	public static class RuntimeGraphManager {
		class RuntimeGraphData {
			public uNodeRoot graph;
			public uNodeRoot originalGraph;
			public Action<uNodeRoot> retargetAction;
		}

		static Dictionary<uNodeRoot, RuntimeGraphData> runtimeGraphMap = new Dictionary<uNodeRoot, RuntimeGraphData>(EqualityComparer<uNodeRoot>.Default);

#if UNITY_2019_3_OR_NEWER
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Init() {
			runtimeGraphMap.Clear();
		}
#endif

		static RuntimeGraphData GetCachedGraph(uNodeRoot targetGraph) {
			RuntimeGraphData graphData;
			if(!runtimeGraphMap.TryGetValue(targetGraph, out graphData)) {
				uNodeRoot graph = Object.Instantiate(targetGraph);//Begin instantiate original graph
				graph.gameObject.name = targetGraph.gameObject.name;//Ensure the cached graph name is same with the original graph
#if !UNODE_DEBUG
				graph.gameObject.hideFlags = HideFlags.HideInHierarchy;//Ensure the cached graph doesn't show in the hierarchy
#endif
				Object.DontDestroyOnLoad(graph.gameObject);//Ensure this cached graph is cached across scenes
				graphData = new RuntimeGraphData() {
					graph = graph,
					originalGraph = targetGraph,
					retargetAction = AnalizerUtility.GetRetargetNodeOwnerAction(graph, graph.RootObject?.GetComponentsInChildren<MonoBehaviour>(true)),
				};
				runtimeGraphMap[targetGraph] = graphData;
			}
			return graphData;
		}

		public static uNodeRuntime InstantiateGraph(uNodeRoot targetGraph, GameObject destination, IList<VariableData> variables) {
			try {
				var data = GetCachedGraph(targetGraph);
				uNodeRuntime main = destination.AddComponent<uNodeRuntime>();
				data.retargetAction?.Invoke(main);//This will retarget the node owner to the new owner
				var graph = Object.Instantiate(data.graph);//Instantiate the cached graph
				main.originalGraph = targetGraph;//Assign the original graph
				main.Name = targetGraph.GraphName;//This will ensure the uid is valid
				var defaultVariable = graph.Variables;
				main.RootObject = graph.RootObject;
				main.RootObject.transform.SetParent(destination.transform);
				Object.Destroy(graph.gameObject);//Clean up cached graph
				main.Refresh();//Refresh the graph so it has up to date data
#if !UNODE_DEBUG_PLUS
				main.hideFlags = HideFlags.HideInInspector;//Ensure the graph doesn't show up in the inspector
#else
				main.RootObject.hideFlags = HideFlags.None;
#endif
				//Initialize variable values
				main.Variables = defaultVariable;//This is for set variable value to be same with the graph
				if(variables != null) {//This is for set variable value to same with overridden variable in instanced graph
					for(int i = 0; i < main.Variables.Count; i++) {
						VariableData var = main.Variables[i];
						for(int x = 0; x < variables.Count; x++) {
							if(var.Name.Equals(variables[x].Name)) {
								main.Variables[i] = variables[x];
							}
						}
					}
				}
				return main;
			}
			catch (Exception ex) {
				Debug.LogError($"Error on trying to initialize graph: {targetGraph.GraphName} to the {destination}.\nError: {ex.ToString()}", targetGraph);
				throw;
			}
		}
	}
}