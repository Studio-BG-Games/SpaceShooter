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
	[NodeCustomEditor(typeof(Nodes.MakeArrayNode))]
	public class MakeArrayView : BaseNodeView {
		protected override void InitializeView() {
			base.InitializeView();
			Nodes.MakeArrayNode node = targetNode as Nodes.MakeArrayNode;
			if(node.elementType.isAssigned) {
				System.Type type = node.elementType.Get<System.Type>();
				FilterAttribute filter = new FilterAttribute(type);
				for(int i = 0; i < node.values.Count; i++) {
					int x = i;
					AddInputValuePort(
						new PortData() {
							portID = "Element#" + x,
							onValueChanged = (o) => {
								RegisterUndo();
								node.values[x] = o as MemberData;
							},
							getPortName = () => "Element " + x,
							getPortType = () => filter.GetActualType(),
							getPortValue = () => node.values[x],
							filter = filter,
						}
					);
				}
			}
			ControlView control = new ControlView();
			control.Add(new Button(() => {
				if(node.values.Count > 0) {
					RegisterUndo();
					node.values.RemoveAt(node.values.Count - 1);
					MarkRepaint();
				}
			}) { text = "-" });
			control.Add(new Button(() => {
				RegisterUndo();
				MemberData val = MemberData.CreateFromValue(null, node.elementType.Get<Type>());
				node.values.Add(val);
				MarkRepaint();
			}) { text = "+" });
			AddControl(Direction.Input, control);
		}
	}
}