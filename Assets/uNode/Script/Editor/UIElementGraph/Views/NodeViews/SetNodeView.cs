using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(NodeSetValue))]
	public class SetNodeView : BaseNodeView {
		protected override void InitializeView() {
			var node = targetNode as NodeSetValue;
			InitializeDefaultPort();
			AddInputValuePort(nameof(node.target), new FilterAttribute(typeof(object)) { SetMember = true }, "");
			AddInputValuePort(nameof(node.value), () => {
				var type = node.target?.type;
				if(type == null)
					type = typeof(object);
				return type;
			}, "");
			AddOutputFlowPort(nameof(node.onFinished), "");
			if(uNodeUtility.preferredDisplay != DisplayKind.Full) {
				ConstructCompactStyle();
			}
		}
	}
}
