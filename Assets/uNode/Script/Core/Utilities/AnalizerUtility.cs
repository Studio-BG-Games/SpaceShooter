using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode {
	public struct AnalizerData {
		public object owner;
		public FieldInfo field;
		public Type type;
		public object value;

		public AnalizerData(object owner, FieldInfo field, Type type, object value) {
			this.owner = owner;
			this.field = field;
			this.type = type;
			this.value = value;
		}
	}

    public class AnalizerUtility {
		private static readonly BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
		/// <summary>
		/// Perform field reflection in obj.
		/// </summary>
		/// <param name="obj">The object to analize</param>
		/// <param name="validation">The analize validation, should return true when condition is meet or you change something.</param>
		/// <param name="doAction">The action to perform when the validation is true</param>
		/// <returns>True when validation is valid</returns>
		public static bool AnalizeObject(object obj, Func<object, bool> validation, Action<object> doAction = null) {
			if(object.ReferenceEquals(obj, null) || validation == null)
				return false;
			if(!(obj is UnityEngine.Object) && validation(obj)) {
				if(doAction != null)
					doAction(obj);
				if(obj is MemberData) {
					var mInstane = (obj as MemberData).instance;
					if(mInstane != null && !(mInstane is Object)) {
						if(AnalizeObject(mInstane, validation, doAction)) {
							//This make sure to serialize the data.
							(obj as MemberData).instance = mInstane;
						}
					}
				}
				return true;
			}
			if(obj is MemberData) {
				MemberData mData = obj as MemberData;
				if(mData != null && mData.instance != null && !(mData.instance is UnityEngine.Object)) {
					bool flag = AnalizeObject(mData.instance, validation, doAction);
					if(flag) {
						//This make sure to serialize the data.
						mData.instance = mData.instance;
					}
					return flag;
				}
				return false;
			}
			bool changed = false;
			if(obj is EventData) {
				EventData member = obj as EventData;
				if(member != null && member.blocks.Count > 0) {
					foreach(EventActionData action in member.blocks) {
						if(validation(action)) {
							if(doAction != null)
								doAction(action);
							changed = true;
							continue;
						}
						changed = AnalizeObject(action.block, validation, doAction) || changed;
					}
				}
				return changed;
			} else if(obj is IList) {
				IList list = obj as IList;
				for(int i = 0; i < list.Count; i++) {
					object element = list[i];
					if(element == null)
						continue;
					if(element is UnityEngine.Object) {
						if(validation(element)) {
							if(doAction != null)
								doAction(element);
							changed = true;
						}
						continue;
					}
					changed = AnalizeObject(element, validation, doAction) || changed;
				}
				return changed;
			}
			FieldInfo[] fieldInfo = ReflectionUtils.GetFields(obj, flags);
			foreach(FieldInfo field in fieldInfo) {
				Type fieldType = field.FieldType;
				if(!fieldType.IsClass)
					continue;
				object value = field.GetValueOptimized(obj);
				if(object.ReferenceEquals(value, null))
					continue;
				if(value is UnityEngine.Object) {
					if(validation(value)) {
						if(doAction != null)
							doAction(value);
						changed = true;
					}
					continue;
				}
				changed = AnalizeObject(value, validation, doAction) || changed;
			}
			return changed;
		}

		/// <summary>
		/// Perform field reflection in obj.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="validation"></param>
		/// <param name="onAction">Parameter: owner or the FieldInfo, field, type, value. </param>
		public static void AnalizeObject(object obj, Func<object, bool> validation, Action<object, FieldInfo, Type, object> onAction, bool nonPublic = false) {
			if(object.ReferenceEquals(obj, null) || validation == null)
				return;
			if(!(obj is UnityEngine.Object) && validation(obj)) {
				return;
			}
			if(obj is MemberData) {
				MemberData mData = obj as MemberData;
				if(mData != null && mData.instance is MemberData) {
					AnalizeObject(mData.instance, validation, onAction, nonPublic);
				}
				return;
			}
			if(obj is EventData) {
				EventData member = obj as EventData;
				if(member != null && member.blocks.Count > 0) {
					foreach(EventActionData action in member.blocks) {
						if(validation(action)) {
							continue;
						}
						AnalizeObject(action.block, validation, onAction, nonPublic);
					}
				}
				return;
			} else if(obj is IList) {
				IList list = obj as IList;
				for(int i = 0; i < list.Count; i++) {
					object element = list[i];
					if(element == null || validation(element) || element is UnityEngine.Object)
						continue;
					AnalizeObject(element, validation, onAction, nonPublic);
				}
				return;
			}
			FieldInfo[] fieldInfo = ReflectionUtils.GetFields(obj, nonPublic ? flags | BindingFlags.NonPublic : flags);
			foreach(FieldInfo field in fieldInfo) {
				Type fieldType = field.FieldType;
				if(!fieldType.IsClass)
					continue;
				object value = field.GetValueOptimized(obj);
				if(object.ReferenceEquals(value, null))
					continue;
				if(validation(value)) {
					if(onAction != null) {
						onAction(obj, field, fieldType, value);
					}
					continue;
				}
				if(value is UnityEngine.Object) {
					continue;
				}
				AnalizeObject(value, validation, onAction, nonPublic);
				if(onAction != null) {
					onAction(obj, field, fieldType, value);
				}
			}
		}

		/// <summary>
		/// Perform field reflection in obj.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="validation"></param>
		/// <param name="onAction">Parameter: owner or the FieldInfo, field, type, value. </param>
		public static void AnalizeField(object obj, Func<AnalizerData, bool> validation, bool nonPublic = false) {
			if(object.ReferenceEquals(obj, null) || validation == null)
				return;
			if(obj is MemberData) {
				MemberData mData = obj as MemberData;
				if(mData != null && mData.instance is MemberData) {
					AnalizeField(mData.instance, validation, nonPublic);
				}
				return;
			}
			if(obj is EventData) {
				EventData member = obj as EventData;
				if(member != null && member.blocks.Count > 0) {
					foreach(EventActionData action in member.blocks) {
						AnalizeField(action.block, validation, nonPublic);
					}
				}
				return;
			} else if(obj is IList) {
				IList list = obj as IList;
				for(int i = 0; i < list.Count; i++) {
					object element = list[i];
					if(element == null || !element.GetType().IsClass || element is UnityEngine.Object)
						continue;
					AnalizeField(element, validation, nonPublic);
				}
				return;
			}
			FieldInfo[] fieldInfo = ReflectionUtils.GetFields(obj, nonPublic ? flags | BindingFlags.NonPublic : flags);
			foreach(FieldInfo field in fieldInfo) {
				Type fieldType = field.FieldType;
				if(!fieldType.IsClass)
					continue;
				object value = field.GetValueOptimized(obj);
				if(object.ReferenceEquals(value, null))
					continue;
				if(validation(new AnalizerData(obj, field, fieldType, value))) {
					continue;
				}
				if(value is UnityEngine.Object) {
					continue;
				}
				AnalizeField(value, validation, nonPublic);
			}
		}

		/// <summary>
		/// Retarget node owner.
		/// </summary>
		/// <param name="fromOwner"></param>
		/// <param name="toOwner"></param>
		/// <param name="nodes"></param>
		public static void RetargetNodeOwner(uNodeRoot fromOwner, uNodeRoot toOwner, IList<uNodeComponent> nodes) {
			Object[] from = new Object[] { fromOwner, fromOwner.transform, fromOwner.gameObject };
			Object[] to = new Object[] { toOwner, toOwner.transform, toOwner.gameObject };
			foreach(var behavior in nodes) {
				if(behavior is Node) {
					Node node = behavior as Node;
					node.owner = toOwner;
				} else if(behavior is RootObject) {
					RootObject root = behavior as RootObject;
					root.owner = toOwner;
				}
				AnalizeObject(behavior, (obj) => {
					if(obj is MemberData) {
						MemberData data = obj as MemberData;
						data.RefactorUnityObject(from, to);
						//return true;
					}
					return false;
				}, (instance, field, type, value) => {
					Object o = value as Object;
					if(o) {
						if(o == fromOwner) {
							field.SetValueOptimized(instance, toOwner);
						} else if(o == from[1]) {
							field.SetValueOptimized(instance, to[1]);
						} else if(o == from[2]) {
							field.SetValueOptimized(instance, to[2]);
						}
					}
				});
			}
		}

		/// <summary>
		/// Get retarget node Action
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="nodes"></param>
		/// <returns></returns>
		public static Action<uNodeRoot> GetRetargetNodeOwnerAction(uNodeRoot graph, IList<MonoBehaviour> nodes) {
			Action<uNodeRoot> action = null;
			var fromTR = graph.transform;
			var fromGO = graph.gameObject;
			var from = new Object[] { graph, fromTR, fromGO };
			void Func(MonoBehaviour script) {
				AnalizeObject(script, (obj) => {
					if(obj is MemberData) {
						MemberData data = obj as MemberData;
						if(data.targetReference != null) {
							for(int i = 0; i < data.targetReference.Count; i++) {
								Object o = data.targetReference[i];
								int index = i;
								if(o == graph) {
									action += (owner) => {
										data.targetReference[index] = owner;
									};
								} else if(o == fromTR) {
									action += (owner) => {
										data.targetReference[index] = owner.transform;
									};
								} else if(o == fromGO) {
									action += (owner) => {
										data.targetReference[index] = owner.gameObject;
									};
								}
							}
						}
						if(data.odinTargetData?.references != null) {
							for(int i = 0; i < data.odinTargetData.references.Count; i++) {
								Object o = data.odinTargetData.references[i];
								int index = i;
								if(o == graph) {
									action += (owner) => {
										data.odinTargetData.references[index] = owner;
									};
								} else if(o == fromTR) {
									action += (owner) => {
										data.odinTargetData.references[index] = owner.transform;
									};
								} else if(o == fromGO) {
									action += (owner) => {
										data.odinTargetData.references[index] = owner.gameObject;
									};
								}
							}
						}
						if(data.odinInstanceData?.references != null) {
							for(int i = 0; i < data.odinInstanceData.references.Count; i++) {
								Object o = data.odinInstanceData.references[i];
								int index = i;
								if(o == graph) {
									action += (owner) => {
										data.odinInstanceData.references[index] = owner;
									};
								} else if(o == fromTR) {
									action += (owner) => {
										data.odinInstanceData.references[index] = owner.transform;
									};
								} else if(o == fromGO) {
									action += (owner) => {
										data.odinInstanceData.references[index] = owner.gameObject;
									};
								}
							}
						}
						if(data.StartSerializedType.references != null) {
							for(int i = 0; i < data.StartSerializedType.references.Count; i++) {
								Object o = data.StartSerializedType.references[i];
								int index = i;
								if(o == graph) {
									action += (owner) => {
										data.StartSerializedType.references[index] = owner;
									};
								} else if(o == fromTR) {
									action += (owner) => {
										data.StartSerializedType.references[index] = owner.transform;
									};
								} else if(o == fromGO) {
									action += (owner) => {
										data.StartSerializedType.references[index] = owner.gameObject;
									};
								}
							}
						}
						if(data.TargetSerializedType.references != null) {
							for(int i = 0; i < data.TargetSerializedType.references.Count; i++) {
								Object o = data.TargetSerializedType.references[i];
								int index = i;
								if(o == graph) {
									action += (owner) => {
										data.TargetSerializedType.references[index] = owner;
									};
								} else if(o == fromTR) {
									action += (owner) => {
										data.TargetSerializedType.references[index] = owner.transform;
									};
								} else if(o == fromGO) {
									action += (owner) => {
										data.TargetSerializedType.references[index] = owner.gameObject;
									};
								}
							}
						}
						//if(data.HasUnityReference(from)) {
						//	var act = data.GetActionForRefactorUnityObject(from);
						//	action += (owner) => {
						//		act(new Object[] { owner, owner.transform, owner.gameObject });
						//	};
						//}
						//return true;
					}
					return false;
				}, (instance, field, type, value) => {
					Object o = value as Object;
					if(o) {
						if(o == graph) {
							action += (owner) => {
								field.SetValueOptimized(instance, owner);
							};
						} else if(o == fromTR) {
							action += (owner) => {
								field.SetValueOptimized(instance, owner.transform);
							};
						} else if(o == fromGO) {
							action += (owner) => {
								field.SetValueOptimized(instance, owner.gameObject);
							};
						}
					}
				}, nonPublic: true);
			}
			Func(graph);
			if(nodes != null) {
				foreach(var behavior in nodes) {
					if(behavior is Node) {
						Node node = behavior as Node;
						action += (owner) => {
							node.owner = owner;
						};
					} else if(behavior is RootObject) {
						RootObject root = behavior as RootObject;
						action += (owner) => {
							root.owner = owner;
						};
					}
					Func(behavior);
				}
			}
			return action;
		}

		/// <summary>
		/// Retarget node owner.
		/// </summary>
		/// <param name="fromOwner"></param>
		/// <param name="toOwner"></param>
		/// <param name="nodes"></param>
		public static void RetargetNodeOwner(uNodeRoot fromOwner, uNodeRoot toOwner, IList<MonoBehaviour> nodes, Action<object> valueAction = null) {
			Object[] from = new Object[] { fromOwner, fromOwner.transform, fromOwner.gameObject };
			Object[] to = new Object[] { toOwner, toOwner.transform, toOwner.gameObject };
			void Func(MonoBehaviour script) {
				AnalizeObject(script, (obj) => {
					if(valueAction != null) {
						valueAction(obj);
					}
					if(obj is MemberData) {
						MemberData data = obj as MemberData;
						data.RefactorUnityObject(from, to);
						//return true;
					}
					return false;
				}, (instance, field, type, value) => {
					Object o = value as Object;
					if(o) {
						if(o == fromOwner) {
							field.SetValueOptimized(instance, toOwner);
						} else if(o == from[1]) {
							field.SetValueOptimized(instance, to[1]);
						} else if(o == from[2]) {
							field.SetValueOptimized(instance, to[2]);
						}
					}
				}, nonPublic: true);
			}
			foreach(var behavior in nodes) {
				if(behavior is Node) {
					Node node = behavior as Node;
					node.owner = toOwner;
				} else if(behavior is RootObject) {
					RootObject root = behavior as RootObject;
					root.owner = toOwner;
				}
				Func(behavior);
			}
		}
    }
}