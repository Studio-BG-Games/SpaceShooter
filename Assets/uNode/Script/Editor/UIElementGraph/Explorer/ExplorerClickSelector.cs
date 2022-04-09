using UnityEngine;
using UnityEngine.UIElements;

namespace MaxyGames.uNode.Editors.TreeViews {
	public class TreeClickSelector : MouseManipulator {
		public TreeClickSelector() {
			base.activators.Add(new ManipulatorActivationFilter {
				button = MouseButton.LeftMouse
			});
			base.activators.Add(new ManipulatorActivationFilter {
				button = MouseButton.RightMouse
			});
			if(Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) {
				base.activators.Add(new ManipulatorActivationFilter {
					button = MouseButton.LeftMouse,
					modifiers = EventModifiers.Command
				});
			} else {
				base.activators.Add(new ManipulatorActivationFilter {
					button = MouseButton.LeftMouse,
					modifiers = EventModifiers.Control
				});
			}
		}

		private static bool WasSelectableDescendantHitByMouse(TreeViewItem currentTarget, MouseDownEvent evt) {
			VisualElement visualElement = evt.target as VisualElement;
			if(visualElement == null || currentTarget == visualElement) {
				return false;
			}
			VisualElement visualElement2 = visualElement;
			while(visualElement2 != null && currentTarget != visualElement2) {
				var treeElement = visualElement2 as TreeViewItem;
				if(treeElement != null && treeElement.enabledInHierarchy && treeElement.pickingMode != PickingMode.Ignore && treeElement.IsSelectable()) {
					Vector2 localPoint = currentTarget.ChangeCoordinatesTo(visualElement2, evt.localMousePosition);
					if(treeElement.HitTest(localPoint)) {
						return true;
					}
				}
				visualElement2 = visualElement2.parent;
			}
			return false;
		}

		protected override void RegisterCallbacksOnTarget() {
			base.target.RegisterCallback<MouseDownEvent>(OnMouseDown);
		}

		protected override void UnregisterCallbacksFromTarget() {
			base.target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
		}

		protected void OnMouseDown(MouseDownEvent e) {
			TreeViewItem treeElement = e.currentTarget as TreeViewItem;
			if(treeElement != null && CanStartManipulation(e) && treeElement.IsSelectable() && treeElement.HitTest(e.localMousePosition) && !WasSelectableDescendantHitByMouse(treeElement, e)) {
				ITreeManager firstAncestorOfType = treeElement.GetFirstAncestorOfType<ITreeManager>();
				if(treeElement.IsSelected(firstAncestorOfType)) {
					if(e.actionKey) {
						treeElement.Unselect(firstAncestorOfType);
					} else {
						treeElement.Select(firstAncestorOfType, false);
					}
				} else {
					treeElement.Select(firstAncestorOfType, e.actionKey);
				}
			}
		}
	}
}