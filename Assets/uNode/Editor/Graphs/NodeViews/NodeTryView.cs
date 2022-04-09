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
	[NodeCustomEditor(typeof(Nodes.NodeTry))]
	public class NodeTryView : BaseNodeView {
		FilterAttribute filter = new FilterAttribute(typeof(Exception)) {
			OnlyGetType = true,
			ArrayManipulator = false
		};

		protected override void InitializeView() {
			base.InitializeView();
			Nodes.NodeTry node = targetNode as Nodes.NodeTry;
			while(node.Exceptions.Count != node.ExceptionTypes.Count) {
				if(node.Exceptions.Count > node.ExceptionTypes.Count) {
					node.Exceptions.RemoveAt(node.Exceptions.Count - 1);
				} else {
					node.Exceptions.Add(null);
				}
			}
			while(node.Flows.Count != node.ExceptionTypes.Count) {
				if(node.Flows.Count > node.ExceptionTypes.Count) {
					node.Flows.RemoveAt(node.Flows.Count - 1);
				} else {
					node.Flows.Add(null);
				}
			}
			for(int i = 0; i < node.ExceptionTypes.Count; i++) {
				int x = i;
				var member = node.ExceptionTypes[i];
				Type type = typeof(Exception);
				if(member.isAssigned) {
					type = member.Get<Type>();
				}
				AddOutputValuePort(
					new PortData() {
						getPortName = () => "Ex " + x.ToString(),
						getPortType = () => type,
						getConnection = () => {
							return MemberData.ValueOutput(node, "Exceptions", x, type);
						},
					}
				);
				ControlView controlType = new ControlView();
				//controlType.Add(new Label(x.ToString()));
				controlType.Add(new MemberControl(
						new ControlConfig() {
							owner = this,
							type = typeof(MemberData),
							value = member,
							onValueChanged = (obj) => {
								node.ExceptionTypes[x] = obj as MemberData;
							},
							filter = filter,
						},
						true
					));
				AddControl(Direction.Input, controlType);
				AddOutputFlowPort(
					new PortData() {
						getPortName = () => x.ToString(),
						getPortValue = () => node.Flows[x],
						onValueChanged = (val) => {
							node.Flows[x] = val as MemberData;
						},
					}
				);
			}
			ControlView control = new ControlView();
			control.style.alignSelf = Align.Center;
			control.Add(new Button(() => {
				if(node.ExceptionTypes.Count > 0) {
					RegisterUndo();
					node.ExceptionTypes.RemoveAt(node.ExceptionTypes.Count - 1);
					node.Exceptions.RemoveAt(node.Exceptions.Count - 1);
					node.Flows.RemoveAt(node.Flows.Count - 1);
					MarkRepaint();
				}
			}) { text = "-" });
			control.Add(new Button(() => {
				RegisterUndo();
				node.ExceptionTypes.Add(MemberData.none);
				node.Exceptions.Add(null);
				node.Flows.Add(MemberData.none);
				MarkRepaint();
			}) { text = "+" });
			AddControl(Direction.Input, control);

			AddOutputFlowPort("Finally");
			AddOutputFlowPort("onFinished", "Next");
		}

		public override void OnValueChanged() {
			//Ensure to repaint every value changed.
			MarkRepaint();
		}
	}
}