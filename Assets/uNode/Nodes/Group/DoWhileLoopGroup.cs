using System.Collections;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	//[NodeMenu("Group", "DoWhileLoop")]
	[Description("The do statement executes its start node repeatedly until a condition to false.")]
	[AddComponentMenu("")]
	public class DoWhileLoopGroup : GroupNode {
		[EventType(EventData.EventType.Condition)]
		public EventData condition;

		public override void OnExecute() {
			if(IsCoroutine()) {
				owner.StartCoroutine(OnUpdate(), this);
			} else {
				do {
					JumpStatement js = nodeToExecute.ActivateAndFindJumpState();
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
				} while(condition.Validate(owner));
				Finish();
			}
		}

		IEnumerator OnUpdate() {
			do {
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
			} while(condition.Validate(owner));
			Finish();
		}

		public override string GenerateCode() {
			string data = condition.GenerateCode(this, EventData.EventType.Condition);
			if(!string.IsNullOrEmpty(data)) {
				data = "do {" + CG.GenerateFlowCode(nodeToExecute, this).AddLineInFirst().AddTabAfterNewLine(1).AddLineInEnd() + "}\nwhile(" + data + ");";
				data += CG.FlowFinish(this, true, false, false);
				return data;
			}
			return null;
		}
	}
}
