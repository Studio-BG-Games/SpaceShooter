using UnityEngine;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Operator", "Bitwise {|} {&} {^}")]
	[AddComponentMenu("")]
	public class BitwiseNode : ValueNode {
		public BitwiseType operatorType = BitwiseType.Or;
		[Hide, ValueIn, Filter(typeof(int), typeof(uint), typeof(long), typeof(short), typeof(byte), typeof(ulong))]
		public MemberData targetA = MemberData.none;
		[Hide, ValueIn, Filter(typeof(int), typeof(uint), typeof(long), typeof(short), typeof(byte), typeof(ulong))]
		public MemberData targetB = MemberData.none;

		public override System.Type ReturnType() {
			if(targetA.isAssigned && targetB.isAssigned) {
				try {
					var val = ReflectionUtils.CreateInstance(targetA.type);
					object obj = uNodeHelper.BitwiseOperator(
						val,
						val, operatorType);
					if(!object.ReferenceEquals(obj, null)) {
						return obj.GetType();
					}
				}
				catch { }
			}
			return typeof(object);
		}

		protected override object Value() {
			return uNodeHelper.BitwiseOperator(targetA.Get(), targetB.Get(), operatorType);
		}

		public override string GenerateValueCode() {
			if(targetA.isAssigned && targetB.isAssigned) {
				return CG.Operator(CG.Value(targetA), 
					CG.Value(targetB), operatorType).AddFirst("(").Add(")");
			}
			throw new System.Exception();
		}

		public override string GetNodeName() {
			return operatorType.ToString();
		}

		public override string GetRichName() {
			return CG.Operator(targetA.GetNicelyDisplayName(richName:true), targetB.GetNicelyDisplayName(richName:true), operatorType);
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(targetA, this, "targetA");
			uNodeUtility.CheckError(targetB, this, "targetB");
			if(targetA.isAssigned && targetB.isAssigned) {
				try {
					if(targetA.type != null && targetA.type != typeof(object)) {
						var val = ReflectionUtils.CreateInstance(targetA.type);
						uNodeHelper.BitwiseOperator(
							val,
							val, 
							operatorType);
					}
				}
				catch(System.Exception ex) {
					RegisterEditorError(ex.Message);
				}
			}
		}
	}
}