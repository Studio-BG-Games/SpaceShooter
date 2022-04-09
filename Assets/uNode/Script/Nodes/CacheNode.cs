using System;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu("")]
	public class CacheNode : ValueNode {
		[Hide, ValueIn("Value")]
		public MemberData target = MemberData.none;
		[Hide, FlowOut("", true, hideOnNotFlowNode = true)]
		public MemberData onFinished = new MemberData();

		[Tooltip(@"If true, the cached variable is generated in local variable.
Please note: you may have compile error when you're getting the variable that's out of scope from the flow.
If you're not really need it, it is recommended to keep it false.")]
		public bool localVariable;
		[Tooltip("The name of variable for generation")]
		public string variableName = "cachedValue";

		private object cachedValue;
		private VariableData generatedVariable;

		protected override object Value() => cachedValue;

		public override void OnExecute() {
			cachedValue = target.Get();
			Finish(onFinished);
		}

		public override System.Type ReturnType() {
			if(target.isAssigned) {
				try {
					return target.type;
				}
				catch { }
			}
			return typeof(object);
		}

		public override void OnGeneratorInitialize() {
			generatedVariable = new VariableData(variableName, ReturnType()) {
				modifier = FieldModifier.PrivateModifier,
			};
			generatedVariable.Name = CG.RegisterVariable(generatedVariable, !localVariable);
		}

		public override string GenerateCode() {
			if(localVariable) {
				return CG.Flow(
					"var " + generatedVariable.Name.CGSet(target.CGValue()),
					CG.FlowFinish(this, true, false, false, onFinished)
				);
			}
			return CG.Flow(
				generatedVariable.Name.CGSet(target.CGValue()),
				CG.FlowFinish(this, true, false, false, onFinished)
			);
		}

		public override string GenerateValueCode() {
			return generatedVariable.Name;
		}

		public override void SetValue(object value) {
			cachedValue = value;
		}

		public override bool CanSetValue() {
			return true;
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.DatabaseIcon);
		}

		public override string GetNodeName() => "Cached";

		public override string GetRichName() {
			return $"Cache Value: {target.GetNicelyDisplayName(richName:true)}";
		}

		public override bool IsFlowNode() => true;

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(onFinished);
		}

		public override void CheckError() => uNodeUtility.CheckError(target, this, nameof(target));
	}
}