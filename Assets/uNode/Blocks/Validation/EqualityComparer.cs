using UnityEngine;

namespace MaxyGames.Events {
	//[EventMenu("★General/EqualityComparer", "EqualityComparer")]
	public class EqualityComparer : Condition {
		public ComparisonType operatorType = ComparisonType.Equal;
		public MemberData target = new MemberData();
		[ObjectType("target")]
		public MemberData value = MemberData.empty;

		public override string Name {
			get {
				return string.Format("<b>{0}</b> {2} <b>{1}</b>",
					uNode.uNodeUtility.GetNicelyDisplayName(target),
					uNode.uNodeUtility.GetNicelyDisplayName(value),
					uNode.uNodeUtility.GetNicelyDisplayName(operatorType));
			}
		}

		protected override bool OnValidate() {
			return uNodeHelper.OperatorComparison(target.Get(), value.Get(), operatorType);
		}

		public override string GenerateConditionCode(Object obj) {
			return CG.Compare(
				CG.Value((object)target),
				CG.Value((object)value), operatorType);
		}
	}
}