using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.ConditionNode))]
	public class ConditionNodeView : BlockNodeView {
		protected override void InitializeView() {
			base.InitializeView();
			Nodes.ConditionNode node = targetNode as Nodes.ConditionNode;
			InitializeBlocks(node.Condition, BlockType.Condition);
			EnableInClassList("compact", true);
			if(nodeValuePort != null) {
				nodeValuePort.SetName("");
				if (UIElementUtility.Theme.coloredNodeBorder) {
					Color c = uNodePreference.GetColorForType(nodeValuePort.GetPortType());
					c.a = 0.8f;
					border.style.SetBorderColor(c);
				}
			}
		}

		protected override void OnCustomStyleResolved(ICustomStyle style) {
			base.OnCustomStyleResolved(style);
			if (UIElementUtility.Theme.coloredNodeBorder) {
				Color c = uNodePreference.GetColorForType(nodeValuePort.GetPortType());
				c.a = 0.8f;
				border.style.SetBorderColor(c);
			}
		}
	}
}