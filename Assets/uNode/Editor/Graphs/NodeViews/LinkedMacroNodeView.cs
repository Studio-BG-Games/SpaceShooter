using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.LinkedMacroNode))]
	public class LinkedMacroNodeView : BaseNodeView {
		protected override void InitializeView() {
			base.InitializeView();
			Nodes.LinkedMacroNode node = targetNode as Nodes.LinkedMacroNode;
			node.Refresh();
			titleContainer.RegisterCallback<MouseDownEvent>(e => {
				if(e.button == 0 && e.clickCount == 2) {
					if(node.runtimeMacro && Application.isPlaying) {
						uNodeEditor.Open(node.runtimeMacro);
					} else if(node.macroAsset) {
						uNodeEditor.Open(node.macroAsset);
					}
				}
			});
			foreach(var m in node.inputFlows) {
				var port = AddInputFlowPort(
					new PortData() {
						portID = m.GetInstanceID().ToString(),
						getPortName = () => m.GetName(),
						getConnection = () => {
							return new MemberData(m, MemberData.TargetType.FlowNode);
						},
					}
				);
				owner.RegisterFlowPortAliases(port, m);
			}
			foreach(var m in node.outputFlows) {
				AddOutputFlowPort(
					new PortData() {
						portID = m.GetInstanceID().ToString(),
						getPortName = () => m.GetName(),
						getPortValue = () => m.target,
						onValueChanged = (val) => {
							m.target = val as MemberData;
						},
					}
				);
			}
			foreach(var m in node.inputValues) {
				AddInputValuePort(
					new PortData() {
						portID = m.GetInstanceID().ToString(),
						getPortName = () => m.GetName(),
						getPortType = () => m.ReturnType(),
						getPortValue = () => m.target,
						onValueChanged = (val) => {
							m.target = val as MemberData;
						},
					}
				);
			}
			foreach(var m in node.outputValues) {
				var port = AddOutputValuePort(
					new PortData() {
						portID = m.GetInstanceID().ToString(),
						getPortName = () => m.GetName(),
						getPortType = () => m.ReturnType(),
						getConnection = () => {
							return new MemberData(m, MemberData.TargetType.ValueNode);
						},
					}
				);
				owner.RegisterValuePortAliases(port, m);
			}
		}
	}
}