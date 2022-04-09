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
	[NodeCustomEditor(typeof(Nodes.SelectNode))]
	public class SelectNodeView : BaseNodeView {
		protected override void InitializeView() {
			base.InitializeView();
			Nodes.SelectNode node = targetNode as Nodes.SelectNode;
			if(!node.target.isAssigned || node.target.type == null)
				return;
			AddInputValuePort(
				new PortData() {
					getPortName = () => "default",
					getPortType = () => node.targetType.Get<Type>(),
					getPortValue = () => node.defaultTarget,
					onValueChanged = (o) => {
						RegisterUndo();
						node.defaultTarget = o as MemberData;
					},
				});
			FilterAttribute valueFilter = new FilterAttribute(node.target.type) {
				ValidTargetType = MemberData.TargetType.Values
			};
			FilterAttribute filter = new FilterAttribute(node.ReturnType());
			for(int i = 0; i < node.values.Count; i++) {
				int x = i;
				var member = node.values[i];
				var port = AddInputValuePort(
					new PortData() {
						getPortValue = () => node.targetNodes[x],
						onValueChanged = (o) => {
							RegisterUndo();
							node.targetNodes[x] = o as MemberData;
						},
						filter = filter,
				});
				ControlView controlType = new ControlView();
				controlType.Add(new MemberControl(
					new ControlConfig() {
						owner = this,
						value = member,
						onValueChanged = (obj) => {
							node.values[x] = obj as MemberData;
						},
						filter = valueFilter,
					},
					true
				));
				port.SetControl(controlType, true);
			}
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
				node.targetNodes.Add(MemberData.none);
				MarkRepaint();
			}) { text = "+" });
			AddControl(Direction.Input, control);
		}
	}
}