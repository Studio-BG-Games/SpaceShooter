using System;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Flow", "Validation", order = 10)]
	[AddComponentMenu("")]
	public class NodeValidation : Node {
		[EventType(EventData.EventType.Condition)]
		public EventData Validation = new EventData();
		[Hide, FlowOut("True")]
		public MemberData onTrue = new MemberData();
		[Hide, FlowOut("False")]
		public MemberData onFalse = new MemberData();
		[Hide, FlowOut("Finished", true)]
		public MemberData onFinished = new MemberData();

		public override void OnExecute() {
			if(Validation.Validate(owner)) {
				state = StateType.Success;
				Finish(onFinished, onTrue);
			} else {
				state = StateType.Failure;
				Finish(onFinished, onFalse);
			}
		}

		public override string GenerateCode() {
			if(Validation != null) {
				if(CG.IsStateNode(this)) {
					CG.SetStateInitialization(this,
						CG.New(
							typeof(Runtime.Conditional),
							CG.SimplifiedLambda(Validation.GenerateCode(this, EventData.EventType.Condition)),
							CG.GetEvent(onTrue).AddFirst("onTrue: "),
							CG.GetEvent(onFalse).AddFirst("onFalse: "),
							CG.GetEvent(onFinished).AddFirst("onFinished: ")
						));
					return null;
				} else if(CG.debugScript) {
					return Validation.GenerateConditionCode(this,
						CG.FlowFinish(this, true, true, false, onTrue, onFinished),
						CG.FlowFinish(this, false, true, false, onFalse, onFinished)
					);
				}
				if(onTrue.isAssigned) {
					if(onFalse.isAssigned) {
						//True and False is assigned
						return CG.Flow(
							Validation.GenerateConditionCode(this, CG.Flow(onTrue, this), CG.Flow(onFalse, this)),
							CG.FlowFinish(this, true, false, false, onFinished)
						);
					} else {
						//True only
						return CG.Flow(
							Validation.GenerateConditionCode(this, CG.Flow(onTrue, this)),
							CG.FlowFinish(this, true, false, false, onFinished)
						);
					}
				} else if(onFalse.isAssigned) {
					//False only
					return CG.Flow(
						CG.If(
							Validation.GenerateCode(this, EventData.EventType.Condition).CGNot(true),
							CG.Flow(onFalse, this)),
						CG.FlowFinish(this, false, true, false, onFinished)
					);
				} else {
					//No true and False
					return CG.Flow(
						Validation.GenerateConditionCode(this, null),
						CG.FlowFinish(this, true, false, false, onFinished)
					);
				}
			}
			return CG.FlowFinish(this, true, false, false, onFinished);
		}

		public override void CheckError() {
			base.CheckError();
			Validation.CheckError(this);
		}
		
		public override string GetNodeName() {
			return gameObject.name;
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.ValidationIcon);
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(onFinished, onFalse, onTrue);
		}
	}
}
