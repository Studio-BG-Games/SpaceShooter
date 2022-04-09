using System.Collections;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Flow", "YieldReturn", IsCoroutine=true)]
	[AddComponentMenu("")]
	public class NodeYieldReturn : Node {
		[Hide, FlowOut("", true)]
		public MemberData onFinished = new MemberData();
		[Hide, ValueIn]
		public MemberData value = new MemberData(null, MemberData.TargetType.Null);

		public override void OnExecute() {
			if(value.isAssigned) {
				owner.StartCoroutine(DoYield(), this);
			} else {
				throw new System.Exception("Target is unassigned.");
			}
		}

		IEnumerator DoYield() {
			yield return value.Get();
			Finish(onFinished);
		}

		public override string GetRichName() {
			return uNodeUtility.WrapTextWithKeywordColor("yield return ") + value.GetNicelyDisplayName(richName:true);
		}

		public override bool IsSelfCoroutine() {
			return true;
		}

		public override void OnGeneratorInitialize() {
			if(CG.IsStateNode(this)) {
				CG.SetStateInitialization(this, () => {
					return CG.Routine(
						CG.Routine(CG.SimplifiedLambda(CG.Value(value))),
						onFinished.isAssigned ? CG.Routine(CG.GetEvent(onFinished)) : null
					);
				});
				var finishedNode = onFinished.GetTargetNode();
				if(finishedNode != null)
					CG.RegisterAsStateNode(finishedNode);
			}
		}

		public override string GenerateCode() {
			if(!value.isAssigned) throw new System.Exception("Unassigned value");
			return CG.Flow(
				CG.YieldReturn(CG.Value(value)),
				CG.FlowFinish(this, true, false, false, onFinished)
			);
		}
	}
}
