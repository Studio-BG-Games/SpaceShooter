using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace MaxyGames.uNode.Editors {
	[System.Serializable]
	public class BrowserState : TreeViewState {
		public enum TypeKind {
			All,
			Function,
			Variable,
			Property,
			Type,
		}

		public SearchKind searchKind;
		public TypeKind typeKind;
	}

	public class NodeBrowserWindow : EditorWindow {
		[SerializeField] BrowserState browserState;

		public NodeBrowser browser;
		SearchField searchField;

		[MenuItem("Tools/uNode/Node Browser", false, 100)]
		public static NodeBrowserWindow ShowWindow() {
			var window = GetWindow<NodeBrowserWindow>();
			window.titleContent = new GUIContent("Node Browser");
			window.Show();
			return window;
		}

		void OnEnable() {
			if(browserState == null)
				browserState = new BrowserState();

			browser = new NodeBrowser(browserState);
			searchField = new SearchField();
			searchField.downOrUpArrowKeyPressed += browser.SetFocusAndEnsureSelectedItem;
			wantsMouseEnterLeaveWindow = true;
		}

		void OnGUI() {
			DoToolbar();
			DoTreeView();

		}

		void DoTreeView() {
			Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
			browser.window = this;
			browser.OnGUI(rect);
		}

		void DoToolbar() {
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			browser.searchString = searchField.OnToolbarGUI(browser.searchString);
			var searchKind = (SearchKind)EditorGUILayout.EnumPopup(browserState.searchKind, EditorStyles.toolbarPopup, GUILayout.Width(65));
			var typeKind = (BrowserState.TypeKind)EditorGUILayout.EnumPopup(browserState.typeKind, EditorStyles.toolbarPopup, GUILayout.Width(50));
			if(searchKind != browserState.searchKind || typeKind != browserState.typeKind) {
				browserState.searchKind = searchKind;
				browserState.typeKind = typeKind;
				browser.SearchChanged();
			}
			//GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
	}
}
