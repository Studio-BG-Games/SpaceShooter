using System;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Operator", "Coalescing {??}")]
	[AddComponentMenu("")]
	public class CoalescingNode : ValueNode {
		[Hide, ValueIn("Input")]
		public MemberData targetA = MemberData.none;
		[Hide, ValueIn("Fallback"), ObjectType("targetA")]
		public MemberData targetB = MemberData.none;

		public override System.Type ReturnType() {
			if(targetA.isAssigned || targetB.isAssigned) {
				try {
					if(targetA.isAssigned) {
						return targetA.type;
					} else {
						return targetB.type;
					}
				}
				catch { }
			}
			return typeof(object);
		}

		protected override object Value() {
			return targetA.Get() ?? targetB.Get();
		}

		public override string GenerateValueCode() {
			return CG.Value(targetA) + " ?? " + CG.Value(targetB);
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.NullTypeIcon);
		}

		public override string GetNodeName() {
			return "Null Coalesce";
		}

		public override string GetRichName() {
			return targetA.GetNicelyDisplayName(richName:true) + " ?? " + targetB.GetNicelyDisplayName(richName:true);
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(targetA, this, "targetA");
			uNodeUtility.CheckError(targetB, this, "targetB");
		}
	}
}