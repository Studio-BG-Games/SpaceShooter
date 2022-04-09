using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Flow", "Toggle")]
	[Description("When In is called, calls On or Off depending on the current toggle state. Whenever Toggle input is called the state changes.")]
	[AddComponentMenu("")]
	public class FlowToggle : CustomNode {
		[System.NonSerialized, Tooltip("Input flow to execute the node.")]
		public FlowInput input = new FlowInput("In");
		[System.NonSerialized, Tooltip("Turn toggle state to on.")]
		public FlowInput turnOn = new FlowInput("On");
		[System.NonSerialized, Tooltip("Turn toggle state to off.")]
		public FlowInput turnOff = new FlowInput("Off");
		[System.NonSerialized, Tooltip("Invert the toggle state.")]
		public FlowInput toggle = new FlowInput("Toggle");
		[Hide, FlowOut("On", true), Tooltip("Flow to execute when the toggle is on.")]
		public MemberData onOpen = new MemberData();
		[Hide, FlowOut("Off", true), Tooltip("Flow to execute when the toggle is off.")]
		public MemberData onClosed = new MemberData();
		[Hide, FlowOut("Turned On", true), Tooltip("Flow to execute when the toggle is turned on.")]
		public MemberData onTurnedOn = new MemberData();
		[Hide, FlowOut("Turned Off", true), Tooltip("Flow to execute when the toggle is turned off.")]
		public MemberData onTurnedOff = new MemberData();
		[ValueOut("Is On", typeof(bool)), Tooltip("The toggle state value.")]
		public bool open;

		public override void OnRuntimeInitialize() {
			input = new FlowInput("In", () => {
				if(open) {
					Finish(onOpen);
				} else {
					Finish(onClosed);
				}
			});
			turnOn = new FlowInput("On", () => {
				if(!open) {
					open = true;
					Finish(onTurnedOn);
				}
			});
			turnOff = new FlowInput("Off", () => {
				if(open) {
					open = false;
					Finish(onTurnedOff);
				}
			});
			toggle = new FlowInput("Toggle", () => {
				open = !open;
				if(open) {
					Finish(onTurnedOn);
				} else {
					Finish(onTurnedOff);
				}
			});
		}

		public override void OnGeneratorInitialize() {
			CG.RegisterFlowNode(this);
			string varName = CG.RegisterInstanceVariable(this, nameof(open));
			input.codeGeneration = () => {
				return CG.If(varName,
					CG.FlowFinish(this, true, onOpen),
					CG.FlowFinish(this, true, onClosed));
			};
			turnOn.codeGeneration = () => {
				return CG.If(varName.CGNot(),
					CG.Flow(
						CG.Set(varName, true.CGValue()),
						CG.FlowFinish(this, true, onTurnedOn))
				);
			};
			turnOff.codeGeneration = () => {
				return CG.If(varName,
					CG.Flow(
						CG.Set(varName, false.CGValue()),
						CG.FlowFinish(this, true, onTurnedOff))
				);
			};
			toggle.codeGeneration = () => {
				return CG.Flow(
					CG.Set(varName, "!" + varName),
					CG.If(
						varName,
						CG.FlowFinish(this, true, onTurnedOn),
						CG.FlowFinish(this, true, onTurnedOff))
				);
			};
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(onOpen, onClosed);
		}
	}
}
