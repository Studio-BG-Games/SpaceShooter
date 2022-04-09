using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors.TreeViews {
	public interface ITreeManager {
		List<TreeViewItem> selection { get; }
		SearchKind searchKind { get; }

		void AddToSelection(TreeViewItem item, bool additive = false);
		void RemoveFromSelection(TreeViewItem item);
		void ClearSelection();

		void Save();
	}

	public interface ITreeDataSystem {
		ITreeData GetData();
		void SetData(ITreeData value);
	}

	public interface ITreeData {

	}

	public interface IDragableTree {

	}

	public abstract class TreeViewItem : VisualElement {
		public List<TreeViewItem> childTree = new List<TreeViewItem>();

		public ITreeManager manager;

		/// <summary>
		/// The title of tree
		/// </summary>
		public string title {
			get {
				return titleLabel.text;
			}
			set {
				titleLabel.text = value;
			}
		}

		/// <summary>
		/// Called on item is expanded for first time (can be used for displaying child tree)
		/// </summary>
		public event Action onFirstExpanded;
		/// <summary>
		/// Called on expand button is clicked
		/// </summary>
		public event Action onExpandClicked;
		/// <summary>
		/// Called on save is being performed
		/// </summary>
		public event Action onSavePerformed;

		#region Containers
		public VisualElement mainContainer {
			get;
			private set;
		}
		public VisualElement titleContainer {
			get;
			private set;
		}
		public VisualElement childContainer {
			get;
			private set;
		}
		public VisualElement headerContainer {
			get;
			private set;
		}
		#endregion

		public Foldout expandedElement {
			get;
			private set;
		}
		public Label titleLabel {
			get;
			private set;
		}
		public Image titleIcon {
			get;
			private set;
		}

		public TreeViewItem() {
			name = "tree-item";
			styleSheets.Add(Resources.Load<StyleSheet>("ExplorerStyles/TreeStyle"));
			VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>("uxml/Explorer");
			visualTreeAsset.CloneTree(this);

			mainContainer = this.Q("node-border");
			titleContainer = this.Q("title");
			childContainer = this.Q("childs");
			headerContainer = this.Q("header");

			expandedElement = this.Q<Foldout>("expanded");
			expandedElement.RegisterValueChangedCallback((evt) => {
				expanded = evt.newValue;
				onExpandClicked?.Invoke();
			});
			expandedElement.value = expanded;
			titleLabel = this.Q<Label>("title-label");
			titleIcon = this.Q<Image>("title-icon");

			this.AddManipulator(new TreeClickSelector());
			this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
		}

		protected bool hasInitialize;

		/// <summary>
		/// Call this to initialize tree view.
		/// Note: this will auto called on adding tree using AddChild()
		/// And when overriding make sure to call this too using base.Initialize()
		/// </summary>
		public virtual void Initialize() {
			if(titleIcon.image == null) {
				titleIcon.HideElement();
			}
			RefreshExpandedState();
			hasInitialize = true;
		}

		private bool m_hasFirstExpanded = false;
		protected bool m_expanded = false;
		public virtual bool expanded {
			get {
				if(isInSearch) {
					return searchExpanded;
				}
				return m_expanded;
			}
			set {
				if(isInSearch) {
					searchExpanded = value;
				} else {
					m_expanded = value;
				}
				RefreshExpandedState();
			}
		}

		protected bool m_selected;
		public virtual bool selected {
			get {
				return m_selected;
			}
			set {
				m_selected = value;
				if(m_selected) {
					AddToClassList("selected");
				} else {
					RemoveFromClassList("selected");
				}
			}
		}

		public virtual void AddChild(TreeViewItem child) {
			childTree.Add(child);
			childContainer.Add(child);
			if(manager != null) {
				child.manager = manager;
			}
			child.Initialize();
		}

		public virtual void RemoveChild(TreeViewItem child) {
			childTree.Remove(child);
			child.RemoveFromHierarchy();
		}

		public virtual void RefreshExpandedState() {
			expandedElement.SetValueWithoutNotify(expanded);
			childContainer.SetElementVisibility(expanded);
			EnableInClassList("expanded", expanded);
			EnableInClassList("collapsed", !expanded);
			if(expanded && onFirstExpanded != null && !m_hasFirstExpanded) {
				m_hasFirstExpanded = true;
				if(hasInitialize) {
					onFirstExpanded();
				} else {
					uNodeThreadUtility.Queue(onFirstExpanded);
				}
			}
		}

		public bool IsSelectable() {
			return true;
		}

		public virtual bool HitTest(Vector2 localPoint) {
			// localPoint = this.ChangeCoordinatesTo(titleLabel, localPoint);
			// var rect = titleLabel.GetRect();
			// return rect.Contains(localPoint);
			return ContainsPoint(localPoint);
		}

		public virtual void OnSelected() {
			selected = true;
			MarkDirtyRepaint();
		}

		public virtual void OnUnselected() {
			selected = false;
			MarkDirtyRepaint();
		}

		public virtual void Select(ITreeManager manager, bool additive) {
			manager.AddToSelection(this, additive);
		}

		public virtual void Unselect(ITreeManager manager) {
			manager.RemoveFromSelection(this);
		}

		public virtual bool IsSelected(ITreeManager manager) {
			return selected && manager.selection.Contains(this);
		}

		public virtual void Save() {
			onSavePerformed?.Invoke();
			if(manager != null) {
				manager.Save();
			}
		}

		public virtual void SetVisibility(bool enable) {
			if(enable) {
				this.ShowElement();
			} else {
				this.HideElement();
			}
		}

		public void Expand() {
			expanded = true;
			onExpandClicked?.Invoke();
		}

		public void Colapse() {
			expanded = false;
			onExpandClicked?.Invoke();
		}

		public void ExpandAll() {
			expanded = true;
			onExpandClicked?.Invoke();
			foreach(var child in childTree) {
				child.ExpandAll();
			}
		}

		public void ColapseAll() {
			expanded = false;
			onExpandClicked?.Invoke();
			foreach(var child in childTree) {
				child.ColapseAll();
			}
		}

		public virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
			evt.menu.AppendAction("Expand All", (act) => {
				ExpandAll();
			}, DropdownMenuAction.AlwaysEnabled);
			evt.menu.AppendAction("Colapse All", (act) => {
				ColapseAll();
			}, DropdownMenuAction.AlwaysEnabled);
		}

		public List<TreeViewItem> GetChildTrees() {
			List<TreeViewItem> trees = new List<TreeViewItem>();
			trees.AddRange(childTree);
			foreach(var t in childTree) {
				trees.AddRange(t.GetChildTrees());
			}
			return trees;
		}

		#region Search
		public string searchText;
		public bool searchExpanded = true;
		public bool isInSearch {
			get {
				return !string.IsNullOrEmpty(searchText);
			}
		}

		public bool isValidSearch {
			get {
				return searchText == null || IsValidSearch(searchText);
			}
		}

		public void Search(string value) {
			searchText = value;
			OnSearch(value);
		}

		protected virtual void OnSearch(string value) {
			RefreshExpandedState();
			bool flag = false;
			foreach(var c in childTree) {
				c.Search(value);
				if(c.style.visibility == Visibility.Visible) {
					flag = true;
				}
			}
			SetVisibility(flag || IsValidSearch(value));
		}

		protected virtual bool IsValidSearch(string searchText) {
			return IsValidSearch(title, searchText);
		}

		protected bool IsValidSearch(string text, string searchText) {
			if(manager != null) {
				switch(manager.searchKind) {
					case SearchKind.Endswith:
						return text.EndsWith(searchText, StringComparison.OrdinalIgnoreCase);
					case SearchKind.Equals:
						return text.Equals(searchText, StringComparison.OrdinalIgnoreCase);
					case SearchKind.Startwith:
						return text.StartsWith(searchText, StringComparison.OrdinalIgnoreCase);
				}
			}
			return text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
		}
		#endregion
	}
}