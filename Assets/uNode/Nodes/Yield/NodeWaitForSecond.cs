using UnityEngine;
using System.Collections;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Yield", "WaitForSecond", IsCoroutine = true)]
	[AddComponentMenu("")]
	public class NodeWaitForSecond : Node {
		[Hide, ValueIn, Filter(typeof(float))]
		public MemberData waitTime = new MemberData(1f);

		[Hide, FlowOut("", true)]
		public MemberData onFinished = new MemberData();

		public override void OnExecute() {
			owner.StartCoroutine(OnCall(), this);
		}

		public IEnumerator OnCall() {
			yield return new WaitForSeconds(waitTime.Get<float>());
			Finish(onFinished);
		}

		public override bool IsSelfCoroutine() {
			return true;
		}

		public override void OnGeneratorInitialize() {
			if(CG.IsStateNode(this)) {
				CG.SetStateInitialization(this, () => {
					return CG.Routine(
						CG.Invoke(typeof(Runtime.Routine), nameof(Runtime.Routine.Wait), CG.Value(waitTime)),
						onFinished.isAssigned ? CG.Routine(CG.GetEvent(onFinished)) : null
					);
				});
				var finishedNode = onFinished.GetTargetNode();
				if(finishedNode != null)
					CG.RegisterAsStateNode(finishedNode);
			}
		}

		public override string GenerateCode() {
			return "yield return new " + CG.Type(typeof(WaitForSeconds)) + "(" + CG.Value((object)waitTime) + ");" + CG.FlowFinish(this, true, false, false, onFinished).AddLineInFirst();
		}

		public override string GetRichName() {
			return "Wait For Second:" + waitTime.GetNicelyDisplayName(richName:true);
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(waitTime, this, "waitTime");
		}
	}
}
