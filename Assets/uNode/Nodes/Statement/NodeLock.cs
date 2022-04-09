using UnityEngine;
using System.Collections;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Statement", "Lock")]
	[AddComponentMenu("")]
	public class NodeLock : Node {
		[Hide, ValueIn("Target")]
		public MemberData target = new MemberData();

		[HideInInspector, FlowOut("Body", displayFlowInHierarchy =false)]
		public MemberData body = new MemberData();
		[Hide, FlowOut("Next", true)]
		public MemberData onFinished = new MemberData();

		public override void OnExecute() {
			if(!body.isAssigned) {
				throw new System.Exception("body is unassigned");
			}
			if(HasCoroutineInFlow(body)) {
				owner.StartCoroutine(OnUpdate(), this);
			} else {
				lock(target.Get()) {
					Node n;
					WaitUntil w;
					if(!body.ActivateFlowNode(out n, out w)) {
						throw new System.Exception("body is not coroutine but body is not finished.");
					}
					if(n != null) {
						jumpState = n.GetJumpState();
						if(jumpState != null) {
							Finish();
							return;
						}
					}
				}
				Finish(onFinished);
			}
		}

		IEnumerator OnUpdate() {
			lock(target.Get()) {
				Node n;
				WaitUntil w;
				if(!body.ActivateFlowNode(out n, out w)) {
					yield return w;
				}
				if(n != null) {
					jumpState = n.GetJumpState();
					if(jumpState != null) {
						Finish();
						yield break;
					}
				}
			}
			Finish(onFinished);
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(body, onFinished);
		}

		public override string GenerateCode() {
			if(!body.isAssigned) {
				throw new System.Exception("body is unassigned");
			}
			string data = CG.Condition("lock", CG.Value((object)target), CG.Flow(body, this));
			return data + CG.FlowFinish(this, true, false, false, onFinished);
		}

		public override string GetRichName() {
			return uNodeUtility.WrapTextWithKeywordColor("lock: ") + target.GetNicelyDisplayName(richName:true);
		}
	}
}
