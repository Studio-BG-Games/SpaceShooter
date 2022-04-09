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
		#region TreeView
		internal class SelectorSearchTreeView : TreeViewItem {
			public Func<Action<SearchProgress>, List<TreeViewItem>> treeViews;

			public SelectorSearchTreeView() {

			}

			public SelectorSearchTreeView(Func<Action<SearchProgress>, List<TreeViewItem>> treeViews, string displayName, int id, int depth) : base(id, depth, displayName) {
				this.treeViews = treeViews;
			}
		}

		public class SelectorCategoryTreeView : TreeViewItem {
			public string category;
			public string description;
			public bool hideOnSearch;

			internal List<TreeViewItem> childTrees;

			private bool isExpanded = true;
			public bool expanded {
				get => isExpanded;
				set {
					if(value) {
						if(childTrees != null) {
							foreach(var tree in childTrees) {
								AddChild(tree);
							}
							childTrees = null;
						}
					} else if(childTrees == null) {
						childTrees = children;
						children = new List<TreeViewItem>();
					}
					isExpanded = value;
				}
			}

			public override bool hasChildren {
				get {
					if(expanded) {
						return base.hasChildren;
					}
					return childTrees != null && childTrees.Count > 0;
				}
			}
			public SelectorCategoryTreeView() {

			}

			public SelectorCategoryTreeView(string category, int id, int depth) : base(id, depth, category) {
				this.category = category;
			}

			public SelectorCategoryTreeView(string category, string description, int id, int depth) : base(id, depth, category) {
				this.category = category;
				this.description = description;
			}
		}

		internal class SelectorNamespaceTreeView : TreeViewItem {
			public string Namespace;

			public SelectorNamespaceTreeView() {

			}

			public SelectorNamespaceTreeView(string Namespace, int id, int depth) : base(id, depth, Namespace) {
				this.Namespace = Namespace;
			}
		}

		internal class SelectorGroupedTreeView : TreeViewItem {
			public Func<List<TreeViewItem>> treeViews;

			public SelectorGroupedTreeView() {

			}

			public SelectorGroupedTreeView(Func<List<TreeViewItem>> treeViews, string displayName, int id, int depth) : base(id, depth, displayName) {
				this.treeViews = treeViews;
			}
		}

		internal class SelectorCallbackTreeView : TreeViewItem {
			public Action<Rect> onSelect;
			//public Action<Rect> onNext;
			//public Action<bool> onExpandChange;

			public SelectorCallbackTreeView() {

			}

			public SelectorCallbackTreeView(Action<Rect> onSelect, string displayName, int id, int depth) : base(id, depth, displayName) {
				this.onSelect = onSelect;
			}
		}

		internal class SelectorCustomTreeView : TreeViewItem {
			public readonly CustomItem item;
			public readonly GraphItem graphItem;

			public SelectorCustomTreeView() {

			}

			public SelectorCustomTreeView(CustomItem item, int id, int depth) : base(id, depth, item.name) {
				this.item = item;
				icon = item.GetIcon() as Texture2D ?? uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.ExtensionIcon)) as Texture2D;
			}

			public SelectorCustomTreeView(GraphItem item, int id, int depth) : base(id, depth, item.DisplayName) {
				this.graphItem = item;
				switch(item.targetType) {
					case MemberData.TargetType.SelfTarget:
						icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.KeywordIcon)) as Texture2D;
						break;
					case MemberData.TargetType.uNodeVariable:
						icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FieldIcon)) as Texture2D;
						break;
					case MemberData.TargetType.uNodeLocalVariable:
					case MemberData.TargetType.uNodeGroupVariable:
						icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.LocalVariableIcon)) as Texture2D;
						break;
					case MemberData.TargetType.uNodeProperty:
						icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.PropertyIcon)) as Texture2D;
						break;
					case MemberData.TargetType.uNodeConstructor:
					case MemberData.TargetType.uNodeFunction:
						icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.MethodIcon)) as Texture2D;
						break;
					case MemberData.TargetType.uNodeParameter:
					case MemberData.TargetType.uNodeGenericParameter:
						icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.LocalVariableIcon)) as Texture2D;
						break;
					default:
						icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.KeywordIcon)) as Texture2D;
						break;
				}
			}
		}

		internal class SelectorMemberTreeView : TreeViewItem {
			public MemberData member;

			public SelectorMemberTreeView() {

			}

			public SelectorMemberTreeView(MemberData member, string displayName, int id) : base(id, -1, displayName) {
				this.member = member;
			}
		}

		//public abstract class SelectorTreeView : TreeViewItem {
		//	public abstract void OnSelect();
		//	public abstract void OnNext();
		//}
		#endregion

		#region Enum
		enum SearchKind {
			Contains,
			Startwith,
			Equal,
			Endwith,
		}

		enum SearchFilter {
			All,
			Function,
			Variable,
			Property,
			Type,
		}
		#endregion

		class Data : IDisposable {
			public Rect windowRect;
			public Action<MemberData> selectCallback;

			public List<TreeViewItem> items = new List<TreeViewItem>();
			public FilterAttribute filter;
			public HashSet<string> usingNamespaces = new HashSet<string>() { "UnityEngine" };

			public SearchField searchField;
			public string searchString {
				get => manager.searchString;
				set => manager.searchString = value;
			}
			public SearchFilter searchFilter = SearchFilter.All;

			private SearchKind m_searchKind = SearchKind.Contains;
			private SearchKind m_deepSearchKind = SearchKind.Contains;
			public SearchKind searchKind {
				get {
					if(manager != null && manager.isDeep) {
						return m_deepSearchKind;
					}
					return m_searchKind;
				}
				set {
					if(manager != null && manager.isDeep) {
						m_deepSearchKind = value;
						uNodePreference.preferenceData.m_itemDeepSearchKind = (int)value;
						uNodePreference.SavePreference();
					} else {
						m_searchKind = value;
						uNodePreference.preferenceData.m_itemSearchKind = (int)value;
						uNodePreference.SavePreference();
					}
				}
			}

			public Manager manager;

			public DataSetup setup = new DataSetup();

			public Data() {
				m_searchKind = (SearchKind)uNodePreference.preferenceData.m_itemSearchKind;
				m_deepSearchKind = (SearchKind)uNodePreference.preferenceData.m_itemSearchKind;
			}

			public void Dispose() {
				setup.Dispose();
				manager?.Dispose();
			}
		}

		class DataSetup : IDisposable {
			public List<KeyValuePair<string, List<Type>>> typeList;
			private Thread setupThread;

			public bool isFinished => progress == 1;
			public float progress;
			public Data data;

			public void Setup(Action<float> onProgress, ItemSelector window) {
				if(!window.filter.DisplayDefaultStaticType || !window.displayDefaultItem) {
					onProgress?.Invoke(1);
					return;
				}
				this.data = window.editorData;
				if(setupThread != null) {
					setupThread.Abort();
					setupThread.Join(500);
					setupThread = null;
				}
				setupThread = new Thread(() => SetupProgress(onProgress));
				setupThread.Priority = System.Threading.ThreadPriority.Highest;
				setupThread.Start();
			}

			void SetupProgress(Action<float> onProgress) {
				typeList = GetNamespaceTypes(data.usingNamespaces, (p) => {
					progress = p;
					onProgress?.Invoke(p);
				}, includeGlobal: true);
			}

			public void Dispose() {
				if(setupThread != null) {
					setupThread.Abort();
					setupThread.Join(500);
					setupThread = null;
				}
			}
		}

		public class GraphItem {
			public string Name;
			public string DisplayName;
			public VariableData variable;
			public uNodeProperty property;
			public uNodeFunction function;
			public MemberData.TargetType targetType;
			public Type type;
			public bool onlyGet;
			public bool haveNextItem = true;
			public string toolTip;
			public Object targetObject;
			public ParameterData parameter;
			public GenericParameterData genericParameter;
			public MemberData[] genericParameterTypes;

			public GraphItem(GraphItem other) {
				this.Name = other.Name;
				this.DisplayName = other.DisplayName;
				this.variable = other.variable;
				this.function = other.function;
				this.targetType = other.targetType;
				this.type = other.type;
				this.onlyGet = other.onlyGet;
				this.haveNextItem = other.haveNextItem;
				this.toolTip = other.toolTip;
				this.targetObject = other.targetObject;
				this.parameter = other.parameter;
				this.genericParameter = other.genericParameter;
				this.genericParameterTypes = other.genericParameterTypes;
			}

			public GraphItem(uNodeRoot targetRoot) {
				this.Name = "this";
				this.targetType = MemberData.TargetType.SelfTarget;
				this.type = targetRoot.GetType();
				this.targetObject = targetRoot;
				this.DisplayName = this.Name;
				this.haveNextItem = false;
			}

			public GraphItem(VariableData variable, Object targetObject, MemberData.TargetType targetType = MemberData.TargetType.uNodeVariable) {
				this.Name = variable.Name;
				this.onlyGet = variable.onlyGet;
				this.targetType = targetType;
				this.type = variable.Type;
				this.variable = variable;
				this.targetObject = targetObject;
				this.DisplayName = this.Name;
			}

			public GraphItem(uNodeProperty property, Object targetObject, MemberData.TargetType targetType = MemberData.TargetType.uNodeProperty) {
				this.Name = property.Name;
				this.onlyGet = property.CanGetValue() && !property.CanSetValue();
				this.targetType = targetType;
				this.type = property.ReturnType();
				this.property = property;
				this.targetObject = targetObject;
				this.DisplayName = this.Name;
			}

			public GraphItem(ParameterData parameter, Object targetObject, MemberData.TargetType targetType = MemberData.TargetType.uNodeParameter) {
				this.Name = parameter.name;
				this.onlyGet = false;
				this.haveNextItem = true;
				this.targetType = targetType;
				this.type = parameter.Type;
				this.parameter = parameter;
				this.targetObject = targetObject;
				this.DisplayName = /*parameter.type.DisplayName(false, false) + " " +*/ this.Name;
			}

			public GraphItem(GenericParameterData parameter, Object targetObject, MemberData.TargetType targetType = MemberData.TargetType.uNodeGenericParameter) {
				this.Name = parameter.name;
				this.onlyGet = true;
				this.haveNextItem = false;
				this.targetType = targetType;
				this.genericParameter = parameter;
				this.targetObject = targetObject;
				this.DisplayName = this.Name;
			}

			public GraphItem(uNodeFunction function, MemberData.TargetType targetType = MemberData.TargetType.uNodeFunction) {
				this.Name = function.Name;
				this.onlyGet = true;
				this.targetType = targetType;
				this.type = function.ReturnType();
				this.function = function;
				this.haveNextItem = false;
				this.targetObject = function.owner;
				this.DisplayName = this.Name;
				if(type != null) {
					if(function != null) {
						string parameterData = "";
						if(function.genericParameters.Length > 0) {
							parameterData += "<";
							for(int g = 0; g < function.genericParameters.Length; g++) {
								if(g != 0) {
									parameterData += ", ";
								}
								parameterData += function.genericParameters[g].name;
							}
							parameterData += ">";
						}
						parameterData += "(";
						for(int g = 0; g < function.parameters.Length; g++) {
							if(g != 0) {
								parameterData += ", ";
							}
							parameterData += function.parameters[g].type.DisplayName(false, false) + " " + function.parameters[g].name;
						}
						parameterData += ")";
						this.DisplayName += parameterData;
					}
				}
			}
		}

		public interface IFavoritable {
			bool IsFavorited();
			void SetFavorite(bool value);
			bool CanSetFavorite();
		}

		class ItemNode : CustomItem, IFavoritable {
			private Action action;
			private NodeMenu menu;

			public ItemNode(NodeMenu menu, Action action) {
				name = menu.name;
				category = menu.category.Replace("/", ".");
				tooltip = new GUIContent(menu.tooltip);
				this.action = action;
				this.menu = menu;
			}

			public bool CanSetFavorite() {
				return menu.type != null;
			}

			public bool IsFavorited() {
				return uNodeEditor.SavedData.HasFavorite("NODES", menu.type.FullName);
			}

			public override void OnSelect(ItemSelector selector) {
				action();
			}

			public void SetFavorite(bool value) {
				if(value) {
					uNodeEditor.SavedData.AddFavorite("NODES", menu.type.FullName, null);
				} else {
					uNodeEditor.SavedData.RemoveFavorite("NODES", menu.type.FullName);
				}
			}
		}

		class ItemReflection : CustomItem, IFavoritable {
			public EditorReflectionUtility.ReflectionItem item;

			public override bool CanSelect(ItemSelector selector) {
				return item.canSelectItems;
			}

			public bool CanSetFavorite() {
				return item != null && item.memberInfo != null;
			}

			public override Texture GetIcon() {
				switch(item.targetType) {
					case MemberData.TargetType.Field:
					case MemberData.TargetType.uNodeVariable:
					case MemberData.TargetType.uNodeLocalVariable:
					case MemberData.TargetType.uNodeGroupVariable:
						return uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FieldIcon));
					case MemberData.TargetType.Property:
					case MemberData.TargetType.uNodeProperty:
						return uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.PropertyIcon));
					case MemberData.TargetType.Method:
					case MemberData.TargetType.uNodeFunction:
						return uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.MethodIcon));
				}
				return icon;
			}

			public override Texture GetSecondaryIcon() {
				return uNodeEditorUtility.GetTypeIcon(item.memberType);
			}

			public bool IsFavorited() {
				return uNodeEditor.SavedData.HasFavorite(item.memberInfo);
			}

			public void SetFavorite(bool value) {
				if(value) {
					uNodeEditor.SavedData.AddFavorite(item.memberInfo);
				} else {
					uNodeEditor.SavedData.RemoveFavorite(item.memberInfo);
				}
			}
		}

		class ItemClickable : CustomItem {
			public Action<object> onClick;
			public object userObject;

			public override void OnSelect(ItemSelector selector) {
				onClick?.Invoke(userObject);
			}

			public override bool CanSelect(ItemSelector selector) {
				return onClick != null;
			}
		}

		class ItemGraph : CustomItem {
			public GraphItem item;
		}

		public abstract class CustomItem {
			public string name;
			public string category = "Data";

			protected Texture icon;
			public GUIContent tooltip;

			public virtual Texture GetIcon() {
				return icon;
			}

			public virtual Texture GetSecondaryIcon() {
				return null;
			}

			public virtual void OnSelect(ItemSelector selector) {

			}

			public virtual bool CanSelect(ItemSelector selector) {
				return true;
			}

			public static CustomItem Create(NodeMenu menu, Action action, Texture icon = null, GUIContent tooltip = null, string category = null) {
				var item = new ItemNode(menu, action);
				if(icon != null)
					item.icon = icon;
				if(tooltip != null)
					item.tooltip = tooltip;
				if(!string.IsNullOrEmpty(category))
					item.category = category;
				return item;
			}

			public static CustomItem Create(string name, EditorReflectionUtility.ReflectionItem item, string category = "Data", Texture icon = null, GUIContent tooltip = null) {
				return new ItemReflection() {
					name = name,
					item = item,
					icon = icon,
					tooltip = tooltip,
					category = category,
				};
			}

			public static CustomItem Create(string name, Action onClick, string category = "Data", Texture icon = null, GUIContent tooltip = null) {
				return new ItemClickable() {
					name = name,
					onClick = delegate (object obj) {
						onClick();
					},
					category = category,
					icon = icon,
					tooltip = tooltip,
				};
			}

			public static CustomItem Create(string name, Action<object> onClick, object userObject, string category = "Data", Texture icon = null, GUIContent tooltip = null) {
				return new ItemClickable() {
					name = name,
					onClick = onClick,
					category = category,
					userObject = userObject,
					icon = icon,
					tooltip = tooltip,
				};
			}

			public static CustomItem Create(GraphItem item, string category = "Data", Texture icon = null, GUIContent tooltip = null) {
				return new ItemGraph() {
					name = "#ESItem",
					item = item,
					category = category,
					icon = icon,
					tooltip = tooltip,
				};
			}
		}

		public class SearchProgress {
			public float progress;
			public string info;
		}

		public static class TreeFunction {
			public static List<TypeTreeView> GetGeneralTrees() {
				var result = new List<TypeTreeView>();
				List<Type> types = GetGeneralTypes();
				types.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
				foreach(Type type in types) {
					result.Add(new TypeTreeView(type, type.GetHashCode(), -1));
				}
				return result;
			}

			public static List<TypeTreeView> GetRuntimeItems() {
				var result = new List<TypeTreeView>();
				List<Type> types = new List<Type>(EditorReflectionUtility.GetRuntimeTypes());
				types.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
				foreach(Type type in types) {
					result.Add(new TypeTreeView(type, type.GetHashCode(), -1));
				}
				return result;
			}

			public static MemberTreeView CreateItemFromMember(MemberInfo member, FilterAttribute filter) {
				if(member is Type) {
					Type type = member as Type;
					return new TypeTreeView(type, type.GetHashCode(), -1);
				}
				if(member.DeclaringType.IsGenericTypeDefinition)
					return null;
				bool canSelect = filter.ValidMemberType.HasFlags(member.MemberType);
				if(canSelect) {
					canSelect = EditorReflectionUtility.ValidateMember(member, filter);
				}
				if(canSelect && filter.ValidMemberType.HasFlags(member.MemberType)) {
					bool flag = filter.SetMember ? ReflectionUtils.CanSetMemberValue(member) : ReflectionUtils.CanGetMember(member, filter);
					if(flag) {
						if(member.MemberType == MemberTypes.Field ||
							member.MemberType == MemberTypes.Property ||
							member.MemberType == MemberTypes.Event ||
							member.MemberType == MemberTypes.NestedType) {
							return new MemberTreeView(member, member.GetHashCode(), -1) {
								selectValidation = () => EditorReflectionUtility.ValidateMember(member, filter),
								nextValidation = () => EditorReflectionUtility.ValidateNextMember(member, filter),
							};
						} else if(member.MemberType == MemberTypes.Method) {
							MethodInfo method = member as MethodInfo;
							if(ReflectionUtils.IsValidMethod(method, filter.MaxMethodParam, filter.MinMethodParam, filter)) {
								return new MemberTreeView(member, member.GetHashCode(), -1) {
									selectValidation = () => EditorReflectionUtility.ValidateMember(member, filter),
									nextValidation = () => EditorReflectionUtility.ValidateNextMember(member, filter),
								};
							}
						} else if(member.MemberType == MemberTypes.Constructor) {
							ConstructorInfo ctor = member as ConstructorInfo;
							if(ReflectionUtils.IsValidConstructor(ctor, filter.MaxMethodParam, filter.MinMethodParam)) {
								return new MemberTreeView(member, member.GetHashCode(), -1) {
									selectValidation = () => EditorReflectionUtility.ValidateMember(member, filter),
									nextValidation = () => EditorReflectionUtility.ValidateNextMember(member, filter),
								};
							}
						}
					} else if(member.MemberType != MemberTypes.Constructor) {
						bool canGet = ReflectionUtils.CanGetMember(member, filter);
						if(canGet) {
							return new MemberTreeView(member, member.GetHashCode(), -1) {
								selectValidation = () => EditorReflectionUtility.ValidateMember(member, filter),
								nextValidation = () => EditorReflectionUtility.ValidateNextMember(member, filter),
							};
						}
					}
				} else if(EditorReflectionUtility.ValidateNextMember(member, filter)) {
					bool flag = ReflectionUtils.CanGetMember(member, filter);
					if(flag) {
						return new MemberTreeView(member, member.GetHashCode(), -1) {
							selectValidation = () => EditorReflectionUtility.ValidateMember(member, filter),
							nextValidation = () => EditorReflectionUtility.ValidateNextMember(member, filter),
						};
					}
				}
				return null;
			}

			public static List<MemberTreeView> CreateItemsFromType(Type type, FilterAttribute filter, bool declaredOnly = false, Func<MemberInfo, bool> validation = null) {
				var result = new List<MemberTreeView>();
				if(type == null)
					return result;
				if(filter == null)
					filter = new FilterAttribute();
				var flags = filter.validBindingFlags;
				if(declaredOnly)
					flags |= BindingFlags.DeclaredOnly;
				if(type is RuntimeType runtimeType) {
					var members = runtimeType.GetRuntimeMembers();
					for(int i = 0; i < members.Length; i++) {
						if(validation == null || validation(members[i])) {
							var item = CreateItemFromMember(members[i], filter);
							if(item != null) {
								result.Add(item);
							}
						}
					}
					result.Sort((x, y) => string.Compare(x.displayName, y.displayName, StringComparison.OrdinalIgnoreCase));
				} else {
					var members = EditorReflectionUtility.GetSortedMembers(type, flags);
					if(filter.Static && !filter.SetMember && filter.ValidMemberType.HasFlags(MemberTypes.Constructor) &&
						!type.IsCastableTo(typeof(Delegate)) && !type.IsSubclassOf(typeof(Component))) {
						BindingFlags flag = BindingFlags.Public | BindingFlags.Instance;
						if(type.IsValueType) {
							flag |= BindingFlags.Static | BindingFlags.NonPublic;
						}
						ConstructorInfo[] ctor = type.GetConstructors(flag);
						for(int i = ctor.Length - 1; i >= 0; i--) {
							if((validation == null || validation(ctor[i]))) {
								var item = CreateItemFromMember(ctor[i], filter);
								if(item != null) {
									result.Add(item);
								}
							}
						}
					}
					for(int i = 0; i < members.Length; i++) {
						if(members[i].MemberType != MemberTypes.Constructor && (validation == null || validation(members[i]))) {
							var item = CreateItemFromMember(members[i], filter);
							if(item != null) {
								result.Add(item);
							}
						}
					}
				}
				return result;
			}

			public static List<TreeViewItem> CreateGraphItem(Object targetObject, MemberData reflectionValue, FilterAttribute filter) {
				List<TreeViewItem> result = new List<TreeViewItem>();
				uNodeRoot graph = null;
				List<IVariableSystem> variableSystems = null;
				if(targetObject) {
					graph = targetObject as uNodeRoot;
					ILocalVariableSystem LVS = null;
					if(graph == null && targetObject != null && (targetObject is GameObject || targetObject is Component)) {
						if(targetObject is GameObject) {
							GameObject go = targetObject as GameObject;
							graph = go.GetComponent<uNodeRoot>();
							if(graph == null) {
								NodeComponent com = go.GetComponent<NodeComponent>();
								if(com != null) {
									graph = com.owner;
									if(graph != null && com.transform.parent != null) {
										NodeComponent parentNode = com.transform.parent.GetComponent<NodeComponent>();
										if(com is IVariableSystem) {
											parentNode = com as NodeComponent;
											variableSystems = new List<IVariableSystem>();
											variableSystems.Add(parentNode as IVariableSystem);
											NodeEditorUtility.FindParentNode(parentNode.transform, ref variableSystems, graph);
											variableSystems.RemoveAll(item => item == null || item.Variables == null || item.Variables.Count == 0);
										} else if(parentNode is IVariableSystem) {
											variableSystems = new List<IVariableSystem>();
											variableSystems.Add(parentNode as IVariableSystem);
											NodeEditorUtility.FindParentNode(parentNode.transform, ref variableSystems, graph);
											variableSystems.RemoveAll(item => item == null || item.Variables == null || item.Variables.Count == 0);
										}
									}
									LVS = NodeEditorUtility.FindRootNode(com.transform, graph);
								} else if(go.GetComponent<TransitionEvent>()) {
									graph = go.GetComponent<TransitionEvent>().owner;
								}
							}
						} else {
							Component comp = targetObject as Component;
							graph = comp.GetComponent<uNodeRoot>();
							if(graph == null) {
								NodeComponent com = comp.GetComponent<NodeComponent>();
								if(com != null) {
									graph = com.owner;
									if(graph != null && com.transform.parent != null) {
										NodeComponent parentNode = com.transform.parent.GetComponent<NodeComponent>();
										if(com is IVariableSystem) {
											parentNode = com;
											variableSystems = new List<IVariableSystem>();
											variableSystems.Add(parentNode as IVariableSystem);
											NodeEditorUtility.FindParentNode(parentNode.transform, ref variableSystems, graph);
											variableSystems.RemoveAll(item => item == null || item.Variables == null || item.Variables.Count == 0);
										} else if(parentNode is IVariableSystem) {
											variableSystems = new List<IVariableSystem>();
											variableSystems.Add(parentNode as IVariableSystem);
											NodeEditorUtility.FindParentNode(parentNode.transform, ref variableSystems, graph);
											variableSystems.RemoveAll(item => item == null || item.Variables == null || item.Variables.Count == 0);
										}
									}
									LVS = NodeEditorUtility.FindRootNode(com.transform, graph);
								} else if(comp is RootObject) {
									RootObject root = comp as RootObject;
									LVS = root;
									if(root.owner is uNodeRoot) {
										graph = root.owner as uNodeRoot;
										if(graph != null && comp.transform.parent != null) {
											NodeComponent parentNode = comp.transform.parent.GetComponent<NodeComponent>();
											if(parentNode is IVariableSystem) {
												variableSystems = new List<IVariableSystem>();
												variableSystems.Add(parentNode as IVariableSystem);
												NodeEditorUtility.FindParentNode(parentNode.transform, ref variableSystems, graph);
												variableSystems.RemoveAll(item => item == null || item.Variables == null || item.Variables.Count == 0);
											}
										}
									}
								} else if(comp.GetComponent<TransitionEvent>()) {
									graph = comp.GetComponent<TransitionEvent>().owner;
								}
							}
						}
					}
					if(LVS != null) {
						var localVariable = LVS.LocalVariables;
						if(localVariable != null && localVariable.Count > 0) {
							var itemData = new List<GraphItem>(localVariable.Select(item => new GraphItem(item, LVS as Object, MemberData.TargetType.uNodeLocalVariable)));
							itemData.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
							var categTree = new SelectorCategoryTreeView("Local Variable", "", uNodeEditorUtility.GetUIDFromString("[Local-Variable]"), -1);
							itemData.ForEach(item => categTree.AddChild(new SelectorCustomTreeView(item, item.GetHashCode(), -1)));
							result.Add(categTree);
						}
					}
					if(graph != null) {
						var graphItem = new List<GraphItem>();
						graphItem.AddRange(graph.Variables.Select(item => new GraphItem(item, graph)));
						graphItem.AddRange(graph.Properties.Select(item => new GraphItem(item, graph)));
						if(!filter.SetMember && filter.ValidMemberType.HasFlags(MemberTypes.Method) && filter.IsValidTarget(MemberTypes.Method)) {
							graphItem.AddRange(graph.Functions.Where(item => IsCorrectItem(item, filter)).Select(item => new GraphItem(item)));
						}
						graphItem.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
						if(filter == null || !filter.SetMember) {
							bool flag = false;
							{
								var data = graph.GetComponent<uNodeData>();
								if(data != null && !string.IsNullOrEmpty(data.generatorSettings.Namespace)) {
									flag = TypeSerializer.Deserialize(data.generatorSettings.Namespace + "." + graph.Name, false) != null;
								} else {
									flag = TypeSerializer.Deserialize(graph.Name, false) != null;
								}
							}
							if((flag || filter.IsValidType(ReflectionUtils.GetRuntimeType(graph))) &&
								filter.IsValidTarget(MemberData.TargetType.SelfTarget | MemberData.TargetType.ValueNode)) {
								graphItem.Insert(0, new GraphItem(graph));
							}
						}
						RemoveIncorrectGraphItem(graphItem, filter);
						if(graphItem != null && graphItem.Count > 0) {
							var categTree = new SelectorCategoryTreeView("Graph (Self)", "", uNodeEditorUtility.GetUIDFromString("[GRAPH-SELF]"), -1);
							graphItem.ForEach(item => categTree.AddChild(new SelectorCustomTreeView(item, item.GetHashCode(), -1)));
							result.Add(categTree);
						}
						if(reflectionValue == null || reflectionValue.GetInstance() == null || reflectionValue.GetInstance() is IGraphSystem || reflectionValue.GetInstance() is INode) {
							var tree = CreateTargetItem(graph, "Graph Inherit Member", filter);
							if(tree != null) {
								tree.expanded = false;
								result.Add(tree);
							}
						}
					}
				}
				Object o;
				if(reflectionValue != null) {
					o = reflectionValue.GetInstance() as Object;
				} else {
					o = targetObject;
				}
				uNodeRoot targetUNR = o as uNodeRoot;
				if(o != null) {
					if(targetUNR == null && o != null && (o is GameObject || o is Component)) {
						if(o is GameObject) {
							GameObject go = o as GameObject;
							targetUNR = go.GetComponent<uNodeRoot>();
							if(targetUNR == null) {
								NodeComponent com = go.GetComponent<NodeComponent>();
								if(com != null) {
									targetUNR = com.owner;
								}
							}
						} else {
							Component comp = o as Component;
							targetUNR = comp.GetComponent<uNodeRoot>();
							if(targetUNR == null) {
								NodeComponent com = comp.GetComponent<NodeComponent>();
								if(com != null) {
									targetUNR = com.owner;
								}
							}
						}
					}
				}
				if(targetUNR != null && targetUNR != graph) {
					var itemData = new List<GraphItem>();
					itemData.AddRange(targetUNR.Variables.Where(i => i != null).Select(item => new GraphItem(item, targetUNR)));
					itemData.AddRange(targetUNR.Properties.Where(i => i != null).Select(item => new GraphItem(item, graph)));
					itemData.AddRange(targetUNR.Functions.Where(i => i != null).Select(item => new GraphItem(item)));
					itemData.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
					RemoveIncorrectGraphItem(itemData, filter);
					if(itemData.Count > 0) {
						var categTree = new SelectorCategoryTreeView("Graph (Target)", "", uNodeEditorUtility.GetUIDFromString("[GRAPH-TARGET]"), -1);
						itemData.ForEach(item => categTree.AddChild(new SelectorCustomTreeView(item, item.GetHashCode(), -1)));
						result.Add(categTree);
					}
				} else if(graph == null && targetObject != null) {
					var itemData = new List<GraphItem>();
					if(targetObject is Component) {
						itemData = new List<GraphItem>();
						if(targetObject is IVariableSystem) {
							var variableSystem = targetObject as IVariableSystem;
							itemData.AddRange(variableSystem.Variables.Where(i => i != null).Select(item => new GraphItem(item, targetObject)));
						}
						if(targetObject is IPropertySystem) {
							var propertySystem = targetObject as IPropertySystem;
							itemData.AddRange(propertySystem.Properties.Where(i => i != null).Select(item => new GraphItem(item, targetObject)));
						}
						if(targetObject.GetType().GetInterface("INodeSystem`1") != null) {
							Type t = targetObject.GetType();
							Type interfaceType = t.GetInterface("INodeSystem`1");
							Type rootType = interfaceType.GetGenericArguments()[0];
							MethodInfo mInfo = t.GetMethod("GetOwner");
							if(mInfo != null && mInfo.ReturnType == rootType) {
								Object tObj = mInfo.Invoke(targetObject, null) as Object;
								if(tObj != null) {
									if(tObj is IVariableSystem) {
										var variableSystem = tObj as IVariableSystem;
										itemData.AddRange(variableSystem.Variables.Where(i => i != null).Select(item => new GraphItem(item, tObj)));
									}
									if(tObj is IPropertySystem) {
										var propertySystem = tObj as IPropertySystem;
										itemData.AddRange(propertySystem.Properties.Where(i => i != null).Select(item => new GraphItem(item, tObj)));
									}
								}
							}
						}
					}
					if(itemData.Count > 0) {
						itemData.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
						RemoveIncorrectGraphItem(itemData, filter);
						var categTree = new SelectorCategoryTreeView("Target", "", uNodeEditorUtility.GetUIDFromString("[GRAPH-TARGET]"), -1);
						itemData.ForEach(item => categTree.AddChild(new SelectorCustomTreeView(item, item.GetHashCode(), -1)));
						result.Add(categTree);
					}
				}
				//if(variableSystems != null && variableSystems.Count > 0) {
				//	show4 = GUILayout.Toggle(show4, new GUIContent("Group Variable", ""), EditorStyles.miniButton);
				//	if(show4) {
				//		string[] groupNames = new string[variableSystems.Count];
				//		for(int i = 0; i < variableSystems.Count; i++) {
				//			Component comp = variableSystems[i] as Component;
				//			if(comp) {
				//				groupNames[i] = "[" + i + "]" + comp.gameObject.name;
				//			}
				//		}
				//		if(selectedGroupIndex > groupNames.Length - 1) {
				//			selectedGroupIndex = 0;
				//		}
				//		selectedGroupIndex = EditorGUILayout.Popup("Group", selectedGroupIndex, groupNames);
				//		{
				//			IVariableSystem IVS = variableSystems[selectedGroupIndex];
				//			var variable = IVS.Variables;
				//			if(variable != null && variable.Count > 0) {
				//				//variable.Sort((x, y) => string.Compare(x.Name, y.Name));
				//				ShowESItem(new List<GraphItem>(variable.Select(item => new GraphItem(item, IVS as UnityEngine.Object, MemberData.TargetType.uNodeGroupVariable))));
				//			}
				//		}
				//	}
				//}
				return result;
			}

			public static List<TreeViewItem> CreateCustomItem(List<CustomItem> customItems, bool expanded = true) {
				var result = new List<TreeViewItem>();
				if(customItems != null && customItems.Count > 0) {
					Dictionary<string, SelectorCategoryTreeView> trees = new Dictionary<string, SelectorCategoryTreeView>();
					//customItems.Sort((x, y) => CompareUtility.Compare(x.category, y.category, x.name, y.name));
					foreach(var ci in customItems) {
						if(!trees.TryGetValue(ci.category, out var tree)) {
							var path = ci.category.Split('.');
							string p = null;
							for(int i = 0; i < path.Length; i++) {
								if(!string.IsNullOrEmpty(p))
									p += '.';
								p += path[i];
								if(!trees.TryGetValue(p, out var cTree)) {
									cTree = new SelectorCategoryTreeView(path[i], "", uNodeEditorUtility.GetUIDFromString("[CITEM]" + p), -1);
									if(tree != null) {
										tree.AddChild(cTree);
									} else {
										result.Add(cTree);
									}
									tree = cTree;
									trees[p] = cTree;
								} else {
									tree = cTree;
								}
							}
						}
					}
					foreach(var ci in customItems) {
						trees.TryGetValue(ci.category, out var cTree);
						cTree.AddChild(new SelectorCustomTreeView(ci, ci.GetHashCode(), -1));
					}
					if(!expanded) {
						foreach(var pair in trees) {
							pair.Value.expanded = false;
						}
					} else {
						foreach(var pair in trees) {
							if(pair.Key.IndexOf('.') >= 0) {
								pair.Value.expanded = false;
							}
						}
					}
				}
				return result;
			}

			public static List<TreeViewItem> CreateRootItem(Object targetObject, FilterAttribute filter) {
				var result = new List<TreeViewItem>();
				if(targetObject) {
					uNodeFunction function = null;
					if(targetObject is NodeComponent) {
						NodeComponent comp = targetObject as NodeComponent;
						function = uNodeUtility.FindParentComponent<uNodeFunction>(comp.transform, comp.owner.transform);
					} else if(targetObject is uNodeFunction) {
						function = targetObject as uNodeFunction;
					} else if(targetObject is TransitionEvent) {
						TransitionEvent comp = targetObject as TransitionEvent;
						function = uNodeUtility.FindParentComponent<uNodeFunction>(comp.transform, comp.owner.transform);
					}
					if(function != null) {
						var graphItems = new List<GraphItem>();
						graphItems.AddRange(function.parameters.Where(item => IsCorrectItem(item, filter)).Select(item => new GraphItem(item, function)));
						graphItems.AddRange(function.genericParameters.Where(item => IsCorrectItem(item, filter)).Select(item => new GraphItem(item, function)));
						if(function.owner is IGenericParameterSystem) {
							graphItems.AddRange((function.owner as IGenericParameterSystem).GenericParameters.Where(item => {
								if(IsCorrectItem(item, filter)) {
									foreach(var lItem in function.genericParameters) {
										if(item.name.Equals(lItem.name)) {
											return false;
										}
									}
									return true;
								}
								return false;
							}).Select(item => new GraphItem(item, function.owner)));
						}
						//graphItems.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
						RemoveIncorrectGraphItem(graphItems, filter);
						if(graphItems != null && graphItems.Count > 0) {
							graphItems.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
							var categTree = new SelectorCategoryTreeView("Parameter", "", uNodeEditorUtility.GetUIDFromString("[Parameter]"), -1);
							graphItems.ForEach(item => categTree.AddChild(new SelectorCustomTreeView(item, item.GetHashCode(), -1)));
							result.Add(categTree);
						}
					} else {
						uNodeConstuctor ctor = targetObject as uNodeConstuctor;
						if(ctor == null) {
							if(targetObject is NodeComponent) {
								NodeComponent comp = targetObject as NodeComponent;
								ctor = uNodeUtility.FindParentComponent<uNodeConstuctor>(comp.transform, comp.owner.transform);
							} else if(targetObject is TransitionEvent) {
								TransitionEvent comp = targetObject as TransitionEvent;
								function = uNodeUtility.FindParentComponent<uNodeFunction>(comp.transform, comp.owner.transform);
							}
						}
						if(ctor != null) {
							var graphItems = new List<GraphItem>();
							graphItems.AddRange(ctor.parameters.Where(item => IsCorrectItem(item, filter)).Select(item => new GraphItem(item, ctor)));
							if(ctor.owner is IGenericParameterSystem) {
								graphItems.AddRange((ctor.owner as IGenericParameterSystem).GenericParameters.Where(item => IsCorrectItem(item, filter)).Select(item => new GraphItem(item, ctor.owner)));
							}
							//graphItems.Sort((x, y) => string.Compare(x.Name, y.Name));
							RemoveIncorrectGraphItem(graphItems, filter);
							if(graphItems != null && graphItems.Count > 0) {
								graphItems.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
								var categTree = new SelectorCategoryTreeView("Parameter", "", uNodeEditorUtility.GetUIDFromString("[Parameter]"), -1);
								graphItems.ForEach(item => categTree.AddChild(new SelectorCustomTreeView(item, item.GetHashCode(), -1)));
								result.Add(categTree);
							}
						} else {
							uNodeRoot root = targetObject as uNodeRoot;
							if(root == null) {
								if(targetObject is INode<uNodeRoot>) {
									root = (targetObject as INode<uNodeRoot>).GetOwner();
								}
							}
							if(root is IGenericParameterSystem) {
								var graphItems = new List<GraphItem>();
								graphItems.AddRange((root as IGenericParameterSystem).GenericParameters.Where(item => IsCorrectItem(item, filter)).Select(item => new GraphItem(item, root)));
								//graphItems.Sort((x, y) => string.Compare(x.Name, y.Name));
								RemoveIncorrectGraphItem(graphItems, filter);
								if(graphItems != null && graphItems.Count > 0) {
									graphItems.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
									var categTree = new SelectorCategoryTreeView("Parameter", "", uNodeEditorUtility.GetUIDFromString("[Parameter]"), -1);
									graphItems.ForEach(item => categTree.AddChild(new SelectorCustomTreeView(item, item.GetHashCode(), -1)));
									result.Add(categTree);
								}
							}
						}
					}
				}
				return result;
			}

			public static SelectorCategoryTreeView CreateTargetItem(object targetValue, string category, FilterAttribute filter, Type targetType = null) {
				if(targetType == null) {
					targetType = targetValue.GetType();
					if(targetValue is uNodeRoot) {
						targetType = (targetValue as uNodeRoot).GetInheritType();
					}
				}
				var categoryTree = new SelectorCategoryTreeView(category, "", uNodeEditorUtility.GetUIDFromString("[CATEG]" + category), -1);
				var instance = new MemberData(targetValue, MemberData.TargetType.SelfTarget);
				if(targetValue is uNodeRoot) {
					Type rootType = (targetValue as uNodeRoot).GetInheritType();
					FilterAttribute fil = new FilterAttribute(filter);
					fil.NestedType = false;
					fil.NonPublic = true;
					var items = CreateItemsFromType(rootType, new FilterAttribute(fil) { Static = false }, false);
					if(items != null) {
						if(fil != null && !fil.SetMember) {
							bool flag = false;
							{
								var root = targetValue as uNodeRoot;
								var data = root.GetComponent<uNodeData>();
								flag = data == null ?
									targetType.Name == root.Name :
									targetType.Name == root.Name && data.generatorSettings.Namespace == targetType.Namespace;
							}
							if((fil.IsValidType(targetType) || flag) && fil.IsValidTarget(MemberData.TargetType.SelfTarget | MemberData.TargetType.ValueNode)) {
								categoryTree.AddChild(new SelectorMemberTreeView(MemberData.This(instance, targetType), "this", uNodeEditorUtility.GetUIDFromString("this" + instance.GetHashCode())));
							}
						}
						//RemoveIncorrectGeneralItem(TargetType);
						//items.RemoveAll(i => i.memberInfo != null && i.memberInfo.MemberType == MemberTypes.Constructor);
						items.ForEach(tree => {
							tree.instance = targetValue;
							categoryTree.AddChild(tree);
						});
					}
				} else {
					var fil = new FilterAttribute(filter);
					fil.NestedType = false;
					fil.Static = false;
					var items = CreateItemsFromType(targetType, fil, false);
					if(items != null) {
						if(filter != null && !filter.SetMember && filter.IsValidType(targetType)) {
							categoryTree.AddChild(new SelectorMemberTreeView(MemberData.This(instance, targetType), "this", uNodeEditorUtility.GetUIDFromString("this" + instance.GetHashCode())));
						}
						//RemoveIncorrectGeneralItem(TargetType);
						//items.RemoveAll(i => i.memberInfo != null && i.memberInfo.MemberType == MemberTypes.Constructor);
					}
					//items.ForEach(item => item.instance = instance);
					//EditorReflectionUtility.SortItem(items);
					items.ForEach(tree => {
						tree.instance = targetValue;
						categoryTree.AddChild(tree);
					});
				}
				return categoryTree;
			}
		}
	}
}