using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Flow", "Throw")]
	[AddComponentMenu("")]
	public class NodeThrow : Node {
		[Hide, ValueIn, Filter(typeof(System.Exception))]
		public MemberData value;

		public override void OnExecute() {
			throw value.Get<System.Exception>();
		}

		public override string GetNodeName() {
			return "Throw";
		}

		public override string GetRichName() {
			return uNodeUtility.WrapTextWithKeywordColor("throw ") + value.GetNicelyDisplayName(richName: true);
		}

		public override string GenerateCode() {
			if(!value.isAssigned) throw new System.Exception("Unassigned value");
			return CG.Value(value).AddFirst("throw ").Add(";");
		}
	}
}