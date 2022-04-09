using System;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu("")]
	public class ArithmeticNode : ValueNode {
		public ArithmeticType operatorType = ArithmeticType.Add;
		[Hide, ValueIn]
		public MemberData targetA = MemberData.none;
		[Hide, ValueIn]
		public MemberData targetB = MemberData.none;

		public override System.Type ReturnType() {
			if(targetA.isAssigned && targetB.isAssigned) {
				try {
					if(operatorType == ArithmeticType.Divide) {
						return targetA.type;
					}
					object obj = uNodeHelper.ArithmeticOperator(
						ReflectionUtils.CreateInstance(targetA.type),
						ReflectionUtils.CreateInstance(targetB.type), operatorType);
					if(!object.ReferenceEquals(obj, null)) {
						return obj.GetType();
					}
				}
				catch { }
			}
			return typeof(object);
		}

		protected override object Value() {
			return uNodeHelper.ArithmeticOperator(targetA.Get(), targetB.Get(), operatorType);
		}

		public override string GenerateValueCode() {
			if(targetA.isAssigned && targetB.isAssigned) {
				return CG.Arithmetic(
					CG.Value(targetA),
					CG.Value(targetB), operatorType).AddFirst("(").Add(")");
			}
			throw new System.Exception("Target is unassigned");
		}

		public override string GetNodeName() {
			return operatorType.ToString();
		}

		public override Type GetNodeIcon() {
			switch(operatorType) {
				case ArithmeticType.Add:
					return typeof(TypeIcons.AddIcon2);
				case ArithmeticType.Divide:
					return typeof(TypeIcons.DivideIcon2);
				case ArithmeticType.Subtract:
					return typeof(TypeIcons.SubtractIcon2);
				case ArithmeticType.Multiply:
					return typeof(TypeIcons.MultiplyIcon2);
				case ArithmeticType.Modulo:
					return typeof(TypeIcons.ModuloIcon2);
			}
			return typeof(TypeIcons.CalculatorIcon);
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(targetA, this, "targetA");
			uNodeUtility.CheckError(targetB, this, "targetB");
			if(targetA.isAssigned && targetB.isAssigned) {
				try {
					if(targetA.type != null && targetB.type != null && operatorType != ArithmeticType.Divide) {
						uNodeHelper.ArithmeticOperator(
							ReflectionUtils.CreateInstance(targetA.type),
							ReflectionUtils.CreateInstance(targetB.type), operatorType);
					}
				}
				catch(System.Exception ex) {
					RegisterEditorError(ex.Message);
				}
			}
		}
	}
}