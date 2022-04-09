using System.Collections;
using UnityEngine;
namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "Yield.WaitForEndOfFrame", true)]
	public class YieldWaitForEndOfFrame : CoroutineAction {
		protected override IEnumerator ExecuteCoroutine() {
			yield return new WaitForEndOfFrame();
		}

		public override string GenerateCode(Object obj) {
			return CG.YieldReturn(CG.New(typeof(WaitForEndOfFrame)));
		}

		public override string GetDescription() {
			return "Waits until the end of the frame after all cameras and GUI is rendered, just before displaying the frame on screen.";
		}
	}
}