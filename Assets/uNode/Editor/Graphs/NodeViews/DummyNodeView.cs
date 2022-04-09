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
	[NodeCustomEditor(typeof(Nodes.DummyNode))]
	public class DummyNodeView : BaseNodeView {
		protected override void InitializeView() {
			var node = targetNode as Nodes.DummyNode;
			int index = 0;
			for(int i=0;i< node.inputPorts.Count;i++ ) {
				var p = node.inputPorts[i];
				if(p.isFlow) {
					AddInputFlowPort(node, index++);
				} else {
					AddInputValuePort(
						new PortData() {
							portID = "in." + p.name,
							onValueChanged = (o) => {
								RegisterUndo();
								p.value = o;
							},
							getPortName = () => p.name,
							getPortValue = () => p.value,
						});
				}
			}
			index = 0;
			for(int i = 0; i < node.outputPorts.Count; i++) {
				var p = node.outputPorts[i];
				if(p.isFlow) {
					AddOutputFlowPort(
						new PortData() {
							portID = "out." + p.name,
							onValueChanged = (o) => {
								RegisterUndo();
								p.value = o;
							},
							getPortName = () => p.name,
							getPortType = () => typeof(FlowInput),
							getPortValue = () => p.value,
						}
					);
				} else {
					AddOutputValuePort(node, index++);
				}
			}
		}
	}
}