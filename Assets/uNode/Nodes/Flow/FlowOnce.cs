using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Flow", "Once")]
	[AddComponentMenu("")]
	public class FlowOnce : CustomNode {
		[System.NonSerialized, Tooltip("The flow to execute the node.")]
		public FlowInput input = new FlowInput("In");
		[System.NonSerialized, Tooltip("Reset the once state.")]
		public FlowInput reset = new FlowInput("Reset");

		[Hide, FlowOut("Once", true), Tooltip("The flow to execute only once at first time node get executed.")]
		public MemberData output = new MemberData();
		[Hide, FlowOut("After", true), Tooltip("The flow to execute after once the node is executed twice or more.")]
		public MemberData after = new MemberData();

		private bool hasEnter = false;

		public override void OnRuntimeInitialize() {
			input.onExecute = () => {
				if(!hasEnter) {
					hasEnter = true;
					Finish(output);
				} else {
					Finish(after);
				}
			};
			reset.onExecute = () => {
				hasEnter = false;
			};
		}

		public override void OnGeneratorInitialize() {
			CG.RegisterFlowNode(this);
			string varName = CG.RegisterVariable(new VariableData("hasEnter", typeof(bool), false) {
				modifier = FieldModifier.PrivateModifier,
			});
			input.codeGeneration = () => {
				return CG.If(
					varName.CGNot(),
					varName.CGSet(true.CGValue()).AddStatement(CG.FlowFinish(this, true, output)),
					CG.FlowFinish(this, true, after)
				);
			};
			reset.codeGeneration = () => {
				return varName.CGSet(false.CGValue());
			};
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(output);
		}
	}
}
