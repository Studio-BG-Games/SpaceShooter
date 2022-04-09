using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu("")]
	public class ANDNode : ValueNode {
		[Hide, ValueIn, Filter(typeof(bool))]
		public MemberData targetA;
		[Hide, ValueIn, Filter(typeof(bool))]
		public MemberData targetB;

		public override System.Type ReturnType() {
			return typeof(bool);
		}

		protected override object Value() {
			return targetA.Get<bool>() && targetB.Get<bool>();
		}

		public override string GenerateValueCode() {
			if(targetA.isAssigned && targetB.isAssigned) {
				return "(" + CG.Value((object)targetA) + " && " + CG.Value((object)targetB) + ")";
            }
            throw new System.Exception("Target is unassigned");
		}

		public override string GetNodeName() {
			return "AND";
		}

		public override System.Type GetNodeIcon() {
			return typeof(TypeIcons.AndIcon2);
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(targetA, this, "targetA");
			uNodeUtility.CheckError(targetB, this, "targetB");
		}
	}
}