using System;
using UnityEngine;

namespace MaxyGames.uNode {
	[AddComponentMenu("")]
	public class NodeJumpStatement : Node {
		[Hide]
		public JumpStatementType statementType;

		public override void OnExecute() {
			jumpState = new JumpStatement(statementType, this);
			Finish();
		}

		public override string GenerateCode() {
			switch(statementType) {
				case JumpStatementType.Break:
					return "\nbreak;";
				case JumpStatementType.Continue:
					return "\ncontinue;";
				default:
					throw new System.Exception("Statement " + statementType.ToString() + " is not supported");
			}
		}

		public override string GetNodeName() {
			return statementType.ToString();
		}

		public override Type GetNodeIcon() {
			return null;
		}
	}
}
