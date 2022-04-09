using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("★General", "BitwiseOperator")]
	public class BitwiseOperator : Action {
		public BitwiseType operatorType = BitwiseType.Or;
		public MemberData targetA = new MemberData();
		public MemberData targetB = new MemberData();
		[Filter(SetMember = true)]
		public MemberData storeValue;

		protected override void OnExecute() {
			storeValue.Set(uNodeHelper.BitwiseOperator(targetA.Get(), targetB.Get(), operatorType));
		}

		public override string GenerateCode(Object obj) {
			if(targetA.isAssigned && targetB.isAssigned) {
				if(storeValue.isAssigned) {
					return CG.Set(storeValue, CG.Operator(
						CG.Value((object)targetA),
						CG.Value((object)targetB), operatorType));
				}
			}
			throw new System.Exception("Target is unassigned");
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(targetA, owner, Name + " - targetA");
			uNode.uNodeUtility.CheckError(targetB, owner, Name + " - targetB");
			if(targetA.isAssigned && targetB.isAssigned) {
				try {
					if(targetA.type != null && targetB.type != null) {
						uNodeHelper.BitwiseOperator(ReflectionUtils.CreateInstance(targetA.type),
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