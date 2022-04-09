using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Behavior Tree.Composites", "Random Sequence", IsCoroutine = true)]
	[DescriptionAttribute("Execute node randomly, it will return success if all node return success " +
		"and if one of the node return failure it will return failure.")]
	[AddComponentMenu("")]
	public class RandomSequence : Node {
		[HideInInspector, FlowOut(finishedFlow =true)]
		public List<MemberData> targetNodes = new List<MemberData>() { new MemberData() };

		IEnumerator OnUpdate() {
			List<int> eventIndex = new List<int>();
			for(int i = 0; i < targetNodes.Count; ++i) {
				eventIndex.Add(i);
			}
			List<int> randomOrder = new List<int>();
			for(int i = targetNodes.Count; i > 0; --i) {
				int index = Random.Range(0, i);
				randomOrder.Add(eventIndex[index]);
				eventIndex.RemoveAt(index);
			}
			for(int i = 0; i < targetNodes.Count; i++) {
				var t = targetNodes[randomOrder[i]];
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
			return CG.New(typeof(Runtime.RandomSequence), data);
		}

		public override System.Type GetNodeIcon() {
			return typeof(TypeIcons.BranchIcon);
		}
	}
}
