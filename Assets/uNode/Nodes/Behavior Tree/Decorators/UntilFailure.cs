using UnityEngine;
using System.Collections;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Behavior Tree.Decorators", "Until Failure", IsCoroutine = true)]
	[Description("Will execute target node until target Failure" +
		"\nThis will always return success.")]
	[AddComponentMenu("")]
	public class UntilFailure : Node {
		[Hide, FlowOut("", true)]
		public MemberData targetNode = new MemberData();

		public IEnumerator OnUpdate() {
			while(state == StateType.Running) {
				if(!targetNode.isAssigned) {
					Debug.LogError("No Target Event", this);
					Finish();
					yield break;
				}
				Node n;
				WaitUntil w;
				if(!targetNode.ActivateFlowNode(out n, out w)) {
					yield return w;
				}
				if(n == null) {
					throw new System.Exception("targetNode must be FlowNode");
				}
				JumpStatement js = n.GetJumpState();
				if(js != null) {
					if(js.jumpType == JumpStatementType.Continue) {
						continue;
					} else if(js.jumpType == JumpStatementType.Break) {
						Finish();
						yield break;
					}
					jumpState = js;
					Finish();
					yield break;
				}
				if(n.currentState == StateType.Failure) {
					Finish();
					yield break;
				}
				yield return null;
			}
		}

		public override void OnExecute() {
			owner.StartCoroutine(OnUpdate(), this);
		}

		public override bool IsSelfCoroutine() {
			return true;
		}

		public override void OnGeneratorInitialize() {
			//Register this node as state node, because this is coroutine node with state.
			CG.RegisterAsStateNode(this);
			CG.SetStateInitialization(this, () => CG.GenerateNode(this));
			var node = targetNode.GetTargetNode();
			if(node != null) {
				//Register each target node as state node, because this node need to compare the target state.
				CG.RegisterAsStateNode(node);
			}
		}

		public override string GenerateCode() {
			if(!targetNode.isAssigned)
				throw new System.Exception("Target is not assigned");
			return CG.New(typeof(Runtime.UntilFailure), CG.GetEvent(targetNode));
		}
	}
}
