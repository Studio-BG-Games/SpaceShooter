using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;
using System;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Flow", "Event Hook")]
	[AddComponentMenu("")]
	public class EventHook : CustomNode {
		[Hide, ValueIn("Target"), Filter(typeof(System.Delegate), typeof(UnityEngine.Events.UnityEventBase), SetMember = true)]
		public MemberData target = MemberData.none;
		[NonSerialized]
		public FlowInput register = new FlowInput("Register");
		[NonSerialized]
		public FlowInput unregister = new FlowInput("Unregister");

		[Hide, FlowOut("Body", localFunction =true)]
		public MemberData body = new MemberData();

		[HideInInspector]
		public List<object> parameters = new List<object>();

		private System.Delegate m_Delegate;

		public override void OnRuntimeInitialize() {
			register.onExecute = () => {
				if(target.isAssigned) {
					object val = target.Get();
					if(val == null) {
						val = new MemberData.Event(target.CreateRuntimeEvent(), null);
					}
					if(val is MemberData.Event) {
						MemberData.Event e = val as MemberData.Event;
						if(e.eventInfo != null) {
							if(m_Delegate == null) {
								if(e.eventInfo is RuntimeEvent) {
									var returnType = target.type.GetMethod("Invoke").ReturnType;
									m_Delegate = new MemberData.EventCallback((obj => {
										if(owner == null)
											return null;
										if (obj != null && parameters.Count == obj.Length) {
											for (int i = 0; i < obj.Length; i++) {
												parameters[i] = obj[i];
											}
										}
										if(returnType == typeof(void)) {
											body.InvokeFlow();
											return null;
										} else {
											Node n;
											WaitUntil w;
											if (!body.ActivateFlowNode(out n, out w)) {
												throw new uNodeException("Coroutine aren't supported by EventHook node in runtime.", this);
											}
											if (n == null) {
												throw new uNodeException("No return value", this);
											}
											JumpStatement js = n.GetJumpState();
											if (js == null || js.jumpType != JumpStatementType.Return || !(js.from is NodeReturn)) {
												throw new uNodeException("No return value", this);
											}
											return (js.from as NodeReturn).GetReturnValue();
										}
									}));
								} else {
									var method = e.eventInfo.EventHandlerType.GetMethod("Invoke");
									var type = method.ReturnType;
									if (type == typeof(void)) {
										m_Delegate = CustomDelegate.CreateActionDelegate((obj) => {
											if(owner == null)
												return;
											if (obj != null && parameters.Count == obj.Length) {
												for (int i = 0; i < obj.Length; i++) {
													parameters[i] = obj[i];
												}
											}
											body.InvokeFlow();
										}, method.GetParameters().Select(i => i.ParameterType).ToArray());
									} else {
										var types = method.GetParameters().Select(i => i.ParameterType).ToList();
										types.Add(type);
										m_Delegate = CustomDelegate.CreateFuncDelegate((obj) => {
											if(owner == null)
												return null;
											if (obj != null && parameters.Count == obj.Length) {
												for (int i = 0; i < obj.Length; i++) {
													parameters[i] = obj[i];
												}
											}
											Node n;
											WaitUntil w;
											if (!body.ActivateFlowNode(out n, out w)) {
												throw new uNodeException("Coroutine aren't supported by EventHook node in runtime.", this);
											}
											if (n == null) {
												throw new uNodeException("No return value", this);
											}
											JumpStatement js = n.GetJumpState();
											if (js == null || js.jumpType != JumpStatementType.Return || !(js.from is NodeReturn)) {
												throw new uNodeException("No return value", this);
											}
											return (js.from as NodeReturn).GetReturnValue();
										}, types.ToArray());
									}
									m_Delegate = ReflectionUtils.ConvertDelegate(m_Delegate, e.eventInfo.EventHandlerType);
								}
							}
							e.eventInfo.AddEventHandler(e.instance, m_Delegate);
						}
					} else if(val is UnityEventBase) {
						var method = val.GetType().GetMethod("AddListener");
						if(m_Delegate == null) {
							var param = method.GetParameters()[0].ParameterType;
							var gType = param.GetGenericArguments();
							m_Delegate = CustomDelegate.CreateActionDelegate((obj) => {
								if(owner == null)
									return;
								if(obj != null && parameters.Count == obj.Length) {
									for(int i = 0; i < obj.Length; i++) {
										parameters[i] = obj[i];
									}
								}
								body.InvokeFlow();
							}, gType);
							m_Delegate = System.Delegate.CreateDelegate(param, m_Delegate.Target, m_Delegate.Method);
						}
						method.InvokeOptimized(val, new object[] { m_Delegate });
					} else {
						if(val == null) {
							throw new uNodeException("The target event is null", this);
						}
						throw new uNodeException("Invalid target value: " + val, this);
					} 
				}
			};
			unregister.onExecute = () => {
				if(m_Delegate != null && target.isAssigned) {
					object val = target.Get();
					if(val is MemberData.Event) {
						MemberData.Event e = val as MemberData.Event;
						if(e.eventInfo != null) {
							e.eventInfo.RemoveEventHandler(e.instance, m_Delegate);
						}
					} else if(val is UnityEventBase) {
						var method = val.GetType().GetMethod("RemoveListener");
						method.InvokeOptimized(val, new object[] { m_Delegate });
					}
				}
			};
		}

		public override void OnGeneratorInitialize() {
			CG.RegisterFlowNode(this);
			register.codeGeneration = () => {
				if(target.type.IsCastableTo(typeof(UnityEventBase))) {
					return target.CGValue().CGInvoke("AddListener", GenerateEventCodes()).AddSemicolon();
				}
				return CG.Set(target, GenerateEventCodes(), SetType.Add, target.type);
			};
			unregister.codeGeneration = () => {
				if(target.type.IsCastableTo(typeof(UnityEventBase))) {
					return target.CGValue().CGInvoke("RemoveListener", GenerateEventCodes()).AddSemicolon();
				}
				return CG.Set(target, GenerateEventCodes(), SetType.Subtract, target.type);
			};
		}

		private string GenerateEventCodes() {
			if(!body.isAssigned) {
				return null;
			}
			Type targetType = target.type;
			if(targetType == null)
				return null;
			System.Type[] parameterTypes = null;
			if(targetType.IsCastableTo(typeof(Delegate))) {
				parameterTypes = targetType.GetMethod("Invoke").GetParameters().Select(i => i.ParameterType).ToArray();

				if(CG.IsRegularNode(this) && CG.CanSimplifyToLambda(body, targetType, parameterTypes)) {
					var bodyNode = body.GetTargetNode() as Node;
					if(bodyNode as MultipurposeNode) {
						string result = bodyNode.GenerateValueCode();
						if(result.EndsWith(")")) {
							int deep = 0;
							for(int i = result.Length - 1; i > 0; i--) {
								var c = result[i];
								if(c == '(') {
									if(deep == 0) {
										result = result.Remove(i);
										break;
									} else {
										deep--;
									}
								} else if(c == ')' && i != result.Length - 1) {
									deep++;
								}
							}
						}
						return result;
					}
				}
			} else if(targetType.IsCastableTo(typeof(UnityEventBase))) {
				var method = targetType.GetMethod("AddListener");
				var param = method.GetParameters()[0].ParameterType;
				parameterTypes = param.GetGenericArguments();

				if(CG.IsRegularNode(this) && CG.CanSimplifyToLambda(body, typeof(void), parameterTypes)) {
					var bodyNode = body.GetTargetNode() as Node;
					if(bodyNode as MultipurposeNode) {
						string result = bodyNode.GenerateValueCode();
						if(result.EndsWith(")")) {
							int deep = 0;
							for(int i = result.Length - 1; i > 0; i--) {
								var c = result[i];
								if(c == '(') {
									if(deep == 0) {
										result = result.Remove(i);
										break;
									} else {
										deep--;
									}
								} else if(c == ')' && i != result.Length - 1) {
									deep++;
								}
							}
						}
						return result;
					}
				}
			} else {
				throw new Exception("Unsupported event to hook:" + target.DisplayName(true));
			}
			//Generate lambda code
			string contents = null;
			List<Type> types = new List<Type>();
			List<string> parameterNames = new List<string>();
			for(int i = 0; i < parameterTypes.Length; i++) {
				var pType = parameterTypes[i];
				if(pType != null) {
					string varName = null;
					if(!CG.CanDeclareLocal(this, nameof(parameters), i, new Node[] { this })) {
						varName = CG.GenerateVariableName("tempVar", this);
						contents = CG.RegisterInstanceVariable(this, nameof(parameters), i, pType) + " = " + varName + ";" + contents.AddLineInFirst();
					} else {
						varName = CG.GetOutputName(this, nameof(parameters), i);
					}
					types.Add(pType);
					parameterNames.Add(varName);

				}
			}
			contents += CG.Flow(body, this).AddLineInFirst();
			return CG.Lambda(types, parameterNames, contents);
		}

		public override void CheckError() {
			uNodeUtility.CheckError(target, this, "target");
			uNodeUtility.CheckError(body, this, "body");
			base.CheckError();
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(body);
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.EventIcon);
		}
	}
}