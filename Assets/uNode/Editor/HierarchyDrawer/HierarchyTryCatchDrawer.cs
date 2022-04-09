using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors.Hierarchy {
	class HierarchyTryCatchDrawer : HierarchyDrawer {
		public override bool IsValid(Type type) {
			return type == typeof(NodeTry);
		}

		public override HierarchyNodeTree CreateNodeTree(NodeComponent nodeComponent) {
			var result = base.CreateNodeTree(nodeComponent);
			result.displayName = uNodeUtility.WrapTextWithKeywordColor("try");
			return result;
		}

		public override void AddChildNodes(NodeComponent nodeComponent, TreeViewItem parentTree, IList<TreeViewItem> rows) {
			var node = nodeComponent as NodeTry;
			manager.AddNodeTree(node.Try, parentTree, rows, true);
			for(int i = 0; i < node.Flows.Count; i++) {
				if(manager.CanAddTree(node.Flows[i])) {
					var caseTree = CreateFlowTree(node, nameof(node.ExceptionTypes) + i, node.ExceptionTypes[i], $"{uNodeUtility.WrapTextWithKeywordColor("catch")} {node.ExceptionTypes[i].GetNicelyDisplayName(richName: true, typeTargetWithTypeof:false)}:");
					manager.AddNodeTree(
						caseTree,
						parentTree,
						rows,
						false
					);
					manager.AddNodeTree(node.Flows[i], caseTree, rows, true);
				}
			}
			if(manager.CanAddTree(node.Finally)) {
				var flowTree = CreateFlowTree(node, nameof(node.Finally), node.Finally, uNodeUtility.WrapTextWithKeywordColor("finally"));
				manager.AddNodeTree(
					flowTree,
					parentTree,
					rows,
					false
				);
				manager.AddNodeTree(node.Finally, flowTree, rows, true);
			}
			manager.AddNodeTree(node.onFinished, parentTree, rows, false);
		}
	}
}