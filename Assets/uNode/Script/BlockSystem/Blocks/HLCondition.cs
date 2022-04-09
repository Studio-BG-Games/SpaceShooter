using System;
using System.Collections.Generic;
using MaxyGames.uNode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MaxyGames.Events {
	public class HLCondition : Condition {
		[Hide]
		public MemberData type = MemberData.none;
		[HideInInspector]
		public List<FieldValueData> initializers = new List<FieldValueData>();

		private new object instance;

		private void Init() {
			if(instance != null) return;
			var instanceType = type.startType;
			if (instanceType != null) {
				instance = ReflectionUtils.CreateInstance(instanceType);
			}
		}
		
		protected override bool OnValidate() {
			Init();
			if(instance is IDataNode<bool>) {
				return (instance as IDataNode<bool>).GetValue(base.instance);
			}
			throw new InvalidOperationException();
		}

		public override string GenerateCode(Object obj) {
			if(obj is INode) {
				//Ensure we are targeting graph so the generated code will using 'this' keyword
				obj = (obj as INode).GetNodeOwner() as Object;
			}
			Init();
			if(!CG.HasUserObject(this)) {
				foreach(var init in initializers) {
					if(init.value.CanSafeGetValue()) {
						var field = instance.GetType().GetField(init.name);
						if(field != null) {
							field.SetValueOptimized(instance, init.value.Get());
						}
					}
				}
				CG.RegisterUserObject(new VariableData(Name, instance.GetType(), instance) {
					modifier = FieldModifier.PrivateModifier,
				}, this);
			}
			var variable = CG.GetUserObject<VariableData>(this);
			string generatedInstanceName = CG.RegisterVariable(variable);
			//Initialize instance
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			foreach(var init in initializers) {
				if(init.value.isAssigned && !init.value.CanSafeGetValue()) { //Ensure we are only set the dynamic value
					builder.Append(generatedInstanceName.CGAccess(init.name).CGSet(init.value.CGValue()).AddLineInFirst());
				}
			}
			string initCode = builder.ToString();
			string invoke = null;
			if(instance is IDataNode<bool>) {
				invoke = generatedInstanceName.CGInvoke(nameof(IDataNode<bool>.GetValue), obj.CGValue());
			} else {
				throw new InvalidOperationException("The type must implement IDataNode<bool>");
			}
			if(string.IsNullOrEmpty(initCode)) {
				return invoke;
			} else {
				return typeof(uNodeUtility).CGType().CGInvoke(
					nameof(uNodeUtility.RuntimeGetValue),
					CG.Lambda(null, null,
						CG.Flow(
							initCode,
							CG.Return(invoke)
						)));
			}
		}

		public override string Name {
			get {
				Type instancecType = type.startType;
				if (instancecType != null) {
					if (instancecType.IsDefined(typeof(BlockMenuAttribute), true)) {
						return (instancecType.GetCustomAttributes(typeof(BlockMenuAttribute), true)[0] as BlockMenuAttribute).name;
					}
				} else {
					return "Missing Type";
				}
				return type.DisplayName(false, false);
			}
		}

		public override string ToolTip {
			get {
				
				return base.ToolTip;
			}
		}
	}
}