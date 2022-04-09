using UnityEngine;

namespace MaxyGames.Events {
	//[EventMenu("★General/ObjectComparison", "ObjectComparison")]
	public class ObjectComparison : Condition {
		public ComparisonType operatorType = ComparisonType.Equal;
		public MemberData targetA;
		public MemberData targetB;

		public override string Name {
			get {
				return string.Format("<b>{0}</b> {2} <b>{1}</b>",
					uNode.uNodeUtility.GetNicelyDisplayName(targetA),
					uNode.uNodeUtility.GetNicelyDisplayName(targetB),
					uNode.uNodeUtility.GetNicelyDisplayName(operatorType));
			}
		}

		protected override bool OnValidate() {
			return uNodeHelper.OperatorComparison(targetA.Get(), targetB.Get(), operatorType);
		}

		public override string GenerateConditionCode(Object obj) {
			return CG.Compare(
				CG.Value((object)targetA),
				CG.Value((object)targetB),
				operatorType);
		}
	}
}