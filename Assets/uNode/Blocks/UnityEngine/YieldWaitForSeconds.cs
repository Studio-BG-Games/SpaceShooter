using System.Collections;
using UnityEngine;
using MaxyGames.uNode;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "Yield.WaitForSeconds", true)]
	public class YieldWaitForSeconds : CoroutineAction {
		[ObjectType(typeof(float))]
		public MemberData waitTime;
		[Tooltip("If true, suspends the execution using unscaled time.")]
		public bool realTime;

		protected override IEnumerator ExecuteCoroutine() {
			if(!realTime) {
				yield return new WaitForSeconds(waitTime.Get<float>());
			} else {
				yield return new WaitForSecondsRealtime(waitTime.Get<float>());
			}
		}

		public override string GenerateCode(Object obj) {
			if(!realTime) {
				return CG.YieldReturn(CG.New(typeof(WaitForSeconds), waitTime.CGValue()));
			} else {
				return CG.YieldReturn(CG.New(typeof(WaitForSecondsRealtime), waitTime.CGValue()));
			}
		}

		public override string GetDescription() {
			return "Suspends the execution for the given amount of seconds.";
		}
	}
}