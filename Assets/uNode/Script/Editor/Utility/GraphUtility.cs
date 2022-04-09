using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode.Editors {
	public static class GraphUtility {
		#region Temp Manager
		internal static class TempGraphManager {
			private static GameObject _managerObject;
			public static GameObject managerObject {
				get {
					if(_managerObject == null) {
						var go = GameObject.Find("[uNode_Temp_GraphManager]");
						if(go == null) {
							go = new GameObject("[uNode_Temp_GraphManager]");
							//go.SetActive(false);
						}
						_managerObject = go;
						OnSaved();
					}
					return _managerObject;
				}
			}

			internal static void OnSaving() {
				if(managerObject == null)
					return;
				if(managerObject != null) {
					managerObject.hideFlags = HideFlags.HideAndDontSave;
				}
			}

			internal static void OnSaved() {
				if(managerObject == null)
					return;
#if UNODE_DEBUG
				managerObject.hideFlags = HideFlags.None;
#else
				managerObject.hideFlags = HideFlags.HideInHierarchy;
#endif
			}
		}

		public const string KEY_TEMP_OBJECT = "[uNode_Temp_";
		#endregion

		#region Analizer
		public static void CheckGraphErrors(IList<uNodeRoot> graphs = null, bool checkProjectGraphs = true) {
			HashSet<uNodeRoot> allGraphs = new HashSet<uNodeRoot>();
			if(graphs != null) {
				foreach(var g in graphs) {
					allGraphs.Add(g);
				}
			}
			if(checkProjectGraphs) {
				var graphPrefabs = GraphUtility.FindGraphPrefabs();
				foreach(var prefab in graphPrefabs) {
					var gameObject = prefab;
					if(HasTempGraphObject(prefab)) {
						gameObject = GetTempGraphObject(prefab);
					}
					if(gameObject != null) {
						var graph = gameObject.GetComponents<uNodeRoot>();
						foreach(var g in graph) {
							allGraphs.Add(g);
						}
					}
				}
			}
			if(allGraphs.Count > 0) {
				uNodeUtility.editorErrorMap = null;
				foreach(var g in allGraphs) {
					var root = g.RootObject;
					if(root != null) {
						var nodes = root.GetComponentsInChildren<NodeComponent>();
						foreach(var comp in nodes) {
							comp.CheckError();
						}
					}
				}
				if(uNodeUtility.editorErrorMap != null) {
					ShowErrorsInWindow(uNodeUtility.editorErrorMap);
					uNodeUtility.editorErrorMap = null;
					return;
				}
			}
			uNodeEditorUtility.DisplayMessage("", "No error found.");
		}

		/// <summary>
		/// Find references of MemberInfo from all graphs in the project.
		/// </summary>
		/// <param name="memberInfo"></param>
		/// <returns></returns>
		public static List<Object> FindReferences(MemberInfo memberInfo) {
			List<Object> results = new List<Object>();
			var graphPrefabs = GraphUtility.FindGraphPrefabsWithComponent<uNodeRoot>();
			foreach(var prefab in graphPrefabs) {
				var gameObject = prefab;
				if(HasTempGraphObject(prefab)) {
					gameObject = GetTempGraphObject(prefab);
				}
				var scripts = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
				Func<object, bool> scriptValidation = (obj) => {
					MemberData member = obj as MemberData;
					if(member != null) {
						if(memberInfo is Type) {
							if(member.startType == memberInfo || member.type == memberInfo) {
								return true;
							}
						}
						var members = member.GetMembers(false);
						if(members != null) {
							for(int i = 0; i < members.Length; i++) {
								var m = members[i];
								if(member.namePath.Length > i + 1) {
									if(m == memberInfo) {
										return true;
									}
								}
							}
						}
					}
					return false;
				};
				Array.ForEach(scripts, script => {
					bool flag = AnalizerUtility.AnalizeObject(script, scriptValidation);
					if(flag) {
						results.Add(script);
					}
				});
			}
			return results;
		}

		public static List<Object> FindVariableUsages(uNodeRoot graph, string name, bool allGraphs = true) {
			var references = new List<Object>();
			if(graph == null || graph.RootObject == null)
				return references;
			var scripts = graph.RootObject.GetComponentsInChildren<MonoBehaviour>(true);
			Func<object, bool> scriptValidation = (obj) => {
				MemberData member = obj as MemberData;
				if(member != null) {
					if(member.targetType == MemberData.TargetType.uNodeVariable && member.startName == name) {
						return true;
					}
				}
				return false;
			};
			Array.ForEach(scripts, script => {
				bool flag = AnalizerUtility.AnalizeObject(script, scriptValidation);
				if(flag) {
					references.Add(script);
				}
			});
			if(allGraphs) {
				if(IsTempGraphObject(graph.gameObject)) {
					var prefab = uNodeEditorUtility.GetComponentSource(graph, null);
					if(prefab != null) {
						var runtimeInfo = ReflectionUtils.GetRuntimeType(prefab).GetField(name);
						if(runtimeInfo != null) {
							references.AddRange(FindReferences(runtimeInfo));
							references = references.Distinct().ToList();
						}
					}
				} else {
					var runtimeInfo = ReflectionUtils.GetRuntimeType(graph).GetField(name);
					if(runtimeInfo != null) {
						references.AddRange(FindReferences(runtimeInfo));
						references = references.Distinct().ToList();
					}
				}
			}
			return references;
		}

		public static List<Object> FindPropertyUsages(uNodeRoot graph, string name, bool allGraphs = true) {
			var references = new List<Object>();
			if(graph == null || graph.RootObject == null)
				return references;
			var scripts = graph.RootObject.GetComponentsInChildren<MonoBehaviour>(true);
			Func<object, bool> scriptValidation = (obj) => {
				MemberData member = obj as MemberData;
				if(member != null) {
					if(member.targetType == MemberData.TargetType.uNodeProperty && member.startName == name) {
						return true;
					}
				}
				return false;
			};
			Array.ForEach(scripts, script => {
				bool flag = AnalizerUtility.AnalizeObject(script, scriptValidation);
				if(flag) {
					references.Add(script);
				}
			});
			if(allGraphs) {
				if(IsTempGraphObject(graph.gameObject)) {
					var prefab = uNodeEditorUtility.GetComponentSource(graph, null);
					if(prefab != null) {
						var runtimeInfo = ReflectionUtils.GetRuntimeType(prefab).GetProperty(name);
						if(runtimeInfo != null) {
							references.AddRange(FindReferences(runtimeInfo));
							references = references.Distinct().ToList();
						}
					}
				} else {
					var runtimeInfo = ReflectionUtils.GetRuntimeType(graph).GetField(name);
					if(runtimeInfo != null) {
						references.AddRange(FindReferences(runtimeInfo));
						references = references.Distinct().ToList();
					}
				}
			}
			return references;
		}

		public static List<Object> FindFunctionUsages(uNodeRoot graph, string name, bool allGraphs = true) {
			var references = new List<Object>();
			if(graph == null || graph.RootObject == null)
				return references;
			var scripts = graph.RootObject.GetComponentsInChildren<MonoBehaviour>(true);
			Func<object, bool> scriptValidation = (obj) => {
				MemberData member = obj as MemberData;
				if(member != null) {
					if(member.targetType == MemberData.TargetType.uNodeFunction && member.startName == name) {
						return true;
					}
				}
				return false;
			};
			Array.ForEach(scripts, script => {
				bool flag = AnalizerUtility.AnalizeObject(script, scriptValidation);
				if(flag) {
					references.Add(script);
				}
			});
			if(allGraphs) {
				if(IsTempGraphObject(graph.gameObject)) {
					var prefab = uNodeEditorUtility.GetComponentSource(graph, null);
					if(prefab != null) {
						var runtimeInfo = ReflectionUtils.GetRuntimeType(prefab).GetMethod(name);
						if(runtimeInfo != null) {
							references.AddRange(FindReferences(runtimeInfo));
							references = references.Distinct().ToList();
						}
					}
				} else {
					var runtimeInfo = ReflectionUtils.GetRuntimeType(graph).GetField(name);
					if(runtimeInfo != null) {
						references.AddRange(FindReferences(runtimeInfo));
						references = references.Distinct().ToList();
					}
				}
			}
			return references;
		}

		public static List<Object> FindLocalVariableUsages(ILocalVariableSystem root, string name) {
			var references = new List<Object>();
			if(root == null)
				return references;
			var scripts = (root as Component).GetComponentsInChildren<MonoBehaviour>(true);
			Func<object, bool> scriptValidation = (obj) => {
				MemberData member = obj as MemberData;
				if(member != null) {
					if(member.targetType == MemberData.TargetType.uNodeLocalVariable && member.startName == name && member.startTarget == root) {
						return true;
					}
				}
				return false;
			};
			Array.ForEach(scripts, script => {
				bool flag = AnalizerUtility.AnalizeObject(script, scriptValidation);
				if(flag) {
					references.Add(script);
				}
			});
			return references;
		}

		public static List<Object> FindNodeUsages(Type type) {
			var references = new List<Object>();
			var graphPrefabs = GraphUtility.FindGraphPrefabsWithComponent<uNodeRoot>();
			foreach(var prefab in graphPrefabs) {
				var gameObject = prefab;
				if(HasTempGraphObject(prefab)) {
					gameObject = GetTempGraphObject(prefab);
				}
				var scripts = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
				Array.ForEach(scripts, script => {
					if(script.GetType() == type) {
						references.Add(script);
					}
				});
			}
			return references;
		}

		/// <summary>
		/// Show specific variable usage in window
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="name"></param>
		/// <param name="allGraphs"></param>
		public static void ShowVariableUsages(uNodeRoot graph, string name, bool allGraphs = true) {
			if(graph == null || graph.RootObject == null)
				return;
			var references = FindVariableUsages(graph, name, allGraphs);
			ShowReferencesInWindow(references);
		}

		/// <summary>
		/// Show specific variable usage in window
		/// </summary>
		/// <param name="type"></param>
		public static void ShowNodeUsages(Type type) {
			var references = FindNodeUsages(type);
			ShowReferencesInWindow(references);
		}

		/// <summary>
		/// Show specific variable usage in window
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="name"></param>
		/// <param name="allGraphs"></param>
		public static void ShowPropertyUsages(uNodeRoot graph, string name, bool allGraphs = true) {
			if(graph == null || graph.RootObject == null)
				return;
			var references = FindPropertyUsages(graph, name, allGraphs);
			ShowReferencesInWindow(references);
		}

		/// <summary>
		/// Show specific variable usage in window
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="name"></param>
		/// <param name="allGraphs"></param>
		public static void ShowFunctionUsages(uNodeRoot graph, string name, bool allGraphs = true) {
			if(graph == null || graph.RootObject == null)
				return;
			var references = FindFunctionUsages(graph, name, allGraphs);
			ShowReferencesInWindow(references);
		}

		/// <summary>
		/// Show specific variable usage in window
		/// </summary>
		/// <param name="root"></param>
		/// <param name="name"></param>
		public static void ShowLocalVariableUsages(ILocalVariableSystem root, string name) {
			if(root == null)
				return;
			var references = FindLocalVariableUsages(root, name);
			ShowReferencesInWindow(references);
		}

		/// <summary>
		/// Show specific parameter usage in window
		/// </summary>
		/// <param name="root"></param>
		/// <param name="name"></param>
		public static void ShowParameterUsages(RootObject root, string name) {
			if(root == null)
				return;
			var references = new List<Object>();
			var scripts = root.GetComponentsInChildren<MonoBehaviour>(true);
			Func<object, bool> scriptValidation = (obj) => {
				MemberData member = obj as MemberData;
				if(member != null) {
					if(member.targetType == MemberData.TargetType.uNodeParameter && member.startName == name) {
						return true;
					}
				}
				return false;
			};
			Array.ForEach(scripts, script => {
				bool flag = AnalizerUtility.AnalizeObject(script, scriptValidation);
				if(flag) {
					references.Add(script);
				}
			});
			ShowReferencesInWindow(references);
		}

		/// <summary>
		/// Show specific generic parameter usage in window
		/// </summary>
		/// <param name="root"></param>
		/// <param name="name"></param>
		public static void ShowGenericParameterUsages(RootObject root, string name) {
			if(root == null)
				return;
			var references = new List<Object>();
			var scripts = root.GetComponentsInChildren<MonoBehaviour>(true);
			Func<object, bool> scriptValidation = (obj) => {
				MemberData member = obj as MemberData;
				if(member != null) {
					if(member.targetType == MemberData.TargetType.uNodeGenericParameter && member.startName == name) {
						return true;
					}
				}
				return false;
			};
			Array.ForEach(scripts, script => {
				bool flag = AnalizerUtility.AnalizeObject(script, scriptValidation);
				if(flag) {
					references.Add(script);
				}
			});
			ShowReferencesInWindow(references);
		}

		/// <summary>
		/// Find references of MemberInfo from all graphs in the project and show it in window.
		/// </summary>
		/// <param name="info"></param>
		public static void ShowMemberUsages(MemberInfo info) {
			var references = FindReferences(info);
			ShowReferencesInWindow(references);
		}

		private class ReferenceTree : TreeView {
			public List<Object> references;
			public Dictionary<Object, List<uNodeUtility.ErrorMessage>> errors;

			class ReferenceTreeView : TreeViewItem {
				public Object reference;
				public List<KeyValuePair<string, Texture>> paths;

				public ReferenceTreeView(Object reference) : base(uNodeEditorUtility.GetUIDFromString("R:" + reference.GetInstanceID()), -1, reference.name) {
					this.reference = reference;
					paths = ErrorCheckWindow.GetNodePathWithIcon(reference, null, richText:true);
				}
			}

			class ErrorTreeView : TreeViewItem {
				public uNodeUtility.ErrorMessage error;

				public ErrorTreeView(uNodeUtility.ErrorMessage error) : base(error.GetHashCode(), -1, error.message) {
					this.error = error;
				}
			}

			public ReferenceTree(List<Object> references) : base(new TreeViewState()) {
				this.references = references;
				showBorder = true;
				showAlternatingRowBackgrounds = true;
				Reload();
			}

			public ReferenceTree(Dictionary<Object, List<uNodeUtility.ErrorMessage>> errors) : base(new TreeViewState()) {
				this.errors = errors;
				showBorder = true;
				showAlternatingRowBackgrounds = true;
				Reload();
			}

			protected override TreeViewItem BuildRoot() {
				var root = new TreeViewItem { id = 0, depth = -1 };
				if(references != null) {
					var map = new Dictionary<Object, HashSet<Object>>();
					foreach(var r in references) {
						if(r == null)
							continue;
						Object owner = r;
						if(r is Component comp) {
							if(comp is INode<uNodeRoot> node) {
								owner = node.GetOwner();
							} else {
								owner = comp.transform.root;
							}
						} else if(r is GameObject go) {
							owner = go.GetComponentInParent<uNodeRoot>();
							if(owner == null) {
								owner = go.transform.root;
							}
						}
						if(owner == null)
							owner = r;
						if(!map.TryGetValue(owner, out var list)) {
							list = new HashSet<Object>();
							map[owner] = list;
						}
						list.Add(r);
					}
					foreach(var pair in map) {
						if(pair.Value.Count > 0) {
							if(pair.Value.Count == 1 && pair.Value.Contains(pair.Key)) {
								root.AddChild(new ReferenceTreeView(pair.Key));
							} else {
								var tree = new TreeViewItem(pair.Key.GetInstanceID(), -1, pair.Key.name) {
									icon = uNodeEditorUtility.GetTypeIcon(pair.Key) as Texture2D
								};
								foreach(var val in pair.Value) {
									tree.AddChild(new ReferenceTreeView(val));
								}
								root.AddChild(tree);
								SetExpanded(tree.id, true);
							}
						}
					}
				} else if(errors != null) {
					var map = new Dictionary<Object, Dictionary<Object, List<uNodeUtility.ErrorMessage>>>();
					foreach(var pair in errors) {
						if(pair.Key == null)
							continue;
						Object owner = pair.Key;
						if(pair.Key is Component comp) {
							if(comp is INode<uNodeRoot> node) {
								owner = node.GetOwner();
							} else {
								owner = comp.transform.root;
							}
						} else if(pair.Key is GameObject go) {
							owner = go.GetComponentInParent<uNodeRoot>();
							if(owner == null) {
								owner = go.transform.root;
							}
						}
						if(owner == null)
							owner = pair.Key;
						if(!map.TryGetValue(owner, out var list)) {
							list = new Dictionary<Object, List<uNodeUtility.ErrorMessage>>();
							map[owner] = list;
						}
						list.Add(pair.Key, pair.Value);
					}
					foreach(var pair in map) {
						if(pair.Value.Count > 0) {
							var tree = new TreeViewItem(pair.Key.GetInstanceID(), -1, pair.Key.name) {
								icon = uNodeEditorUtility.GetTypeIcon(pair.Key) as Texture2D
							};
							foreach(var val in pair.Value) {
								var reference = new ReferenceTreeView(val.Key);
								foreach(var error in val.Value) {
									reference.AddChild(new ErrorTreeView(error));
								}
								tree.AddChild(reference);
								SetExpanded(reference.id, true);
							}
							root.AddChild(tree);
							SetExpanded(tree.id, true);
						}
					}
				}
				if(root.children != null) {
					root.children.Sort((x, y) => string.Compare(x.displayName, y.displayName));
				} else {
					root.children = new List<TreeViewItem>();
				}
				SetupDepthsFromParentsAndChildren(root);
				return root;
			}

			protected override void RowGUI(RowGUIArgs args) {
				Event evt = Event.current;
				if(evt.type == EventType.Repaint) {
					if(args.item is ReferenceTreeView tree) {
						args.label = "";
						Rect labelRect = args.rowRect;
						labelRect.x += GetContentIndent(args.item);
						var style = uNodeGUIStyle.itemNormal;
						bool flag = false;
						foreach(var pair in tree.paths) {
							Texture icon = pair.Value;
							if(flag) {
								uNodeGUIStyle.itemNext.Draw(new Rect(labelRect.x, labelRect.y, 13, 16), GUIContent.none, false, false, false, false);
								labelRect.x += 13;
								labelRect.width -= 13;
							}
							flag = true;
							if(icon != null) {
								GUI.DrawTexture(new Rect(labelRect.x, labelRect.y, 16, 16), icon);
								labelRect.x += 16;
								labelRect.width -= 16;
							}
							var content = new GUIContent(pair.Key);
							style.Draw(labelRect, content, false, false, false, false);
							labelRect.x += style.CalcSize(content).x;
						}
					}
				} else if(evt.type == EventType.MouseDown && args.rowRect.Contains(evt.mousePosition)) {
					if(args.item is ReferenceTreeView reference) {
						if(evt.button == 1) {
							//GenericMenu menu = new GenericMenu();
							//menu.AddItem(new GUIContent("Highlight Node"), false, () => {
							//	uNodeEditor.HighlightNode(errors.Key);
							//});
							//menu.AddItem(new GUIContent("Select Node"), false, () => {
							//	uNodeEditor.ChangeSelection(errors.Key, true);
							//});
							//menu.ShowAsContext();
						} else if(evt.button == 0 && evt.clickCount == 2) {
							if(reference.reference is NodeComponent) {
								uNodeEditor.HighlightNode(reference.reference as NodeComponent);
							} else if(reference.reference is INode<uNodeRoot>) {
								uNodeEditor.Open(reference.reference as INode<uNodeRoot>);
							}
						}
					}
				}
				base.RowGUI(args);
			}
		}

		private static void ShowErrorsInWindow(Dictionary<Object, List<uNodeUtility.ErrorMessage>> errors) {
			if(errors.Count > 0) {
				GUIStyle selectedStyle = new GUIStyle(EditorStyles.label);
				selectedStyle.normal.textColor = Color.white;
				var tree = new ReferenceTree(errors);
				var win = ActionWindow.ShowWindow(() => {
					tree.OnGUI(GUILayoutUtility.GetRect(0, 100000, 0, 100000));
				});
				win.titleContent = new GUIContent("Error Checkers");
			} else {
				uNodeEditorUtility.DisplayMessage("", "No error found.");
			}
		}

		private static void ShowReferencesInWindow(List<Object> references) {
			if(references.Count > 0) {
				GUIStyle selectedStyle = new GUIStyle(EditorStyles.label);
				selectedStyle.normal.textColor = Color.white;
				var tree = new ReferenceTree(references);
				var win = ActionWindow.ShowWindow(() => {
					tree.OnGUI(GUILayoutUtility.GetRect(0, 100000, 0, 100000));
				});
				win.titleContent = new GUIContent("Found: " + references.Count + " references");
			} else {
				uNodeEditorUtility.DisplayMessage("", "No references found.");
			}
		}
		#endregion

		private static List<GraphSystemAttribute> _graphSystems;
		/// <summary>
		/// Find all graph system attributes.
		/// </summary>
		/// <returns></returns>
		public static List<GraphSystemAttribute> FindGraphSystemAttributes() {
			if(_graphSystems == null) {
				_graphSystems = new List<GraphSystemAttribute>();
				foreach(var assembly in EditorReflectionUtility.GetAssemblies()) {
					try {
						foreach(System.Type type in EditorReflectionUtility.GetAssemblyTypes(assembly)) {
							if(type.IsDefined(typeof(GraphSystemAttribute), false)) {
								var menuItem = (GraphSystemAttribute)type.GetCustomAttributes(typeof(GraphSystemAttribute), false)[0];
								menuItem.type = type;
								_graphSystems.Add(menuItem);
							}
						}
					}
					catch { continue; }
				}
				_graphSystems.Sort((x, y) => CompareUtility.Compare(x.menu, x.order, y.menu, y.order));
			}
			return _graphSystems;
		}

		private static List<GraphConverter> _graphConverters;
		/// <summary>
		/// Find all available graph converters
		/// </summary>
		/// <returns></returns>
		public static List<GraphConverter> FindGraphConverters() {
			if(_graphConverters == null) {
				_graphConverters = new List<GraphConverter>();
				foreach(var assembly in EditorReflectionUtility.GetAssemblies()) {
					try {
						foreach(System.Type type in EditorReflectionUtility.GetAssemblyTypes(assembly)) {
							if(!type.IsAbstract && type.IsSubclassOf(typeof(GraphConverter))) {
								var converter = System.Activator.CreateInstance(type, true);
								_graphConverters.Add(converter as GraphConverter);
							}
						}
					}
					catch { continue; }
				}
				_graphConverters.Sort((x, y) => Comparer<int>.Default.Compare(x.order, y.order));
			}
			return _graphConverters;
		}

		/// <summary>
		/// Find all graph in the project
		/// </summary>
		/// <returns></returns>
		public static GameObject[] FindGraphPrefabs() {
			return CachingUtility.FindGraphsInProject();
		}

		/// <summary>
		/// Find all graph in the project
		/// </summary>
		/// <returns></returns>
		public static GameObject[] FindGraphPrefabsWithComponent<T>() {
			return CachingUtility.FindGraphsInProject().Where(g => g != null && g.GetComponent<T>() != null).ToArray();
		}

		/// <summary>
		/// Find all graph with component 'T' in the project
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<T> FindGraphComponents<T>() {
			var result = new List<T>();
			var prefabs = FindGraphPrefabs();
			foreach(var prefab in prefabs) {
				if(prefab == null)
					continue;
				result.AddRange(prefab.GetComponents<T>());
			}
			return result;
		}

		/// <summary>
		/// Find all graph with component type: 'type' in the project
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static List<Component> FindGraphComponents(Type type) {
			var result = new List<Component>();
			var prefabs = FindGraphPrefabs();
			foreach(var prefab in prefabs) {
				if(prefab == null)
					continue;
				result.AddRange(prefab.GetComponents(type));
			}
			return result;
		}

		/// <summary>
		/// Find all graph interface in the project
		/// </summary>
		/// <returns></returns>
		public static List<uNodeInterface> FindGraphInterfaces() {
			return uNodeEditorUtility.FindAssetsByType<uNodeInterface>();
		}

		/// <summary>
		/// Find All uNode Object ( Graph, Interface, etc ) in the projects
		/// </summary>
		/// <returns></returns>
		public static List<Object> FindUNodeObjectsInProject() {
			List<Object> objects = new List<Object>();
			objects.AddRange(FindGraphPrefabs());
			objects.AddRange(FindGraphInterfaces());
			return objects;
		}

		/// <summary>
		/// Get a graph system from a type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static GraphSystemAttribute GetGraphSystem(System.Type type) {
			var graphs = FindGraphSystemAttributes();
			return graphs.FirstOrDefault((g) => g.type == type);
		}

		/// <summary>
		/// Get a graph system from a graph object
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static GraphSystemAttribute GetGraphSystem(uNodeRoot graph) {
			if(graph == null)
				return null;
			return GetGraphSystem(graph.GetType());
		}

		/// <summary>
		/// Save All temporary graph objects into prefabs
		/// </summary>
		/// <param name="destroyRoot"></param>
		public static void SaveAllGraph(bool destroyRoot = true) {
			if(TempGraphManager.managerObject.transform.childCount > 0) {
				foreach(Transform tr in TempGraphManager.managerObject.transform) {
					if(tr.childCount == 0)
						continue;
					int id;
					if(int.TryParse(tr.gameObject.name, out id)) {
						var graph = EditorUtility.InstanceIDToObject(id) as GameObject;
						if(graph != null) {
							SaveGraphAsset(graph, tr.GetChild(0).gameObject);
						}
					}
				}
			}
			if(destroyRoot) {
				Object.DestroyImmediate(TempGraphManager.managerObject);
			}
		}

		public static GameObject GetTempManager() {
			return TempGraphManager.managerObject;
		}

		/// <summary>
		/// Destroy all the temporary graphs
		/// </summary>
		public static void DestroyTempGraph() {
			if(TempGraphManager.managerObject)
				Object.DestroyImmediate(TempGraphManager.managerObject);
		}

		public static void PreventDestroyOnPlayMode() {
			if(Application.isPlaying)
				Object.DontDestroyOnLoad(TempGraphManager.managerObject);
		}

		// static bool hasCompilingInit;
		internal static void Initialize() {
			// EditorBinding.onFinishCompiling += () => {
			// 	if(!hasCompilingInit) {
			// 		SaveAllGraph(false);
			// 		hasCompilingInit = true;
			// 	}
			// };
			CG.OnSuccessGeneratingGraph += (generatedData, settings) => {
				if(settings.isPreview)
					return;
				foreach(var graph in generatedData.graphs) {
					if(generatedData.classNames.TryGetValue(graph, out var className)) {
						graph.graphData.typeName = settings.nameSpace.Add(".") + className;
						uNodeEditorUtility.MarkDirty(graph);//this will ensure the graph will be saved
						if(HasTempGraphObject(graph.gameObject)) {
							var tempGraph = GraphUtility.GetTempGraphObject(graph);
							if(tempGraph != null) {
								tempGraph.graphData.typeName = graph.graphData.typeName;
							}
						}
						// if (!settings.isAsync) { // Skip on generating in background
						// 	graph.graphData.lastCompiled = UnityEngine.Random.Range(1, int.MaxValue);
						// 	graph.graphData.lastSaved = graph.graphData.lastCompiled;
						// }
					}
				}
			};
			EditorBinding.onSceneSaving += (UnityEngine.SceneManagement.Scene scene, string path) => {
				//Save all graph.
				AutoSaveAllGraph(false);
				TempGraphManager.OnSaving();
			};
			EditorBinding.onSceneSaved += (UnityEngine.SceneManagement.Scene scene) => {
				//After scene is saved, back the temp graph flag to hide in hierarchy.
				// TempGraphManager.managerObject.hideFlags = HideFlags.HideInHierarchy;
				uNodeUtility.TempManagement.DestroyTempObjets();
				TempGraphManager.OnSaved();
			};
			EditorBinding.onSceneClosing += (UnityEngine.SceneManagement.Scene scene, bool removingScene) => {
				AutoSaveAllGraph();
				uNodeUtility.TempManagement.DestroyTempObjets();
			};
			EditorBinding.onSceneOpening += (string path, UnityEditor.SceneManagement.OpenSceneMode mode) => {
				AutoSaveAllGraph();
				uNodeUtility.TempManagement.DestroyTempObjets();
			};
			EditorBinding.onSceneOpened += (UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode) => {
				DestroyTempGraph();
				uNodeUtility.TempManagement.DestroyTempObjets();
			};
			EditorApplication.quitting += () => {
				SaveAllGraph(true);
				uNodeUtility.TempManagement.DestroyTempObjets();
				uNodeEditorUtility.SaveEditorData("", "EditorTabData");
			};
		}

		/// <summary>
		/// Find the original object from the temporary object
		/// </summary>
		/// <param name="tempObject"></param>
		/// <returns></returns>
		public static GameObject GetOriginalObject(GameObject tempObject) {
			TemporaryGraph temp = uNodeHelper.GetComponentInParent<TemporaryGraph>(tempObject);
			if(temp != null) {
				return temp.prefab;
			}
			return null;
		}

		/// <summary>
		/// Find the original object from the temporary object
		/// </summary>
		/// <param name="tempObject"></param>
		/// <param name="root"></param>
		/// <returns></returns>
		public static GameObject GetOriginalObject(GameObject tempObject, out Transform root) {
			TemporaryGraph temp = uNodeHelper.GetComponentInParent<TemporaryGraph>(tempObject);
			if(temp != null) {
				root = temp.transform.GetChild(0);
				return temp.prefab;
			}
			root = null;
			return null;
		}

		/// <summary>
		/// Is the graph is temporary object?
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static bool IsTempGraphObject(GameObject graph) {
			return graph != null && graph.transform.root == TempGraphManager.managerObject.transform;
		}

		/// <summary>
		/// Are the graph has temporary objects?
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static bool HasTempGraphObject(GameObject graph) {
			if(graph != null && uNodeEditorUtility.IsPrefab(graph)) {
				var tr = TempGraphManager.managerObject.transform.Find(graph.GetInstanceID().ToString());
				return tr != null && tr.childCount == 1;
			}
			return false;
		}

		/// <summary>
		/// Destroy the temporary graph objects
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static bool DestroyTempGraphObject(GameObject graph) {
			if(graph != null && uNodeEditorUtility.IsPrefab(graph)) {
				var tr = TempGraphManager.managerObject.transform.Find(graph.GetInstanceID().ToString());
				if(tr != null && tr.childCount == 1) {
					Object.DestroyImmediate(tr.gameObject);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Get the temporary graph object and create it if not have
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static GameObject GetTempGraphObject(GameObject graph) {
			if(graph != null && uNodeEditorUtility.IsPrefab(graph)) {
				var tr = TempGraphManager.managerObject.transform.Find(graph.GetInstanceID().ToString());
				if(tr == null) {
					tr = new GameObject(graph.GetInstanceID().ToString()).transform;
					tr.SetParent(TempGraphManager.managerObject.transform);
				}
				if(tr.childCount == 1) {
					return tr.GetChild(0).gameObject;
				}
				GameObject go = PrefabUtility.InstantiatePrefab(graph, tr) as GameObject;
				go.SetActive(false);
				tr.gameObject.AddComponent<TemporaryGraph>().prefab = graph;
				return go;
			}
			return null;
		}

		/// <summary>
		/// Get the temporary graph object and create it if not have
		/// </summary>
		/// <param name="graph"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetTempGraphObject<T>(T graph) where T : uNodeComponentSystem {
			if(graph != null && uNodeEditorUtility.IsPrefab(graph.gameObject)) {
				var temp = GetTempGraphObject(graph.gameObject);
				var comps = temp.GetComponents<T>();
				if(comps.Length == 1) {
					return comps[0];
				}
				for(int i = 0; i < comps.Length; i++) {
					var correspondingObj = uNodeEditorUtility.GetComponentSource(comps[i], graph.gameObject);
					if(correspondingObj != null && correspondingObj == graph) {
						return correspondingObj;
					}
				}
				return comps.Length > 0 ? comps[0] : null;
			}
			return null;
		}

		/// <summary>
		/// Get the temporary graph object and create it if not have
		/// </summary>
		/// <param name="node"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static INode<T> GetTempGraphObject<T>(INode<T> node) where T : uNodeRoot {
			if(node != null && uNodeEditorUtility.IsPrefab(node.GetOwner())) {
				var temp = GetTempGraphObject(node.GetOwner().gameObject);
				var nodeComp = node as Component;
				var obj = uNodeEditorUtility.GetPrefabTransform(nodeComp.transform, node.GetOwner().transform, temp.transform);
				return obj.GetComponent<uNodeComponent>() as INode<T>;
			}
			return null;
		}

		/// <summary>
		/// Get the temporary variable from the graph object
		/// </summary>
		/// <param name="variable"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static VariableData GetTempGraphVariable(string variable, uNodeRoot owner) {
			if(uNodeEditorUtility.IsPrefab(owner.gameObject)) {
				var graph = GetTempGraphObject(owner);
				if(graph == null)
					return null;
				return graph.GetVariableData(variable);
			}
			return null;
		}

		/// <summary>
		/// Save the graph into prefab.
		/// Note: this only work on not in play mode as it for auto save.
		/// </summary>
		/// <param name="graphAsset"></param>
		/// <param name="graph"></param>
		public static void AutoSaveGraph(GameObject graph) {
			if(Application.isPlaying)
				return;
			// EditorUtility.DisplayProgressBar("Saving", "Saving graph assets.", 1);
			SaveGraph(graph);
			// EditorUtility.ClearProgressBar();
		}

		/// <summary>
		/// Save all graphs into prefab.
		/// Note: this only work on not in play mode as it for auto save.
		/// </summary>
		/// <param name="destroyRoot"></param>
		public static void AutoSaveAllGraph(bool destroyRoot = true) {
			if(Application.isPlaying)
				return;
			SaveAllGraph(destroyRoot);
		}

		/// <summary>
		/// Save the runtime graph to a prefab
		/// </summary>
		/// <param name="runtimeGraph"></param>
		/// <param name="graphAsset"></param>
		public static void SaveRuntimeGraph(uNodeRuntime runtimeGraph) {
			if(!Application.isPlaying)
				throw new System.Exception("Saving runtime graph can only be done in playmode");
			if(runtimeGraph.originalGraph == null)
				throw new System.Exception("Cannot save runtime graph because the original graph was null / missing");
			var graph = runtimeGraph.originalGraph;
			if(!EditorUtility.IsPersistent(graph))
				throw new System.Exception("Cannot save graph to unpersistent asset");
			var prefabContent = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(graph));
			var originalGraph = uNodeHelper.GetGraphComponent(prefabContent, graph.GraphName);
			if(originalGraph != null) {
				if(runtimeGraph.RootObject != null) {
					//Duplicate graph data
					var tempRoot = Object.Instantiate(runtimeGraph.RootObject);
					tempRoot.name = "Root";
					//Move graph data to original graph
					tempRoot.transform.SetParent(originalGraph.transform);
					//Retarget graph data owner
					AnalizerUtility.RetargetNodeOwner(runtimeGraph, originalGraph, tempRoot.GetComponentsInChildren<MonoBehaviour>(true));
					if(originalGraph.RootObject != null) {
						//Destroy old graph data
						Object.DestroyImmediate(originalGraph.RootObject);
					}
					//Update graph data to new
					originalGraph.RootObject = tempRoot;
					//Save the graph to prefab
					uNodeEditorUtility.SavePrefabAsset(prefabContent, graph.gameObject);
					//GraphUtility.DestroyTempGraphObject(originalGraph.gameObject);

					//This will update the original graph
					GraphUtility.DestroyTempGraphObject(graph.gameObject);
					//Refresh uNode Editor window
					uNodeEditor.window?.Refresh();
				}
			} else {
				Debug.LogError("Cannot save instanced graph because the cannot find original graph with id:" + graph.GraphName);
			}
			PrefabUtility.UnloadPrefabContents(prefabContent);
		}

		public static void SaveGraph(GameObject graph) {
			if(IsTempGraphObject(graph)) {
				SaveTemporaryGraph(graph);
			} else {
				uNodeEditorUtility.MarkDirty(graph);
			}
		}

		/// <summary>
		/// Save the temporary graph object into the original prefab
		/// </summary>
		/// <param name="graph"></param>
		public static void SaveTemporaryGraph(GameObject graph) {
			var asset = GraphUtility.GetOriginalObject(graph);
			if(asset != null) {
				SaveGraphAsset(asset, graph);
			} else {
				Debug.Log("Cannot save temporary graph: " + graph.name + " because the original graph cannot be found.");
			}
		}

		/// <summary>
		/// Save the temporary graph object into the original prefab
		/// </summary>
		/// <param name="graphAsset"></param>
		/// <param name="graph"></param>
		public static void SaveGraphAsset(GameObject graphAsset, GameObject graph) {
			if(graph != null && graphAsset != null) {
				if(graphAsset.name != graph.name) { //Ensure the name is same.
					graph.name = graphAsset.name;
				}
				{//Reset cache data & update last saved data.
					var roots = (graphAsset as GameObject).GetComponents<uNodeRoot>();
					var tempRoots = (graph as GameObject).GetComponents<uNodeRoot>();
					//Reset cached data
					if(roots.Length != tempRoots.Length) {
						UGraphView.ClearCache();
					} else {
						for(int i = 0; i < roots.Length; i++) {
							if(roots[i].Name != tempRoots[i].Name) {
								UGraphView.ClearCache();
								break;
							}
						}
					}
					//Update last saved data
					//var dateUID = DateTime.Now.GetTimeUID();
					//var scriptData = GenerationUtility.persistenceData.GetGraphData(graphAsset);
					//scriptData.compiledHash = default;
				}
				uNodeEditorUtility.SavePrefabAsset(graph, graphAsset);
				var graphs = (graphAsset as GameObject).GetComponents<uNodeRoot>();
				//Reset the cache data
				foreach(var r in graphs) {
					if(r == null)
						continue;
					var rType = ReflectionUtils.GetRuntimeType(r);
					if(rType is RuntimeGraphType graphType) {
						graphType.RebuildMembers();
					}
				}
			}
		}
	}
}