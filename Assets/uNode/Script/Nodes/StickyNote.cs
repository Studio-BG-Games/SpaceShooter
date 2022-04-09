using System;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu("")]
	public class StickyNote : Node {
		public override void CheckError() {

		}

		public override bool IsFlowNode() {
			return false;
		}

		public override string GetNodeName() {
			return gameObject.name;
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.NoteIcon);
		}
	}
}
