using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.MultiORNode))]
	public class OrNodeView : BaseNodeView {
		protected override void InitializeView() {
			InitializeDefaultPort();
			if(uNodeUtility.preferredDisplay != DisplayKind.Full) {
				ConstructCompactStyle();
			}
			var node = targetNode as Nodes.MultiORNode;
			for (int x = 0; x < node.targets.Count; x++) {
				int index = x;
				AddInputValuePort(
					new PortData() {
						portID = nameof(node.targets) + "#" + index,
						onValueChanged = (o) => {
							RegisterUndo();
							var val = o as MemberData;
							node.targets[index] = val;
							RefreshPortTypes();
						},
						getPortName = () => "",
						getPortValue = () => node.targets[index],
						getPortType = () => node.targets[index].type,
					}
				);
			}
		}
	}
}