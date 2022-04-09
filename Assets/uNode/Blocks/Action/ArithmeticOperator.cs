using UnityEngine;

namespace MaxyGames.Events {
	// [BlockMenu("★General", "ArithmeticOperator")]
	public class ArithmeticOperator : Action {
		public ArithmeticType operatorType = ArithmeticType.Add;
		[Filter(SetMember = true)]
		public MemberData storeValue;
		public MemberData targetA = MemberData.none;
		public MemberData targetB = MemberData.none;

		public override string Name {
			get {
				string str = "<b>" + uNode.uNodeUtility.GetNicelyDisplayName(targetA) + "</b> " +
					uNode.uNodeUtility.GetNicelyDisplayName(operatorType) + " <b>" +
					uNode.uNodeUtility.GetNicelyDisplayName(targetB) + "</b>";
				return "Set: <b>" + uNode.uNodeUtility.GetNicelyDisplayName(storeValue) + "</b> to " + str;
			}
		}

		protected override void OnExecute() {
			storeValue.Set(uNodeHelper.ArithmeticOperator(targetA.Get(), targetB.Get(), operatorType));
		}

		public override string GenerateCode(Object obj) {
			if(targetA.isAssigned && targetB.isAssigned) {
				if(storeValue.isAssigned) {
					return CG.Set(storeValue,
						CG.Arithmetic(
							CG.Value((object)targetA),
							CG.Value((object)targetB),
							operatorType));
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
						bool isDivide = operatorType == ArithmeticType.Divide;
						object obj = ReflectionUtils.CreateInstance(targetA.type);
						if(isDivide) {
							//For fix zero divide error.
							obj = Operator.IncrementPrimitive(obj);
						}
						object obj2 = ReflectionUtils.CreateInstance(targetB.type);
						if(isDivide) {
							//For fix zero divide error.
							obj2 = Operator.IncrementPrimitive(obj2);
						}
						uNodeHelper.ArithmeticOperator(obj, obj2, operatorType);
					}
				} catch(System.Exception ex) {
					uNode.uNodeUtility.RegisterEditorError(owner, ex.Message);
				}
			}
		}
	}
}