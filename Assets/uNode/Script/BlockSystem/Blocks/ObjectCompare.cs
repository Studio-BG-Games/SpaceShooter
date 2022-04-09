using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("★General", "ObjectComparison")]
	public class ObjectCompare : Condition {
		public ComparisonType operatorType = ComparisonType.Equal;
		[Filter(MaxMethodParam = int.MaxValue)]
		public MultipurposeMember targetA = new MultipurposeMember();
		[Filter(MaxMethodParam = int.MaxValue)]
		public MultipurposeMember targetB = new MultipurposeMember();

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
				CG.Value(targetA), 
				CG.Value(targetB), 
				operatorType);
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(targetA, owner, Name + " - targetA");
			uNode.uNodeUtility.CheckError(targetB, owner, Name + " - targetB");
		}
	}
}