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
	[NodeCustomEditor(typeof(StateNode))]
	public class StateNodeView : BaseNodeView {
		//To ensure the node always reload when the graph changed
		public override bool autoReload => true;

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
			evt.menu.AppendAction("Open State", (e) => {
				owner.graph.editorData.selectedGroup = targetNode as Node;
				owner.graph.Refresh();
				owner.graph.UpdatePosition();
			}, DropdownMenuAction.AlwaysEnabled);
			base.BuildContextualMenu(evt);
		}

		protected override void InitializeView() {
			base.InitializeView();
			titleContainer.RegisterCallback<MouseDownEvent>(e => {
				if(e.button == 0 && e.clickCount == 2) {
					owner.graph.editorData.selectedGroup = targetNode as Node;
					owner.graph.Refresh();
					owner.graph.UpdatePosition();
				}
			});
			BuildTransitions();
		}

		protected void BuildTransitions() {
			StateNode node = targetNode as StateNode;
			TransitionEvent[] transitions = node.GetTransitions();

			for(int x = 0; x < transitions.Length; x++) {
				var tr = transitions[x];
				var transition = owner.AddTransition(tr);
				var port = AddOutputFlowPort(
					new PortData() {
						portID = "[Transition]" + x,
						userData = transition,
					}
				);
				port.SetEnabled(false);
			}
		}

		public override void InitializeEdge() {
			foreach(var port in outputPorts) {
				if(port.isFlow && !port.enabledSelf) {
					TransitionView transition = port.portData.userData as TransitionView;
					if(transition != null) {
						EdgeView edge = new EdgeView(transition.input, port);
						owner.Connect(edge, false);
						edge.SetEnabled(false);
					}
				}
			}
		}

		public override void SetPosition(Rect newPos) {
			// if(newPos != targetNode.editorRect && uNodePreference.GetPreference().snapNode) {
			// 	var preference = uNodePreference.GetPreference();
			// 	float range = preference.snapRange;
			// 	newPos.x = NodeEditorUtility.SnapTo(newPos.x, range);
			// 	newPos.y = NodeEditorUtility.SnapTo(newPos.y, range);
			// 	if(preference.snapToPin && owner.selection.Count == 1) {
			// 		var connectedPort = inputPorts.Where((p) => p.connected).ToList();
			// 		for(int i = 0; i < connectedPort.Count; i++) {
			// 			if(connectedPort[i].orientation != Orientation.Vertical) continue;
			// 			var edges = connectedPort[i].GetEdges();
			// 			foreach(var e in edges) {
			// 				if(e != null) {
			// 					float distanceToPort = e.edgeControl.to.x - e.edgeControl.from.x;
			// 					if(Mathf.Abs(distanceToPort) <= preference.snapToPinRange && Mathf.Abs(newPos.x - layout.x) <= preference.snapToPinRange) {
			// 						newPos.x = layout.x - distanceToPort;
			// 						break;
			// 					}
			// 				}
			// 			}
			// 		}
			// 	}
			// }
			float xPos = newPos.x - targetNode.editorRect.x;
			float yPos = newPos.y - targetNode.editorRect.y;

			Teleport(newPos);

			if(xPos != 0 || yPos != 0) {
				foreach(var port in outputPorts) {
					if(port.isFlow && !port.enabledSelf) {
						TransitionView transition = port.userData as TransitionView;
						if(transition != null) {
							Rect rect = transition.transition.editorPosition;
							rect.x += targetNode.editorRect.x;
							rect.y += targetNode.editorRect.y;
							transition.Teleport(rect);
						}
					}
				}
			}
		}
	}
}