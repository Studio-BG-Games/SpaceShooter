using System;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu("")]
	public class NodeReroute : Node {
		public enum RerouteKind {
			Flow,
			Value,
		}
		[Hide]
		public RerouteKind kind;
		[Hide]
		public MemberData target = MemberData.none;
		[Hide]
		public MemberData onFinished = MemberData.none;

		public override void OnExecute() {
			Finish(onFinished);
		}

		public override Type ReturnType() {
			return target.type;
		}

		public override object GetValue() {
			return target.Get();
		}

		public override bool CanGetValue() {
			if(IsFlowNode())
				return false;
			return target.isAssigned ? target.CanGetValue() : true;
		}

		public override void SetValue(object value) {
			target.Set(value);
		}

		public override bool CanSetValue() {
			if(IsFlowNode())
				return false;
			return target.isAssigned ? target.CanSetValue() : false;
		}

		public override bool IsFlowNode() {
			return kind == RerouteKind.Flow;
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(onFinished);
		}

		public override Type GetNodeIcon() {
			return IsFlowNode() ? typeof(TypeIcons.FlowIcon) : ReturnType();
		}

		public override string GenerateCode() {
			return CG.FlowFinish(this, true, false, false, onFinished);
		}

		public override string GenerateValueCode() {
			return CG.Value((object)target);
		}

		public override string GetNodeName() {
			//if(target.isAssigned) {
			//	switch(target.targetType) {
			//		case MemberData.TargetType.FlowNode:
			//		case MemberData.TargetType.ValueNode:
			//			return target.GetTargetNode()?.GetNodeName();
			//		default:
			//			return target.DisplayName();
			//	}
			//}
			return "Reroute";
		}

		public override string GetRichName() {
			if(target.isAssigned) {
				switch(target.targetType) {
					case MemberData.TargetType.FlowNode:
					case MemberData.TargetType.ValueNode:
						return target.GetTargetNode()?.GetRichName();
					default:
						return target.GetNicelyDisplayName(richName: true);
				}
			}
			return base.GetRichName();
		}
	}
}