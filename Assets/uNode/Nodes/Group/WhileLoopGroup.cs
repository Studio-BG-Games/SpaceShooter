using UnityEngine;
using System.Collections;

namespace MaxyGames.uNode.Nodes {
	//[NodeMenu("Group", "WhileLoop")]
	[DescriptionAttribute("The while statement executes a event until a Condition evaluates to false.")]
	[AddComponentMenu("")]
	public class WhileLoopGroup : GroupNode {
		[EventType(EventData.EventType.Condition)]
		public EventData condition;

		public override void OnExecute() {
			if(IsCoroutine()) {
				owner.StartCoroutine(OnUpdate(), this);
			} else {
				while(condition.Validate(owner)) {
					JumpStatement js = nodeToExecute.ActivateAndFindJumpState();
					if(js != null) {
						if(js .jumpType== JumpStatementType.Continue) {
							continue;
						} else {
							if(js.jumpType == JumpStatementType.Return) {
								jumpState = js;
							}
							break;
						}
					}
				}
				Finish();
			}
		}

		IEnumerator OnUpdate() {
			while(condition.Validate(owner)) {
				JumpStatement js = nodeToExecute.ActivateAndFindJumpState();
				if(!nodeToExecute.IsFinished()) {
					yield return nodeToExecute.WaitUntilFinish();
				}
				if(js != null) {
					if(js.jumpType == JumpStatementType.Continue) {
						continue;
					} else {
						if(js.jumpType == JumpStatementType.Return) {
							jumpState = js;
						}
						break;
					}
				}
			}
			Finish();
		}

		public override string GenerateCode() {
			string data = condition.GenerateConditionCode(this, CG.GenerateFlowCode(nodeToExecute, this));
			if(!string.IsNullOrEmpty(data)) {
				return data.Remove(0, 2).Insert(0, "while") + CG.FlowFinish(this, true, false, false);
			}
			return null;
		}
	}
}
