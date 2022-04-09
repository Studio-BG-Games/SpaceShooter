using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.ASNode))]
	public class AsNodeView : BaseNodeView {
		protected override void InitializeView() {
			var node = targetNode as Nodes.ASNode;
			if(!node.compactDisplay) {
				base.InitializeView();
				return;
			}
			InitializeDefaultPort();
			ConstructCompactStyle(false);
			AddInputValuePort(nameof(node.target), () => typeof(object), "").AddToClassList("hide-image");
			if (UIElementUtility.Theme.coloredNodeBorder) {
				//Set border color
				Color c = uNodePreference.GetColorForType(nodeValuePort.GetPortType());
				c.a = 0.8f;
				border.style.SetBorderColor(c);
			}
		}
	}
}