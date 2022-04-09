using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors.TreeViews {
	public abstract class TreeManager : VisualElement, ITreeManager {
		public List<TreeViewItem> selection { get; protected set; }
		public abstract SearchKind searchKind { get; }

		private VisualElement m_DragDisplay;
		protected TreeViewItem m_DropTree;
		protected bool m_DragReorderIsTop;
		protected bool m_DragStarted;

		public TreeManager() {
			selection = new List<TreeViewItem>();

			RegisterCallback<AttachToPanelEvent>(OnEnterPanel);
			RegisterCallback<DetachFromPanelEvent>(OnLeavePanel);

			RegisterCallback<MouseDownEvent>(OnMouseDown);
			RegisterCallback<MouseUpEvent>(OnMouseUp);

			m_DragDisplay = new VisualElement();
			m_DragDisplay.AddToClassList("dragdisplay");

			this.RegisterCallback(new DropableTargetEvent() {
				onDragLeave = OnDragLeaveEvent,
				onDragUpdate = OnDragUpdatedEvent,
				onDragPerform = OnDragPerformEvent,
				onDragExited = OnDragExitedEvent,
			});
			this.AddManipulator(new TreeClickSelector());
		}

		#region Drop
		protected virtual void OnDragLeaveEvent(DragLeaveEvent obj) {
			RemoveDragIndicator();
		}

		protected virtual void OnDragExitedEvent(DragExitedEvent obj) {
			RemoveDragIndicator();
			m_DragStarted = false;
		}

		protected virtual void OnDragPerformEvent(DragPerformEvent evt) {
			var tree = m_DropTree;
			RemoveDragIndicator();
			if(tree == null) return;
			object dragData = DragAndDrop.GetGenericData("uNode");
			if(dragData == null && DragAndDrop.objectReferences.Length > 0) {
				dragData = DragAndDrop.objectReferences[0];
			}
			AcceptDrop(tree, dragData);
			DragAndDrop.SetGenericData("uNode", null);
		}

		protected virtual void OnDragUpdatedEvent(DragUpdatedEvent evt) {
			VisualElement visualElement = evt.currentTarget as VisualElement;
			TreeViewItem tree = null;
			if(visualElement != null) {
				var trees = this.Query().Where(v => 
					v is TreeViewItem item && 
					item.HitTest(
						visualElement.ChangeCoordinatesTo(v, evt.localMousePosition))
					).ToList();
				tree = trees.LastOrDefault() as TreeViewItem;
			}
			object dragData = DragAndDrop.GetGenericData("uNode");
			if(dragData == null && DragAndDrop.objectReferences.Length > 0) {
				dragData = DragAndDrop.objectReferences[0];
			}
			if(tree == null || dragData == null) {
				RemoveDragIndicator();
				DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
				return;
			}
			var mPos = visualElement.ChangeCoordinatesTo(tree.titleContainer, evt.localMousePosition);
			bool isTop = mPos.y <= tree.titleContainer.layout.height / 2;
			if(!CanAcceptDrop(tree, dragData)) {
				RemoveDragIndicator();
				DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
				return;
			}
			SetDragIndicator(tree, isTop);
			if(!m_DragStarted) {
				// TODO: Do something on first DragUpdated event (initiate drag)
				m_DragStarted = true;
				AddToClassList("dropping");
			} else {
				// TODO: Do something on subsequent DragUpdated events
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
			}
		}

		protected virtual bool CanAcceptDrop(TreeViewItem tree, object dragData) {
			return false;
		}

		protected virtual void AcceptDrop(TreeViewItem tree, object dragData) {

		}

		protected void SetDragIndicator(TreeViewItem tree, bool isTop) {
			RemoveDragIndicator();
			this.Add(m_DragDisplay);
			var title = tree.titleContainer;
			if(isTop) {
				m_DragDisplay.style.top = title.ChangeCoordinatesTo(this, Vector2.zero).y;
			} else {
				m_DragDisplay.style.top = title.ChangeCoordinatesTo(this, new Vector2(0, title.layout.height)).y;
			}
			m_DropTree = tree;
			m_DragReorderIsTop = isTop;
		}

		protected void RemoveDragIndicator() {
			m_DragDisplay.RemoveFromHierarchy();
			m_DropTree = null;
		}
		#endregion

		#region Drag
		private Vector2 m_MouseDownStartPos;
		private TreeViewItem currentClickedTree;

		private void OnMouseMove(MouseMoveEvent evt) {
			if(evt.imguiEvent != null && evt.imguiEvent.type == EventType.MouseDrag && (m_MouseDownStartPos - evt.localMousePosition).sqrMagnitude > 9.0) {
				StartDrag();
			}
		}

		private void OnMouseUp(MouseUpEvent evt) {
			if(currentClickedTree != null) {
				currentClickedTree.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
			}
		}

		private void OnMouseDown(MouseDownEvent evt) {
			TreeViewItem tree = selection.FirstOrDefault();
			if(tree != null) {
				m_MouseDownStartPos = evt.localMousePosition;
				currentClickedTree = tree;

				tree.RegisterCallback<MouseMoveEvent>(OnMouseMove);
			}
		}

		private void StartDrag() {
			TreeViewItem tree = currentClickedTree;
			if(tree != null) {
				tree.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
				if(tree is IDragableTree) {
					DragAndDrop.activeControlID = tree.GetHashCode();
					DragAndDrop.PrepareStartDrag();
					DragAndDrop.SetGenericData("uNode", tree);
					DragAndDrop.StartDrag("Dragging Trees");
				}
			}
		}
		#endregion

		private void OnLeavePanel(DetachFromPanelEvent evt) {

		}

		private void OnEnterPanel(AttachToPanelEvent evt) {
			UIElementUtility.ForceDarkStyleSheet(this);
		}

		public void AddToSelection(TreeViewItem item, bool additive = false) {
			if(item == null)
				return;
			if(additive) {
				if(!selection.Contains(item)) {
					selection.Add(item);
					item.OnSelected();
				} else {
					selection.Remove(item);
					item.OnUnselected();
				}
			} else {
				ClearSelection();
				selection.Add(item);
				item.OnSelected();
			}
		}

		public void ClearSelection() {
			for(int i = 0; i < selection.Count; i++) {
				if(selection[i] != null) {
					selection[i].OnUnselected();
				}
				selection.RemoveAt(i);
				i--;
			}
		}

		public void RemoveFromSelection(TreeViewItem item) {
			if(item != null && selection.Contains(item)) {
				selection.Remove(item);
				item.OnUnselected();
			}
		}

		public abstract void Save();
	}
}