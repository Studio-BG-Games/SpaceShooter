using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode.Editors {
	public partial class ItemSelector {
		private Data editorData = new Data();

		public Object targetObject;
		public List<CustomItem> customItems = new List<CustomItem>();
		public bool customItemDefaultExpandState = true;
		public bool canSearch = true,
			displayNoneOption = true,
			displayCustomVariable = true,
			showSubItem = false,
			displayDefaultItem = true,
			displayGeneralType = true,
			displayRecentItem = true;
		public Func<List<CustomItem>> favoriteHandler;

		public static int MinWordForDeepTypeSearch {
			get {
				return preferenceData.minDeepTypeSearch;
			}
		}
		public Action<MemberData> selectCallback {
			get => editorData.selectCallback;
			set => editorData.selectCallback = value;
		}
		public HashSet<string> usingNamespaces {
			get => editorData.usingNamespaces;
		}

		#region PrivateFields
		private MemberData reflectionValue;
		private FilterAttribute filter {
			get => editorData.filter;
			set => editorData.filter = value;
		}
		private bool _hasFocus, requiredRepaint;
		#endregion

		#region ShowItems
		static bool IsCorrectItem(ParameterData item, FilterAttribute filter) {
			if (item != null) {
				if (filter != null) {
					return !filter.OnlyGetType && filter.UnityReference;
				}
				return true;
			}
			return false;
		}

		static bool IsCorrectItem(GenericParameterData item, FilterAttribute filter) {
			if (item != null) {
				if (filter != null) {
					if (!(filter.CanSelectType && filter.UnityReference)) {
						return false;
					}
					return filter.IsValidType(typeof(System.Type));
				}
				return true;
			}
			return false;
		}

		static bool IsCorrectItem(uNodeFunction item, FilterAttribute filter) {
			if (item != null) {
				if (filter != null) {
					if (!filter.IsValidType(item.ReturnType()))
						return false;
					if (item.parameters != null) {
						if (filter.MaxMethodParam < item.parameters.Length) {
							return false;
						}
						if (filter.MinMethodParam > item.parameters.Length) {
							return false;
						}
					}
					if (item.genericParameters != null && !filter.DisplayGenericType && item.genericParameters.Length > 0) {
						return false;
					}
					return filter.IsValidTarget(MemberTypes.Method);
				}
				return true;
			}
			return false;
		}
		#endregion

		#region Select
		public void Select(MemberData member) {
			if(targetObject != null) {
				uNodeEditorUtility.RegisterUndo(targetObject, targetObject.name + " edit member : " + reflectionValue.name);
				targetObject = null;
			}
			reflectionValue.CopyFrom(member);
			if(selectCallback != null) {
				selectCallback(reflectionValue);
			}
			Close();
		}

		static bool HasRuntimeType(IList<MemberData> members) {
			for (int i = 0; i < members.Count; i++) {
				var m = members[i];
				if (m.targetType == MemberData.TargetType.uNodeType) {
					return true;
				}
			}
			return false;
		}

		void SelectValues(Type t) {
			if (filter.IsValidType(t)) {
				if (targetObject != null) {
					uNodeEditorUtility.RegisterUndo(targetObject, targetObject.name + " edit member : " + reflectionValue.name);
					targetObject = null;
				}
				MemberData member = MemberData.CreateValueFromType(t);
				reflectionValue.CopyFrom(member);
				if (selectCallback != null) {
					selectCallback(reflectionValue);
				}
				Close();
				Event.current?.Use();
			}
		}

		static bool IsGenericParameter(Type type) {
			return type.IsGenericParameter ||
				type.HasElementType && IsGenericParameter(type.GetElementType()) ||
				type.GetGenericArguments().Any(x => IsGenericParameter(x));
		}

		static bool IsGenericTypeDefinition(Type type) {
			return type.IsGenericTypeDefinition ||
				type.HasElementType && IsGenericTypeDefinition(type.GetElementType()) ||
				type.GetGenericArguments().Any(x => IsGenericTypeDefinition(x));
		}
		#endregion

		#region Others
		public static void SortCustomItems(List<CustomItem> customItems) {
			customItems.Sort((x, y) => {
				int index = string.Compare(x.category, y.category, StringComparison.OrdinalIgnoreCase);
				if (index == 0) {
					return string.Compare(x.name, y.name);
				}
				return index;
			});
		}

		public static List<GraphItem> GetGraphItems(Object target, FilterAttribute filter = null) {
			List<GraphItem> ESItems = new List<GraphItem>();
			var VS = target as IVariableSystem;
			var PS = target as IPropertySystem;
			if (VS != null)
				ESItems.AddRange(VS.Variables.Select(item => new GraphItem(item, target)));
			if (PS != null)
				ESItems.AddRange(PS.Properties.Select(item => new GraphItem(item, target)));
			if (target as uNodeRoot) {
				if (filter == null || !filter.SetMember && filter.ValidMemberType.HasFlags(MemberTypes.Method) && filter.IsValidTarget(MemberTypes.Method)) {
					ESItems.AddRange((target as uNodeRoot).Functions.Where(item => IsCorrectItem(item, filter)).Select(item => new GraphItem(item)));
				}
			}
			ESItems.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
			RemoveIncorrectGraphItem(ESItems, filter);
			return ESItems;
		}

		public static List<CustomItem> MakeExtensionItems(Type type, ICollection<string> ns, FilterAttribute filter, string category = "Data") {
			List<CustomItem> customItems = new List<CustomItem>();
			var assemblies = EditorReflectionUtility.GetAssemblies();
			foreach (var assembly in assemblies) {
				var extensions = EditorReflectionUtility.GetExtensionMethods(assembly, type, (mi) => {
					var nsName = mi.DeclaringType.Namespace;
					return string.IsNullOrEmpty(nsName) || ns.Contains(nsName);
				});
				if (extensions.Count > 0) {
					customItems.AddRange(MakeCustomItems(extensions.Select(item => item as MemberInfo), filter, category));
				}
			}
			customItems.Sort((x, y) => string.Compare(x.name, y.name, StringComparison.OrdinalIgnoreCase));
			return customItems;
		}

		public static List<CustomItem> MakeCustomTypeItems(ICollection<Type> types, string category = "Data") {
			List<EditorReflectionUtility.ReflectionItem> items = new List<EditorReflectionUtility.ReflectionItem>();
			foreach (Type type in types) {
				items.Add(GetItemFromType(type, null));
			}
			return items.Select(i => CustomItem.Create(i.displayName, i, category: category)).ToList();
		}

		public static List<CustomItem> MakeCustomItems(Object target, FilterAttribute filter = null, string category = "Data") {
			if (target == null)
				return null;
			var items = GetGraphItems(target, filter);
			return items.Select(i => CustomItem.Create(i, category: category)).ToList();
		}

		public static List<CustomItem> MakeCustomItems(Type type, FilterAttribute filter = null, string category = "Data", string inheritCategory = "") {
			if(type == null)
				return null;
			if(string.IsNullOrEmpty(inheritCategory)) {
				var items = EditorReflectionUtility.AddGeneralReflectionItems(type, filter);
				items.Sort((x, y) => string.Compare(x.displayName, y.displayName, StringComparison.OrdinalIgnoreCase));
				return items.Select(i => CustomItem.Create(i.displayName, i, category: category)).ToList();
			} else {
				var items = EditorReflectionUtility.AddGeneralReflectionItems(type, filter);
				items.Sort((x, y) => string.Compare(x.displayName, y.displayName, StringComparison.OrdinalIgnoreCase));
				var result = items.Select(i => CustomItem.Create(i.displayName, i, category: category)).ToList();
				var inheritItems = new List<CustomItem>();
				for(int i = 0; i < result.Count; i++) {
					var item = result[i] as ItemReflection;
					if(item.item.memberInfo?.DeclaringType != type) {
						inheritItems.Add(result[i]);
						result.RemoveAt(i);
						i--;
					}
				}
				if(result.Count == 0) {
					result = inheritItems;
				} else {
					for(int i = 0; i < inheritItems.Count; i++) {
						inheritItems[i].category = inheritCategory;
						result.Add(inheritItems[i]);
					}
				}
				return result;
			}
		}

		public static List<CustomItem> MakeCustomItems(IEnumerable<MemberInfo> members, FilterAttribute filter, string category = "Data") {
			var items = EditorReflectionUtility.AddGeneralReflectionItems(null, members, filter);
			items.Sort((x, y) => string.Compare(x.displayName, y.displayName, StringComparison.OrdinalIgnoreCase));
			return items.Select(i => CustomItem.Create(i.displayName, i, category: category)).ToList();
		}

		public static List<CustomItem> MakeCustomItems(Type type, object instance, FilterAttribute filter, string category = "Data") {
			if (type == null)
				return null;
			List<EditorReflectionUtility.ReflectionItem> items = null;
			if (instance is uNodeRoot) {
				items = EditorReflectionUtility.AddGeneralReflectionItems((instance as uNodeRoot).GetInheritType(), new FilterAttribute(filter) { Static = false });
			} else {
				items = EditorReflectionUtility.AddGeneralReflectionItems(type, new FilterAttribute(filter) { Static = false });
			}
			if (items != null) {
				bool flag = false;
				if (instance is uNodeRoot) {
					var root = instance as uNodeRoot;
					var data = root.GetComponent<uNodeData>();
					flag = data == null ? type.Name == root.Name : type.Name == root.Name && data.generatorSettings.Namespace == type.Namespace;
				}
				if (filter != null && !filter.SetMember && (filter.IsValidType(type) || flag) && filter.IsValidTarget(MemberData.TargetType.SelfTarget)) {
					var item = new EditorReflectionUtility.ReflectionItem() {
						canSelectItems = true,
						hasNextItems = false,
						isStatic = false,
						memberInfo = null,
						memberType = type,
						instance = new MemberData("this", type, MemberData.TargetType.SelfTarget) { instance = instance },
					};
					items.Insert(0, item);
				}
				//RemoveIncorrectGeneralItem(TargetType);
				items.RemoveAll(i => i.memberInfo != null && i.memberInfo.MemberType == MemberTypes.Constructor);
				items.Sort((x, y) => string.Compare(x.displayName, y.displayName, StringComparison.OrdinalIgnoreCase));
			}
			items.ForEach(item => { if (item.instance == null) item.instance = instance; });
			return items.Select(i => CustomItem.Create(i.displayName, i, category: category)).ToList();
		}

		public static List<CustomItem> MakeCustomItems(Type type, FilterAttribute filter,
			Func<EditorReflectionUtility.ReflectionItem, bool> validation, string category = "Data") {
			if (type == null)
				return null;
			var items = EditorReflectionUtility.AddGeneralReflectionItems(type, filter);
			items.Sort((x, y) => string.Compare(x.displayName, y.displayName, StringComparison.OrdinalIgnoreCase));
			return items.Where(i => validation(i)).Select(i => CustomItem.Create(i.displayName, i, category: category)).ToList();
		}

		public static List<CustomItem> MakeCustomItemsForInstancedType(Type[] types, Action<object> onClick, bool allowSceneObject) {
			var items = new List<ItemSelector.CustomItem>();
			foreach (var type in types) {
				List<Component> components;
				if(type.IsCastableTo(typeof(uNodeComponentSystem))) {
					components = GraphUtility.FindGraphComponents(type);
				} else {
					components = uNodeEditorUtility.FindComponentInPrefabs(type);
				}
				foreach (var c in components) {
					if (c.GetType().IsCastableTo(type)) {
						items.Add(ItemSelector.CustomItem.Create($"{c.gameObject.name} ({c.GetType().PrettyName()})", onClick, c, "Project", icon: uNodeEditorUtility.GetTypeIcon(c)));
					}
				}
				if (allowSceneObject) {
					var objs = GameObject.FindObjectsOfType<MonoBehaviour>();
					foreach (var c in objs) {
						if (!c.GetType().IsCastableTo(type)) continue;
						items.Add(ItemSelector.CustomItem.Create($"{c.gameObject.name} ({c.GetType().PrettyName()})", onClick, c, "Scene", icon: uNodeEditorUtility.GetTypeIcon(c)));
					}
				}
			}
			return items;
		}

		public static List<CustomItem> MakeCustomItemsForInstancedType<T>(Action<object> onClick, bool allowSceneObject) where T : UnityEngine.Object {
			var items = new List<CustomItem>();
			if (typeof(T).IsCastableTo(typeof(Component))) {
				List<T> components;
				if(typeof(T).IsCastableTo(typeof(uNodeComponentSystem))) {
					components = GraphUtility.FindGraphComponents<T>();
				} else {
					components = uNodeEditorUtility.FindComponentInPrefabs<T>();
				}
				foreach (var c in components) {
					if (c is Component comp) {
						items.Add(ItemSelector.CustomItem.Create($"{comp.gameObject.name} ({c.GetType().PrettyName()})", onClick, c, "Project", icon: uNodeEditorUtility.GetTypeIcon(c)));
					}
				}
				if (allowSceneObject) {
					var objs = GameObject.FindObjectsOfType<T>();
					foreach (var c in objs) {
						items.Add(ItemSelector.CustomItem.Create($"{(c as Component).gameObject.name} ({c.GetType().PrettyName()})", onClick, c, "Scene", icon: uNodeEditorUtility.GetTypeIcon(c)));
					}
				}
			}
			return items;
		}

		public static List<CustomItem> MakeCustomItemsForInstancedType(RuntimeType type, Action<object> onClick, bool allowSceneObject) {
			var items = new List<ItemSelector.CustomItem>();
			var icon = uNodeEditorUtility.GetTypeIcon(type);
			if (type.IsCastableTo(typeof(Component))) {
				string uid = type.Name;
				var components = uNodeEditorUtility.FindComponentInPrefabs<IRuntimeComponent>();
				foreach (var c in components) {
					if (c is uNodeRoot) continue; //Ensure to continue if it's a graph
					if (c.uniqueIdentifier != uid) continue; //Ensure to continue when the identifier is not same
					items.Add(ItemSelector.CustomItem.Create($"{(c as Component).gameObject.name} ({c.GetType().PrettyName()})", onClick, c, "Project", icon: icon));
				}
				if (allowSceneObject) {
					var objs = GameObject.FindObjectsOfType<MonoBehaviour>();
					foreach (var c in objs) {
						if (c is IRuntimeComponent comp) {
							if (comp.uniqueIdentifier != uid) continue; //Ensure to continue when the identifier is not same
							items.Add(ItemSelector.CustomItem.Create($"{c.gameObject.name} ({c.GetType().PrettyName()})", onClick, c, "Scene", icon: icon));
						}
					}
				}
			} else if (type.IsCastableTo(typeof(ScriptableObject))) {
				string uid = type.Name;
				var assets = uNodeEditorUtility.FindAssetsByType<uNodeAssetInstance>();
				foreach (var c in assets) {
					if (c.uniqueIdentifier != uid) continue; //Ensure to continue when the identifier is not same
					items.Add(ItemSelector.CustomItem.Create($"{c.name} ({type.Name})", onClick, c, "Project", icon: icon));
				}
			} else if(type.IsInterface) {
				var components = uNodeEditorUtility.FindComponentInPrefabs<IRuntimeComponent>();
				foreach (var c in components) {
					if (c is uNodeRoot) continue; //Ensure to continue if it's a graph
					if (!ReflectionUtils.GetRuntimeType(c as Component).HasImplementInterface(type)) continue;
					items.Add(ItemSelector.CustomItem.Create($"{(c as Component).gameObject.name} ({c.GetType().PrettyName()})", onClick, c, "Project", icon: icon));
				}
				var assets = uNodeEditorUtility.FindAssetsByType<uNodeAssetInstance>();
				foreach (var c in assets) {
					if (!ReflectionUtils.GetRuntimeType(c).HasImplementInterface(type)) continue;
					items.Add(ItemSelector.CustomItem.Create($"{c.name} ({type.Name})", onClick, c, "Project", icon: icon));
				}
				if (allowSceneObject) {
					var objs = GameObject.FindObjectsOfType<MonoBehaviour>();
					foreach (var c in objs) {
						if (c is IRuntimeComponent comp) {
							if (!ReflectionUtils.GetRuntimeType(c).HasImplementInterface(type)) continue;
							items.Add(ItemSelector.CustomItem.Create($"{c.gameObject.name} ({c.GetType().PrettyName()})", onClick, c, "Scene", icon: icon));
						}
					}
				}
			} else {
				throw new InvalidOperationException();
			}
			return items;
		}

		public static List<TreeViewItem> MakeFavoriteTrees(Func<List<CustomItem>> favoriteHandler, FilterAttribute filter) {
			var result = new List<TreeViewItem>();
			if(favoriteHandler != null) {
				var customItems = favoriteHandler();
				if(customItems != null) {
					List<SelectorCategoryTreeView> categTrees = new List<SelectorCategoryTreeView>();
					foreach(var item in customItems) {
						var categ = categTrees.FirstOrDefault(t => t.category == item.category);
						if(categ == null) {
							categ = new SelectorCategoryTreeView(item.category, "", uNodeEditorUtility.GetUIDFromString("[CATEG]" + item.category), -1);
							categTrees.Add(categ);
						}
						categ.AddChild(new SelectorCustomTreeView(item, item.GetHashCode(), -1));
					}
					categTrees.Sort((x, y) => string.Compare(x.displayName, y.displayName, StringComparison.OrdinalIgnoreCase));
					foreach(var categ in categTrees) {
						result.Add(categ);
					}
				}
			}
			var favorites = uNodeEditor.SavedData.favoriteItems;
			if(favorites != null) {//Favorite Type and Members
				var typeTrees = new List<TypeTreeView>();
				var memberTrees = new List<MemberTreeView>();
				foreach(var fav in favorites) {
					if(fav.info != null && (filter == null || filter.IsValidMember(fav.info))) {
						if(fav.info is Type type) {
							typeTrees.Add(new TypeTreeView(type));
						} else {
							var tree = new MemberTreeView(fav.info);
							tree.displayName = tree.member.DeclaringType.Name + "." + tree.displayName;
							memberTrees.Add(tree);
						}
					}
				}
				typeTrees.Sort((x, y) => string.Compare(x.displayName, y.displayName, StringComparison.OrdinalIgnoreCase));
				memberTrees.Sort((x, y) => string.Compare(x.displayName, y.displayName, StringComparison.OrdinalIgnoreCase));
				var typeCategory = new SelectorCategoryTreeView("Types", "", uNodeEditorUtility.GetUIDFromString("[TYPES]"), -1);
				foreach(var tree in typeTrees) {
					typeCategory.AddChild(tree);
				}
				var memberCategory = new SelectorCategoryTreeView("Members", "", uNodeEditorUtility.GetUIDFromString("[MEMBERS]"), -1);
				foreach(var tree in memberTrees) {
					memberCategory.AddChild(tree);
				}
				if(typeCategory.hasChildren)
					result.Add(typeCategory);
				if(memberCategory.hasChildren)
					result.Add(memberCategory);
			}
			{//Favorite Namespaces
				var nsTrees = new List<NamespaceTreeView>();
				foreach(var fav in uNodeEditor.SavedData.favoriteNamespaces) {
					nsTrees.Add(new NamespaceTreeView(fav, uNodeEditorUtility.GetUIDFromString("[NS-FAV]" + fav), -1));
				}
				nsTrees.Sort((x, y) => string.Compare(x.displayName, y.displayName, StringComparison.OrdinalIgnoreCase));
				var nsCategory = new SelectorCategoryTreeView("Namespaces", "", uNodeEditorUtility.GetUIDFromString("[NS]"), -1);
				foreach(var tree in nsTrees) {
					nsCategory.AddChild(tree);
				}
				if(nsCategory.hasChildren)
					result.Add(nsCategory);
			}
			return result;
		}

		public static void RemoveIncorrectGraphItem(List<GraphItem> RESData, FilterAttribute filter) {
			if (filter == null)
				return;
			for (int i = 0; i < RESData.Count; i++) {
				var var = RESData[i];
				bool canShow = true;
				if (canShow && filter.Types.Count > 0) {
					bool hasType = false;
					if (var.type == null || var.targetType == MemberData.TargetType.SelfTarget)
						continue;
					hasType = filter.IsValidType(var.type);
					if (!hasType) {
						canShow = false;
					}
					if (canShow && var.type != null) {
						if (filter.OnlyArrayType || filter.OnlyGenericType) {
							if (filter.OnlyGenericType && filter.OnlyArrayType) {
								canShow = var.type.IsArray || var.type.IsGenericType;
							} else if (filter.OnlyArrayType) {
								canShow = var.type.IsArray;
							} else if (filter.OnlyGenericType) {
								canShow = var.type.IsGenericType;
							}
						}
					}
					if (!canShow && !var.haveNextItem) {
						RESData.RemoveAt(i);
						i--;
					}
				}
			}
		}

		static EditorReflectionUtility.ReflectionItem GetItemFromType(Type type, FilterAttribute filter) {
			return new EditorReflectionUtility.ReflectionItem() {
				isStatic = true,
				memberInfo = type,
				canSelectItems = filter == null || 
					filter.CanSelectType && filter.IsValidType(type) || 
					filter.IsValidTarget(MemberData.TargetType.Values) && filter.IsValidTypeForValue(type) ||
					filter.Types?.Count == 1 && filter.Types[0] == typeof(Type) && !(type is RuntimeType),
				hasNextItems = true,
				memberType = type,
			};
		}

		static List<Type> GetGeneralTypes() {
			List<Type> type = new List<Type>();
			type.Add(typeof(string));
			type.Add(typeof(float));
			type.Add(typeof(bool));
			type.Add(typeof(int));
			//type.Add(typeof(Enum));
			type.Add(typeof(Color));
			type.Add(typeof(Vector2));
			type.Add(typeof(Vector3));
			type.Add(typeof(Transform));
			type.Add(typeof(GameObject));
			//type.Add(typeof(uNodeRuntime));
			return type;
		}
		#endregion
	}
}