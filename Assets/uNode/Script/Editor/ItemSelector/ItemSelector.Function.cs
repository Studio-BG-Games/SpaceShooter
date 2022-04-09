using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor.IMGUI.Controls;

namespace MaxyGames.uNode.Editors {
	public partial class ItemSelector {
		#region Setup
		void Init() {
			Setup();
			Rect rect = new Rect(new Vector2(preferenceData.itemSelectorWidth, 0), new Vector2(preferenceData.itemSelectorWidth, preferenceData.itemSelectorHeight));
			ShowAsDropDown(rect, new Vector2(preferenceData.itemSelectorWidth, preferenceData.itemSelectorHeight));
			editorData.windowRect = rect;
			wantsMouseMove = true;
			Focus();
		}

		void Setup() {
			if(reflectionValue == null) {
				reflectionValue = MemberData.none;
			}
			if(filter == null) {
				filter = new FilterAttribute() { UnityReference = false };
			}
			if(filter.OnlyGetType) {
				filter.ValidTargetType = MemberData.TargetType.Type | MemberData.TargetType.Null;
			}
			if(targetObject) {
				uNodeRoot UNR = null;
				if(targetObject is uNodeRoot) {
					UNR = targetObject as uNodeRoot;
				} else if(targetObject is RootObject) {
					UNR = (targetObject as RootObject).owner;
				} else if(targetObject is NodeComponent) {
					UNR = (targetObject as NodeComponent).owner;
				} else if(targetObject is TransitionEvent) {
					UNR = (targetObject as TransitionEvent).owner;
				}
				if(UNR) {
					uNodeData data = UNR.GetComponent<uNodeData>();
					if(data) {
						//Clear the default namespace
						usingNamespaces.Clear();
						//Add graph namespaces
						foreach(var n in data.GetNamespaces()) {
							usingNamespaces.Add(n);
						}
					}
					if(UNR is IIndependentGraph graph) {
						if(data == null) {
							//Clear the default namespace
							usingNamespaces.Clear();
						}
						foreach(var n in graph.UsingNamespaces) {
							usingNamespaces.Add(n);
						}
					}
				} else if(targetObject is IIndependentGraph independentGraph) {
					//Clear the default namespace
					usingNamespaces.Clear();
					foreach(var n in independentGraph.UsingNamespaces) {
						usingNamespaces.Add(n);
					}
				}
			}
			editorData.manager = new Manager(new TreeViewState());
			editorData.manager.window = this;
			editorData.searchField = new SearchField();
			editorData.searchField.downOrUpArrowKeyPressed += editorData.manager.SetFocusAndEnsureSelectedItem;
			editorData.searchField.autoSetFocusOnFindCommand = true;
			window = this;
			uNodeThreadUtility.Queue(DoSetup);
		}

		public List<TreeViewItem> CustomTrees { get; set; }

