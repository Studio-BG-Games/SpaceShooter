using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.NodeCoroutineAction))]
	public class CoroutineActionNodeView : BlockNodeView {
		protected override void InitializeView() {
			base.InitializeView();
			Nodes.NodeCoroutineAction node = targetNode as Nodes.NodeCoroutineAction;
			InitializeBlocks(node.action, BlockType.CoroutineAction);
		}
	}
}