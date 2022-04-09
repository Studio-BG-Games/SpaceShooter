using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.CacheNode))]
	public class CacheNodeView : BaseNodeView {
		protected override void InitializeView() {
			var node = targetNode as Nodes.CacheNode;
			InitializeDefaultPort();
			ConstructCompactStyle(true);
			AddInputValuePort(nameof(node.target), () => typeof(object), "").AddToClassList("hide-image");
			AddOutputFlowPort(nameof(node.onFinished), "");
			if (UIElementUtility.Theme.coloredNodeBorder) {
				//Set border color
				Color c = uNodePreference.GetColorForType(nodeValuePort.GetPortType());
				c.a = 0.8f;
				border.style.SetBorderColor(c);
			}
		}
	}
}