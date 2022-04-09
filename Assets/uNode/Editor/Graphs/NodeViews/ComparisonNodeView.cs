using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
    [NodeCustomEditor(typeof(Nodes.ComparisonNode))]
	public class ComparisonNodeView : BaseNodeView {
		protected override void InitializeView() {
			var node = targetNode as Nodes.ComparisonNode;
			InitializeDefaultPort();
			AddInputValuePort(nameof(node.targetA), () => node.targetA.type ?? typeof(object), new FilterAttribute(typeof(object)), "");
			AddInputValuePort(nameof(node.targetB), () => node.targetA.type ?? typeof(object), "");
			if(uNodeUtility.preferredDisplay != DisplayKind.Full) {
				ConstructCompactStyle();
			}
		}
	}
}