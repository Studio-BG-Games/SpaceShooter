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
	[NodeCustomEditor(typeof(Nodes.NodeAnonymousFunction))]
	public class NodeAnonymousView : BaseNodeView {
		protected override void InitializeView() {
			base.InitializeView();
			Nodes.NodeAnonymousFunction node = targetNode as Nodes.NodeAnonymousFunction;
			while(node.parameterValues.Count != node.parameterTypes.Count) {
				if(node.parameterValues.Count > node.parameterTypes.Count) {
					node.parameterValues.RemoveAt(node.parameterValues.Count - 1);
				} else {
					node.parameterValues.Add(null);
				}
			}
			for(int i = 0; i < node.parameterTypes.Count; i++) {
				int x = i;
				var member = node.parameterTypes[i];
				Type type = typeof(object);
				if(member.isAssigned) {
					type = member.startType;
				}
				//var port = 
				AddOutputValuePort(
					new PortData() {
						portID = "parameterValues#" + x,
						getPortName = () => "",
						getPortType = () => type,
						getPortValue = () => node.parameterTypes[x],
						getConnection = () => {
							return MemberData.ValueOutput(node, "parameterValues", x, type);
						},
					}
				);
				ControlView controlType = new ControlView();
				controlType.style.flexDirection = FlexDirection.RowReverse;
				controlType.Add(new Label("P " + x));
				controlType.Add(new MemberControl(
						new ControlConfig() {
							owner = this,
							type = typeof(MemberData),
							filter = new FilterAttribute() { OnlyGetType= true},
							value = member,
							onValueChanged = (obj) => {
								node.parameterTypes[x] = obj as MemberData;
							},
						}
					));
				//port.SetControl(controlType);
				AddControl(Direction.Input, controlType);
			}
			ControlView control = new ControlView();
			control.style.alignSelf = Align.FlexEnd;
			control.Add(new Button(() => {
				if(node.parameterTypes.Count > 0) {
					RegisterUndo();
					node.parameterTypes.RemoveAt(node.parameterTypes.Count - 1);
					node.parameterValues.RemoveAt(node.parameterValues.Count - 1);
					MarkRepaint();
				}
			}) { text = "-" });
			control.Add(new Button(() => {
				RegisterUndo();
				node.parameterTypes.Add(new MemberData(typeof(object), MemberData.TargetType.Type));
				node.parameterValues.Add(null);
				MarkRepaint();
			}) { text = "+" });
			AddControl(Direction.Input, control);
		}
	}
}