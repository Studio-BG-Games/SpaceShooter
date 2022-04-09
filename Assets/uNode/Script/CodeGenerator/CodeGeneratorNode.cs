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
        #region Generate Node
		/// <summary>
		/// Generate code for target node.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static string GenerateNode(NodeComponent target) {
			if(target) {
				if(target is Node) {
					return GenerateNode(target as Node);
				} else if(target is StateEventNode) {
					return GenerateNode(target as StateEventNode);
				}
			}
			throw new ArgumentNullException("target");
		}

		/// <summary>
		/// Generate code for target node
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static string GenerateNode(StateEventNode target) {
			if(target) {
				if(generatorData.generatedData.ContainsKey(target)) {
					return generatorData.generatedData[target];
				}
				string data = null;
				try {
					string fName = target.eventType.ToString();
					List<string> parameterType = new List<string>();
					switch(target.eventType) {
						case StateEventNode.EventType.OnAnimatorIK:
							parameterType.Add(Type(typeof(int)));
							break;
						case StateEventNode.EventType.OnApplicationFocus:
						case StateEventNode.EventType.OnApplicationPause:
							parameterType.Add(Type(typeof(bool)));
							break;
						case StateEventNode.EventType.OnCollisionEnter:
						case StateEventNode.EventType.OnCollisionExit:
						case StateEventNode.EventType.OnCollisionStay:
							parameterType.Add(Type(typeof(Collision)));
							break;
						case StateEventNode.EventType.OnCollisionEnter2D:
						case StateEventNode.EventType.OnCollisionExit2D:
						case StateEventNode.EventType.OnCollisionStay2D:
							parameterType.Add(Type(typeof(Collision2D)));
							break;
						case StateEventNode.EventType.OnTriggerEnter:
						case StateEventNode.EventType.OnTriggerExit:
						case StateEventNode.EventType.OnTriggerStay:
							parameterType.Add(Type(typeof(Collider)));
							break;
						case StateEventNode.EventType.OnTriggerEnter2D:
						case StateEventNode.EventType.OnTriggerExit2D:
						case StateEventNode.EventType.OnTriggerStay2D:
							parameterType.Add(Type(typeof(Collider2D)));
							break;
					}
					MData mData = generatorData.GetMethodData(fName, parameterType);
					if(mData == null && target.eventType != StateEventNode.EventType.OnEnter && target.eventType != StateEventNode.EventType.OnExit) {
						var func = graph.GetFunction(fName);
						Type funcType = typeof(void);
						if(func != null) {
							funcType = func.ReturnType();
						}
						mData = generatorData.AddMethod(fName, Type(funcType), parameterType);
					}
					string initStatement = null;
					switch(target.eventType) {
						case StateEventNode.EventType.OnAnimatorIK:
						case StateEventNode.EventType.OnApplicationFocus:
						case StateEventNode.EventType.OnApplicationPause:
						case StateEventNode.EventType.OnCollisionEnter:
						case StateEventNode.EventType.OnCollisionExit:
						case StateEventNode.EventType.OnCollisionStay:
						case StateEventNode.EventType.OnCollisionEnter2D:
						case StateEventNode.EventType.OnCollisionExit2D:
						case StateEventNode.EventType.OnCollisionStay2D:
						case StateEventNode.EventType.OnTriggerEnter:
						case StateEventNode.EventType.OnTriggerExit:
						case StateEventNode.EventType.OnTriggerStay:
						case StateEventNode.EventType.OnTriggerEnter2D:
						case StateEventNode.EventType.OnTriggerExit2D:
						case StateEventNode.EventType.OnTriggerStay2D:
							if(target.storeParameter.isAssigned) {
								initStatement = Set(Value((object)target.storeParameter), mData.parameters[0].name);
							}
							break;
					}
					var flows = target.GetFlows();
					if(flows != null && flows.Count > 0) {
						string contents = GenerateFlowCode(flows, target, false);
						if(debugScript) {
							contents += Debug(target, StateType.Success);
						}
						data = contents;
						if(!string.IsNullOrEmpty(data) && mData != null) {
							mData.AddCode(
								CG.If(
									CG.CompareEventState(uNodeHelper.GetComponentInParent<StateNode>(target), null), 
									initStatement + GetInvokeNodeCode(target).AddLineInFirst()
								), -100);
						}
					}
					if(setting.fullComment && !string.IsNullOrEmpty(data)) {
						data = data.Insert(0, "//" + target.gameObject.name + " | Type:" + Type(target.GetType()).AddLineInEnd());
					}
					data = data.AddLineInFirst();
				}
				catch (Exception ex) {
					if(!generatorData.hasError) {
						if(setting != null && setting.isAsync) {
							generatorData.errors.Add(
								new uNodeException(
									"Error from node:" + target.gameObject.name + " |Type:" + target.GetType() +
									"\nFrom graph:" + target.owner.FullGraphName, ex, target));
							//In case async return error commentaries.
							return "/*Error from node: " + target.gameObject.name + " */";
						}
                        UnityEngine.Debug.LogError("Error from node:" + target.gameObject.name + " |Type:" + target.GetType() + "\nError:" + ex.ToString(), target);
					}
					generatorData.hasError = true;
					throw;
				}
				if(string.IsNullOrEmpty(data)) {
                    UnityEngine.Debug.Log("Node not generated data", target);
				}
				if(includeGraphInformation) {
					data = WrapWithInformation(data, target);
				}
				generatorData.generatedData.Add(target, data);
				return data;
			}
			throw new System.Exception("Target node is null");
		}

		/// <summary>
		/// Generate code for target node.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="isValue"></param>
		/// <returns></returns>
		public static string GenerateNode(NodeComponent target, bool isValue) {
			if(target is Node node) {
				return GenerateNode(node, isValue);
			} else {
				throw new Exception("The parameter: target must inherith from Node");
			}
		}

		/// <summary>
		/// Generate code for target node
		/// </summary>
		/// <param name="target"></param>
		/// <param name="isValue"></param>
		/// <returns></returns>
		public static string GenerateNode(Node target, bool isValue = false) {
			if(target != null) {
				if(!isValue && !isInUngrouped && generatorData.generatedData.ContainsKey(target)) {
					return generatorData.generatedData[target];
				}
				string data;
				try {
					data = isValue ? target.GenerateValueCode() : target.GenerateCode();
					if(!isValue && setting.fullComment && !string.IsNullOrEmpty(data)) {
						data = data.Insert(0, "//" + target.gameObject.name + " | Type:" + Type(target.GetType()).AddLineInEnd());
					}
					if(!isValue) {
						data = data.AddLineInFirst();
						if(!string.IsNullOrEmpty(target.comment)) {//Generate Commentaries for nodes
							string[] str = target.comment.Split('\n');
							foreach(var s in str) {
								data = data.AddFirst(s.AddFirst("//").AddLineInFirst());
							}
						}
					} else if(setting.debugScript && setting.debugValueNode) {
						if(typeof(Delegate).IsAssignableFrom(target.ReturnType())) {
							data = New(target.ReturnType(), data);
						}
					}
					if(includeGraphInformation) {
						data = WrapWithInformation(data, target);
					}
				}
				catch (Exception ex) {
					if(!generatorData.hasError) {
						if(setting != null && setting.isAsync) {
							generatorData.errors.Add(
								new uNodeException(
									"Error from node:" + target.gameObject.name + " |Type:" + target.GetType() +
									"\nFrom graph:" + target.owner.FullGraphName,
									ex, target));
							//In case async return error commentaries (See uNode Console).
							//uNodeDebug.LogError("Error from node:" + target.gameObject.name + " |Type:" + target.GetType(), target);
							return WrapWithInformation("/*Error from node: " + target.gameObject.name + " */", target);
						}
                        UnityEngine.Debug.LogError(
							"Error from node:" + target.gameObject.name + " |Type:" + target.GetType() + 
							"\nFrom graph:" + target.owner.FullGraphName + 
							"\nError:" + ex.ToString(), target);
						uNodeDebug.LogError("Error from node:" + target.gameObject.name + " |Type:" + target.GetType(), target);
					}
					generatorData.hasError = true;
					throw;
				}
				//if(string.IsNullOrEmpty(data)) {
				//	Debug.Log("Node not generated data", target);
				//}
				if(!isValue && !isInUngrouped)
					generatorData.generatedData.Add(target, data);
				return data;
			}
			throw new System.Exception("Target node is null");
		}
		#endregion

        #region Node Functions
		/// <summary>
		/// Get state code from coroutine event
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static string GetNodeState(NodeComponent target) {
			if(target == null)
				throw new ArgumentNullException(nameof(target));
			if(!generatorData.stateNodes.Contains(target)) {
				throw new uNodeException($"Forbidden to generate state code because the node: {target.GetNodeName()} is not registered as State Node.\nEnsure to register it using {nameof(CG)}.{nameof(CG.RegisterAsStateNode)}", target);
			}
			return GetCoroutineName(target) + ".state";
		}

		/// <summary>
		/// Compare node state, The compared node will automatic placed to new generated function.
		/// </summary>
		/// <param name="target">The target node to compare</param>
		/// <param name="state">The compare state</param>
		/// <returns></returns>
		public static string CompareNodeState(NodeComponent target, bool? state, bool invert = false) {
			if(target == null)
				throw new ArgumentNullException(nameof(target));
			string s = GetCoroutineName(target);
			if(!string.IsNullOrEmpty(s)) {
				string result = s.CGAccess(state == null ? "IsRunning" : state.Value ? "IsSuccess" : "IsFailure");
				if(invert) {
					result = result.CGNot();
				}
				if(!generatorData.stateNodes.Contains(target)) {
					throw new uNodeException($"Forbidden to generate state code because the node: {target.GetNodeName()} is not registered as State Node.\nEnsure to register it using {nameof(CG)}.{nameof(CG.RegisterAsStateNode)}", target);
				}
				return result;
			}
			return null;
		}

		/// <summary>
		/// Get is finished event from coroutine event
		/// </summary>
		/// <param name="target"></param>
		/// <param name="invert"></param>
		/// <returns></returns>
		public static string CompareNodeIsFinished(Node target, bool invert = false) {
			string invertCode = "";
			if(invert) {
				invertCode = "!";
			}
			return invertCode + GetCoroutineName(target).CGAccess("IsFinished");
		}
		#endregion

		#region Event Functions
		/// <summary>
		/// Compare Event State
		/// </summary>
		/// <param name="target"></param>
		/// <param name="state"></param>
		/// <param name="invert"></param>
		/// <returns></returns>
		public static string CompareEventState(object target, bool? state, bool invert = false) {
			if(target is NodeComponent) {
				return CompareNodeState(target as NodeComponent, state, invert);
			}
			string s = GetCoroutineName(target);
			if(!string.IsNullOrEmpty(s)) {
				string result = s.CGAccess(state == null ? "IsRunning" : state.Value ? "IsSuccess" : "IsFailure");
				if(invert) {
					result = result.CGNot();
				}
				return result;
			}
			return null;
		}

		/// <summary>
		/// Get wait event code for coroutine event
		/// </summary>
		/// <param name="target"></param>
		/// <param name="invokeTarget"></param>
		/// <returns></returns>
		public static string WaitEvent(Node target, bool invokeTarget = true) {
			string result;
			if(invokeTarget) {
				result = GetInvokeNodeCode(target);
				if(!string.IsNullOrEmpty(result)) {
					result = "yield return " + result;
				}
			} else {
				result = "yield return " + GetCoroutineName(target) + ".coroutine;";
			}
			return result;
		}

		/// <summary>
		/// Get wait event code for coroutine event
		/// </summary>
		/// <param name="target"></param>
		/// <param name="invokeTarget"></param>
		/// <returns></returns>
		public static string WaitEvent(object target, bool invokeTarget = true) {
			if(target is Node) {
				return WaitEvent(target as Node, invokeTarget);
			}
			string result;
			if(invokeTarget) {
				result = RunEvent(target);
				if(!string.IsNullOrEmpty(result)) {
					result = "yield return " + result;
				}
			} else {
				result = "yield return " + GetCoroutineName(target) + ".coroutine;";
			}
			return result;
		}

		/// <summary>
		/// Stop coroutine event
		/// </summary>
		/// <param name="target"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public static string StopEvent(object target, bool? state = null) {
			if(target == null) {
				throw new ArgumentNullException("target");
			}
			if(state == null) {
				return GetCoroutineName(target) + ".Stop();";
			} else if(state.Value) {
				return GetCoroutineName(target) + ".Stop(true);";
			} else {
				return GetCoroutineName(target) + ".Stop(false);";
			}
		}

		/// <summary>
		/// Run coroutine event
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static string RunEvent(object target) {
			return GetCoroutineName(target) + ".Run();";
		}

		public static string GetEvent(MemberData member) {
			if(!member.isAssigned)
				return null;
			if(member.IsTargetingPortOrNode) {
				var node = member.GetTargetNode();
				if(node == null)
					return null;
				if(member.targetType == MemberData.TargetType.FlowNode) {
					string debug = null;
					if(setting.debugScript) {
						debug = Debug(member).AddLineInEnd();
					}
					if(IsStateNode(node)) {
						if(debug != null) {
							return Invoke(typeof(Runtime.EventCoroutine), nameof(Runtime.EventCoroutine.Create), Value(graph), CG.RoutineEvent(Lambda(debug + Return(GetCoroutineName(node)))));
						}
						return GetCoroutineName(node);
					}
					return Invoke(typeof(Runtime.EventCoroutine), nameof(Runtime.EventCoroutine.Create), Value(graph), Lambda(debug + GenerateNode(node))
					);
				} else if(member.targetType == MemberData.TargetType.FlowInput) {
					string debug = null;
					if(setting.debugScript) {
						debug = Debug(member).AddLineInEnd();
					}
					return Invoke(typeof(Runtime.EventCoroutine), nameof(Runtime.EventCoroutine.Create), Value(graph), Lambda(debug + GenerateFlowCode(member.Invoke(null) as IFlowGenerate, node)));
				}
			} else {
				throw new Exception("Invalid target type: " + member.targetType);
			}
			throw new InvalidOperationException();
		}

		public static string GetEventAndRun(MemberData member) {
			return GetEvent(member).Add(".Run()");
		}

		public static string ReturnEvent(MemberData member) {
			return CG.Return(GetEvent(member).Add(".Run()"));
		}
		#endregion

		#region GetFinishCode
		/// <summary>
		/// Generate finish code for node.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="isSuccess"></param>
		/// <param name="flowConnection"></param>
		/// <returns></returns>
		public static string FlowFinish(Node node, bool isSuccess, params MemberData[] flowConnection) {
			return FlowFinish(node, isSuccess, true, false, flowConnection);
		}

		/// <summary>
		/// Generate finish code for node.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="isSuccess"></param>
		/// <param name="alwaysHaveReturnValue"></param>
		/// <param name="flowConnection"></param>
		/// <returns></returns>
		public static string FlowFinish(Node node, bool isSuccess, bool alwaysHaveReturnValue = true, params MemberData[] flowConnection) {
			return FlowFinish(node, isSuccess, alwaysHaveReturnValue, true, flowConnection);
		}

		/// <summary>
		/// Generate finish code for node.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="isSuccess"></param>
		/// <param name="alwaysHaveReturnValue"></param>
		/// <param name="breakCoroutine"></param>
		/// <param name="flowConnection"></param>
		/// <returns></returns>
		public static string FlowFinish(Node node, bool isSuccess, bool alwaysHaveReturnValue = true, bool breakCoroutine = true, params MemberData[] flowConnection) {
			string result = null;
			if(isSuccess) {
				string success = null;
				if(setting.debugScript && !node.IsSelfCoroutine()) {
					success += Debug(node, StateType.Success).AddFirst("\n", !string.IsNullOrEmpty(success));
				}
				result = success;
				if(alwaysHaveReturnValue) {
					result += GetReturnValue(node, true, breakCoroutine).AddFirst("\n", result);
				} else {
					if(breakCoroutine && node.IsSelfCoroutine()) {
						result = success.Add("\n") + "yield break;";
					} else {
						result = success;
					}
				}
			} else {
				string failure = null;
				if(setting.debugScript && !node.IsSelfCoroutine()) {
					failure += Debug(node, StateType.Failure).AddFirst("\n", !string.IsNullOrEmpty(failure));
				}
				result = failure;
				if(alwaysHaveReturnValue) {
					result += GetReturnValue(node, false, breakCoroutine).AddFirst("\n", result);
				} else {
					if(breakCoroutine && node.IsSelfCoroutine()) {
						result = failure.Add("\n") + "yield break;";
					} else {
						result = failure;
					}
				}
			}
			result = result.AddLineInFirst();
			if(flowConnection != null && flowConnection.Length > 0) {
				string flow = null;
				foreach(var f in flowConnection) {
					if(f == null || !f.isAssigned)
						continue;
					flow += Flow(f, node).AddLineInEnd();
				}
				if(!string.IsNullOrEmpty(flow)) {
					if(result == null)
						result = string.Empty;
					result = result.Insert(0, flow);
				}
			}
			return result;
		}

		/// <summary>
		/// Generate finish code for transition.
		/// </summary>
		/// <param name="transition"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public static string FlowFinish(TransitionEvent transition, bool? state = true) {
			if(transition != null) {
				string result = StopEvent(transition.node, state);
				if(debugScript) {
					result += Debug(transition.node, transition);
				}
				return result + Flow(transition.target, null, false).AddLineInFirst();
			}
			throw new ArgumentNullException(nameof(transition));
		}
		#endregion

        #region GetInvokeNodeCode
		/// <summary>
		/// Get invoke node code.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="forcedNotGrouped"></param>
		/// <returns></returns>
		public static string GetInvokeNodeCode(NodeComponent target, bool forcedNotGrouped = false) {
			if(target == null)
				throw new ArgumentNullException(nameof(target));
			if(target is Node && !(target as Node).IsFlowNode())
				throw new Exception("The target node:" + target.GetNodeName() + " is not a flow node.");
			if(!forcedNotGrouped && !IsStateNode(target)) {
				return GenerateNode(target);
			}
			if(forcedNotGrouped && !generatorData.stateNodes.Contains(target)) {
				throw new uNodeException($"Forbidden to generate state code because the node: ({target.GetNodeName()}-{target.GetType()}) is not registered as State Node.\nEnsure to register it using {nameof(CG)}.{nameof(CG.RegisterAsStateNode)}", target);
			}
			return RunEvent(target);
		}

		/// <summary>
		/// Get invoke node code.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static string GetInvokeNodeCode(StateEventNode target) {
			if(target == null)
				throw new System.ArgumentNullException(nameof(target));
			if(!generatorData.stateNodes.Contains(target)) {
				throw new uNodeException($"Forbidden to generate state code because the node: ({target.GetNodeName()}-{target.GetType()}) is not registered as State Node.\nEnsure to register it using {nameof(CG)}.{nameof(CG.RegisterAsStateNode)}", target);
			}
			return RunEvent(target);
		}
		#endregion

        #region GenerateFlowCode
		/// <summary>
		/// Function for generating code for flow node.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="from"></param>
		/// <param name="waitTarget"></param>
		/// <returns></returns>
		public static string GenerateFlowCode(Node target, Node from, bool waitTarget = true) {
			if(target == null)
				throw new ArgumentNullException("target");
			if(!target.IsFlowNode())
				throw new System.Exception("node can't be invoked.");
			if(!uNodeUtility.IsInStateGraph(target)) {
				return GenerateNode(target);
			}
			if(waitTarget/* && from && target.IsCoroutine() && from.IsCoroutine()*/) {
				return "yield return " + RunEvent(target);
			}
			if(!isInUngrouped && generatorData.regularNodes.Contains(target)) {
				return GenerateNode(target);
			}
			return RunEvent(target);
		}

		/// <summary>
		/// Function for generating code for flow member.
		/// </summary>
		/// <param name="flowMember">The flow member</param>
		/// <param name="from">The node which flow member comes from</param>
		/// <param name="waitTarget">If true, will generate wait code on coroutine member.</param>
		/// <returns></returns>
		public static string Flow(MemberData flowMember, NodeComponent from, bool waitTarget = true) {
			if(flowMember == null)
				return null;
			if(!flowMember.isAssigned)
				return null;
			var target = flowMember.GetFlowNode();
			if(target == null)
				return null;
			if(flowMember.targetType != MemberData.TargetType.FlowNode && flowMember.targetType != MemberData.TargetType.FlowInput)
				throw new System.Exception("Incorrect target type : " + flowMember.targetType.ToString() + ", TargetType must FlowNode or FlowInput");
			if(flowMember.targetType != MemberData.TargetType.FlowInput && !target.IsFlowNode())
				throw new System.Exception("node is not flow node.");
			string debug = null;
			if(setting.debugScript) {
				debug = Debug(flowMember).AddLineInEnd();
			}
			if(flowMember.targetType == MemberData.TargetType.FlowNode) {
				if(!isInUngrouped && !uNodeUtility.IsInStateGraph(target) && !IsStateNode(target)) {
					return debug + GenerateNode(target);
				}
				if(isInUngrouped || allowYieldStatement && IsStateNode(target)) {
					if(!generatorData.stateNodes.Contains(target)) {
						return debug + GenerateNode(target);
					}
					if(!allowYieldStatement) {
						throw new Exception("The current block doesn't allow coroutines / yield statements");
					}
					if(waitTarget) {
						return debug + "yield return " + RunEvent(target);
					} else {
						return debug + RunEvent(target);
					}
				}
				if(!isInUngrouped && generatorData.regularNodes.Contains(target)) {
					return debug + GenerateNode(target);
				}
				if(!generatorData.stateNodes.Contains(target)) {
					if(!allowYieldStatement && target.IsSelfCoroutine()) {
						throw new Exception("The current block doesn't allow coroutines / yield statements");
					}
					throw new uNodeException($"Forbidden to generate state code because the node: {target.GetNodeName()} is not registered as State Node.\nEnsure to register it using {nameof(CG)}.{nameof(CG.RegisterAsStateNode)}", target);
				}
				return debug + RunEvent(target);
			} else {
				return debug + GenerateFlowCode(flowMember.Invoke(null) as IFlowGenerate, target);
			}
			// return debug + GetInvokeCode(_eventCode, false, generatorData.GetEventName(target));
		}

		private static string GenerateFlowCode(IFlowGenerate flowInput, NodeComponent source) {
			if(flowInput == null) {
				throw new ArgumentNullException("flowInput");
			}
			if(source == null) {
				throw new ArgumentNullException("source");
			}
			string data;
			try {
				data = flowInput.GenerateCode();
				if(setting.fullComment && !string.IsNullOrEmpty(data)) {
					data = data.Insert(0, "//" + source.gameObject.name + " : " + flowInput.ToString() + " | Type:" + Type(source.GetType()).AddLineInEnd());
				}
				data = data.AddLineInFirst();
			}
			catch (Exception ex) {
				if(!generatorData.hasError) {
					if(setting != null && setting.isAsync) {
						generatorData.errors.Add(
							new uNodeException(
								"Error from node:" + source.gameObject.name + " : " + flowInput.ToString() + " |Type:" + source.GetType() +
								"\nFrom graph:" + source.owner.FullGraphName, 
								ex, source));
						//In case async return error commentaries.
						return "/*Error from node: " + source.gameObject.name + " : " + flowInput.ToString() + " */";
					}
					UnityEngine.Debug.LogError("Error from node:" + source.gameObject.name + " : " + flowInput.ToString() + " |Type:" + source.GetType()  +
						"\nFrom graph:" + source.owner.FullGraphName +
						"\nError:" + ex.ToString(), source);
				}
				generatorData.hasError = true;
				throw;
			}
			if(includeGraphInformation) {
				data = WrapWithInformation(data, source);
			}
			return data;
		}

		/// <summary>
		/// Function for generating code for flow member.
		/// </summary>
		/// <param name="flowMembers"></param>
		/// <param name="from"></param>
		/// <param name="waitTarget"></param>
		/// <returns></returns>
		public static string GenerateFlowCode(IList<MemberData> flowMembers, NodeComponent from, bool waitTarget = true) {
			string data = null;
			if(flowMembers != null) {
				for(int i = 0; i < flowMembers.Count; i++) {
					data += Flow(flowMembers[i], from, waitTarget).AddLineInFirst();
				}
			}
			return data;
		}

		/// <summary>
		/// Function for generating code for flow member.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="waitTarget"></param>
		/// <param name="flowMembers"></param>
		/// <returns></returns>
		public static string GenerateFlowCode(NodeComponent from, bool waitTarget, params MemberData[] flowMembers) {
			string data = null;
			if(flowMembers != null) {
				for(int i = 0; i < flowMembers.Length; i++) {
					data += Flow(flowMembers[i], from, waitTarget).AddLineInFirst();
				}
			}
			return data;
		}
		#endregion

		#region Generate EventCode
		/// <summary>
		/// Generate event code.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string GenerateEventCode(Node from, Block target, EventData.EventType type) {
			if(target == null)
				throw new System.Exception("target can't null");
			if(type == EventData.EventType.Action) {
				if(setting.fullComment) {
					string data = target.GenerateCode(from);
					if(!string.IsNullOrEmpty(data)) {
						data = data.Insert(0, "//Action: " + target.Name + " | Type: " + CG.Type(target.GetType()) + "\n");
					}
					return data;
				}
				return target.GenerateCode(from);
			} else {
				return target.GenerateConditionCode(from);
			}
		}
		#endregion

        #region GetReturnValue
		/// <summary>
		/// Get return value code for node.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="value"></param>
		/// <param name="breakCoroutine"></param>
		/// <returns></returns>
		public static string GetReturnValue(Node node, Node value, bool breakCoroutine = false) {
			if(node != null && isInUngrouped) {
				string result = YieldReturn(GetNodeState(value));
				if(breakCoroutine) {
					result += "\nyield break;";
				}
				return result;
			}
			return null;
		}

		/// <summary>
		/// Get return value code for node.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="value"></param>
		/// <param name="breakCoroutine"></param>
		/// <returns></returns>
		public static string GetReturnValue(Node node, bool value, bool breakCoroutine = false) {
			if(node != null && isInUngrouped) {
				string result = YieldReturn(value.CGValue());
				if(breakCoroutine) {
					result += "\nyield break;";
				}
				return result;
			}
			return null;
		}
		#endregion
	}
}