using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Yield", "Yield Break", IsCoroutine = true, HideOnStateMachine =true)]
	[AddComponentMenu("")]
	public class NodeYieldBreak : Node {
		public override void OnExecute() {
			jumpState = new JumpStatement(JumpStatementType.Return, this);
			Finish();
		}

		public override string GenerateCode() {
			return CG.YieldBreak();
		}

		public override string GetNodeName() {
			return "YieldBreak";
		}
	}
}