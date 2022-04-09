using UnityEngine;
using System.Collections;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Behavior Tree.Decorators", "Inverter", HideOnFlow = true)]
	[Description("Invert target event state." +
		"\nThis will return success if target node is Failure, and will return Failure when target node Success." +
		"\nThis state will running when target node running.")]
	[AddComponentMenu("")]
	public class Inverter : Node {
		[Hide, FlowOut("", true)]
		public MemberData targetNode = new MemberData();

		public override void OnExecute() {
			owner.StartCoroutine(DoInvert(), this);
		}

		IEnumerator DoInvert() {
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
			if(n.currentState == StateType.Failure) {
				state = StateType.Success;
			} else {
				state = StateType.Failure;
			}
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
