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
		/// <summary>
		/// Begin a new block statement ( use this for generating lambda block )
		/// </summary>
		/// <param name="allowYield"></param>
		public static void BeginBlock(bool allowYield) {
			generatorData.blockStacks.Add(new BlockStack() {
				allowYield = allowYield
			});
		}

		/// <summary>
		/// End the previous block statment
		/// </summary>
		public static void EndBlock() {
			if(generatorData.blockStacks.Count > 0) {
				generatorData.blockStacks.RemoveAt(generatorData.blockStacks.Count - 1);
			}
		}

		/// <summary>
		/// Register node as flow node.
		/// Note: Call this inside RegisterPin in node. 
		/// </summary>
		/// <param name="node"></param>
		public static void RegisterFlowNode(NodeComponent node) {
			if(!generatorData.registeredFlowNodes.Contains(node)) {
				generatorData.registeredFlowNodes.Add(node);
			}
		}

		/// <summary>
		/// Are the node has flow ports?
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static bool HasFlowPort(Node node) {
			return generatorData.registeredFlowNodes.Contains(node) || node.IsFlowNode();
		}

		public static bool IsContainOperatorCode(string value) {
			if(value.Contains("op_")) {
				switch(value) {
					case "op_Addition":
					case "op_Subtraction":
					case "op_Division":
					case "op_Multiply":
					case "op_Modulus":
					case "op_Equality":
					case "op_Inequality":
					case "op_LessThan":
					case "op_GreaterThan":
					case "op_LessThanOrEqual":
					case "op_GreaterThanOrEqual":
					case "op_BitwiseAnd":
					case "op_BitwiseOr":
					case "op_LeftShift":
					case "op_RightShift":
					case "op_ExclusiveOr":
					case "op_UnaryNegation":
					case "op_UnaryPlus":
					case "op_LogicalNot":
					case "op_OnesComplement":
					case "op_Increment":
					case "op_Decrement":
						return true;
				}
			}
			return false;
		}

		#region Grouped
		/// <summary>
		/// Register node as grouped node.
		/// </summary>
		/// <param name="node"></param>
		public static void RegisterAsRegularNode(NodeComponent node) {
			if(generatorData.state.state != State.Classes)
				throw new InvalidOperationException("The Register action must be performed on Initialization / " + nameof(NodeComponent.OnGeneratorInitialize));
			if(!generatorData.regularNodes.Contains(node)) {
				generatorData.regularNodes.Add(node);
				generatorData.stateNodes.Remove(node);
			}
		}

		/// <summary>
		/// Register node as coroutine state node ( non-coroutine state node doesn't need this ).
		/// </summary>
		/// <param name="node"></param>
		public static void RegisterAsStateNode(NodeComponent node) {
			if(generatorData.state.state != State.Classes)
				throw new InvalidOperationException("The Register action must be performed on Initialization / " + nameof(NodeComponent.OnGeneratorInitialize));
			if(node == null)
				return;
			generatorData.stateNodes.Add(node);
		}

		/// <summary>
		/// Register node as coroutine state node ( non-coroutine state node doesn't need this ).
		/// </summary>
		/// <param name="member"></param>
		public static void RegisterAsStateNode(MemberData member) {
			if(member == null)
				return;
			if(member.isAssigned && member.IsTargetingPortOrNode) {
				var node = member.GetTargetNode();
				if(node != null) {
					RegisterAsStateNode(node);
				}
			}
		}

		/// <summary>
		/// Register node as coroutine state node ( non-coroutine state node doesn't need this ).
		/// </summary>
		/// <param name="members"></param>
		public static void RegisterAsStateNode(IEnumerable<MemberData> members) {
			if(members == null)
				return;
			foreach(var member in members) {
				RegisterAsStateNode(member);
			}
		}

		/// <summary>
		/// Is the node is regular node?
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static bool IsRegularNode(NodeComponent node) {
			return generatorData.regularNodes.Contains(node);
		}

		/// <summary>
		/// Is the node is state node?
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static bool IsStateNode(NodeComponent node) {
			return generatorData.stateNodes.Contains(node);
		}

		/// <summary>
		/// Is the node is in state graph?
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static bool IsInStateGraph(NodeComponent node) {
			return uNodeUtility.IsInStateGraph(node);
		}
		#endregion

		#region Variables
		/// <summary>
		/// True if variable from node can be declared as local variable.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="fieldName"></param>
		/// <param name="flows"></param>
		/// <returns></returns>
		public static bool CanDeclareLocal(NodeComponent from, string fieldName, params MemberData[] flows) {
			return CanDeclareLocal(from, from.GetType().GetFieldCached(fieldName), flows.Select((m) => m.GetTargetNode()).ToList());
		}

		/// <summary>
		/// True if variable from node can be declared as local variable.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="field"></param>
		/// <param name="flows"></param>
		/// <returns></returns>
		public static bool CanDeclareLocal(NodeComponent from, string fieldName, IList<MemberData> flows) {
			return CanDeclareLocal(from, from.GetType().GetFieldCached(fieldName), flows.Select((m) => m.GetTargetNode()).ToList());
		}

		/// <summary>
		/// True if variable from node can be declared as local variable.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="field"></param>
		/// <param name="flows"></param>
		/// <returns></returns>
		public static bool CanDeclareLocal(NodeComponent from, string fieldName, IList<NodeComponent> flows) {
			return CanDeclareLocal(from, from.GetType().GetFieldCached(fieldName), flows);
		}

		private static bool CanDeclareLocal(NodeComponent from, FieldInfo field, IList<NodeComponent> flows) {
			if(from && flows != null && flows.Count > 0 && !IsStateNode(from)) {
				var allFlows = new HashSet<NodeComponent>();
				allFlows.Add(from);
				foreach(var flow in flows) {
					if(flow == null)
						continue;
					Nodes.FindAllConnections(Nodes.GetNodeData(flow), ref allFlows);
				}
				var allConnection = GetAllNode(from.transform.parent);
				foreach(var node in allConnection) {
					var ndata = Nodes.GetNodeData(node);
					foreach(var port in ndata.valueInputs) {
						if(port.target == from && port.connection.targetType == MemberData.TargetType.NodeField && port.connection.startName == field.Name) {
							if(IsStateNode(node)) {
								return false;
							}
							if(allFlows.Contains(node)) {
								var flowData = Nodes.GetNodeData(node);
								if(flowData != null) {
									foreach(var d in flowData.valueOutputs) {
										if(!allFlows.Contains(d.owner)) {
											return false;
										}
									}
								}
							} else {
								return false;
							}
						}
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// True if variable from node can be declared as local variable.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="field"></param>
		/// <param name="index"></param>
		/// <param name="flows"></param>
		/// <returns></returns>
		public static bool CanDeclareLocal(NodeComponent from, string fieldName, int index, params MemberData[] flows) {
			return CanDeclareLocal(from, from.GetType().GetFieldCached(fieldName), index, flows);
		}

		/// <summary>
		/// True if variable from node can be declared as local variable.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="field"></param>
		/// <param name="index"></param>
		/// <param name="flows"></param>
		/// <returns></returns>
		public static bool CanDeclareLocal(NodeComponent from, string fieldName, int index, IList<NodeComponent> flows) {
			return CanDeclareLocal(from, from.GetType().GetFieldCached(fieldName), index, flows);
		}

		private static bool CanDeclareLocal(NodeComponent from, FieldInfo field, int index, params MemberData[] flows) {
			return CanDeclareLocal(from, field, index, flows.Select((m) => m.GetTargetNode()).ToList());
		}

		private static bool CanDeclareLocal(NodeComponent from, FieldInfo field, int index, IList<NodeComponent> flows) {
			if(from && flows != null && flows.Count > 0 && !IsStateNode(from)) {
				var allFlows = new HashSet<NodeComponent>();
				allFlows.Add(from);
				foreach(var flow in flows) {
					if(flow == null)
						continue;
					Nodes.FindAllConnections(Nodes.GetNodeData(flow), ref allFlows);
				}
				var allConnection = GetAllNode(from.transform.parent);
				foreach(var node in allConnection) {
					var ndata = Nodes.GetNodeData(node);
					foreach(var port in ndata.valueInputs) {
						if(port.target == from && port.connection.targetType == MemberData.TargetType.NodeFieldElement &&
								int.TryParse(port.connection.startName.Split('#')[1], out var tes) && tes == index &&
								port.connection.startName.Split('#')[0] == field.Name) {
							if(IsStateNode(node)) {
								return false;
							}
							if(allFlows.Contains(node)) {
								var flowData = Nodes.GetNodeData(node);
								if(flowData != null) {
									foreach(var d in flowData.valueOutputs) {
										if(!allFlows.Contains(d.owner)) {
											return false;
										}
									}
								}
							} else {
								return false;
							}
						}
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// True if variable from node can be declared as local variable.
		/// </summary>
		/// <param name="groupNode"></param>
		/// <returns></returns>
		public static bool CanDeclareLocal<T>(T superNode) where T : ISuperNode {
			if(superNode != null) {
				var flows = new HashSet<NodeComponent>();
				foreach(var n in superNode.nestedFlowNodes) {
					Nodes.FindFlowConnectionAfterCoroutineNode(n, ref flows);
				}
				bool check = false;
				Func<object, bool> validation = null;
				validation = delegate (object obj) {
					if(obj is MemberData) {
						MemberData member = obj as MemberData;
						Node n = member.GetInstance() as Node;
						if(n != null) {
							if(n is IVariableSystem && n is T && n == superNode as UnityEngine.Object &&
								(member.targetType == MemberData.TargetType.uNodeVariable ||
								member.targetType == MemberData.TargetType.uNodeGroupVariable ||
								member.targetType == MemberData.TargetType.uNodeLocalVariable)) {
								check = true;
							} else if(member.targetType == MemberData.TargetType.ValueNode) {
								AnalizerUtility.AnalizeObject(n, validation);
							}
						}
					}
					return false;
				};
				foreach(Node node in flows) {
					if(node == null)
						continue;
					AnalizerUtility.AnalizeObject(node, validation);
					if(check) {
						return true;
					}
				}
				var allConnection = new HashSet<NodeComponent>();
				foreach(var n in superNode.nestedFlowNodes) {
					Nodes.FindAllFlowConnection(n, ref allConnection);
				}
				foreach(var node in allConnection) {
					if(node == null)
						continue;
					AnalizerUtility.AnalizeObject(node, validation);
					if(check) {
						var nodes = GetFlowConnectedTo(node);
						if(nodes.Count > 0) {
							if(nodes.Any(n => !flows.Contains(n))) {
								return true;
							}
						}
						if((generatorData.stateNodes.Contains(node) || generatorData.portableActionInNode.Contains(node))) {
							return true;
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// True if the variable is instanced variable or are declared within the class.
		/// </summary>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static bool IsInstanceVariable(VariableData variable) {
			foreach(VData vdata in generatorData.GetVariables()) {
				if(vdata.variableRef == variable) {
					return vdata.isInstance;
				}
			}
			//throw new Exception("The variable is not registered.");
			return false;
		}

		/// <summary>
		/// True if the variable is local variable or are not declared within the class.
		/// </summary>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static bool IsLocalVariable(VariableData variable) {
			foreach(VData vdata in generatorData.GetVariables()) {
				if(vdata.variableRef == variable) {
					return !vdata.isInstance;
				}
			}
			//throw new Exception("The variable is not registered.");
			return false;
		}
		#endregion

		#region GetAllNode
		/// <summary>
		/// Get all nodes.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<NodeComponent> GetAllNode() {
			return generatorData.allNode;
		}

		/// <summary>
		/// Get all nodes in child of parent.
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static HashSet<NodeComponent> GetAllNode(Transform parent) {
			if(parent == graph.transform) {
				parent = graph.RootObject.transform;
			}
			HashSet<NodeComponent> nodes;
			if(generatorData.nodesMap.TryGetValue(parent, out nodes)) {
				return nodes;
			}
			nodes = new HashSet<NodeComponent>();
			foreach(NodeComponent node in GetAllNode()) {
				if(node == null)
					continue;
				if(node.transform.parent == parent) {
					nodes.Add(node);
				}
			}
			generatorData.nodesMap[parent] = nodes;
			return nodes;
		}
		#endregion

		#region ActionData
		private static int actionID = 0;
		private static Dictionary<Block, int> actionDataID = new Dictionary<Block, int>();

		/// <summary>
		/// Get invoke action code.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static string GetInvokeActionCode(Block target) {
			if(target == null)
				throw new System.Exception("target can't null");
			if(!actionDataID.ContainsKey(target)) {
				actionDataID.Add(target, ++actionID);
			}
			return Invoke(_activateActionCode, actionDataID[target].CGValue());
		}

		[System.NonSerialized]
		private static int coNum = 0;

		private static CoroutineData GetOrRegisterCoroutineEvent(object owner) {
			CoroutineData data;
			if(!generatorData.coroutineEvent.TryGetValue(owner, out data)) {
				data = new CoroutineData();
				data.variableName = "coroutine" + (++coNum).ToString();
				generatorData.coroutineEvent[owner] = data;
			}
			return data;
		}

		private static CoroutineData GetCoroutineEvent(object owner) {
			generatorData.coroutineEvent.TryGetValue(owner, out var data);
			return data;
		}

		/// <summary>
		/// Set state node stop action
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="contents"></param>
		public static void SetStateStopAction(object owner, string contents) {
			var data = GetOrRegisterCoroutineEvent(owner);
			data.onStop = contents;
		}

		/// <summary>
		/// Set state node action
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="contents"></param>
		public static void SetStateAction(object owner, string contents) {
			var data = GetOrRegisterCoroutineEvent(owner);
			data.contents = contents;
		}

		/// <summary>
		/// Set custom state node initialization / custom action
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="contents"></param>
		public static void SetStateInitialization(object owner, string contents) {
			var data = GetOrRegisterCoroutineEvent(owner);
			data.customExecution = () => contents;
		}

		/// <summary>
		/// Set custom state node initialization / custom action
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="contents"></param>
		public static void SetStateInitialization(object owner, Func<string> contents) {
			var data = GetOrRegisterCoroutineEvent(owner);
			data.customExecution = contents;
		}

		/// <summary>
		/// Register a Coroutine Event
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="generator"></param>
		/// <returns></returns>
		public static string RegisterCoroutineEvent(object obj, Func<string> generator, bool customExecution = false) {
			if(CG.generatorData.coroutineEvent.ContainsKey(obj)) {
				return CG.RunEvent(obj);
			} else {
				if(customExecution) {
					CG.SetStateInitialization(obj, generator);
				} else {
					CG.SetStateAction(obj, generator());
				}
				return CG.RunEvent(obj);
			}
		}

		/// <summary>
		/// Get the variable name of coroutine event
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static string GetCoroutineName(object target) {
			if(target != null) {
				return WrapWithInformation(GetOrRegisterCoroutineEvent(target).variableName, target);
			}
			return null;
		}
		#endregion

		#region Unused / Other
		/// <summary>
		/// Return true on flow body can be simplify to lambda expression code.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="returnType"></param>
		/// <param name="parameterTypes"></param>
		/// <returns></returns>
		public static bool CanSimplifyToLambda(MemberData target, Type returnType, IList<Type> parameterTypes) {
			if(target.IsTargetingNode) {
				var bodyNode = target.GetTargetNode();
				if(bodyNode is MultipurposeNode) {
					var node = bodyNode as MultipurposeNode;
					if(node.target.target.isAssigned && !node.onFinished.isAssigned) {
						System.Type[] memberTypes = null;
						if(node.target.target.targetType == MemberData.TargetType.Method) {
							var members = node.target.target.GetMembers(false);
							if(members != null && members.Length > 0) {
								var lastMember = members.LastOrDefault() as System.Reflection.MethodInfo;
								if(lastMember != null && lastMember.ReturnType == returnType) {
									memberTypes = lastMember.GetParameters().Select(i => i.ParameterType).ToArray();
								}
							}
						} else if(node.target.target.targetType == MemberData.TargetType.uNodeFunction) {
							uNodeFunction func = node.target.target.GetUnityObject() as uNodeFunction;
							if(func != null && func.ReturnType() == returnType) {
								memberTypes = func.parameters.Select(i => i.Type).ToArray();
							}
						}
						if(memberTypes != null) {
							if(parameterTypes.Count == memberTypes.Length && node.target.parameters.Length == memberTypes.Length) {
								bool flag = true;
								for(int x = 0; x < parameterTypes.Count; x++) {
									if(parameterTypes[x] != memberTypes[x]) {
										flag = false;
										break;
									}
								}
								if(flag) {
									for(int x = 0; x < parameterTypes.Count; x++) {
										var p = node.target.parameters[x];
										if(p.targetType != MemberData.TargetType.NodeFieldElement || p.GetAccessIndex() != x) {
											flag = false;
											break;
										}
									}
									if(flag) {
										return true;
									}
								}
							}
						}
					}
				}
			}
			return false;
		}

		public static HashSet<NodeComponent> GetFlowConnectedTo(NodeComponent target) {
			if(target != null) {
				if(generatorData.FlowConnectedTo.TryGetValue(target, out var connections)) {
					return connections;
				}
				connections = new HashSet<NodeComponent>();
				M_AssignFlowConnectedTo(target, ref connections);
				connections.Remove(target);
				generatorData.FlowConnectedTo[target] = connections;
				return connections;
			}
			return null;
		}

		private static void M_AssignFlowConnectedTo(NodeComponent target, ref HashSet<NodeComponent> nodes) {
			if(nodes == null)
				nodes = new HashSet<NodeComponent>();
			if(nodes.Add(target)) {
				generatorData.nodeConnections.TryGetValue(target, out var data);
				foreach(var n in data.flowInputNodes) {
					M_AssignFlowConnectedTo(n, ref nodes);
				}
			}
		}
		#endregion
	}
}