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
	[NodeCustomEditor(typeof(Nodes.NodeSwitch))]
	public class NodeSwitchView : BaseNodeView {
		FilterAttribute filter = new FilterAttribute() {
			ValidTargetType = MemberData.TargetType.Values,
			InvalidTargetType=MemberData.TargetType.Null,
			UnityReference=false,
		};

		public override bool ShowExpandButton() {
			return true;
		}

		protected override void InitializeView() {
			InitializeDefaultPort();
			Nodes.NodeSwitch node = targetNode as Nodes.NodeSwitch;
			AddInputValuePort(nameof(node.target));
			var targetType = node.target.type;
			if(node.target != null && node.target.isAssigned && targetType != null) {
				while(node.values.Count != node.targetNodes.Count) {
					if(node.values.Count > node.targetNodes.Count) {
						node.values.RemoveAt(node.values.Count - 1);
					} else {
						node.values.Add(null);
					}
				}
				filter.SetType(targetType);
			}
			for(int i = 0; i < node.values.Count; i++) {
				int x = i;
				if(targetType != null) {
					var member = node.values[i];
					if(member.isTargeted && member.startType != targetType) {
						if(member.startType.IsCastableTo(targetType)) {
							member = MemberData.CreateFromValue(Operator.Convert(member.Get(), targetType), targetType);
						} else {
							member = MemberData.CreateFromValue(null, targetType);
						}
						node.values[i] = member;
					}
					ControlView controlType = new ControlView();
					if(graphLayout == GraphLayout.Vertical) {
						controlType.Add(new Label(x.ToString()));
					}
					controlType.Add(new MemberControl(
							new ControlConfig() {
								owner = this,
								type = typeof(MemberData),
								value = member,
								onValueChanged = (obj) => {
									node.values[x] = obj as MemberData;
								},
								filter = filter,
							},
							graphLayout == GraphLayout.Vertical
						));
					var port = AddOutputFlowPort(
						new PortData() {
							portID = "values#" + x,
							getPortName = () => x.ToString(),
							getPortValue = () => node.targetNodes[x],
							onValueChanged = (val) => {
								node.targetNodes[x] = val as MemberData;
							},
						}
					);
					if(graphLayout == GraphLayout.Vertical) {
						port.portData.getPortName = () => x.ToString();
						AddControl(Direction.Input, controlType);
						//port.SetControl(controlType);
					} else {
						port.SetControl(controlType);
					}
				}
			}
			if(targetType != null) {
				ControlView control = new ControlView();
				control.style.alignSelf = Align.Center;
				control.Add(new Button(() => {
					if(node.values.Count > 0) {
						RegisterUndo();
						node.values.RemoveAt(node.values.Count - 1);
						node.targetNodes.RemoveAt(node.targetNodes.Count - 1);
						MarkRepaint();
					}
				}) { text = "-" });
				control.Add(new Button(() => {
					RegisterUndo();
					node.values.Add(new MemberData(ReflectionUtils.CreateInstance(node.target.type)));
					node.targetNodes.Add(null);
					MarkRepaint();
				}) { text = "+" });
				AddControl(Direction.Input, control);
			}

			{//Default
				var member = node.defaultTarget;
				Type type = typeof(object);
				if(member.isAssigned) {
					type = member.startType;
				}
				AddOutputFlowPort(
					new PortData() {
						portID = "default",
						getPortName = () => "Default",
						getPortValue = () => node.defaultTarget,
						onValueChanged = (val) => {
							node.defaultTarget = val as MemberData;
						},
					}
				);
			}
			AddOutputFlowPort("onFinished", "Next");
		}

		public override void OnValueChanged() {
			//Ensure to repaint every value changed.
			MarkRepaint();
		}
	}
}