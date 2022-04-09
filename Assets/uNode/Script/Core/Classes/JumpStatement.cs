namespace MaxyGames.uNode {
	public sealed class JumpStatement {
		public readonly Node from;
		public readonly JumpStatementType jumpType;

		public JumpStatement(JumpStatementType jumpType, Node from = null) {
			this.jumpType = jumpType;
			this.from = from;
		}

		public JumpStatement(Node from, JumpStatementType jumpType) {
			this.jumpType = jumpType;
			this.from = from;
		}
	}
}