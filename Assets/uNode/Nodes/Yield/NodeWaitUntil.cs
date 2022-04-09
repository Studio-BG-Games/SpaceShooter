using UnityEngine;
using System.Collections;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Yield", "WaitUntil", IsCoroutine = true)]
	[Description("Waits until condition evaluate to true.")]
	[AddComponentMenu("")]
	public class NodeWaitUntil : Node {
		[EventType(EventData.EventType.Condition)]
		public EventData Condition = new EventData();
		[Hide, FlowOut("", true)]
		public MemberData onFinished = new MemberData();

		public override void OnExecute() {
			owner.StartCoroutine(OnCall(), this);
		}

		public IEnumerator OnCall() {
			yield return new WaitUntil(() => Condition.Validate(owner));
			Finish(onFinished);
		}

		public override bool IsSelfCoroutine() {
			return true;
		}

		public override void OnGeneratorInitialize() {
			if(CG.IsStateNode(this)) {
				CG.SetStateInitialization(this, () => {
					return CG.Routine(
						CG.Routine(CG.SimplifiedLambda(CG.New(typeof(WaitUntil), CG.SimplifiedLambda(Condition.GenerateCode(this, EventData.EventType.Condition))))),
						onFinished.isAssigned ? CG.Routine(CG.GetEvent(onFinished)) : null
					);
				});
				var finishedNode = onFinished.GetTargetNode();
				if(finishedNode != null)
					CG.RegisterAsStateNode(finishedNode);
			}
		}

		public override string GenerateCode() {
			return CG.Flow(
				CG.YieldReturn(CG.New(typeof(WaitUntil), CG.SimplifiedLambda(Condition.GenerateCode(this, EventData.EventType.Condition)))),
				CG.FlowFinish(this, true, onFinished)
			);
		}

		public override void CheckError() {
			base.CheckError();
			if(Condition != null)
				Condition.CheckError(this);
		}
	}
}
