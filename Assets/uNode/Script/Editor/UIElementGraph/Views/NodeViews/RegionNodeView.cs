using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(NodeRegion))]
	public class RegionNodeView : BaseNodeView, IElementResizable {
		protected Label comment;
		protected List<NodeComponent> nodes;
		protected VisualElement horizontalDivider;

		public override void Initialize(UGraphView owner, NodeComponent node) {
			this.owner = owner;
			targetNode = node;
			title = targetNode.GetNodeName();
			titleButtonContainer.RemoveFromHierarchy();
			this.AddStyleSheet("uNodeStyles/NativeRegionStyle");
			var border = this.Q("node-border");
			border.style.overflow = Overflow.Visible;
			horizontalDivider = border.Q("contents").Q("divider");

			comment = new Label(node.comment);
			inputContainer.Add(comment);

			titleContainer.RegisterCallback<MouseDownEvent>((e) => {
				if(e.clickCount == 2 && e.button == 0) {
					ActionPopupWindow.ShowWindow(Vector2.zero, node.gameObject.name,
						(ref object obj) => {
							object str = EditorGUILayout.TextField(obj as string);
							if(obj != str) {
								obj = str;
								node.gameObject.name = obj as string;
								if(GUI.changed) {
									uNodeGUIUtility.GUIChanged(node);
								}
							}
						}).ChangePosition(owner.GetScreenMousePosition(e)).headerName = "Rename title";
					e.StopImmediatePropagation();
				}
			});
			RegisterCallback<MouseDownEvent>((e) => {
				if(e.button == 0) {
					nodes = new List<NodeComponent>(owner.graph.nodes);
					if(owner.graph.eventNodes != null) {
						foreach(var c in owner.graph.eventNodes) {
							if(c != null) {
								nodes.Add(c);
							}
						}
					}
					nodes.RemoveAll((n) => n == null || !targetNode.editorRect.Contains(new Vector2(n.editorRect.x + (n.editorRect.width * 0.5f), n.editorRect.y + (n.editorRect.height * 0.5f))));
				}
			});

			Add(new ResizableElement());
			this.SetSize(new Vector2(node.editorRect.width, node.editorRect.height));
			Teleport(targetNode.editorRect);
			ReloadView();
			RefreshPorts();
		}

		public void OnResized() {
			Teleport(layout);
		}

		public void OnStartResize() {

		}

		public override void ReloadView() {
			NodeRegion region = targetNode as NodeRegion;
			comment.text = targetNode.comment;
			elementTypeColor = region.nodeColor;
			mainContainer.style.SetBorderColor(new Color(region.nodeColor.r, region.nodeColor.g, region.nodeColor.b, 0.9f));
			horizontalDivider.style.backgroundColor = new Color(region.nodeColor.r, region.nodeColor.g, region.nodeColor.b, 0.9f);
			mainContainer.style.backgroundColor = new Color(region.nodeColor.r, region.nodeColor.g, region.nodeColor.b, 0.05f);
			titleContainer.style.backgroundColor = new Color(region.nodeColor.r, region.nodeColor.g, region.nodeColor.b, 0.3f);
			base.ReloadView();
		}

		public override void SetPosition(Rect newPos) {
			if(newPos != targetNode.editorRect) {
				// if(uNodePreference.GetPreference().snapNode) {
				// 	float range = uNodePreference.GetPreference().snapRange;
				// 	newPos.x = NodeEditorUtility.SnapTo(newPos.x, range);
				// 	newPos.y = NodeEditorUtility.SnapTo(newPos.y, range);
				// }
				if(nodes != null && newPos.width == targetNode.editorRect.width && newPos.height == targetNode.editorRect.height) {
					float xPos = newPos.x - targetNode.editorRect.x;
					float yPos = newPos.y - targetNode.editorRect.y;
					if(xPos != 0 || yPos != 0) {
						for(int n = 0; n < nodes.Count; n++) {
							UNodeView node;
							if(owner.nodeViewsPerNode.TryGetValue(nodes[n], out node)) {
								nodes[n].editorRect.x = nodes[n].editorRect.x + xPos;
								nodes[n].editorRect.y = nodes[n].editorRect.y + yPos;
								node.Teleport(nodes[n].editorRect);
							}
						}
						// if(uNodePreference.GetPreference().enableSnapping) {
						// 	float range = uNodePreference.GetPreference().snappingRange;
						// 	for(int n = 0; n < nodes.Count; n++) {
						// 		UNodeView node;
						// 		if(owner.nodeViewsPerNode.TryGetValue(nodes[n], out node)) {
						// 			nodes[n].editorRect.x = NodeEditorUtility.SnapTo(nodes[n].editorRect.x + xPos, range);
						// 			nodes[n].editorRect.y = NodeEditorUtility.SnapTo(nodes[n].editorRect.y + yPos, range);
						// 			node.SetPosition(nodes[n].editorRect);
						// 		}
						// 	}
						// } else {
						// }
					}
				}
			}
			base.SetPosition(newPos);
		}

		//public override bool HitTest(Vector2 localPoint) {
		//	return titleContainer.ContainsPoint(this.ChangeCoordinatesTo(titleContainer, localPoint));
		//}

		public override bool ContainsPoint(Vector2 localPoint) {
			return titleContainer.ContainsPoint(this.ChangeCoordinatesTo(titleContainer, localPoint));
		}

		public override bool Overlaps(Rect rectangle) {
			return titleContainer.Overlaps(this.ChangeCoordinatesTo(titleContainer, rectangle));
		}
	}
}