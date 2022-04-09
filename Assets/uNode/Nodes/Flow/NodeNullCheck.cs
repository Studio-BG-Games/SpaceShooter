using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Flow", "Null Check")]
	[AddComponentMenu("")]
	public class NodeNullCheck : Node {
		[Hide, ValueIn]
		public MemberData value = MemberData.none;
		[Hide, FlowOut("Not Null", true), Tooltip("Flow to execute when the value is not null")]
		public MemberData onNotNull = new MemberData();
		[Hide, FlowOut("Null", true), Tooltip("Flow to execute when the value is null")]
		public MemberData onNull = new MemberData();

		public override void OnExecute() {
			if(value.isAssigned) {
				var val = value.Get();
				bool isNull;
				if(val == null) {
					isNull = true;
				} else {
					isNull = val.Equals(null);
				}
				if(isNull) {
					state = StateType.Success;
					Finish(onNull);
				} else {
					state = StateType.Failure;
					Finish(onNotNull);
				}
			} else {
				Finish();
			}
		}

		public override string GenerateCode() {
			return CG.If(
				CG.Compare(value.CGValue(), CG.Null, ComparisonType.Equal), 
				CG.FlowFinish(this, true, onNull), 
				CG.FlowFinish(this, false, onNotNull)
			);
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(onNotNull) || HasCoroutineInFlow(onNull);
		}
	}
}
