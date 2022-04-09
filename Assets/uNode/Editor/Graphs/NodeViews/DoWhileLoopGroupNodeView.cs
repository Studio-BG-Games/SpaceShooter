using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.DoWhileLoopGroup))]
	public class DoWhileLoopGroupNodeView : BlockNodeView {
		protected override void InitializeView() {
			base.InitializeView();
			Nodes.DoWhileLoopGroup node = targetNode as Nodes.DoWhileLoopGroup;
			titleContainer.RegisterCallback<MouseDownEvent>(e => {
				if(e.button == 0 && e.clickCount == 2) {
					owner.graph.editorData.selectedGroup = node as GroupNode;
					owner.graph.Refresh();
					owner.graph.UpdatePosition();
				}
			});
			InitializeBlocks(node.condition, BlockType.Condition);
		}
	}
}