		void DoSetup() {
			editorData.setup.Setup((progress) => {
				if(progress == 1) {
					uNodeThreadUtility.Queue(() => {
						UnityEngine.Profiling.Profiler.BeginSample("AAA");
						var categories = new List<TreeViewItem>();
						if(CustomTrees != null) {
							foreach(var tree in CustomTrees) {
								categories.Add(tree);
							}
						}
						if(displayDefaultItem) {
							var categoryTree = new SelectorCategoryTreeView("#", "", uNodeEditorUtility.GetUIDFromString("[CATEG]#"), -1);
							categories.Add(categoryTree);
							var recentTree = new SelectorCategoryTreeView("Recently", "", uNodeEditorUtility.GetUIDFromString("[CATEG]#Recently"), -1);
							recentTree.hideOnSearch = true;
							categories.Add(recentTree);
							if(displayNoneOption && filter.IsValidTarget(MemberData.TargetType.None)) {
								categoryTree.AddChild(new SelectorMemberTreeView(MemberData.none, "None", uNodeEditorUtility.GetUIDFromString("#None")) {
									icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.NullTypeIcon)) as Texture2D
								});
							}
							if(!filter.SetMember) {
								if(!filter.IsValueTypes() && filter.IsValidTarget(MemberData.TargetType.Null) && !filter.OnlyGetType) {
									categoryTree.AddChild(new SelectorMemberTreeView(MemberData.Null, "Null", uNodeEditorUtility.GetUIDFromString("#Null")) {
										icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.NullTypeIcon)) as Texture2D
									});
								}
								//if(!filter.OnlyGetType && filter.IsValidTarget(MemberData.TargetType.Values) &&
								//	(filter.Types == null || filter.Types.Count != 1 || filter.Types[0] != typeof(Type))) {
								//	categoryTree.AddChild(new SelectorCallbackTreeView((cRect) => {
								//		var screenRect = cRect.ToScreenRect();
								//		FilterAttribute F = new FilterAttribute(filter);
								//		F.OnlyGetType = true;
								//		ItemSelector w = null;
								//		Action<MemberData> action = delegate (MemberData m) {
								//			if(w != null) {
								//				w.Close();
								//				//EditorGUIUtility.ExitGUI();
								//			}
								//			if(filter.CanManipulateArray()) {
								//				if(Event.current.button == 0) {
								//					TypeSelectorWindow.ShowAsNew(Rect.zero, F, delegate (MemberData[] members) {
								//						Type t = members[0].Get<Type>();
								//						SelectValues(t);
								//					}, m).ChangePosition(screenRect);
								//				} else {
								//					CommandWindow.CreateWindow(screenRect, (items) => {
								//						var member = CompletionEvaluator.CompletionsToMemberData(items);
								//						if(member != null) {
								//							Type t = member.Get<Type>();
								//							SelectValues(t);
								//							return true;
								//						}
								//						return false;
								//					}, new CompletionEvaluator.CompletionSetting() {
								//						validCompletionKind = CompletionKind.Type | CompletionKind.Namespace | CompletionKind.Keyword,
								//					});
								//				}
								//			} else {
								//				Type t = m.Get<Type>();
								//				SelectValues(t);
								//			}
								//		};
								//		w = ShowAsNew(targetObject, F, action, true).ChangePosition(screenRect);
								//	}, "Values", uNodeEditorUtility.GetUIDFromString("#Values"), -1) {
								//		icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.ValueIcon)) as Texture2D
								//	});
								//}
							}
							if(displayRecentItem) {
								var listRecentItems = new List<TreeViewItem>();
								if(uNodeEditor.SavedData.recentItems != null) {
									foreach(var recent in uNodeEditor.SavedData.recentItems) {
										if(recent != null && recent.info != null) {
											if(recent.info is Type) {
												listRecentItems.Add(new TypeTreeView(recent.info as Type, recent.GetHashCode(), -1) { filter = filter });
											} else if(!filter.OnlyGetType && (recent.isStatic || filter.DisplayInstanceOnStatic)) {
												listRecentItems.Add(new MemberTreeView(recent.info, recent.GetHashCode(), -1));
											}
										}
									}
								}
								while(listRecentItems.Count > 10) {
									listRecentItems.RemoveAt(listRecentItems.Count - 1);
								}
								if(listRecentItems.Count > 0) {
									foreach(var item in listRecentItems) {
										if(item is MemberTreeView) {
											var tree = item as MemberTreeView;
											if(!(tree.member is Type)) {
												tree.displayName = tree.member.DeclaringType.Name + "." + tree.displayName;
											}
										}
										recentTree.AddChild(item);
									}
									recentTree.expanded = false;
								}
							}
							if(filter.UnityReference) {
								categories.AddRange(TreeFunction.CreateRootItem(targetObject, filter));
							}
							if(!filter.OnlyGetType && filter.UnityReference) {
								if(reflectionValue != null && reflectionValue.GetInstance() != null && !(reflectionValue.GetInstance() is IGraphSystem) && !(reflectionValue.GetInstance() is INode)) {
									categories.Add(TreeFunction.CreateTargetItem(reflectionValue.GetInstance(), "Target Reference", filter));
								}
								categories.AddRange(TreeFunction.CreateGraphItem(targetObject, reflectionValue, filter));
							}
							categories.AddRange(TreeFunction.CreateCustomItem(customItems, expanded: customItemDefaultExpandState));
							if(filter.DisplayDefaultStaticType) {
								categoryTree.AddChild(new SelectorGroupedTreeView(() => {
									var result = new List<TreeViewItem>();
									result.Add(new SelectorSearchTreeView((prog) => {
										var treeResult = new List<TreeViewItem>();
										var sp = new SearchProgress();
										prog?.Invoke(sp);
										var allTypes = GetAllTypes((currProgress) => {
											prog?.Invoke(currProgress);
										}, true, true);
										sp.info = "Setup Items";
										for(int i = 0; i < allTypes.Count; i++) {
											var pair = allTypes[i];
											var nsTree = new SelectorCategoryTreeView(pair.Key, "", uNodeEditorUtility.GetUIDFromString("[CATEG-SEARCH]" + pair.Key), -1);
											foreach(var type in pair.Value) {
												nsTree.AddChild(new TypeTreeView(type, type.GetHashCode(), -1));
											}
											treeResult.Add(nsTree);
											sp.progress = (float)i / (float)allTypes.Count;
											prog?.Invoke(sp);
										}
										return treeResult;
									}, "Search All Types", uNodeEditorUtility.GetUIDFromString("[SAT]"), -1));
									var nestedNS = new HashSet<string>();
									//var excludedNs = uNodePreference.GetExcludedNamespace();
									var namespaces = new List<string>(EditorReflectionUtility.GetNamespaces());
									namespaces.Sort();
									namespaces.RemoveAll(i => /*excludedNs.Contains(i) ||*/ i == null || i.Contains("."));
									foreach(var ns in namespaces) {
										result.Add(new NamespaceTreeView(ns, uNodeEditorUtility.GetUIDFromString("[N]" + ns), -1));
									}
									//var nsTypes = GetNamespaceTypes(namespaces);
									//foreach(var pair in nsTypes) {
									//	var nsTree = new SelectorCategoryTreeView(pair.Key, "", uNodeEditorUtility.GetUIDFromString("[Nested-NS]" + pair.Key), -1);
									//	foreach(var ns in nestedNS) {
									//		if(ns.StartsWith(pair.Key)) {
									//			nsTree.AddChild(new NamespaceTreeView(ns, uNodeEditorUtility.GetUIDFromString("[N]" + ns), -1));
									//		}
									//	}
									//	foreach(var type in pair.Value) {
									//		nsTree.AddChild(new TypeTreeView(type, type.GetHashCode(), -1));
									//	}
									//	//nsTree.children.Sort((x, y) => string.Compare(x.displayName, y.displayName, StringComparison.Ordinal));
									//	nsTree.expanded = false;
									//	result.Add(nsTree);
									//}
									return result;
								}, "All Namespaces", uNodeEditorUtility.GetUIDFromString("[ALL-NS]"), -1) {
									icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.NamespaceIcon)) as Texture2D
								});
								categoryTree.AddChild(new SelectorGroupedTreeView(() => {
									return MakeFavoriteTrees(favoriteHandler, window.filter);
								}, "Favorites", uNodeEditorUtility.GetUIDFromString("[fav]"), -1) {
									icon = uNodeGUIStyle.favoriteIconOn
								});
								var namespaceTrees = new SelectorCategoryTreeView("Types", "", uNodeEditorUtility.GetUIDFromString("[NS]"), -1);
								if(displayGeneralType) {
									var categTree = new SelectorCategoryTreeView("General", "", uNodeEditorUtility.GetUIDFromString("[GENERAL]"), -1);
									var items = TreeFunction.GetGeneralTrees();
									items.ForEach(tree => categTree.AddChild(tree));
									namespaceTrees.AddChild(categTree);
								}
								if(filter.DisplayRuntimeType) {
									var runtimeItems = TreeFunction.GetRuntimeItems();
									var runtimeTypes = new Dictionary<string, List<TypeTreeView>>();
									foreach(var item in runtimeItems) {
										var ns = item.type.Namespace;
										if(string.IsNullOrEmpty(ns) || ns == RuntimeType.RuntimeNamespace) {
											ns = "Generated Type";
										}
										List<TypeTreeView> list;
										if(!runtimeTypes.TryGetValue(ns, out list)) {
											list = new List<TypeTreeView>();
											runtimeTypes[ns] = list;
										}
										list.Add(item);
									}
									foreach(var pair in runtimeTypes) {
										var categTree = new SelectorCategoryTreeView(pair.Key, "", uNodeEditorUtility.GetUIDFromString("[RT]" + pair.Key), -1);
										var items = pair.Value;
										items.ForEach(tree => categTree.AddChild(tree));
										namespaceTrees.AddChild(categTree);
									}
								}
								var typeList = editorData.setup.typeList;
								foreach(var pair in typeList) {
									var nsTree = new SelectorCategoryTreeView(pair.Key, "", uNodeEditorUtility.GetUIDFromString("[CATEG]" + pair.Key), -1);
									foreach(var type in pair.Value) {
										nsTree.AddChild(new TypeTreeView(type, type.GetHashCode(), -1));
									}
									namespaceTrees.AddChild(nsTree);
								}
								categories.Add(namespaceTrees);
							}
						} else {
							categories.AddRange(TreeFunction.CreateCustomItem(customItems, customItemDefaultExpandState));
						}
						categories.RemoveAll(tree => tree == null || !tree.hasChildren);
						if(displayDefaultItem) {
							categories.Insert(0, new SelectorSearchTreeView((prog) => {
								var treeResult = new List<TreeViewItem>();
								var sp = new SearchProgress();
								prog?.Invoke(sp);
								var namespaces = uNodeEditor.SavedData.favoriteNamespaces;
								var allTypes = GetNamespaceTypes(namespaces, (currProgress) => {
									prog?.Invoke(new SearchProgress() { progress = currProgress, info = "Searching on favorite namespaces" });
								}, ignoreIncludedAssemblies: true);
								sp.info = "Setup Items";
								for(int i = 0; i < allTypes.Count; i++) {
									var pair = allTypes[i];
									var nsTree = new SelectorCategoryTreeView(pair.Key, "", uNodeEditorUtility.GetUIDFromString("[FAV-NS-SEARCH]" + pair.Key), -1);
									foreach(var type in pair.Value) {
										nsTree.AddChild(new TypeTreeView(type, type.GetHashCode(), -1));
									}
									treeResult.Add(nsTree);
									sp.progress = (float)i / (float)allTypes.Count;
									prog?.Invoke(sp);
								}
								return treeResult;
							}, "Search On Favorite Namespaces", uNodeEditorUtility.GetUIDFromString("[SAT]"), -1));
						}
						editorData.manager.Reload(categories);
						hasSetupMember = true;
						requiredRepaint = true;
						UnityEngine.Profiling.Profiler.EndSample();
					});
				} else {
					requiredRepaint = true;
				}
			}, this);
		}
		#endregion

		static List<KeyValuePair<string, List<Type>>> GetNamespaceTypes(IEnumerable<string> namespaces, Action<float> onProgress = null, bool includeGlobal = false, bool includeExcludedType = false, bool ignoreIncludedAssemblies = false) {
			onProgress?.Invoke(0);
			Dictionary<string, List<Type>> typeMaps = new Dictionary<string, List<Type>>();
			var typeList = new List<KeyValuePair<string, List<Type>>>();
			var preference = uNodePreference.preferenceData;
			var excludedTypes = uNodePreference.GetExcludedTypes();
			var assemblies = EditorReflectionUtility.GetAssemblies();
			var includedAssemblies = preference.includedAssemblies;
			var excludedNamespaces = uNodePreference.GetExcludedNamespace();
			for(int i = 0; i < assemblies.Length; i++) {
				var assembly = assemblies[i];
				string assemblyName = assembly.GetName().Name;
				bool isIncluded = includedAssemblies.Contains(assemblyName);
				foreach(var type in EditorReflectionUtility.GetAssemblyTypes(assembly)) {
					string ns = type.Namespace;
					if(string.IsNullOrEmpty(ns)) {
						ns = "global";
						if(!includeGlobal && !namespaces.Contains(ns) || !ignoreIncludedAssemblies && !isIncluded) {
							continue;
						}
					} else {
						if(!namespaces.Contains(ns)) {
							if(ignoreIncludedAssemblies) {
								continue;
							} else if(!isIncluded || excludedNamespaces.Contains(ns)) {
								continue;
							}
						}
					}
					if(type.IsNotPublic ||
						!type.IsVisible ||
						//type.IsEnum ||
						//type.IsInterface ||
						type.IsCOMObject ||
						type.IsAutoClass ||
						//type.IsGenericType ||
						type.Name.StartsWith("<", StringComparison.Ordinal) ||
						type.IsNested ||
						//type.IsCastableTo(typeof(Delegate)) ||
						!preference.showObsoleteItem && type.IsDefined(typeof(ObsoleteAttribute), true) ||
						!includeExcludedType && excludedTypes.Contains(type.FullName))
						continue;
					//if(excludedNS.Contains(ns)) {
					//	continue;
					//}
					List<Type> types;
					if(!typeMaps.TryGetValue(ns, out types)) {
						types = new List<Type>();
						typeMaps[ns] = types;
					}
					types.Add(type);
					//typeCount++;
				}
				onProgress?.Invoke((float)i / (float)assemblies.Length);
			}
			typeList = typeMaps.ToList();
			foreach(var list in typeList) {
				list.Value.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
			}
			typeList.Sort((x, y) => {
				if(x.Key == "global") {
					if(y.Key == "global") {
						return 0;
					}
					return -1;
				} else if(y.Key == "global") {
					return 1;
				}
				//if(x.Key.StartsWith("Unity", StringComparison.Ordinal)) {
				//	if(y.Key.StartsWith("Unity", StringComparison.Ordinal)) {
				//		return string.Compare(x.Key, y.Key, StringComparison.Ordinal);
				//	}
				//	return -1;
				//} else if(y.Key.StartsWith("Unity", StringComparison.Ordinal)) {
				//	return 1;
				//}
				return string.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase);
			});
			onProgress?.Invoke(1);
			return typeList;
		}

		static List<KeyValuePair<string, List<Type>>> GetAllTypes(Action<SearchProgress> onProgress = null, bool includeGlobal = false, bool includeExcludedType = false) {
			var progress = new SearchProgress();
			onProgress?.Invoke(progress);
			Dictionary<string, List<Type>> typeMaps = new Dictionary<string, List<Type>>();
			var typeList = new List<KeyValuePair<string, List<Type>>>();
			var preference = uNodePreference.preferenceData;
			var excludedTypes = uNodePreference.GetExcludedTypes();
			var assemblies = EditorReflectionUtility.GetAssemblies();
			for(int i = 0; i < assemblies.Length; i++) {
				var assembly = assemblies[i];
				string assemblyName = assembly.GetName().Name;
				foreach(var type in EditorReflectionUtility.GetAssemblyTypes(assembly)) {
					string ns = type.Namespace;
					if(string.IsNullOrEmpty(ns)) {
						ns = "global";
						if(!includeGlobal) {
							continue;
						}
					}
					if(type.IsNotPublic ||
						!type.IsVisible ||
						//type.IsEnum ||
						//type.IsInterface ||
						type.IsCOMObject ||
						type.IsAutoClass ||
						//type.IsGenericType ||
						type.Name.StartsWith("<", StringComparison.Ordinal) ||
						type.IsNested ||
						//type.IsCastableTo(typeof(Delegate)) ||
						!preference.showObsoleteItem && type.IsDefined(typeof(ObsoleteAttribute), true) ||
						!includeExcludedType && excludedTypes.Contains(type.FullName))
						continue;
					//if(excludedNS.Contains(ns)) {
					//	continue;
					//}
					List<Type> types;
					if(!typeMaps.TryGetValue(ns, out types)) {
						types = new List<Type>();
						typeMaps[ns] = types;
					}
					types.Add(type);
					//typeCount++;
				}
				progress.progress = (float)i / (float)assemblies.Length;
				progress.info = "Get Types on:" + assemblyName;
				onProgress?.Invoke(progress);
			}
			typeList = typeMaps.ToList();
			foreach(var list in typeList) {
				list.Value.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
			}
			typeList.Sort((x, y) => {
				if(x.Key == "global") {
					if(y.Key == "global") {
						return 0;
					}
					return -1;
				} else if(y.Key == "global") {
					return 1;
				}
				//if(x.Key.StartsWith("Unity", StringComparison.Ordinal)) {
				//	if(y.Key.StartsWith("Unity", StringComparison.Ordinal)) {
				//		return string.Compare(x.Key, y.Key, StringComparison.Ordinal);
				//	}
				//	return -1;
				//} else if(y.Key.StartsWith("Unity", StringComparison.Ordinal)) {
				//	return 1;
				//}
				return string.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase);
			});
			progress.progress = 1;
			onProgress?.Invoke(progress);
			return typeList;
		}
	}
}