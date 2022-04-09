using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.ExposedNode))]
	public class ExposedNodeView : BaseNodeView {
		protected override void InitializeView() {
			InitializeDefaultPort();
			var node = targetNode as Nodes.ExposedNode;
			var type = node.value.type;
			var valuePort = AddInputValuePort(nameof(node.value), type != null ? new FilterAttribute(type, typeof(object)) :  FilterAttribute.Default);
			valuePort.portData.onValueChanged += (value) => {
				var member = value as MemberData;
				if(member != null && member.type != type) {
					node.Refresh(true);
					MarkRepaint();
				}
			};
			if(type != null) {
				int extendedLength = node.OutputCount;
				if(extendedLength > 0) {
					for(int i = 0; i < extendedLength; i++) {
						int index = i;
						AddOutputValuePort(node, index,
							portTooltop: () => {
								var member = type.GetMemberCached(node.GetOutputName(index));
								if(member != null) {
									return XmlDoc.DocFromMember(member);
								}
								return "Missing member";
							}
						);
					}
				}
			}
		}
	}
}