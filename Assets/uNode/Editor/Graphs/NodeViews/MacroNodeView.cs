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

	[NodeCustomEditor(typeof(Nodes.MacroNode))]
	public class MacroNodeView : BaseNodeView {
		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
			evt.menu.AppendAction("Open Macro", (e) => {
				owner.graph.editorData.selectedGroup = targetNode as Node;
				owner.graph.Refresh();
				owner.graph.UpdatePosition();
			}, DropdownMenuAction.AlwaysEnabled);
			var mPos = evt.mousePosition;
			evt.menu.AppendAction("Rename Macro", (e) => {
				ActionPopupWindow.ShowWindow(Vector2.zero, targetNode.gameObject.name,
					(ref object obj) => {
						object str = EditorGUILayout.TextField(obj as string);
						if(obj != str) {
							obj = str;
							targetNode.gameObject.name = obj as string;
							if(GUI.changed) {
								uNodeGUIUtility.GUIChanged(targetNode);
							}
						}
					}).ChangePosition(owner.GetScreenMousePosition(mPos)).headerName = "Rename title";
			}, DropdownMenuAction.AlwaysEnabled);
			base.BuildContextualMenu(evt);
		}

		protected override void InitializeView() {
			base.InitializeView();
			Nodes.MacroNode node = targetNode as Nodes.MacroNode;
			node.Refresh();
			titleContainer.RegisterCallback<MouseDownEvent>(e => {
				if(e.button == 0 && e.clickCount == 2) {
					owner.graph.editorData.selectedGroup = node;
					owner.graph.Refresh();
					owner.graph.UpdatePosition();
				}
			});
			foreach(var m in node.inputFlows) {
				var port = AddInputFlowPort(
					new PortData() {
						portID = m.GetInstanceID().ToString(),
						getPortName = () => m.gameObject.name,
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
						getPortName = () => m.gameObject.name,
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
						getPortName = () => m.gameObject.name,
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
						getPortName = () => m.gameObject.name,
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

	[NodeCustomEditor(typeof(Nodes.MacroPortNode))]
	public class MacroPinNodeView : BaseNodeView {
		protected override void InitializeView() {
			base.InitializeView();
			Nodes.MacroPortNode node = targetNode as Nodes.MacroPortNode;
			if(node.kind == PortKind.FlowInput) {
				AddOutputFlowPort(
					new PortData() {
						portID = "flow",
						getPortName = () => "",
						getPortType = () => typeof(MemberData),
						getPortValue = () => node.target,
						onValueChanged = (val) => {
							node.target = val as MemberData;
						},
					}
				);
			} else if(node.kind == PortKind.ValueOutput) {
				AddInputValuePort(
					new PortData() {
						portID = "value",
						getPortName = () => "",
						getPortType = () => node.ReturnType(),
						getPortValue = () => node.target,
						onValueChanged = (val) => {
							node.target = val as MemberData;
						},
					}
				);
			}
		}
	}
}