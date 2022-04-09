using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("★General", "CompareOperator")]
	public class CompareOperator : Action {
		public ComparisonType operatorType = ComparisonType.Equal;
		public MemberData targetA = MemberData.none;
		public MemberData targetB = MemberData.none;
		[Filter(typeof(bool), SetMember = true)]
		public MemberData storeBool = MemberData.none;

		public override string Name {
			get {
				if(!storeBool.isAssigned) {
					return string.Format("Compare: <b>{0}</b> {1} <b>{2}</b>",
						uNode.uNodeUtility.GetNicelyDisplayName(targetA),
						uNode.uNodeUtility.GetDisplayName(operatorType),
						uNode.uNodeUtility.GetNicelyDisplayName(targetB));
				}
				return string.Format("Compare: <b>{0}</b> {1} <b>{2}</b> store to <b>{3}</b>",
					uNode.uNodeUtility.GetNicelyDisplayName(targetA),
					uNode.uNodeUtility.GetDisplayName(operatorType),
					uNode.uNodeUtility.GetNicelyDisplayName(targetB),
					uNode.uNodeUtility.GetNicelyDisplayName(storeBool));
			}
		}

		protected override void OnExecute() {
			storeBool.Set(uNodeHelper.OperatorComparison(targetA.Get(), targetB.Get(), operatorType));
		}

		public override string GenerateCode(Object obj) {
			if(targetA.isAssigned && targetB.isAssigned) {
				if(storeBool.isAssigned) {
					return CG.Set(storeBool,
						CG.Compare(CG.Value((object)targetA),
						CG.Value((object)targetB), operatorType));
				}
			}
			throw new System.Exception();
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(targetA, owner, Name + " - targetA");
			uNode.uNodeUtility.CheckError(targetB, owner, Name + " - targetB");
			if(targetA.isAssigned && targetB.isAssigned) {
				try {
					if(targetA.type != null && targetB.type != null) {
						uNodeHelper.OperatorComparison(
							ReflectionUtils.CreateInstance(targetA.type),
							ReflectionUtils.CreateInstance(targetB.type), operatorType);
					}
				}
				catch(System.Exception ex) {
					uNode.uNodeUtility.RegisterEditorError(owner, ex.Message);
				}
			}
		}
	}
}