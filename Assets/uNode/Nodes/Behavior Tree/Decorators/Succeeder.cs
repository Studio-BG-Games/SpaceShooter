using UnityEngine;
using System.Collections;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Behavior Tree.Decorators", "Succeeder", HideOnFlow = true)]
	[Description("Always success regardless of whether the targetNode success or failure.")]
	[AddComponentMenu("")]
	public class Succeeder : Node {
		[Hide, FlowOut("", true)]
		public MemberData targetNode = new MemberData();

		public override void OnExecute() {
			owner.StartCoroutine(Do(), this);
		}

		IEnumerator Do() {
			if(!targetNode.isAssigned) {
				Debug.LogError("Unassigned target node", this);
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
				jumpState = js;
			}
			state = StateType.Success;
			Finish();
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
			return CG.New(typeof(Runtime.Inverter), CG.GetEvent(targetNode));
		}
	}
}
