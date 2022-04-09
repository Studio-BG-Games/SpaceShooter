using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.NodeWaitUntil))]
	public class NodeWaitUntilNodeView : BlockNodeView {
		protected override void InitializeView() {
			base.InitializeView();
			Nodes.NodeWaitUntil node = targetNode as Nodes.NodeWaitUntil;
			InitializeBlocks(node.Condition, BlockType.Condition);
		}
	}
}