﻿using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaxyGames.uNode.Editors {
	[ControlField(typeof(MemberData))]
	public class MemberControl : ValueControl {
		ValueControl control;
		PopupElement popupElement;
		MemberData member;

		string oldRichText;
		bool hideInstance = false;
		bool showInstance;

		public MemberControl(ControlConfig config, bool autoLayout = false) : base(config, autoLayout) {
			Init();
		}

		void Init() {
			style.flexDirection = FlexDirection.Row;
			popupElement = new PopupElement();
			popupElement.richText = UIElementUtility.Theme.coloredReference;
			popupElement.visible = false;
			popupElement.AddManipulator(new LeftMouseClickable(PopupClick));
			popupElement.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
			member = config.value as MemberData;
			if(member == null) {
				member = MemberData.none;
				config.OnValueChanged(member);
			}

			if(config.filter == null) {
				config.filter = new FilterAttribute();
			}
			UpdateControl();
		}

		public void HideInstance(bool hide) {
			hideInstance = hide;
			UpdateControl();
		}

		public void UpdateControl() {
			if(control != null) {
				control.RemoveFromHierarchy();
				control = null;
				showInstance = false;
			}
			Type targetType = member.type;
			if(member.isTargeted) {
				tooltip = member.Tooltip;
				//if(config.filter != null) {
				//	tooltip += "\n\nType Filter:\t" + config.filter.Tooltip;
				//}
			}
			if(member.targetType == MemberData.TargetType.Values && targetType != null) {
				Type portType = targetType;
				// var mVal = member.Get();
				// if(mVal != null && !mVal.GetType().IsCastableTo(portType)) {
				// 	portType = mVal.GetType();
				// }
				var controlAtts = UIElementUtility.FindControlsField();
				ControlConfig config = new ControlConfig() {
					owner = this.config.owner,
					value = member.Get(),
					filter = this.config.filter,
					type = portType,
					onValueChanged = (val) => {
						this.config.OnValueChanged(MemberData.CreateFromValue(val, member.type));
						MarkDirtyRepaint();
					},
				};
				if(config.value == null && config.type.IsValueType) {
					//Ensure to initialize value for value type since value type cannot be null
					config.value = ReflectionUtils.CreateInstance(config.type);
					member.CopyFrom(MemberData.CreateFromValue(config.value));
				}
				control = UIElementUtility.CreateControl(portType, config, ClassListContains("Layout"));
				if(control is ObjectControl && config.value as uNodeRoot == config.owner.targetNode.owner) {
					control = new DefaultControl(config, ClassListContains("Layout"));
				}
				if(control is StringControl && ClassListContains("multiline")) {
					(control as StringControl).field.multiline = true;
				}
				if(popupElement != null && popupElement.visible) {
					popupElement.visible = false;
					popupElement.RemoveFromHierarchy();
				}
			} else if(!member.IsTargetingUNode && !member.isStatic && !hideInstance && member.startType != null) {
				if(config.filter == null) {
					config.filter = new FilterAttribute();
				}
				FilterAttribute filter = config.filter;
				if(filter.UnityReference && !filter.OnlyGetType && !member.targetType.HasFlags(
					MemberData.TargetType.Values |
					MemberData.TargetType.Type |
					MemberData.TargetType.Null |
					MemberData.TargetType.uNodeGenericParameter)) {
					Type portType = member.startType;
					if(member.instance is MemberData) {
						portType = typeof(MemberData);
					}
					if(member.GetInstance() as UnityEngine.Object == config.owner.targetNode.owner) {
						//Self instance
						if(member.targetType != MemberData.TargetType.SelfTarget) {
							control = new PopupControl("this", ClassListContains("Layout"));
						}
					} else {
						CreateInstanceControl(portType);
					}
					showInstance = true;
				}
			}
			if(popupElement != null) {
				if(!popupElement.visible && (control == null || showInstance || config.filter == null || config.filter.ValidTargetType != MemberData.TargetType.Values)) {
					UpdatePopupLabel();
					popupElement.visible = true;
					Add(popupElement);
				}
				popupElement.MarkDirtyRepaint();
			}
			if (control != null) {
				control.RemoveFromHierarchy();
				Insert(0, control);
			}
		}

		void CreateInstanceControl(Type instanceType) {
			var controlAtts = UIElementUtility.FindControlsField();
			ControlConfig config = new ControlConfig() {
				owner = this.config.owner,
				value = member.instance,
				filter = new FilterAttribute(member.startType),
				type = member.startType,
				onValueChanged = (val) => {
					bool flag3 = false;
					if(member.instance != null && !member.instance.GetType().IsCastableTo(instanceType)) {
						flag3 = true;
					}
					member.instance = val;
					this.config.OnValueChanged(member);
					MarkDirtyRepaint();
					if(flag3) {
						UpdateControl();
					}
				},
			};
			control = UIElementUtility.CreateControl(instanceType, config, ClassListContains("Layout"));
			if(control is ObjectControl) {
				var objectControl = control as ObjectControl;
				if(config.value != null && !config.value.GetType().IsCastableTo(instanceType)) {
					objectControl.style.backgroundColor = Color.red;
				}
				objectControl.AddManipulator(new ContextualMenuManipulator(evt => {
					object instance = member.instance;
					if(instance != null) {
						if(instance as GameObject) {
							var go = instance as GameObject;
							Component[] comps = go.GetComponents<Component>();
							evt.menu.AppendAction("0-" + typeof(GameObject).Name, (act) => {
								objectControl.config.OnValueChanged(go);
								UpdateControl();
							}, DropdownMenuAction.AlwaysEnabled);
							for(int x = 0; x < comps.Length; x++) {
								Component com = comps[x];
								evt.menu.AppendAction((x + 1) + "-" + com.GetType().Name, (act) => {
									objectControl.config.OnValueChanged(com);
									UpdateControl();
								}, DropdownMenuAction.AlwaysEnabled);
							}
						} else if(instance as Component) {
							var component = instance as Component;
							Component[] comps = component.GetComponents<Component>();
							evt.menu.AppendAction("0-" + typeof(GameObject).Name, (act) => {
								objectControl.config.OnValueChanged(new MemberData(component.gameObject));
								UpdateControl();
							}, DropdownMenuAction.AlwaysEnabled);
							for(int x = 0; x < comps.Length; x++) {
								Component com = comps[x];
								evt.menu.AppendAction((x + 1) + "-" + com.GetType().Name, (act) => {
									objectControl.config.OnValueChanged(com);
									UpdateControl();
								}, DropdownMenuAction.AlwaysEnabled);
							}
						}
					}
					evt.menu.AppendSeparator("");
					if(config.owner.targetNode != null) {
						uNodeRoot UNR = config.owner.targetNode.owner;
						if(UNR != null) {
							GameObject go = UNR.gameObject;
							Component[] comps = go.GetComponents<Component>();
							evt.menu.AppendAction("this uNode/0-" + typeof(GameObject).Name, (act) => {
								objectControl.config.OnValueChanged(go);
								UpdateControl();
							}, DropdownMenuAction.AlwaysEnabled);
							for(int x = 0; x < comps.Length; x++) {
								Component com = comps[x];
								evt.menu.AppendAction("this uNode/" + x + "-" + com.GetType().Name, (act) => {
									objectControl.config.OnValueChanged(com);
									UpdateControl();
								}, DropdownMenuAction.AlwaysEnabled);
							}
						}
					}
					evt.menu.AppendAction("Reset", (act) => {
						objectControl.config.OnValueChanged(null);
						UpdateControl();
					}, DropdownMenuAction.AlwaysEnabled);

				}));
			}
		}

		void PopupClick(MouseUpEvent mouseEvent) {
			var filter = config.filter;
			if(filter.OnlyGetType && filter.CanManipulateArray()) {
				TypeSelectorWindow.ShowWindow(Vector2.zero, filter, delegate (MemberData[] types) {
					uNodeEditorUtility.RegisterUndo(config.owner.targetNode);
					member.CopyFrom(types[0]);
					config.OnValueChanged(member);
					config.owner.OnValueChanged();
					UpdateControl();
					config.owner.MarkRepaint();
				}, new TypeItem[1] { member }).ChangePosition(GUIUtility.GUIToScreenPoint(mouseEvent.mousePosition)).targetObject = config.owner.targetNode;
			} else {
				ItemSelector.ShowWindow(config.owner.targetNode, member, filter, (m) => {
					m.ResetCache();
					config.OnValueChanged(member);
					config.owner.OnValueChanged();
					UpdateControl();
					config.owner.MarkRepaint();
				}).ChangePosition(GUIUtility.GUIToScreenPoint(mouseEvent.mousePosition));
			}
		}

		private void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
			if(config.type != null) {
				if(config.filter == null || !config.filter.SetMember && config.filter.IsValidTarget(MemberData.TargetType.Values)) {
					evt.menu.AppendAction("To Default", (e) => {
						if(config.type == typeof(Type)) {
							member = new MemberData(typeof(object));
						} else if(config.type is RuntimeType) {
							member = MemberData.CreateFromValue(null, config.type);
						} else {
							member = new MemberData(ReflectionUtils.CanCreateInstance(config.type) ? ReflectionUtils.CreateInstance(config.type) : null) {
								startType = config.type,
								type = config.type,
							};
						}
						config.OnValueChanged(member);
						config.owner.OnValueChanged();
						config.owner.MarkRepaint();
						UpdateControl();
						uNodeEditor.GUIChanged();
					}, DropdownMenuAction.AlwaysEnabled);
					if(config.type == typeof(object)) {
						var types = EditorReflectionUtility.GetCommonType();
						foreach(var t in types) {
							evt.menu.AppendAction("To Value/" + t.PrettyName(), (e) => {
								member = new MemberData(ReflectionUtils.CanCreateInstance(t) ? ReflectionUtils.CreateInstance(t) : null) {
									startType = t,
									type = t,
								};
								config.OnValueChanged(member);
								config.owner.OnValueChanged();
								config.owner.MarkRepaint();
								UpdateControl();
								uNodeEditor.GUIChanged();
							}, DropdownMenuAction.AlwaysEnabled);
						}
					} else if(config.type == typeof(Type)) {
						var mPos = NodeGraph.openedGraph.GetMousePosition();
						evt.menu.AppendAction("Change Type", (e) => {
							TypeSelectorWindow.ShowWindow(mPos, new FilterAttribute() { OnlyGetType = true },
								delegate (MemberData[] types) {
									uNodeEditorUtility.RegisterUndo(config.owner.targetNode);
									member.CopyFrom(types[0]);
									config.OnValueChanged(member);
									config.owner.OnValueChanged();
									config.owner.MarkRepaint();
									UpdateControl();
									uNodeEditor.GUIChanged();
								}, new TypeItem[1] { member }).targetObject = config.owner.targetNode;
						}, DropdownMenuAction.AlwaysEnabled);
					}
					if(!config.type.IsValueType) {
						evt.menu.AppendAction("To Null", (e) => {
							uNodeEditorUtility.RegisterUndo(config.owner.targetNode);
							member.CopyFrom(MemberData.Null);
							config.OnValueChanged(member);
							config.owner.OnValueChanged();
							config.owner.MarkRepaint();
							UpdateControl();
							uNodeEditor.GUIChanged();
						}, DropdownMenuAction.AlwaysEnabled);
					}
				}
			}
			if(member.isTargeted) {
				UIElementUtility.ShowReferenceMenu(evt, member);
			}
		}

		void UpdatePopupLabel() {
			if(popupElement != null) {
				if(control == null || showInstance) {
					if(member.IsTargetingPortOrNode) {
						popupElement.text = string.Empty;
					} else {
						popupElement.text = popupElement.richText ? uNodeUtility.GetNicelyDisplayName(config.value) : uNodeUtility.GetDisplayName(config.value);
					}
					popupElement.EnableInClassList("Layout", ClassListContains("Layout"));
				} else {
					popupElement.text = string.Empty;
					popupElement.tooltip = string.Empty;
					popupElement.EnableInClassList("Layout", false);
				}
				if(oldRichText != popupElement.text) {
					if(!string.IsNullOrEmpty(popupElement.text)) {
						popupElement.tooltip = uNodeEditorUtility.RemoveHTMLTag(popupElement.text);
					}
					oldRichText = popupElement.text;
				}
			}
		}

		protected override void ImmediateRepaint() {
			if(uNodeThreadUtility.frame % 2 == 0) return;
			if(visible && resolvedStyle.opacity != 0) {
				UpdatePopupLabel();
			}
		}
	}
}