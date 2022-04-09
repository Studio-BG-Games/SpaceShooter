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
	public class TransitionView : UNodeView {
		public TransitionEvent transition;

		public PortView input { private set; get; }
		public PortView output { private set; get; }

		public virtual void Initialize(UGraphView owner, TransitionEvent transition) {
			this.transition = transition;
			AddToClassList("transition");
			this.AddStyleSheet("uNodeStyles/NativeNodeStyle");
			this.AddStyleSheet(UIElementUtility.Theme.nodeStyle);
			Initialize(owner);
			ReloadView();

			border.style.overflow = Overflow.Visible;

			titleIcon.RemoveFromHierarchy();
			m_CollapseButton.RemoveFromHierarchy();

			RegisterCallback<MouseDownEvent>((e) => {
				if(e.button == 0 && e.clickCount == 2) {
					ActionPopupWindow.ShowWindow(owner.GetScreenMousePosition(e), transition.Name,
						(ref object obj) => {
							object str = EditorGUILayout.TextField(obj as string);
							if(obj != str) {
								obj = str;
								transition.Name = obj as string;
								if(GUI.changed) {
									uNodeGUIUtility.GUIChanged(transition);
								}
							}
						}).headerName = "Edit name";
					e.StopImmediatePropagation();
				}
			});
		}

		public override void SetPosition(Rect newPos) {
			// if(targetNode != null && newPos != targetNode.editorRect && uNodePreference.GetPreference().snapNode) {
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
			base.SetPosition(newPos);

			transition.editorPosition = new Rect(newPos.x - transition.node.editorRect.x, newPos.y - transition.node.editorRect.y, 0, 0);
		}

		public override void Teleport(Rect position) {
			base.SetPosition(position);
			transition.editorPosition = new Rect(position.x - transition.node.editorRect.x, position.y - transition.node.editorRect.y, 0, 0);
		}

		public void UpdatePosition() {
			Rect pos = transition.node.editorRect;
			pos.x += transition.editorPosition.x;
			pos.y += transition.editorPosition.y;
			Teleport(pos);
		}

		public override void ReloadView() {
			base.ReloadView();

			title = transition.Name;
			//titleIcon.image = uNodeEditorUtility.GetTypeIcon(transition.GetIcon());

			UpdatePosition();
			InitializeView();
			RefreshPorts();
		}

		public override void RegisterUndo(string name = "") {
			uNodeEditorUtility.RegisterUndo(transition, name);
		}

		protected virtual void InitializeView() {
			input = AddInputFlowPort(
				new PortData() {
					portID = UGraphView.SelfPortID,
					getPortName = () => "",
					userData = this,
				}
			);
			input.SetEnabled(false);
			output = AddOutputFlowPort(
					new PortData() {
						portID = "[Transition]",
						getPortValue = () => transition.target,
						onValueChanged = (val) => {
							if(val is MemberData) {
								transition.target = val as MemberData;
							}
						},
					}
				);
			output.DisplayProxyTitle(true);
		}
	}

	
	public class TransitionBlockView : TransitionView, INodeBlock, IDropTarget {
		public BlockNodeHandler handler;

		public BlockType blockType { get; protected set; }
		public EventData blocks { get; protected set; }
		public List<BlockView> blockViews => m_blockViews;
		public UNodeView nodeView => this;

		protected List<BlockView> m_blockViews = new List<BlockView>();

		public override void Initialize(UGraphView owner, TransitionEvent transition) {
			handler = new BlockNodeHandler(this);
			base.Initialize(owner, transition);
		}

		public void RemoveBlock(BlockView block) {
			handler.RemoveBlock(block);
		}

		protected void InitializeBlocks(EventData blocks, BlockType blockType) {
			if(blocks != null) {
				for(int i = 0; i < blocks.blocks.Count; i++) {
					BlockView block = new BlockView();
					block.Initialize(blocks.blocks[i], this);
					handler.blockElement.Add(block);
					blockViews.Add(block);
				}
				this.blocks = blocks;
				this.blockType = blockType;
				border.SetToNoClipping();
			}
		}

		public override void ReloadView() {
			for(int i = 0; i < blockViews.Count; i++) {
				blockViews[i].RemoveFromHierarchy();
			}
			blockViews.Clear();
			base.ReloadView();
			handler.ToggleBlockHint(blockViews.Count == 0);
		}

		public override void InitializeEdge() {
			base.InitializeEdge();
			for(int i = 0; i < blockViews.Count; i++) {
				blockViews[i].InitializeEdge();
			}
		}

		bool IDropTarget.CanAcceptDrop(List<ISelectable> selection) {
			return ((IDropTarget)handler).CanAcceptDrop(selection);
		}

		bool IDropTarget.DragEnter(DragEnterEvent evt, IEnumerable<ISelectable> selection, IDropTarget enteredTarget, ISelection dragSource) {
			return ((IDropTarget)handler).DragEnter(evt, selection, enteredTarget, dragSource);
		}

		bool IDropTarget.DragExited() {
			return ((IDropTarget)handler).DragExited();
		}

		bool IDropTarget.DragLeave(DragLeaveEvent evt, IEnumerable<ISelectable> selection, IDropTarget leftTarget, ISelection dragSource) {
			return ((IDropTarget)handler).DragLeave(evt, selection, leftTarget, dragSource);
		}

		bool IDropTarget.DragPerform(DragPerformEvent evt, IEnumerable<ISelectable> selection, IDropTarget dropTarget, ISelection dragSource) {
			return ((IDropTarget)handler).DragPerform(evt, selection, dropTarget, dragSource);
		}

		bool IDropTarget.DragUpdated(DragUpdatedEvent evt, IEnumerable<ISelectable> selection, IDropTarget dropTarget, ISelection dragSource) {
			return ((IDropTarget)handler).DragUpdated(evt, selection, dropTarget, dragSource);
		}
	}
}