using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MaxyGames.Events;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode.Editors {
	public static class BlockUtility {
		public static List<EventActionData> GetActionBlockFromNode(Node node) {
			if(node is NodeSetValue) {
				var source = node as NodeSetValue;
				var action = new SetValue();
				action.setType = source.setType;
				action.target = new MemberData(source.target);
				action.value = new MemberData(source.value);
				return new List<EventActionData>() { action };
			} else if(node is MultipurposeNode) {
				var source = node as MultipurposeNode;
				if(source.IsFlowNode()) {
					var action = new GetValue();
					action.target = new MultipurposeMember(source.target);
					return new List<EventActionData>() { action };
				}
			}
			return new List<EventActionData>();
		}

		public static List<EventActionData> GetConditionBlockFromNode(Node node) {
			if (node is NodeSetValue) {
				var source = node as NodeSetValue;
				var action = new SetValue();
				action.target = new MemberData(source.target);
				action.value = new MemberData(source.value);
				return new List<EventActionData>() { action };
			} else if (node is MultipurposeNode) {
				var source = node as MultipurposeNode;
				if (source.CanGetValue() && source.ReturnType() == typeof(bool)) {
					var action = new EqualityCompare();
					action.target = new MultipurposeMember(source.target);
					action.value = MemberData.CreateValueFromType(source.ReturnType());
					return new List<EventActionData>() { action };
				}
			} else if(node is Nodes.ComparisonNode) {
				var source = node as Nodes.ComparisonNode;
				var action = new ObjectCompare();
				action.targetA = new MultipurposeMember(source.targetA);
				action.targetB = new MultipurposeMember(source.targetB);
				action.operatorType = source.operatorType;
				return new List<EventActionData>() { action };
			} else if(node is Nodes.MultiANDNode) {
				var source = node as Nodes.MultiANDNode;
				var result = new List<EventActionData>();
				foreach(var target in source.targets) {
					var action = new EqualityCompare();
					action.target = new MultipurposeMember(target);
					action.value = new MemberData(true);
					result.Add(action);
				}
				return result;
			} else if(node is Nodes.MultiORNode) {
				var source = node as Nodes.MultiORNode;
				var result = new List<EventActionData>();
				foreach(var target in source.targets) {
					if(result.Count > 0) {
						result.Add(EventActionData.OrEvent);
					}
					var action = new EqualityCompare();
					action.target = new MultipurposeMember(target);
					action.value = new MemberData(true);
					result.Add(action);
				}
				return result;
			} else if(node is Nodes.NotNode) {
				var source = node as Nodes.NotNode;
				var action = new EqualityCompare();
				action.target = new MultipurposeMember(source.target);
				MemberDataUtility.UpdateMultipurposeMember(action.target);
				action.value = new MemberData(false);
				return new List<EventActionData>() { action };
			}
			return new List<EventActionData>();
		}

		private static List<BlockMenuAttribute> _actionMenus;
		public static List<BlockMenuAttribute> GetActionMenus(bool acceptCoroutine = false) {
			if(_actionMenus != null) {
				return _actionMenus;
			}
			_actionMenus = new List<BlockMenuAttribute>();
			var menus = EventDataDrawer.FindAllMenu();
			for(int i = 0; i < menus.Count; i++) {
				if(!acceptCoroutine && menus[i].isCoroutine || menus[i].hideOnBlock)
					continue;
				if(menus[i].type.IsCastableTo(typeof(Events.Action)) || 
					menus[i].type.IsCastableTo(typeof(AnyBlock)) ||
					menus[i].type.IsCastableTo(typeof(IFlowNode)) ||
					menus[i].type.IsCastableTo(typeof(IStateNode)) ||
					 menus[i].type.IsCastableTo(typeof(ICoroutineNode)) ||
					 menus[i].type.IsCastableTo(typeof(IStateCoroutineNode))) {
					_actionMenus.Add(menus[i]);
				}
			}
			return _actionMenus;
		}

		private static List<BlockMenuAttribute> _conditionMenus;
		public static List<BlockMenuAttribute> GetConditionMenus() {
			if(_conditionMenus != null) {
				return _conditionMenus;
			}
			_conditionMenus = new List<BlockMenuAttribute>();
			var menus = EventDataDrawer.FindAllMenu();
			for(int i = 0; i < menus.Count; i++) {
				if(menus[i].hideOnBlock)
					continue;
				if(menus[i].type.IsCastableTo(typeof(Condition)) ||
					menus[i].type.IsCastableTo(typeof(IDataNode<bool>))) {
					_conditionMenus.Add(menus[i]);
				}
			}
			return _conditionMenus;
		}

		public static Condition onAddEqualityComparer(MemberData member) {
			EqualityCompare cond = new EqualityCompare();
			cond.target = new MultipurposeMember() { target = member };
			MemberDataUtility.UpdateMultipurposeMember(cond.target);
			cond.value = MemberData.CreateValueFromType(member.type);
			return cond;
		}

		public static Condition onAddIsComparer(MemberData member) {
			IsCompare cond = new IsCompare();
			cond.target = new MultipurposeMember() { target = member };
			MemberDataUtility.UpdateMultipurposeMember(cond.target);
			return cond;
		}

		public static Events.Action onAddSetAction(MemberData member) {
			SetValue action = new SetValue();
			action.target = member;
			action.value = MemberData.CreateValueFromType(member.type);
			return action;
		}

		public static Events.Action onAddGetAction(MemberData member) {
			GetValue action = new GetValue();
			action.target.target = member;
			MemberDataUtility.UpdateMultipurposeMember(action.target);
			return action;
		}

		public static Events.Action onAddCompareAction(MemberData member) {
			CompareOperator action = new CompareOperator();
			action.targetA = member;
			action.targetB = MemberData.CreateValueFromType(member.type);
			return action;
		}

		public static void ShowAddActionMenu(Vector2 position, Action<Events.Action> action, MemberData instance, bool acceptCoroutine = false) {
			List<ItemSelector.CustomItem> customItems = new List<ItemSelector.CustomItem>();
			if(instance == null || !instance.IsTargetingPortOrNode) {
				var actions = GetActionMenus(acceptCoroutine);
				foreach(var a in actions) {
					var type = a.type;
					customItems.Add(ItemSelector.CustomItem.Create(a.name,
						delegate () {
							Events.Action act;
							if(type.IsSubclassOf(typeof(Events.Action))) {
								act = ReflectionUtils.CreateInstance(type) as Events.Action;
							} else {
								act = new HLAction() {
									type = MemberData.CreateFromType(type)
								};
							}
							action(act);
						}, a.category));
				}
			}
			FilterAttribute filter = new FilterAttribute() {
				InvalidTargetType = MemberData.TargetType.Values |
									MemberData.TargetType.Null |
									//MemberData.TargetType.Constructor |
									MemberData.TargetType.SelfTarget,
				MaxMethodParam = int.MaxValue,
				Instance = true,
				VoidType = true
			};
			Object unityInstance = null;
			if(instance != null) {
				if(instance.IsTargetingPortOrNode) {
					var type = instance.type;
					filter.Static = false;
					filter.UnityReference = false;
					if(type is RuntimeType) {
						customItems = ItemSelector.MakeCustomItems((type as RuntimeType).GetRuntimeMembers(), filter);
						if(type.BaseType != null)
							customItems.AddRange(ItemSelector.MakeCustomItems(type.BaseType, filter, "Inherit Member"));
					} else {
						customItems = ItemSelector.MakeCustomItems(type, filter, "Data", "Data ( Inherited )");
					}
					var graph = instance.GetTargetNode().owner;
					if(graph != null) {
						var usingNamespaces = new HashSet<string>(graph.GetNamespaces());
						customItems.AddRange(ItemSelector.MakeExtensionItems(type, usingNamespaces, filter, "Extensions"));
					}
				} else if(instance.targetType == MemberData.TargetType.Values) {
					unityInstance = instance.Get() as Object;
				} else if(instance.IsTargetingUNode) {
					unityInstance = instance.startTarget as Object;
				}
			}
			ItemSelector.SortCustomItems(customItems);
			ItemSelector w = ItemSelector.ShowWindow(unityInstance, filter, (member) => {
				if(instance.IsTargetingPortOrNode) {
					if(instance.type.IsCastableTo(member.startType)) {
						member.instance = instance;
					}
				} else if(unityInstance != null && !member.isStatic) {
					Type startType = member.startType;
					if(startType != null && member.instance == null) {
						if(unityInstance.GetType().IsCastableTo(startType)) {
							member.instance = unityInstance;
						} else if(member.IsTargetingUNode) {
							if(member.instance == null)
								member.instance = unityInstance;
						} else if(unityInstance is Component) {
							if(startType == typeof(GameObject)) {
								member.instance = (unityInstance as Component).gameObject;
							} else if(startType.IsSubclassOf(typeof(Component))) {
								member.instance = (unityInstance as Component).GetComponent(startType);
							}
						} else if(unityInstance is GameObject) {
							if(startType == typeof(GameObject)) {
								member.instance = unityInstance as GameObject;
							} else if(startType.IsSubclassOf(typeof(Component))) {
								member.instance = (unityInstance as GameObject).GetComponent(startType);
							}
						}
						if(member.instance == null && ReflectionUtils.CanCreateInstance(startType)) {
							member.instance = ReflectionUtils.CreateInstance(startType == typeof(object) ? typeof(string) : startType);
						}
					}
					if(member.instance == null) {
						member.instance = MemberData.none;
					}
				}
				switch(member.targetType) {
					case MemberData.TargetType.uNodeFunction:
					case MemberData.TargetType.Constructor:
					case MemberData.TargetType.Method: {
						if(member.type != typeof(void)) {
							GenericMenu menu = new GenericMenu();
							menu.AddItem(new GUIContent("Invoke"), false, () => {
								action(onAddGetAction(member));
							});
							menu.AddItem(new GUIContent("Compare"), false, () => {
								action(onAddCompareAction(member));
							});
							menu.ShowAsContext();
						} else {
							action(onAddGetAction(member));
						}
						break;
					}
					case MemberData.TargetType.Field:
					case MemberData.TargetType.Property:
					case MemberData.TargetType.uNodeProperty:
					case MemberData.TargetType.uNodeVariable:
					case MemberData.TargetType.uNodeLocalVariable:
					case MemberData.TargetType.uNodeGroupVariable:
					case MemberData.TargetType.uNodeParameter: {
						GenericMenu menu = new GenericMenu();
						var members = member.GetMembers();
						if(members == null || members[members.Length - 1] == null || ReflectionUtils.CanGetMember(members[members.Length - 1], filter)) {
							menu.AddItem(new GUIContent("Get"), false, () => {
								action(onAddGetAction(member));
							});
						}
						if(members == null || members[members.Length - 1] == null || ReflectionUtils.CanSetMember(members[members.Length - 1])) {
							menu.AddItem(new GUIContent("Set"), false, () => {
								action(onAddSetAction(member));
							});
						}
						menu.AddItem(new GUIContent("Compare"), false, () => {
							action(onAddCompareAction(member));
						});
						menu.ShowAsContext();
						break;
					}
					default:
						throw new Exception("Unsupported target kind:" + member.targetType);
				}
			}, false, customItems).ChangePosition(position);
			w.displayNoneOption = false;
			w.displayCustomVariable = false;
			w.customItems = customItems;
		}

        public static void ShowAddEventMenu(
			Vector2 position,
			MemberData instance,
			Action<Block> addConditionEvent) {
			List<ItemSelector.CustomItem> customItems = new List<ItemSelector.CustomItem>();
			if(instance == null || !instance.IsTargetingPortOrNode) {
				var conditions = GetConditionMenus();
				foreach(var c in conditions) {
					var type = c.type;
					customItems.Add(ItemSelector.CustomItem.Create(c.name,
						delegate () {
							if(addConditionEvent != null) {
								Block act;
								if(type.IsSubclassOf(typeof(Block))) {
									act = ReflectionUtils.CreateInstance(type) as Block;
								} else if(type.IsCastableTo(typeof(IDataNode<bool>))) {
									act = new HLCondition() {
										type = MemberData.CreateFromType(type)
									};
								} else {
									throw new Exception("The type must inherith from Block or IDataNode<bool>");
								}
								addConditionEvent(act);
							}
						}, c.category));
				}
			}
			FilterAttribute filter = new FilterAttribute() {
				InvalidTargetType = MemberData.TargetType.Values | MemberData.TargetType.Null,
				MaxMethodParam = int.MaxValue,
				Instance = true,
				VoidType = false,
			};
			Object unityInstance = null;
			if(instance != null) {
				if(instance.IsTargetingPortOrNode) {
					var type = instance.type;
					filter.Static = false;
					filter.UnityReference = false;
					if(type is RuntimeType) {
						customItems = ItemSelector.MakeCustomItems((type as RuntimeType).GetRuntimeMembers(), filter);
						if(type.BaseType != null)
							customItems.AddRange(ItemSelector.MakeCustomItems(type.BaseType, filter, "Inherit Member"));
					} else {
						customItems = ItemSelector.MakeCustomItems(type, filter, "Data", "Data ( Inherited )");
					}
					var graph = instance.GetTargetNode().owner;
					if(graph != null) {
						var usingNamespaces = new HashSet<string>(graph.GetNamespaces());
						customItems.AddRange(ItemSelector.MakeExtensionItems(type, usingNamespaces, filter, "Extensions"));
					}
				} else if(instance.targetType == MemberData.TargetType.Values) {
					unityInstance = instance.Get() as Object;
				} else if(instance.IsTargetingUNode) {
					unityInstance = instance.startTarget as Object;
				}
			}
			ItemSelector.SortCustomItems(customItems);
			ItemSelector w = ItemSelector.ShowWindow(unityInstance, filter, (member) => {
				if(instance.IsTargetingPortOrNode) {
					if(instance.type.IsCastableTo(member.startType)) {
						member.instance = instance;
					}
				} else if(unityInstance != null && !member.isStatic) {
					Type startType = member.startType;
					if(startType != null && member.instance == null) {
						if(unityInstance.GetType().IsCastableTo(startType)) {
							member.instance = unityInstance;
						} else if(member.IsTargetingUNode) {
							if(member.instance == null)
								member.instance = unityInstance;
						} else if(unityInstance is Component) {
							if(startType == typeof(GameObject)) {
								member.instance = (unityInstance as Component).gameObject;
							} else if(startType.IsSubclassOf(typeof(Component))) {
								member.instance = (unityInstance as Component).GetComponent(startType);
							}
						} else if(unityInstance is GameObject) {
							if(startType == typeof(GameObject)) {
								member.instance = unityInstance as GameObject;
							} else if(startType.IsSubclassOf(typeof(Component))) {
								member.instance = (unityInstance as GameObject).GetComponent(startType);
							}
						}
						if(member.instance == null && ReflectionUtils.CanCreateInstance(startType)) {
							member.instance = ReflectionUtils.CreateInstance(startType == typeof(object) ? typeof(string) : startType);
						}
					}
					if(member.instance == null) {
						member.instance = MemberData.none;
					}
				}
				Condition condition = null;
				System.Action addCondition = () => {
                    if(addConditionEvent != null) {
						addConditionEvent(condition);
					}
				};
				switch(member.targetType) {
					case MemberData.TargetType.Constructor:
					case MemberData.TargetType.Method: {
						GenericMenu menu = new GenericMenu();
						menu.AddItem(new GUIContent("Equality Compare"), false, () => {
							condition = onAddEqualityComparer(member);
							addCondition();
						});
						menu.AddItem(new GUIContent("Is Compare"), false, () => {
							condition = onAddIsComparer(member);
							addCondition();
						});
						menu.ShowAsContext();
						break;
					}
					case MemberData.TargetType.Field:
					case MemberData.TargetType.Property:
					case MemberData.TargetType.uNodeVariable:
					case MemberData.TargetType.uNodeProperty:
					case MemberData.TargetType.uNodeLocalVariable:
					case MemberData.TargetType.uNodeGroupVariable: {
						GenericMenu menu = new GenericMenu();
						menu.AddItem(new GUIContent("Equality Compare"), false, () => {
							condition = onAddEqualityComparer(member);
							addCondition();
						});
						menu.AddItem(new GUIContent("Is Compare"), false, () => {
							condition = onAddIsComparer(member);
							addCondition();
						});
						menu.ShowAsContext();
						break;
					}
					default:
						condition = onAddEqualityComparer(member);
						addCondition();
						break;
				}
			}, false, customItems).ChangePosition(position);
			w.displayNoneOption = false;
			w.displayCustomVariable = false;
			w.customItems = customItems;
		}
	}
}