using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.NodeReroute))]
	public class NodeRerouteView : BaseNodeView {
		protected override void InitializeView() {
			var node = targetNode as Nodes.NodeReroute;
			InitializeDefaultPort();
			if(node.IsFlowNode()) {
				title = "";
				AddOutputFlowPort(nameof(node.onFinished), "");
			} else {
				ConstructCompactStyle(false);
				AddInputValuePort(nameof(node.target), () => node.ReturnType(), "").AddToClassList("hide-image");
				if(UIElementUtility.Theme.coloredNodeBorder) {
					//Set border color
					Color c = uNodePreference.GetColorForType(nodeValuePort.GetPortType());
					c.a = 0.8f;
					border.style.SetBorderColor(c);
				}
			}
		}
	}
}