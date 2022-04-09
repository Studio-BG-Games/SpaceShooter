using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Behavior Tree.Composites", "Sequence", IsCoroutine = true)]
	[Description("Execute each node and return Success if all event Success"
		+ "\nit similar to an \"And\" operator."
		+ "\nIt will Failure when one of the event Failure")]
	[AddComponentMenu("")]
	public class Sequence : Node {
		[HideInInspector, FlowOut(finishedFlow =true)]
		public List<MemberData> targetNodes = new List<MemberData>() { new MemberData() };

		IEnumerator OnUpdate() {
			for(int i = 0; i < targetNodes.Count; i++) {
				var t = targetNodes[i];
				if(!t.isAssigned)
					continue;
				Node n;
				WaitUntil w;
				if(!t.ActivateFlowNode(out n, out w)) {
					yield return w;
				}
				if(n == null) {
					throw new System.Exception("targetNode must be FlowNode");
				}
				JumpStatement js = n.GetJumpState();
				if(js != null) {
					jumpState = js;
					Finish();
					yield break;
				}
				if(n.currentState == StateType.Failure) {
					state = StateType.Failure;
					Finish();
					yield break;
				}
			}
			Finish();
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
			for(int i = 0; i < targetNodes.Count; i++) {
				var node = targetNodes[i].GetTargetNode();
				if(node != null) {
					//Register each target node as state node, because this node need to compare the target state.
					CG.RegisterAsStateNode(node);
				}
			}
		}

		public override string GenerateCode() {
			string data = null;
			for(int i = 0; i < targetNodes.Count; i++) {
				var node = targetNodes[i].GetTargetNode();
				if(node != null) {
					if(!string.IsNullOrEmpty(data)) {
						data += ", ";
					}
					data += CG.GetEvent(targetNodes[i]);
				}
			}
			return CG.New(typeof(Runtime.Sequence), data);
		}

		public override System.Type GetNodeIcon() {
			return typeof(TypeIcons.BranchIcon);
		}
	}
}
