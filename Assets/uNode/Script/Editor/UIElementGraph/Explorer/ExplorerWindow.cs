using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using MaxyGames.uNode.Editors.TreeViews;

namespace MaxyGames.uNode.Editors {
	public class ExplorerWindow : EditorWindow {
		public ExplorerManager explorer;
		public ExplorerEditorData explorerData = new ExplorerEditorData();

		[MenuItem("Tools/uNode/Graph Explorer", false, 100)]
		public static ExplorerWindow ShowWindow() {
			ExplorerWindow window = (ExplorerWindow)GetWindow(typeof(ExplorerWindow));
			window.autoRepaintOnSceneChange = true;
			window.wantsMouseMove = true;
			window.titleContent = new GUIContent("Explorer");
			window.Show();
			return window;
		}

		private void Save() {
			uNodeEditorUtility.SaveEditorData(explorerData, "ExplorerData");
		}

		private void Load() {
			explorerData = uNodeEditorUtility.LoadEditorData<ExplorerEditorData>("ExplorerData");
			if(explorerData == null) {
				explorerData = new ExplorerEditorData();
			}
			//explorerData.graphs.RemoveAll(i => i == null || i.guid == null);
			//explorerData.macros.RemoveAll(i => i == null || i.guid == null);
		}

		private void OnEnable() {
			Load();
			ExplorerManager.explorerData = explorerData;
			explorer = new ExplorerManager();
			explorer.onSavePerformed += () => {
				if(explorer.graphTree != null) {
					explorer.graphTree.Query<ObjectTreeView>().ForEach((tree) => {
						var index = explorerData.objects.FindIndex(data => data.guid == tree.data.guid);
						if(index >= 0) {
							explorerData.objects[index] = tree.data;
						}
					});
				}
				if(explorer.macroTree != null) {
					explorer.graphTree.Query<GraphTreeView>().ForEach((tree) => {
						var index = explorerData.graphs.FindIndex(data => data.guid == tree.data.guid);
						if(index >= 0) {
							explorerData.graphs[index] = tree.data;
						}
					});
				}
			};
			explorer.Refresh();

			ScrollView scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal) {
				name = "scroll-view",
			};
			scroll.styleSheets.Add(Resources.Load<StyleSheet>("ExplorerStyles/ExplorerView"));
			scroll.Add(explorer);

			var toolbar = new Toolbar();
			{
				//toolbar.Add(new ToolbarButton(() => {

				//}) { text = "New Project" });

				toolbar.Add(new ToolbarButton(() => {
					explorer.Refresh();
				}) { text = "Refresh" });
				var searchField = new ToolbarPopupSearchField();
				searchField.style.flexGrow = 1;
#if UNITY_2019_3_OR_NEWER
				searchField.style.width = new StyleLength(StyleKeyword.Auto);
#else
				searchField.Children().First().style.flexGrow = 1;
#endif
				searchField.Q<TextField>().style.width = new StyleLength(StyleKeyword.Auto);
				searchField.RegisterCallback<KeyDownEvent>(evt => {
					if(evt.keyCode == KeyCode.Return) {
						explorer.Search(searchField.value.ToLower());
					}
				}, TrickleDown.TrickleDown);
				searchField.RegisterValueChangedCallback((evt) => {
					if(string.IsNullOrEmpty(evt.newValue)) {
						explorer.Search(evt.newValue);
					}
				});
				searchField.menu.AppendAction("Contains", (menu) => {
					explorerData.searchKind = SearchKind.Contains;
				}, (act) => explorerData.searchKind == SearchKind.Contains ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
				searchField.menu.AppendAction("Equals", (menu) => {
					explorerData.searchKind = SearchKind.Equals;
				}, (act) => explorerData.searchKind == SearchKind.Equals ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
				searchField.menu.AppendAction("Ends with", (menu) => {
					explorerData.searchKind = SearchKind.Endswith;
				}, (act) => explorerData.searchKind == SearchKind.Endswith ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
				searchField.menu.AppendAction("Start with", (menu) => {
					explorerData.searchKind = SearchKind.Startwith;
				}, (act) => explorerData.searchKind == SearchKind.Startwith ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
				toolbar.Add(searchField);
				//toolbar.Add(new ToolbarSpacer() {
				//	flex = true,
				//});
				var toolbarMenu = new ToolbarMenu() {
					text = "{}",
				};
				toolbarMenu.style.paddingLeft = 0;
				toolbarMenu.style.paddingRight = 2;
				toolbarMenu.menu.AppendAction("Show or Hide", (menu) => { }, (act) => DropdownMenuAction.AlwaysDisabled(act));
				toolbarMenu.menu.AppendSeparator("");
				toolbarMenu.menu.AppendAction("Summary", (menu) => {
					explorerData.showSummary = !explorerData.showSummary;
					explorer.Refresh();
				}, (act) => explorerData.showSummary ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
				toolbarMenu.menu.AppendAction("Type Icon", (menu) => {
					explorerData.showTypeIcon = !explorerData.showTypeIcon;
					explorer.Refresh();
				}, (act) => explorerData.showTypeIcon ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
				toolbarMenu.menu.AppendSeparator("");
				toolbarMenu.menu.AppendAction("Variables", (menu) => {
					explorerData.showVariable = !explorerData.showVariable;
					explorer.Refresh();
				}, (act) => explorerData.showVariable ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
				toolbarMenu.menu.AppendAction("Properties", (menu) => {
					explorerData.showProperty = !explorerData.showProperty;
					explorer.Refresh();
				}, (act) => explorerData.showProperty ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
				toolbarMenu.menu.AppendAction("Functions", (menu) => {
					explorerData.showFunction = !explorerData.showFunction;
					explorer.Refresh();
				}, (act) => explorerData.showFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
				toolbarMenu.menu.AppendAction("Enums", (menu) => {
					explorerData.showEnum = !explorerData.showEnum;
					explorer.Refresh();
				}, (act) => explorerData.showEnum ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
				toolbarMenu.menu.AppendAction("Interfaces", (menu) => {
					explorerData.showInterface = !explorerData.showInterface;
					explorer.Refresh();
				}, (act) => explorerData.showInterface ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
				toolbarMenu.menu.AppendSeparator("");
				toolbarMenu.menu.AppendAction("Graphs", (menu) => {
					explorerData.showGraph = !explorerData.showGraph;
					explorer.Refresh();
				}, (act) => explorerData.showGraph ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
				toolbarMenu.menu.AppendAction("Macros", (menu) => {
					explorerData.showMacro = !explorerData.showMacro;
					explorer.Refresh();
				}, (act) => explorerData.showMacro ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
				toolbar.Add(toolbarMenu);
				UIElementUtility.ForceDarkToolbarStyleSheet(toolbar);
			}
			rootVisualElement.Add(toolbar);
			rootVisualElement.Add(scroll);
			scroll.style.marginTop = 19;
			scroll.StretchToParentSize();
		}


		private void OnDisable() {
			Save();
		}
	}
}