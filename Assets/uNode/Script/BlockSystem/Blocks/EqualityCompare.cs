using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("★General", "EqualityComparer")]
	public class EqualityCompare : Condition {
		public ComparisonType operatorType = ComparisonType.Equal;
		[Filter(MaxMethodParam = int.MaxValue)]
		public MultipurposeMember target = new MultipurposeMember();
		[ObjectType("target")]
		public MemberData value = MemberData.none;

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
				CG.Value(target),
				CG.Value((object)value), operatorType);
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(target, owner, Name + " - target");
			uNode.uNodeUtility.CheckError(value, owner, Name + " - value");
		}
	}
}