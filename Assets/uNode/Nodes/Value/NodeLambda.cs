using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Data", "Lambda", typeof(System.Delegate))]
	[AddComponentMenu("")]
	public class NodeLambda : ValueNode {
		[Filter(typeof(System.Delegate), OnlyGetType=true)]
		public MemberData delegateType = MemberData.CreateFromType(typeof(System.Action));

		[HideInInspector, FlowOut("Body", localFunction =true)]
		public MemberData body = new MemberData();
		[HideInInspector]
		public MemberData input = new MemberData();

		[HideInInspector]
		public List<object> parameterValues = new List<object>();

		private System.Delegate m_Delegate;
		private System.Reflection.MethodInfo methodInfo;

		private void InitDelegate() {
			if(!delegateType.isAssigned) return;
			var type = delegateType.Get<System.Type>();
			methodInfo = type.GetMethod("Invoke");
			if(methodInfo.ReturnType == typeof(void)) {
				m_Delegate = CustomDelegate.CreateActionDelegate((obj) => {
					if(owner == null)
						return;
					if(obj != null) {
						while(parameterValues.Count < obj.Length) {
							parameterValues.Add(null);
						}
						for(int i = 0; i < obj.Length; i++) {
							parameterValues[i] = obj[i];
						}
					}
					body.InvokeFlow();
				}, methodInfo.GetParameters().Select(i => i.ParameterType).ToArray());
			} else {
				var types = methodInfo.GetParameters().Select(i => i.ParameterType).ToList();
				types.Add(methodInfo.ReturnType);
				m_Delegate = CustomDelegate.CreateFuncDelegate((obj) => {
					if(owner == null)
						return null;
					if(obj != null) {
						while(parameterValues.Count < obj.Length) {
							parameterValues.Add(null);
						}
						for(int i = 0; i < obj.Length; i++) {
							parameterValues[i] = obj[i];
						}
					}
					return input.Get(methodInfo.ReturnType);
				}, types.ToArray());
			}
			// m_Delegate = ReflectionUtils.ConvertDelegate(m_Delegate, e.EventHandlerType);
		}

		protected override object Value() {
			if(m_Delegate == null) {
				InitDelegate();
			}
			return m_Delegate;
		}

		public override bool CanGetValue() {
			return true;
		}

		public override System.Type ReturnType() {
			if(!delegateType.isAssigned) return typeof(object);
			var type = delegateType.Get<System.Type>();
			methodInfo = type.GetMethod("Invoke");
			if(methodInfo != null) {
				if(methodInfo.ReturnType == typeof(void)) {
					return CustomDelegate.GetActionDelegateType(methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
				} else {
					var types = methodInfo.GetParameters().Select(i => i.ParameterType).ToList();
					types.Add(methodInfo.ReturnType);
					return CustomDelegate.GetFuncDelegateType(types.ToArray());
				}
			}
			return typeof(object);
		}

		public override string GenerateValueCode() {
			if(!delegateType.isAssigned) throw new System.Exception("Delegate Type is not assigned");
			var type = delegateType.Get<System.Type>();
			var methodInfo = type.GetMethod("Invoke");
			var paramTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
			string contents = null;
			List<string> parameterNames = new List<string>();
			for(int i = 0; i < paramTypes.Length; i++) {
				string varName = null;
				System.Type pType = paramTypes[i];
				if(pType != null) {
					if(!CG.CanDeclareLocal(this, nameof(parameterValues), i, body)) {//Auto generate instance variable for parameter.
						varName = CG.GenerateVariableName("tempVar", GetInstanceID().ToString() + i);
						contents = CG.RegisterInstanceVariable(this, nameof(parameterValues), i, pType) + " = " + varName + ";" + contents.AddLineInFirst();
					} else {
						varName = CG.GetOutputName(this, nameof(parameterValues), i);
					}
					parameterNames.Add(varName);
				}
			}
			if(methodInfo.ReturnType == typeof(void)) {
				CG.BeginBlock(allowYield:false); //Ensure that there is no yield statement
				contents += CG.Flow(body, this, false).AddLineInFirst();
				CG.EndBlock();
				return CG.Lambda(paramTypes, parameterNames, contents);
			} else {
				contents += CG.Return(input.CGValue());
				CG.EndBlock();
				return CG.Lambda(paramTypes, parameterNames, contents);
			}
		}

		public override string GetNodeName() {
			return "Lambda";
		}

		public override void CheckError() {
			uNodeUtility.CheckError(delegateType, this, nameof(delegateType), false);
		}
	}
}