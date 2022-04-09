using UnityEngine;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Data", "Not {!}", typeof(bool))]
	[AddComponentMenu("")]
	public class NotNode : ValueNode {
		[Hide, ValueIn, Filter(typeof(bool))]
		public MemberData target = new MemberData();

		public override System.Type ReturnType() {
			return typeof(bool);
		}

		protected override object Value() {
			return !target.Get<bool>();
		}

		public override string GenerateValueCode() {
			if(target.isAssigned) {
				return "!(" + CG.Value((object)target) + ")";
			}
			throw new System.Exception();
		}

		public override string GetNodeName() {
			return "Not";
		}

		public override string GetRichName() {
			return $"!({target.GetNicelyDisplayName(richName:true)}";
		}

		public override System.Type GetNodeIcon() {
			return typeof(TypeIcons.NotIcon);
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target");
		}
	}
}