using System;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	// [NodeMenu("Operator", "Comparison {==} {>=} {<=} {>} {<} {!=}", typeof(bool))]
	[AddComponentMenu("")]
	public class ComparisonNode : ValueNode {
		public ComparisonType operatorType = ComparisonType.Equal;
		[Hide, ValueIn]
		public MemberData targetA = new MemberData(0f);
		[Hide, ValueIn, ObjectType("targetA")]
		public MemberData targetB = new MemberData(0f);

		public override System.Type ReturnType() {
			return typeof(bool);
		}

		protected override object Value() {
			return uNodeHelper.OperatorComparison(targetA.Get(), targetB.Get(), operatorType);
		}

		public override string GenerateValueCode() {
			if(targetA.isAssigned && targetB.isAssigned) {
				return CG.Compare(
					CG.Value(targetA),
					CG.Value(targetB),
					operatorType).AddFirst("(").Add(")");
			}
			throw new Exception("The target is unassigned");
		}

		public override string GetNodeName() {
			return operatorType.ToString();
		}

		public override string GetRichName() {
			string separator = null;
			switch(operatorType) {
				case ComparisonType.Equal:
					separator = " == ";
					break;
				case ComparisonType.GreaterThan:
					separator = " > ";
					break;
				case ComparisonType.GreaterThanOrEqual:
					separator = " >= ";
					break;
				case ComparisonType.LessThan:
					separator = " < ";
					break;
				case ComparisonType.LessThanOrEqual:
					separator = " <= ";
					break;
				case ComparisonType.NotEqual:
					separator = " != ";
					break;
			}
			return targetA.GetNicelyDisplayName(richName:true) + separator + targetB.GetNicelyDisplayName(richName:true);
		}

		public override Type GetNodeIcon() {
			switch(operatorType) {
				case ComparisonType.Equal:
					return typeof(TypeIcons.Equal);
				case ComparisonType.NotEqual:
					return typeof(TypeIcons.NotEqual);
				case ComparisonType.LessThan:
					return typeof(TypeIcons.LessThan);
				case ComparisonType.LessThanOrEqual:
					return typeof(TypeIcons.LessThanOrEqual);
				case ComparisonType.GreaterThan:
					return typeof(TypeIcons.GreaterThan);
				case ComparisonType.GreaterThanOrEqual:
					return typeof(TypeIcons.GreaterThanOrEqual);
			}
			return typeof(TypeIcons.CompareIcon);
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(targetA, this, "targetA");
			uNodeUtility.CheckError(targetB, this, "targetB");
			if(targetA.isAssigned && targetB.isAssigned) {
				try {
					if(targetA.targetType != MemberData.TargetType.Null && targetA.type != null && targetB.targetType != MemberData.TargetType.Null && targetB.type != null) {
						uNodeHelper.OperatorComparison(
							ReflectionUtils.CreateInstance(targetA.type),
							ReflectionUtils.CreateInstance(targetB.type), operatorType);
					}
				}
				catch(System.Exception ex) {
					if(ex is NullReferenceException)
						return;
					RegisterEditorError(ex.Message);
				}
			}
		}
	}
}