using System.Collections;
using UnityEngine;

namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnTimerElapsed", "OnTimerElapsed")]
	public class OnTimerElapsed : TransitionEvent {
		[Filter(typeof(float))]
		public MemberData delay = new MemberData(1f);
		public bool unscaled;

		public override void OnEnter() {
			owner.StartCoroutine(Wait(), this);
		}

		IEnumerator Wait() {
			if(unscaled) {
				yield return new WaitForSecondsRealtime(delay.Get<float>());
			} else {
				yield return new WaitForSeconds(delay.Get<float>());
			}
			Finish();
		}
		public override void OnExit() {
			owner.StopCoroutine(Wait());
		}

		public override string GenerateOnEnterCode() {
			CG.SetStateInitialization(this, () => {
				if(unscaled) {
					return CG.Routine(
						CG.Invoke(typeof(Runtime.Routine), nameof(Runtime.Routine.WaitRealtime), CG.Value(delay)),
						CG.Routine(CG.Lambda(CG.StopEvent(GetStateNode()))),
						GetTargetNode() != null ? CG.Routine(CG.GetEvent(target)) : null
					);
				} else {
					return CG.Routine(
						CG.Invoke(typeof(Runtime.Routine), nameof(Runtime.Routine.Wait), CG.Value(delay)),
						CG.Routine(CG.Lambda(CG.StopEvent(GetStateNode()))),
						GetTargetNode() != null ? CG.Routine(CG.GetEvent(target)) : null
					);
				}
			});
			//if(unscaled) {
			//	CG.generatorData.AddEventCoroutineData(this, CG.YieldReturn(CG.New(typeof(WaitForSecondsRealtime), CG.Value(delay))).AddLineInFirst() + CG.FlowFinish(this).AddLineInFirst());
			//} else {
			//	CG.generatorData.AddEventCoroutineData(this, CG.YieldReturn(CG.New(typeof(WaitForSeconds), CG.Value(delay))).AddLineInFirst() + CG.FlowFinish(this).AddLineInFirst());
			//}
			return CG.RunEvent(this);
		}

		public override string GenerateOnExitCode() {
			return CG.StopEvent(this);
		}
	}
}
