using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors.Hierarchy {
	class HierarchyIfDrawer : HierarchyDrawer {
		public override bool IsValid(Type type) {
			return type == typeof(NodeIf);
		}

		public override void AddChildNodes(NodeComponent nodeComponent, TreeViewItem parentTree, IList<TreeViewItem> rows) {
			var node = nodeComponent as NodeIf;
			manager.AddNodeTree(node.onTrue, parentTree, rows, true);
			if(manager.CanAddTree(node.onFalse)) {
				var flowTree = CreateFlowTree(node, nameof(node.onFalse), node.onFalse, uNodeUtility.WrapTextWithKeywordColor("else"));
				manager.AddNodeTree(
					flowTree,
					parentTree,
					rows,
					false
				);
				manager.AddNodeTree(node.onFalse, flowTree, rows, true);
			}
			manager.AddNodeTree(node.onFinished, parentTree, rows, false);
		}
	}
}