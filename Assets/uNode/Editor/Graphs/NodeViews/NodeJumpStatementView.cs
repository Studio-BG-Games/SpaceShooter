namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(NodeJumpStatement))]
	public class NodeJumpStatementView : BaseNodeView {
		protected override void InitializeView() {
			base.InitializeView();
			NodeJumpStatement node = targetNode as NodeJumpStatement;
			if(node.statementType == JumpStatementType.Break) {
				AddToClassList("break-node");
			} else if(node.statementType == JumpStatementType.Continue) {
				AddToClassList("continue-node");
			}
		}
	}
}