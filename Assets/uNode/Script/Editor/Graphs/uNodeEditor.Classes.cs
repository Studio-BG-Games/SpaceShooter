using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace MaxyGames.uNode.Editors {
	/// <summary>
	/// The main editor window for editing uNode.
	/// </summary>
	public partial class uNodeEditor {
		public class GraphExplorerTree : TreeView {
			public HierarchyGraphTree selected;

			Dictionary<int, TreeViewItem> treeMap;

			public GraphExplorerTree() : base(new TreeViewState()) {
				showAlternatingRowBackgrounds = true;
				showBorder = true;
				Reload();
			}

			protected override TreeViewItem BuildRoot() {
				return new TreeViewItem { id = 0, depth = -1 };
			}

			protected override bool CanChangeExpandedState(TreeViewItem item) {
				return false;
			}

			protected override IList<TreeViewItem> BuildRows(TreeViewItem root) {
				var rows = GetRows() ?? new List<TreeViewItem>();
				rows.Clear();
				var prefabs = GraphUtility.FindGraphPrefabs();
				var graphDic = new Dictionary<string, List<uNodeRoot>>();
				foreach(var prefab in prefabs) {
					var comps = prefab.GetComponents<uNodeRoot>();
					for(int i=0;i<comps.Length;i++) {
						var graph = comps[i];
						var id = graph is uNodeMacro ? "Macro" : graph.Namespace;
						if(!graphDic.TryGetValue(id, out var graphs)) {
							graphs = new List<uNodeRoot>();
							graphDic.Add(id, graphs);
						}
						graphs.Add(graph);
					}
				}
				var dic = graphDic.ToList();
				dic.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase));
				treeMap = new Dictionary<int, TreeViewItem>();
				foreach(var pair in dic) {
					var graphs = pair.Value;
					graphs.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.OrdinalIgnoreCase));
					TreeViewItem nsTree = root;
					if(!hasSearch) {
						nsTree = new HiearchyNamespaceTree(string.IsNullOrEmpty(pair.Key) ? "global" : pair.Key, -1);
						root.AddChild(nsTree);
						rows.Add(nsTree);
						treeMap.Add(nsTree.id, nsTree);
					}
					foreach(var graph in graphs) {
						if(hasSearch) {
							if(graph.DisplayName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0) {
								var tree = new HierarchyGraphTree(graph, -1);
								if(!string.IsNullOrEmpty(graph.summary)) {
									var strs = graph.summary.Split('\n');
									for(int i = 0; i < strs.Length; i++) {
										if(string.IsNullOrEmpty(strs[i]))
											continue;
										var summary = new HierarchySummaryTree(strs[i], tree);
										nsTree.AddChild(summary);
										rows.Add(summary);
										treeMap.Add(summary.id, summary);
									}
								}
								nsTree.AddChild(tree);
								rows.Add(tree);
								treeMap.Add(tree.id, tree);
							}
						} else {
							var tree = new HierarchyGraphTree(graph, -1);
							if(!string.IsNullOrEmpty(graph.summary)) {
								var strs = graph.summary.Split('\n');
								for(int i = 0; i < strs.Length; i++) {
									if(string.IsNullOrEmpty(strs[i]))
										continue;
									var summary = new HierarchySummaryTree(strs[i], tree);
									nsTree.AddChild(summary);
									rows.Add(summary);
									treeMap.Add(summary.id, summary);
								}
							}
							nsTree.AddChild(tree);
							rows.Add(tree);
							treeMap.Add(tree.id, tree);
						}
					}
				}
				SetupDepthsFromParentsAndChildren(root);
				return rows;
			}

			protected override void SelectionChanged(IList<int> selectedIds) {
				if(selectedIds.Count > 0) {
					var tree = treeMap[selectedIds.FirstOrDefault()];
					if(tree is HierarchyGraphTree graphTree) {
						selected = graphTree;
					} else if(tree is HierarchySummaryTree summaryTree) {
						selected = summaryTree.owner as HierarchyGraphTree;
					}
				} else {
					selected = null;
				}
			}

			protected override bool CanMultiSelect(TreeViewItem item) {
				return false;
			}

			protected override void DoubleClickedItem(int id) {
				if(treeMap.TryGetValue(id, out var tree)) {
					if(tree is HierarchyGraphTree graphTree) {
						Open(graphTree.graph);
					} else if(tree is HierarchySummaryTree summaryTree) {
						graphTree = summaryTree.owner as HierarchyGraphTree;
						if(graphTree != null) {
							Open(graphTree.graph);
						}
					}
				}
			}

			protected override void RowGUI(RowGUIArgs args) {
				Event evt = Event.current;
				if(evt.type == EventType.Repaint) {
					#region Draw Row
					Rect labelRect = args.rowRect;
					labelRect.x += GetContentIndent(args.item);
					//if(args.selected) {
					//	uNodeGUIStyle.itemStatic.Draw(labelRect, new GUIContent(args.label, icon), false, false, false, false);
					//} else {
					//	uNodeGUIStyle.itemNormal.Draw(labelRect, new GUIContent(args.label, icon), false, false, false, false);
					//}
					if(args.item is HierarchySummaryTree) {
						uNodeGUIStyle.itemNormal.Draw(labelRect, new GUIContent(uNodeUtility.WrapTextWithColor("//" + args.label, uNodeUtility.GetRichTextSetting().summaryColor), args.item.icon), false, false, false, false);
					} else {
						uNodeGUIStyle.itemNormal.Draw(labelRect, new GUIContent(args.label, args.item.icon), false, false, false, false);
					}
					#endregion
				}
				//base.RowGUI(args);
			}
		}

		public static class EditorDataSerializer {
			[Serializable]
			class Data {
				public byte[] data;
				public DataReference[] references;
				public string type;

				public OdinSerializedData Load() {
					var data = new OdinSerializedData();
					data.data = this.data;
					data.serializedType = type;
					data.references = new List<UnityEngine.Object>();
					for(int i = 0; i < references.Length; i++) {
						data.references.Add(references[i].GetObject());
					}
					return data;
				}

				public static Data Create(OdinSerializedData serializedData) {
					var data = new Data();
					data.data = serializedData.data;
					data.type = serializedData.serializedType;
					data.references = new DataReference[serializedData.references.Count];
					for(int i = 0; i < data.references.Length; i++) {
						data.references[i] = DataReference.Create(serializedData.references[i]);
					}
					return data;
				}
			}

			[Serializable]
			class DataReference {
				public string path;
				public int uid;

				public UnityEngine.Object GetObject() {
					var obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
					if(uNodeUtility.GetObjectID(obj) == uid) {
						return obj;
					} else {
						var objs = AssetDatabase.LoadAllAssetsAtPath(path);
						for(int i = 0; i < objs.Length; i++) {
							if(uNodeUtility.GetObjectID(objs[i]) == uid) {
								return objs[i];
							}
						}
						//if(obj is GameObject gameObject) {
						//	var comps = gameObject.GetComponentsInChildren<Component>(true);
						//	for(int i=0;i<comps.Length;i++) {
						//		if(uNodeUtility.GetObjectID(comps[i]) == uid) {
						//			return comps[i];
						//		}
						//	}
						//}
					}
					return null;
				}

				public static DataReference Create(UnityEngine.Object obj) {
					if(obj == null)
						return null;
					var path = AssetDatabase.GetAssetPath(obj);
					if(!string.IsNullOrEmpty(path)) {
						DataReference data = new DataReference();
						data.path = path;
						data.uid = uNodeUtility.GetObjectID(obj);
						return data;
					}
					return null;
				}
			}

			public static void Save<T>(T value, string fileName) {
				Directory.CreateDirectory("uNode2Data");
				char separator = Path.DirectorySeparatorChar;
				string path = "uNode2Data" + separator + fileName + ".json";
				File.WriteAllText(path, JsonUtility.ToJson(Data.Create(SerializerUtility.SerializeValue(value))));
			}

			public static T Load<T>(string fieldName) {
				char separator = Path.DirectorySeparatorChar;
				string path = "uNode2Data" + separator + fieldName + ".json";
				if(File.Exists(path)) {
					var data = JsonUtility.FromJson<Data>(File.ReadAllText(path));
					if(data != null) {
						return SerializerUtility.Deserialize<T>(data.Load());
					}
				}
				return default;
			}
		}

		[System.Serializable]
		public class uNodeEditorData {
			public List<EditorScriptInfo> scriptInformations = new List<EditorScriptInfo>();

			/// <summary>
			/// Are the left panel is visible?
			/// </summary>
			public bool leftVisibility = true;
			/// <summary>
			/// Are the right panel is visible?
			/// </summary>
			public bool rightVisibility = true;
			/// <summary>
			/// The heigh of variable editor.
			/// </summary>
			public float variableEditorHeight = 150f;

			#region Panel
			[SerializeField]
			private float _rightPanelWidth = 300;
			[SerializeField]
			private float _leftPanelWidth = 250;
			public List<string> lastOpenedFile;

			/// <summary>
			/// The width of right panel.
			/// </summary>
			public float rightPanelWidth {
				get {
					if(!rightVisibility)
						return 0;
					return _rightPanelWidth;
				}
				set {
					_rightPanelWidth = value;
				}
			}

			/// <summary>
			/// The width of left panel.
			/// </summary>
			public float leftPanelWidth {
				get {
					if(!leftVisibility)
						return 0;
					return _leftPanelWidth;
				}
				set {
					_leftPanelWidth = value;
				}
			}
			#endregion

			#region Recent
			[Serializable]
			public class RecentItem {
				[SerializeField]
				private MemberData memberData;

				private MemberInfo _info;
				public MemberInfo info {
					get {
						if(_info == null && memberData != null) {
							switch(memberData.targetType) {
								case MemberData.TargetType.Type:
								case MemberData.TargetType.uNodeType:
									_info = memberData.startType;
									break;
								case MemberData.TargetType.Field:
								case MemberData.TargetType.Constructor:
								case MemberData.TargetType.Event:
								case MemberData.TargetType.Method:
								case MemberData.TargetType.Property:
									var members = memberData.GetMembers(false);
									if(members != null) {
										_info = members[members.Length - 1];
									}
									break;
							}
						}
						return _info;
					}
					set {
						_info = value;
						memberData = MemberData.CreateFromMember(_info);
					}
				}
				public bool isStatic {
					get {
						if(info == null)
							return false;
						return ReflectionUtils.GetMemberIsStatic(info);
					}
				}
			}

			/// <summary>
			/// The recent items data.
			/// </summary>
			public List<RecentItem> recentItems = new List<RecentItem>();

			public void AddRecentItem(RecentItem recentItem) {
				while(recentItems.Count >= 50) {
					recentItems.RemoveAt(recentItems.Count - 1);
				}
				recentItems.RemoveAll(item => item.info == recentItem.info);
				recentItems.Insert(0, recentItem);
				SaveOptions();
			}
			#endregion

			#region Favorites
			[SerializeField]
			Dictionary<string, Dictionary<string, object>> customFavoriteDatas;

			public List<RecentItem> favoriteItems;

			public void AddFavorite(MemberInfo member) {
				if(favoriteItems == null)
					favoriteItems = new List<RecentItem>();
				if(!HasFavorite(member)) {
					favoriteItems.Add(new RecentItem() {
						info = member
					});
					SaveOptions();
				}
			}

			public void AddFavorite(string kind, string guid, object data = null) {
				if(customFavoriteDatas == null)
					customFavoriteDatas = new Dictionary<string, Dictionary<string, object>>();
				if(!customFavoriteDatas.TryGetValue(kind, out var map)) {
					map = new Dictionary<string, object>();
					customFavoriteDatas[kind] = map;
				}
				map[guid] = data;
			}

			public void RemoveFavorite(string kind, string guid) {
				if(customFavoriteDatas == null)
					return;
				if(customFavoriteDatas.TryGetValue(kind, out var map)) {
					map.Remove(guid);
				}
			}

			public bool HasFavorite(string kind, string guid) {
				if(customFavoriteDatas == null)
					return false;
				if(customFavoriteDatas.TryGetValue(kind, out var map)) {
					return map.ContainsKey(guid);
				}
				return false;
			}

			public void RemoveFavorite(MemberInfo member) {
				if(favoriteItems == null)
					return;
				if(HasFavorite(member)) {
					favoriteItems.Remove(favoriteItems.First(item => item != null && item.info == member));
					SaveOptions();
				}
			}

			public bool HasFavorite(MemberInfo member) {
				if(favoriteItems == null)
					return false;
				return favoriteItems.Any(item => item != null && item.info == member);
			}

			[SerializeField]
			HashSet<string> _favoriteNamespaces;
			public HashSet<string> favoriteNamespaces {
				get {
					if(_favoriteNamespaces == null) {
						_favoriteNamespaces = new HashSet<string>() {
							"System",
							"System.Collections",
							"UnityEngine.AI",
							"UnityEngine.Events",
							"UnityEngine.EventSystems",
							"UnityEngine.SceneManagement",
							"UnityEngine.UI",
							"UnityEngine.UIElements",
						};
					}
					return _favoriteNamespaces;
				}
			}


			#endregion

			#region Graph Infos
			public void RegisterGraphInfos(IEnumerable<ScriptInformation> informations, UnityEngine.Object owner, string scriptPath) {
				if(informations != null) {
					EditorScriptInfo scriptInfo = new EditorScriptInfo() {
						guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(owner)),
						path = scriptPath,
					};
					scriptInfo.informations = informations.ToArray();
					var prevInfo = scriptInformations.FirstOrDefault(g => g.guid == scriptInfo.guid);
					if(prevInfo != null) {
						scriptInformations.Remove(prevInfo);
					}
					scriptInformations.Add(scriptInfo);
					uNodeThreadUtility.ExecuteOnce(uNodeEditor.SaveOptions, "unode_save_informations");
				}
			}

			public bool UnregisterGraphInfo(UnityEngine.Object owner) {
				var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(owner));
				var prevInfo = scriptInformations.FirstOrDefault(g => g.guid == guid);
				if(prevInfo != null) {
					return scriptInformations.Remove(prevInfo);
				}
				uNodeThreadUtility.ExecuteOnce(uNodeEditor.SaveOptions, "unode_save_informations");
				return false;
			}
			#endregion
		}

		[Serializable]
		public class EditorScriptInfo {
			public string guid;
			public string path;
			public ScriptInformation[] informations;
		}

		[Serializable]
		public class GraphData {
			[SerializeField]
			private UnityEngine.Object _graph;
			[SerializeField]
			private string graphPath;

			public GameObject graph {
				get {
					if(_graph as GameObject == null && !string.IsNullOrEmpty(graphPath)) {
						_graph = AssetDatabase.LoadAssetAtPath(graphPath, typeof(GameObject));
					}
					return _graph as GameObject;
				}
				set {
					_graph = value;
					if(value is GameObject) {
						graphPath = AssetDatabase.GetAssetPath(value);
					} else {
						graphPath = null;
					}
				}
			}

			public GameObject graphPrefab {
				get {
					return graph as GameObject;
				}
			}

			public List<GraphEditorData> data = new List<GraphEditorData>();

			[SerializeField]
			private int selectedIndex;
			/// <summary>
			/// The current selected graph editor data
			/// </summary>
			/// <value></value>
			public GraphEditorData selectedData {
				get {
					if(data.Count == 0 || selectedIndex >= data.Count || selectedIndex < 0) {
						data.Add(new GraphEditorData());
						selectedIndex = data.Count - 1;
						return data[selectedIndex];
					}
					return data[selectedIndex];
				}
				set {
					if(value == null) {
						selectedIndex = -1;
						return;
					}
					for(int i = 0; i < data.Count; i++) {
						if(data[i] == value) {
							selectedIndex = i;
							return;
						}
					}
					data.Add(value);
					selectedIndex = data.Count - 1;
				}
			}

			public string displayName {
				get {
					if(graph != null) {
						return graph.name;
					}
					try {
						if(owner == null || !owner || owner.hideFlags == HideFlags.HideAndDontSave) {
							owner = null;
							return "";
						}
						return owner.name;
					}
					catch {
						//To fix sometime error at editor startup.
						owner = null;
						return "";
					}
				}
			}

			[SerializeField]
			private GameObject _owner;
			/// <summary>
			/// This is the persistence graph
			/// </summary>
			/// <value></value>
			public GameObject owner {
				get {
					if(_owner == null || !_owner) {
						if(graph != null && graph) {
							_owner = LoadTempGraphObject(graph);
							if(_owner != null) {
								var root = _owner.GetComponent<uNodeRoot>();
								if(root != null) {
									selectedData.SetOwner(root);
								} else {
									var data = _owner.GetComponent<uNodeData>();
									if(data != null) {
										selectedData.SetOwner(data);
									} else {
										selectedData.SetOwner(_owner);
									}
								}
							} else {
								graph = null;
							}
						} else {
							foreach(var d in data) {
								if(d.owner != null) {
									_owner = d.owner;
								}
							}
						}
					}
					return _owner;
				}
				set {
					_owner = value;
				}
			}
		}

		[Serializable]
		public class ValueInspector {
			public UnityEngine.Object owner;
			public object value;

			public ValueInspector() {

			}

			public ValueInspector(object value, UnityEngine.Object owner) {
				this.value = value;
				this.owner = owner;
			}
		}

		public class EditorInteraction {
			public string name;
			public object userObject;
			public object userObject2;
			//public object userObject3;
			//public object userObject4;
			public InteractionKind interactionKind = InteractionKind.Drag;
			public Action onClick;
			public Action onDrag;

			public bool hasDragged;

			public enum InteractionKind {
				Drag,
				Click,
				ClickOrDrag
			}

			public EditorInteraction(string name) {
				this.name = name;
			}

			public EditorInteraction(string name, object userObject) {
				this.name = name;
				this.userObject = userObject;
			}

			public static implicit operator EditorInteraction(string name) {
				return new EditorInteraction(name);
			}

			public static bool operator ==(EditorInteraction x, string y) {
				if(ReferenceEquals(x, null)) {
					return y == null;
				} else if(y == null) {
					return ReferenceEquals(x, null);
				}
				return x.name == y;
			}

			public static bool operator !=(EditorInteraction x, string y) {
				return !(x == y);
			}

			public override bool Equals(object obj) {
				var interaction = obj as EditorInteraction;
				return !ReferenceEquals(interaction, null) && name == interaction.name;
			}

			public override int GetHashCode() {
				return 363513814 + EqualityComparer<string>.Default.GetHashCode(name);
			}
		}
	}
}