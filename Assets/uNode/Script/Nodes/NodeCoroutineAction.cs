using System;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Flow", "CoroutineAction", order = 10)]
	[AddComponentMenu("")]
	public class NodeCoroutineAction : Node {
		[EventType(EventData.EventType.Action, supportCoroutine = true)]
		public EventData action = new EventData();
		[UnityEngine.Tooltip("If true, will execute action all at once and wait till finish.")]
		public bool executeInParallel;

		[Hide, FlowOut("", true)]
		public MemberData onFinished = new MemberData();

		public override void OnExecute() {
			if(!executeInParallel) {
				action.StartAction(owner, StartCoroutine, StopCoroutine, () => {
					Finish(onFinished);
				});
			} else {
				action.StartActionInParallel(owner, StartCoroutine, StopCoroutine, () => {
					Finish(onFinished);
				});
			}
		}

		public override void Stop() {
			action.StopAction();//Stop the action.
			base.Stop();//Make sure the default Stop is called.
		}

		public override string GenerateCode() {
			if(action != null) {
				string stopCode = action.GenerateStopCode(this);
				if(!string.IsNullOrEmpty(stopCode)) {
					CG.SetStateStopAction(this, stopCode);
				}
				string code = action.GenerateCoroutineCode(this, executeInParallel);
				return code + CG.FlowFinish(this, true, false, false, onFinished).AddLineInFirst();
			}
			return CG.FlowFinish(this, true, false, false, onFinished);
		}

		public override void CheckError() {
			base.CheckError();
			action.CheckError(this);
		}

		public override bool IsSelfCoroutine() {
			return true;
		}

		public override string GetNodeName() {
			return gameObject.name;
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.ActionIcon);
		}
	}
}
