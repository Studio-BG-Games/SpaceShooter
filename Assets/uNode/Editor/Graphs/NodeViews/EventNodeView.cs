using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using NodeView = UnityEditor.Experimental.GraphView.Node;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(BaseEventNode))]
	public class BaseEventNodeView : BaseNodeView {
		protected override void InitializeView() {
			BaseEventNode node = targetNode as BaseEventNode;

			#region Title
			if(node is BaseGraphEvent) {
				title = node.GetNodeName();
			} else if(node is StateEventNode) {
				title = ObjectNames.NicifyVariableName((node as StateEventNode).eventType.ToString());
			}
			#endregion

			var flows = node.GetFlows();
			if(flows.Count == 0) {
				flows.Add(MemberData.none);
				MarkRepaint();
			}
			for(int x = 0; x < flows.Count; x++) {
				int index = x;
				AddOutputFlowPort(
					new PortData() {
						getPortValue = () => flows[index],
						onValueChanged = (val) => {
							flows[index] = val;
						},
					}
				);
			}
			InitializeOutputExtenderPorts();
			if(node is EventNode) {
				var eventNode = node as EventNode;
				for(int i = 0; i < eventNode.targetObjects.Length; i++) {
					int x = i;
					AddInputValuePort(PortData.CreateForInputValue(
						id: nameof(eventNode.targetObjects) + x,
						name: () => "Target " + x,
						type: () => typeof(GameObject),
						value: () => eventNode.targetObjects[x],
						onValueChange: (val) => eventNode.targetObjects[x] = val
					));
				}
			} else {
				InitializeFields();
			}
		}
	}
}