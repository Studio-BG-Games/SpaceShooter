using MaxyGames.uNode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MaxyGames {
	public static partial class CG {
		private static List<string> InitData = new List<string>();
		private static void Initialize() {
			if(graph != null) {
				uNodeUtility.TempManagement.DestroyTempObjets(); // Make sure the data are clean.
				graph.Refresh();//Ensure the data is up to date.
				if(stateGraph != null) {
					generatorData.eventNodes.AddRange(stateGraph.eventNodes);
				}
				InitStartNode();
				generatorData.allNode.AddRange(generatorData.nodeConnections.Select(pair => pair.Key));

				var flowMaps = new List<KeyValuePair<Node, bool>>();
				foreach(var nodeComp in generatorData.allNode) {
					if(nodeComp == null)
						continue;
					try {
						if(nodeComp is Node node) {
							//Skip if not flow node
							if(!HasFlowPort(node))
								continue;
							if(!uNodeUtility.IsInStateGraph(node) || /*setting.enableOptimization &&*/ IsCanBeGrouped(node)) {
								flowMaps.Add(new KeyValuePair<Node, bool>(node, true));
								continue;
							}
							flowMaps.Add(new KeyValuePair<Node, bool>(node, false));
						}
					}
					catch(System.Exception ex) {
						uNodeDebug.LogException(ex, nodeComp);
						throw;
					}
				}
				foreach(var pair in flowMaps) {
					if(pair.Value) {
						generatorData.flowNode.Add(pair.Key);
						generatorData.regularNodes.Add(pair.Key);
					} else {
						generatorData.stateNodes.Add(pair.Key);
					}
				}

				//if(stateGraph != null) {
				//	foreach(var eventNode in stateGraph.eventNodes) {
				//		if(eventNode == null)
				//			continue;
				//		//Register events.
				//		eventNode.OnGeneratorInitialize();
				//	}
				//}
				for(int i = 0; i < generatorData.allNode.Count; i++) {
					var nodeComp = generatorData.allNode[i];
					if(nodeComp == null)
						continue;
					//Register node pin for custom node.
					nodeComp.OnGeneratorInitialize();
				}
			}
		}

		private static void InitStartNode() {
			if(stateGraph != null) {
				foreach(var eventNode in stateGraph.eventNodes) {
					InitConnect(eventNode);
				}
			}
			foreach(uNodeFunction function in graph.Functions) {
				if(function != null && function.startNode) {
					InitConnect(function.startNode);
				}
			}
			foreach(var property in graph.Properties) {
				if(property != null && !property.AutoProperty) {
					if(property.getRoot != null && property.getRoot.startNode) {
						InitConnect(property.getRoot.startNode);
					}
					if(property.setRoot != null && property.setRoot.startNode) {
						InitConnect(property.setRoot.startNode);
					}
				}
			}
			foreach(uNodeConstuctor ctor in graph.Constuctors) {
				if(ctor != null && ctor.startNode) {
					InitConnect(ctor.startNode);
				}
			}
			if(stateGraph != null) {
				foreach(var eventNode in stateGraph.eventNodes) {
					foreach(var flow in eventNode.GetFlows()) {
						var tNode = flow?.GetTargetNode();
						if(tNode != null && (Nodes.HasStateFlowOutput(tNode) || Nodes.IsStackOverflow(tNode))) {
							RegisterAsStateNode(tNode);
							break;
						}
					}
				}
			}
			//Finalize the connections datas.
			foreach(var pair in generatorData.nodeConnections) {
				var data = pair.Value;
				foreach(var port in data.flowOutputs) {
					generatorData.nodeConnections.TryGetValue(port.target, out var ndata);
					ndata.flowInputs.Add(port);
					if(port.owner == data.node) {
						port.ownerData = data;
					}
					if(port.target == ndata.node) {
						port.targetData = ndata;
					}
				}
				foreach(var port in data.valueInputs) {
					generatorData.nodeConnections.TryGetValue(port.target, out var ndata);
					ndata.valueOutputs.Add(port);
					if(port.owner == data.node) {
						port.ownerData = data;
					}
					if(port.target == ndata.node) {
						port.targetData = ndata;
					}
				}
			}
		}

		public static class Nodes {
			/// <summary>
			/// Is the node is stack overflowed?
			/// </summary>
			/// <param name="node"></param>
			/// <returns></returns>
			public static bool IsStackOverflow(NodeComponent node) {
				if(!generatorData.stackOverflowMap.TryGetValue(node, out var result)) {
					result = IsRecusive(node);
					generatorData.stackOverflowMap[node] = result;
				}
				return result;
			}

			public static NData GetNodeData(NodeComponent node) {
				if(generatorData.nodeConnections.TryGetValue(node, out var data)) {
					return data;
				}
				return null;
			}

			internal static void FindAllConnections(NData node,
				ref HashSet<NodeComponent> connections,
				bool includeFlowOutput = true,
				bool includeValueInput = true,
				bool includeFlowInput = false,
				bool includeValueOutput = false) {
				if(node != null && connections.Add(node.node)) {
					if(includeFlowOutput) {
						for(int i = 0; i < node.flowOutputs.Count; i++) {
							FindAllConnections(node.flowOutputs[i].targetData, ref connections, includeFlowOutput, includeValueInput, includeFlowInput, includeValueOutput);
						}
					}
					if(includeValueInput) {
						for(int i = 0; i < node.valueInputs.Count; i++) {
							FindAllConnections(node.valueInputs[i].targetData, ref connections, includeFlowOutput, includeValueInput, includeFlowInput, includeValueOutput);
						}
					}
					if(includeFlowInput) {
						for(int i = 0; i < node.flowInputs.Count; i++) {
							FindAllConnections(node.flowInputs[i].ownerData, ref connections, includeFlowOutput, includeValueInput, includeFlowInput, includeValueOutput);
						}
					}
					if(includeValueOutput) {
						for(int i = 0; i < node.valueOutputs.Count; i++) {
							FindAllConnections(node.valueOutputs[i].ownerData, ref connections, includeFlowOutput, includeValueInput, includeFlowInput, includeValueOutput);
						}
					}
				}
			}

			private static bool IsRecusive(NodeComponent original, NodeComponent current = null, HashSet<NodeComponent> prevs = null) {
				if(current == null)
					current = original;
				if(prevs == null)
					prevs = new HashSet<NodeComponent>();
				if(prevs.Contains(current)) {
					return false;
				}
				prevs.Add(current);
				generatorData.nodeConnections.TryGetValue(current, out var data);
				for(int i = 0; i < data.flowOutputs.Count; i++) {
					var flow = data.flowOutputs[i].target;
					if(flow == original || IsRecusive(original, flow, prevs)) {
						return true;
					}
				}
				return false;
			}

			/// <summary>
			/// Is the node has connected to a state flow in node output flow ports.
			/// </summary>
			/// <param name="node"></param>
			/// <param name="prevs"></param>
			/// <returns></returns>
			public static bool HasStateFlowOutput(NodeComponent node, HashSet<NodeComponent> prevs = null) {
				if(IsStateNode(node))
					return true;
				if(prevs == null)
					prevs = new HashSet<NodeComponent>();
				if(prevs.Contains(node)) {
					return false;
				}
				prevs.Add(node);
				generatorData.nodeConnections.TryGetValue(node, out var data);
				for(int i = 0; i < data.flowOutputs.Count; i++) {
					if(HasStateFlowOutput(data.flowOutputs[i].target, prevs)) {
						return true;
					}
				}
				return false;
			}

			/// <summary>
			/// 
			/// Is the node has connected to a state flow in node input flow ports.
			/// </summary>
			/// <param name="node"></param>
			/// <param name="prevs"></param>
			/// <returns></returns>
			public static bool HasStateFlowInput(NodeComponent node, HashSet<NodeComponent> prevs = null) {
				if(IsStateNode(node))
					return true;
				if(prevs == null)
					prevs = new HashSet<NodeComponent>();
				if(prevs.Contains(node)) {
					return false;
				}
				prevs.Add(node);
				generatorData.nodeConnections.TryGetValue(node, out var data);
				for(int i = 0; i < data.flowInputs.Count; i++) {
					if(HasStateFlowInput(data.flowInputs[i].owner, prevs)) {
						return true;
					}
				}
				return false;
			}

			#region GetFlowConnection
			/// <summary>
			/// Get flow nodes from node.
			/// </summary>
			/// <param name="node"></param>
			/// <returns></returns>
			internal static HashSet<NodeComponent> GetFlowConnection(NodeComponent node) {
				generatorData.nodeConnections.TryGetValue(node, out var data);
				return data.flowOutputNodes;
			}

			/// <summary>
			/// Find all node connection include first node.
			/// </summary>
			/// <param name="node"></param>
			/// <param name="allNode"></param>
			/// <param name="includeSuperNode"></param>
			internal static void FindAllFlowConnection(NodeComponent node, ref HashSet<NodeComponent> allNode, bool includeSuperNode = true) {
				if(node != null && !allNode.Contains(node)) {
					allNode.Add(node);
					var nodes = GetFlowConnection(node);
					if(nodes != null) {
						foreach(var n in nodes) {
							if(n) {
								FindAllFlowConnection(n, ref allNode, includeSuperNode);
							}
						}
					}
					if(includeSuperNode && node is ISuperNode) {
						ISuperNode superNode = node as ISuperNode;
						foreach(var n in superNode.nestedFlowNodes) {
							FindAllFlowConnection(n, ref allNode, includeSuperNode);
						}
					}
				}
			}

			/// <summary>
			/// Find all node connection after coroutine node.
			/// </summary>
			/// <param name="node"></param>
			/// <param name="allNode"></param>
			/// <param name="includeSuperNode"></param>
			/// <param name="includeCoroutineEvent"></param>
			internal static void FindFlowConnectionAfterCoroutineNode(NodeComponent node, ref HashSet<NodeComponent> allNode,
				bool includeSuperNode = true,
				bool includeCoroutineEvent = true,
				bool passCoroutine = false) {
				if(node != null && !allNode.Contains(node)) {
					bool isCoroutineNode = node.IsSelfCoroutine();
					if(!passCoroutine && isCoroutineNode) {
						passCoroutine = true;
					}
					if(passCoroutine && (!isCoroutineNode || includeCoroutineEvent)) {
						allNode.Add(node);
					}
					var nodes = GetFlowConnection(node);
					if(nodes != null) {
						foreach(Node n in nodes) {
							if(n) {
								FindFlowConnectionAfterCoroutineNode(n, ref allNode, includeSuperNode, includeCoroutineEvent, passCoroutine);
							}
						}
					}
					if(includeSuperNode && node is ISuperNode) {
						ISuperNode superNode = node as ISuperNode;
						foreach(var n in superNode.nestedFlowNodes) {
							FindFlowConnectionAfterCoroutineNode(n, ref allNode, includeSuperNode, includeCoroutineEvent, passCoroutine);
						}
					}
				}
			}
			#endregion
		}

		private static void InitConnect(NodeComponent node) {
			if(node != null && !generatorData.nodeConnections.TryGetValue(node, out var data)) {
				data = new NData(node);
				generatorData.nodeConnections.Add(node, data);
				if(node.IsSelfCoroutine() && uNodeUtility.IsInStateGraph(node)) {
					RegisterAsStateNode(node);
				}
				var nodes = InitConnections(node, data);
				if(nodes != null) {
					foreach(NodeComponent n in nodes) {
						if(n) {
							InitConnect(n);
						}
					}
				}
				if(node is ISuperNode) {
					ISuperNode superNode = node as ISuperNode;
					foreach(var n in superNode.nestedFlowNodes) {
						if(n != null) {
							InitConnect(n);
						}
					}
				}
				//if(node is IMacro macro) {
				//	var flows = macro.OutputFlows;
				//	if(flows != null) {
				//		foreach(var flow in flows) {
				//			if(flow != null) {
				//				ConnectNode(flow.target?.GetTargetNode());
				//			}
				//		}
				//	}
				//}
				if(node is IMacroPort) {
					var macro = node.parentComponent as IMacro;
					if(macro != null) {
						var flows = macro.OutputFlows;
						if(flows != null) {
							foreach(var flow in flows) {
								if(flow != null) {
									InitConnect(flow.target?.GetTargetNode());
								}
							}
						}
					}
				}
				if(node is BaseEventNode) {
					var eventNode = node as BaseEventNode;
					foreach(var flow in eventNode.GetFlows()) {
						InitConnect(flow?.GetTargetNode());
					}
				}
				Func<object, bool> validation = delegate (object obj) {
					if(obj is MemberData) {
						MemberData member = obj as MemberData;
						if(member.IsTargetingPortOrNode) {
							InitConnect(member.GetTargetNode());
						}
					}
					return false;
				};
				AnalizerUtility.AnalizeObject(node, validation);
			}
		}

		private static HashSet<NodeComponent> InitConnections(NodeComponent node, NData data) {
			var allNodes = new HashSet<NodeComponent>();
			if(node is StateNode) {
				StateNode eventNode = node as StateNode;
				TransitionEvent[] TE = eventNode.GetTransitions();
				foreach(TransitionEvent transition in TE) {
					var tNode = transition.GetTargetNode();
					if(tNode != null) {
						data.flowOutputs.Add(new NPData() {
							owner = node,
							connection = transition.target,
							target = tNode,
						});
						allNodes.Add(tNode);
					}
				}
			} else if(node is BaseEventNode) {
				BaseEventNode stateEvent = node as BaseEventNode;
				foreach(var member in stateEvent.GetFlows()) {
					var tNode = member.GetTargetNode();
					if(tNode != null) {
						data.flowOutputs.Add(new NPData() {
							owner = node,
							connection = member,
							target = tNode,
						});
						allNodes.Add(tNode);
					}
				}
			}
			void DoValidate(MemberData member, FieldInfo field) {
				if(member.IsTargetingPortOrNode) {
					var tNode = member.GetTargetNode();
					if(tNode != null) {
						if(member.targetType.IsTargetingFlowPort()) {
							data.flowOutputs.Add(new NPData() {
								owner = node,
								connection = member,
								target = tNode,
								localFunction = field.GetCustomAttribute<FlowOutAttribute>() is var att && att != null && att.localFunction,
							});
						} else if(member.targetType.IsTargetingValuePort()) {
							data.valueInputs.Add(new NPData() {
								owner = node,
								connection = member,
								target = tNode,
							});
						}
						allNodes.Add(tNode);
					}
				}
			}
			AnalizerUtility.AnalizeField(node, (d) => {
				var value = d.value;
				var field = d.field;
				if(d.value is MemberData) {
					DoValidate(d.value as MemberData, field);
				} else if(d.value is IList) {
					foreach(var v in d.value as IList) {
						if(v is MemberData) {
							DoValidate(v as MemberData, field);
						}
					}
				}
				return false;
			});
			return allNodes;
		}
	}
}