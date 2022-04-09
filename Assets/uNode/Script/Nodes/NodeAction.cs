using System;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Flow", "Action", order = 10)]
	[AddComponentMenu("")]
	public class NodeAction : Node {
		[EventType(EventData.EventType.Action)]
		public EventData Action = new EventData();

		[Hide, FlowOut("", true)]
		public MemberData onFinished = new MemberData();

		public override void OnExecute() {
			if(Action != null)
				Action.Execute(owner);
			Finish(onFinished);
		}

		public override string GenerateCode() {
			if(Action != null) {
				string code = Action.GenerateCode(this, EventData.EventType.Action);
				return code + CG.FlowFinish(this, true, false, false, onFinished).AddLineInFirst();
			}
			return CG.FlowFinish(this, true, false, false, onFinished);
		}

		public override void CheckError() {
			base.CheckError();
			Action.CheckError(this);
		}

		public override string GetNodeName() {
			return gameObject.name;
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(onFinished);
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.ActionIcon);
		}
	}
}
