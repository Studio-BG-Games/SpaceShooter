using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors.Hierarchy {
	class HierarchyMacroDrawer : HierarchyDrawer {
		public override bool IsValid(Type type) {
			return type == typeof(NodeSwitch);
		}

		public override void AddChildNodes(NodeComponent nodeComponent, TreeViewItem parentTree, IList<TreeViewItem> rows) {
			var node = nodeComponent as NodeSwitch;
			for(int i = 0; i < node.values.Count; i++) {
				if(manager.CanAddTree(node.targetNodes[i])) {
					var caseTree = CreateFlowTree(node, nameof(node.targetNodes) + i, node.targetNodes[i], $"{uNodeUtility.WrapTextWithKeywordColor("case")} {node.values[i].GetNicelyDisplayName(richName: true)}:");
					manager.AddNodeTree(
						caseTree,
						parentTree,
						rows,
						true
					);
					manager.AddNodeTree(node.targetNodes[i], caseTree, rows, true);
				}
			}
			if(manager.CanAddTree(node.defaultTarget)) {
				var caseTree = CreateFlowTree(node, nameof(node.defaultTarget), node.defaultTarget, uNodeUtility.WrapTextWithKeywordColor("default"));
				manager.AddNodeTree(
					caseTree,
					parentTree,
					rows,
					true
				);
				manager.AddNodeTree(node.defaultTarget, caseTree, rows, true);
			}
			manager.AddNodeTree(node.onFinished, parentTree, rows, false);
		}
	}
}