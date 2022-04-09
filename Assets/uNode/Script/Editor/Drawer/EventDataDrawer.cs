using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using MaxyGames.uNode.Editors;

namespace MaxyGames.Events {
	/// <summary>
	/// Classes for add custom menu in event menu.
	/// </summary>
	public class CustomEventMenu {
		public string menuName = "";
		public bool isSeparator;
		public bool isItemSelector = true;
		public bool isValidationMenu;
		public System.Action onClickMenu;
		public FilterAttribute filter;
		public Func<MemberData, EventActionData> onClickItem;
	}

	public class DragAndDropCapturer {
		public Func<EventData, bool> validation;
		public bool isGenericMenu;
		public List<CustomEventMenu> genericMenu;
		public Action<EventData, UnityEngine.Object> onDragPerformed;
	}

	[CustomPropertyDrawer(typeof(EventData), true)]
	public class EventDataDrawer : PropertyDrawer {
		public static List<CustomEventMenu> customMenu;
		public static List<DragAndDropCapturer> dragAndDropCapturer;

		EventData eventData;
		Block objToEdit {
			get {
				if(eventData.editIndex > eventData.blocks.Count) {
					eventData.editIndex = 0;
				}
				if(eventData.editIndex > 0) {
					return eventData.blocks[eventData.editIndex - 1].block;
				}
				return null;
			}
		}

		public static void ShowContextMenu(
			Action<Type, EventActionData.EventType> action, 
			GenericMenu menu, 
			EventTypeAttribute eventKind = null, 
			string startPath = "", 
			bool showConditionalOr = true) {
			if(eventKind == null) {
				eventKind = new EventTypeAttribute(EventData.EventType.Action);
			}
			string startPathName = "";
			if(startPath != "") {
				startPathName = startPath + "/";
			}
			List<BlockMenuAttribute> actionMenu = new List<BlockMenuAttribute>();
			foreach(BlockMenuAttribute menuItem in FindAllMenu()) {
				if(eventKind.eventType == EventData.EventType.Action) {
					bool isValid = false;
					if(eventKind.type != null && eventKind.type.Length > 0) {
						foreach(Type type in eventKind.type) {
							if(type == null) continue;
							if(type == menuItem.type) {
								isValid = true;
								break;
							} else if(eventKind.includeSubClass && menuItem.type.IsSubclassOf(type)) {
								isValid = true;
								break;
							}
						}
					} else {
						if(menuItem.type.IsSubclassOf(typeof(AnyBlock))) {
							isValid = true;
						} else if(eventKind.includeSubClass && menuItem.type.IsSubclassOf(typeof(AnyBlock))) {
							isValid = true;
						}
						if(menuItem.type.IsSubclassOf(typeof(Action))) {
							isValid = true;
						} else if(eventKind.includeSubClass && menuItem.type.IsSubclassOf(typeof(Action))) {
							isValid = true;
						}
					}
					if(isValid && !eventKind.supportCoroutine && menuItem.isCoroutine) {
						isValid = false;
					}
					if(!isValid) {
						continue;
					}
				} else if(eventKind.eventType == EventData.EventType.Condition) {
					bool isValid = false;
					if(eventKind.type != null && eventKind.type.Length > 0) {
						foreach(Type type in eventKind.type) {
							if(type == null) continue;
							if(eventKind.type != null && type == menuItem.type) {
								isValid = true;
								break;
							} else if(eventKind.type != null && eventKind.includeSubClass && menuItem.type.IsSubclassOf(type)) {
								isValid = true;
								break;
							}
						}
					} else {
						if(menuItem.type.IsSubclassOf(typeof(Block))) {
							isValid = true;
						}
					}
					if(isValid && menuItem.type.IsSubclassOf(typeof(Action))) {
						actionMenu.Add(menuItem);
						isValid = false;
					}
					if(!isValid) {
						continue;
					}
				} else {
					continue;
				}
				menu.AddItem(new GUIContent(startPathName + menuItem.category.Add("/") + menuItem.name.Replace('.', '/')), false, () => {
					action(menuItem.type, EventActionData.EventType.Event);
				});
			}
			if(eventKind == null || eventKind.eventType == EventData.EventType.Condition) {
				bool hasAction = false;
				menu.AddSeparator("");
				foreach(BlockMenuAttribute menuItem in actionMenu) {
					string startName = startPathName;
					if(eventKind.eventType == EventData.EventType.Condition) {
						bool isValid = false;
						if(eventKind.type != null && eventKind.type.Length > 0) {
							foreach(Type type in eventKind.type) {
								if(type == null) continue;
								if(eventKind.type != null && type == menuItem.type) {
									isValid = true;
									break;
								} else if(eventKind.type != null && eventKind.includeSubClass && menuItem.type.IsSubclassOf(type)) {
									isValid = true;
									break;
								}
							}
						} else {
							if(menuItem.type.IsSubclassOf(typeof(Block))) {
								isValid = true;
							} else if(eventKind.includeSubClass && menuItem.type.IsSubclassOf(typeof(Block))) {
								isValid = true;
							}
						}
						if(typeof(Action) == menuItem.type || menuItem.type.IsSubclassOf(typeof(Action))) {
							startName = "Action/" + startName;
						} else if(eventKind.includeSubClass && menuItem.type.IsSubclassOf(typeof(Action))) {
							startName = "Action/" + startName;
						}
						if(!isValid) {
							continue;
						}
					} else {
						continue;
					}
					if(!hasAction)
						hasAction = true;
					menu.AddItem(new GUIContent(startName + menuItem.category.Add("/") + menuItem.name.Replace('.', '/')), false, () => {
						action(menuItem.type, EventActionData.EventType.Event);
					});
				}
				if(showConditionalOr) {
					if(hasAction)
						menu.AddSeparator("");
					menu.AddItem(new GUIContent(startPathName + "Conditional OR operator"), false, () => {
						action(null, EventActionData.EventType.Or);
					});
				}
			}
		}

		private void clickHandler(SerializedProperty property, EventActionData.EventType eType, Type type) {
			eventData = PropertyDrawerUtility.GetActualObjectForSerializedProperty<EventData>(property);
			if(eType == EventActionData.EventType.Event) {//make action event
				if(type.IsSubclassOf(typeof(Block))) {
					Block vEvent = (Block)System.Activator.CreateInstance(type, false);
					EventActionData vaEvent = new EventActionData(vEvent, eType);
					property.FindPropertyRelative("eventList").InsertArrayElementAtIndex(property.FindPropertyRelative("eventList").arraySize);
					property.serializedObject.ApplyModifiedProperties();
					eventData.blocks[eventData.blocks.Count - 1] = vaEvent;
				}
			} else {//make or event
				EventActionData vaEvent = new EventActionData();
				property.FindPropertyRelative("eventList").InsertArrayElementAtIndex(property.FindPropertyRelative("eventList").arraySize);
				property.serializedObject.ApplyModifiedProperties();
				eventData.blocks[eventData.blocks.Count - 1] = vaEvent;
			}
			uNodeGUIUtility.GUIChanged(property.serializedObject.targetObject);
		}

		private class CopyEventData {
			public string Name;
			public List<EventActionData> eventList;
			public bool useLevelValidation;
			public CopyEventData(string Name, List<EventActionData> eventList, bool useLevelValidation = false) {
				this.Name = Name;
				this.eventList = new List<EventActionData>();
				this.useLevelValidation = useLevelValidation;
				foreach(EventActionData Event in eventList) {
					this.eventList.Add(new EventActionData(Event));
				}
			}
			public CopyEventData(string Name, params EventActionData[] eventList) {
				this.Name = Name;
				this.eventList = new List<EventActionData>();
				this.useLevelValidation = false;
				foreach(EventActionData Event in eventList) {
					this.eventList.Add(new EventActionData(Event));
				}
			}

			public CopyEventData(string Name, bool useLevelValidation, params EventActionData[] eventList) {
				this.Name = Name;
				this.eventList = new List<EventActionData>();
				this.useLevelValidation = useLevelValidation;
				foreach(EventActionData Event in eventList) {
					this.eventList.Add(new EventActionData(Event));
				}
			}
		}

		private static CopyEventData CopyAction;
		private static CopyEventData CopyValidation;
		public void ShowContextMenu2(EventData data, EventTypeAttribute item = null, UnityEngine.Object targetObject = null) {
			if(item == null) {
				item = new EventTypeAttribute(EventData.EventType.Action);
			}
			GenericMenu menu = new GenericMenu();
			if(data.blocks.Count > 0) {
				menu.AddItem(new GUIContent("Reset"), false, delegate (object obj) {
					if(targetObject != null) {
						uNodeEditorUtility.RegisterUndo(targetObject, targetObject.name + " Reset EventData");
					}
					EventData ED = obj as EventData;
					ED.blocks.Clear();
				}, data);
			} else {
				menu.AddDisabledItem(new GUIContent("Reset"));
			}
			if(item.eventType == EventData.EventType.Action) {
				menu.AddItem(new GUIContent("Copy Action"), false, delegate (object obj) {
					EventData ED = obj as EventData;
					CopyAction = new CopyEventData("", ED.blocks);
				}, data);
			} else {
				menu.AddItem(new GUIContent("Copy Validation"), false, delegate (object obj) {
					EventData ED = obj as EventData;
					CopyValidation = new CopyEventData("", ED.blocks, ED.useLevelValidation);
				}, data);
			}
			menu.AddSeparator("");
			if(CopyAction != null) {
				menu.AddItem(new GUIContent("Paste Action Event"), false, delegate (object obj) {
					EventData ED = obj as EventData;
					if(ED != null && CopyAction != null && CopyAction.eventList != null) {
						if(targetObject != null) {
							uNodeEditorUtility.RegisterUndo(targetObject, targetObject.name + " Paste Validation");
						}
						ED.blocks.Clear();
						foreach(EventActionData Event in CopyAction.eventList) {
							ED.blocks.Add(new EventActionData(Event));
						}
					}
				}, data);
				menu.AddItem(new GUIContent("Paste Action As New"), false, delegate (object obj) {
					EventData ED = obj as EventData;
					if(ED != null && CopyAction != null && CopyAction.eventList != null) {
						if(targetObject != null) {
							uNodeEditorUtility.RegisterUndo(targetObject, targetObject.name + " Paste Validation");
						}
						//ED.eventList.Clear();
						foreach(EventActionData Event in CopyAction.eventList) {
							ED.blocks.Add(new EventActionData(Event));
						}
					}
				}, data);
			} else {
				menu.AddDisabledItem(new GUIContent("Paste Action Event"));
				menu.AddDisabledItem(new GUIContent("Paste Action As New"));
			}
			if(CopyValidation != null && item.eventType == EventData.EventType.Condition) {
				menu.AddItem(new GUIContent("Paste Validation Event"), false, delegate (object obj) {
					EventData ED = obj as EventData;
					if(ED != null && CopyValidation != null && CopyValidation.eventList != null) {
						if(targetObject != null) {
							uNodeEditorUtility.RegisterUndo(targetObject, targetObject.name + " Paste Validation");
						}
						ED.blocks.Clear();
						foreach(EventActionData Event in CopyValidation.eventList) {
							ED.blocks.Add(new EventActionData(Event));
						}
						if(CopyValidation.useLevelValidation)
							ED.useLevelValidation = CopyValidation.useLevelValidation;
					}
				}, data);
				menu.AddItem(new GUIContent("Paste Validation As New"), false, delegate (object obj) {
					EventData ED = obj as EventData;
					if(ED != null && CopyValidation != null && CopyValidation.eventList != null) {
						if(targetObject != null) {
							uNodeEditorUtility.RegisterUndo(targetObject, targetObject.name + " Paste Validation");
						}
						//ED.eventList.Clear();
						foreach(EventActionData Event in CopyValidation.eventList) {
							ED.blocks.Add(new EventActionData(Event));
						}
						if(CopyValidation.useLevelValidation)
							ED.useLevelValidation = CopyValidation.useLevelValidation;
					}
				}, data);
			} else {
				menu.AddDisabledItem(new GUIContent("Paste Validation Event"));
				menu.AddDisabledItem(new GUIContent("Paste Validation As New"));
			}
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Use Level"), data.useLevelValidation, delegate (object obj) {
				EventData ED = obj as EventData;
				if(ED != null) {
					if(targetObject != null) {
						uNodeEditorUtility.RegisterUndo(targetObject, targetObject.name + " Use Level");
					}
					ED.useLevelValidation = !ED.useLevelValidation;
				}
			}, data);
			menu.ShowAsContext();
		}

		public void ShowContextMenu3(EventData data, SerializedProperty property, int index, EventTypeAttribute item, UnityEngine.Object targetObject) {
			if(item == null) {
				item = new EventTypeAttribute(EventData.EventType.Action);
			}
			GenericMenu menu = new GenericMenu();
			if(property != null) {
				menu.AddItem(new GUIContent("Edit"), false, delegate (object obj) {
					//EventData ED = obj as EventData;
					FieldsEditorWindow window = FieldsEditorWindow.ShowWindow();
					window.titleContent = new GUIContent(data.blocks[index].block.GetType().Name);
					window.propertyPath = property.propertyPath;
					window.actionIndex = index;
					window.targetObject = property.serializedObject.targetObject;
					window.targetField = data.blocks[index].block;
				}, data);
				Type scriptType = null;
				if(data.blocks[index].block != null) {
					scriptType = data.blocks[index].block.GetType();
				}
				if(scriptType != null && uNodeEditorUtility.MonoScripts != null && uNodeEditorUtility.MonoScripts.Length > 0) {
					MonoScript mc = uNodeEditorUtility.GetMonoScript(scriptType);
					if(mc != null) {
						menu.AddSeparator("");
						menu.AddItem(new GUIContent("Find Script"), false, delegate (object obj) {
							EditorGUIUtility.PingObject(obj as MonoScript);
						}, mc);
						menu.AddItem(new GUIContent("Edit Script"), false, delegate (object obj) {
							AssetDatabase.OpenAsset(obj as MonoScript);
						}, mc);
						menu.AddSeparator("");
					}
				}
			}
			if(item.eventType == EventData.EventType.Action) {
				menu.AddItem(new GUIContent("Copy Action"), false, delegate (object obj) {
					EventData ED = obj as EventData;
					CopyAction = new CopyEventData("", ED.blocks[index]);
				}, data);
			} else {
				menu.AddItem(new GUIContent("Copy Validation"), false, delegate (object obj) {
					EventData ED = obj as EventData;
					CopyValidation = new CopyEventData("", ED.blocks[index]);
				}, data);
			}
			menu.ShowAsContext();
		}

		public override float GetPropertyHeight(SerializedProperty property, UnityEngine.GUIContent label) {
			eventData = PropertyDrawerUtility.GetActualObjectForSerializedProperty<EventData>(property);
			Rect eventRect = new Rect();
			float fieldHeight = 0;
			if(property.isExpanded && eventData != null) {
				eventRect.y += 10;
				if(eventData.blocks != null && eventData.blocks.Count > 0) {
					for(var i = 0; i < eventData.blocks.Count; i++) {
						eventRect.height += EditorGUIUtility.singleLineHeight;
					}
				} else {
					eventRect.height += EditorGUIUtility.singleLineHeight;
				}
				eventRect.height += EditorGUIUtility.singleLineHeight;
			}
			return EditorGUIUtility.singleLineHeight + 5 + eventRect.height + fieldHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, UnityEngine.GUIContent label) {
			position = EditorGUI.IndentedRect(position);
			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EventTypeAttribute ItemAtt = null;
			{
				if(fieldInfo.GetCustomAttributes(typeof(EventTypeAttribute), true).Length > 0) {
					ItemAtt = (EventTypeAttribute)fieldInfo.GetCustomAttributes(typeof(EventTypeAttribute), false)[0];
				}
			}
			eventData = PropertyDrawerUtility.GetActualObjectForSerializedProperty<EventData>(property);
			{
				Rect rect = position;
				rect.height = EditorGUIUtility.singleLineHeight;
				int controlID = GUIUtility.GetControlID(FocusType.Passive, rect);
				if(Event.current.type == EventType.Repaint) {
					uNodeGUIStyle.headerStyle.Draw(rect, GUIContent.none, controlID);
				}
				if(dragAndDropCapturer != null && dragAndDropCapturer.Count > 0 && rect.Contains(Event.current.mousePosition)) {
					if((Event.current.type == EventType.DragPerform || Event.current.type == EventType.DragUpdated)) {
						DragAndDropCapturer capturer = null;
						foreach(var drag in dragAndDropCapturer) {
							if(drag.validation(eventData)) {
								capturer = drag;
								break;
							}
						}
						if(capturer != null) {
							DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						}
						if(capturer != null && Event.current.type == EventType.DragPerform) {
							DragAndDrop.AcceptDrag();
							if(capturer.isGenericMenu) {

							} else {
								capturer.onDragPerformed(eventData, property.serializedObject.targetObject);
							}
						}
					}
				}
				rect.x += 15;
				rect.width -= 55;
				bool isExpanded = property.isExpanded;
				string toolTip = "";
				if(fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true).Length > 0) {
					TooltipAttribute Item = (TooltipAttribute)fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), false)[0];
					toolTip = Item.tooltip;
				}
				if(rect.Contains(Event.current.mousePosition) && Event.current.button == 1 && Event.current.type == EventType.MouseUp) {
					ShowContextMenu2(eventData, ItemAtt, property.serializedObject.targetObject);
				}
				isExpanded = EditorGUI.Foldout(rect, isExpanded, new GUIContent(property.displayName, toolTip), uNodeGUIStyle.FoldoutBold);
				Rect editRect = EditorGUI.IndentedRect(position);
				editRect.x += editRect.width - 40;
				editRect.width = 40;
				editRect.height = EditorGUIUtility.singleLineHeight + 3.3f;
				/*
				EditorGUI.BeginDisabledGroup(objToEdit == null);
				if(GUI.Button(editRect, new GUIContent("Edit", "Edit Variable in Selected Event"), EditorStyles.miniButtonLeft)) {

				}
				EditorGUI.EndDisabledGroup();
				*/
				property.isExpanded = isExpanded;
			}
			Rect eventRect = position;
			eventRect.y = position.y + 5;
			eventRect.x += 5;
			eventRect.width -= 10;
			if(property.isExpanded && eventData != null) {
				bool isActionEvent = false;
				if(ItemAtt != null) {
					isActionEvent = ItemAtt.eventType == EventData.EventType.Action;
				}
				if(Event.current.type == EventType.Repaint) {//Background
					int eventCount = 1;
					if(eventData.blocks != null && eventData.blocks.Count > 0) {
						eventCount = eventData.blocks.Count;
					}
					Rect bgRect = new Rect(eventRect.x - 5,
						eventRect.y + EditorGUIUtility.singleLineHeight - 3,
						eventRect.width + 10,
						EditorGUIUtility.singleLineHeight * (eventCount + 1) + 9);
					uNodeGUIStyle.backgroundStyle.Draw(bgRect, false, false, false, false);
				}
				if(eventData.blocks != null && eventData.blocks.Count > 0 && !isActionEvent) {
					for(var i = 0; i < eventData.blocks.Count; i++) {
						eventRect.height = EditorGUIUtility.singleLineHeight;
						eventRect.y += EditorGUIUtility.singleLineHeight;
						Rect eventPos = eventRect;
						if(eventData.useLevelValidation && eventData.blocks[i].levelValidation > 0) {
							eventPos.width -= 5 * eventData.blocks[i].levelValidation;
							eventPos.x += 5 * eventData.blocks[i].levelValidation;
						}
						if(eventData.blocks[i].block is MissingEvent) {
							MissingEvent missingEvent = eventData.blocks[i].block as MissingEvent;
							if(!missingEvent.hasShowLog) {
								Debug.LogWarning(eventData.blocks[i].TypeName + " Type not found", property.serializedObject.targetObject);
								missingEvent.hasShowLog = true;
							}
							if(eventPos.Contains(Event.current.mousePosition) && (Event.current.button == 0 && Event.current.clickCount == 2 || Event.current.button == 1 && Event.current.type == EventType.MouseUp)) {
								FilterAttribute filter = new FilterAttribute(typeof(Block));
								filter.OnlyGetType = true;
								EventActionData actionData = eventData.blocks[i];
								ItemSelector.ShowWindow(null, filter, delegate (MemberData value) {
									Type RType = value.type;
									if(RType != null && (RType.IsSubclassOf(typeof(Block)) || RType == typeof(Block))) {
										uNodeEditorUtility.RegisterUndo(property.serializedObject.targetObject, "Change Event Type: " + RType.FullName);
										actionData.TypeName = RType.FullName;
										actionData.OnAfterDeserialize();
									}
								}, true).ChangePosition(eventPos.ToScreenRect()).usingNamespaces.Add("MaxyGames.Events");
							}
							GUI.contentColor = Color.red;
							if(eventData.editIndex == i + 1) {
								GUI.backgroundColor = Color.green;
							}
							if(GUI.Button(eventPos, new GUIContent("<size=11>Missing Block</size>", eventData.blocks[i].TypeName +
								"\n\nRight Click to select correct type"), uNodeGUIStyle.ButtonRichText)) {
								eventData.editIndex = i + 1;
							}
							GUI.contentColor = Color.white;
							GUI.backgroundColor = Color.white;
							continue;
						}
						if(eventData.blocks[i].eventType == EventActionData.EventType.Event) {
							if(eventPos.Contains(Event.current.mousePosition) && Event.current.button == 0 && Event.current.clickCount == 2) {
								if(eventData.blocks[i].block == null) {
									FilterAttribute filter = new FilterAttribute(typeof(Block));
									filter.OnlyGetType = true;
									filter.DisplayAbstractType = false;
									EventActionData actionData = eventData.blocks[i];
									ItemSelector.ShowWindow(null, filter, delegate (MemberData value) {
										System.Type RType = value.type;
										if(RType != null && (RType.IsSubclassOf(typeof(Block)) || RType == typeof(Block))) {
											uNodeEditorUtility.RegisterUndo(property.serializedObject.targetObject, "Change Event Type: " + RType.FullName);
											actionData.TypeName = RType.FullName;
											actionData.OnAfterDeserialize();
										}
									}, true).ChangePosition(eventPos.ToScreenRect()).usingNamespaces.Add("MaxyGames.Events");
								} else {
									FieldsEditorWindow window = FieldsEditorWindow.ShowWindow();
									window.titleContent = new GUIContent(eventData.blocks[i].block.GetType().Name);
									window.propertyPath = property.propertyPath;
									window.actionIndex = i;
									window.targetObject = property.serializedObject.targetObject;
									window.targetField = eventData.blocks[i].block;
								}
							}
							if(eventPos.Contains(Event.current.mousePosition) && Event.current.button == 1 && Event.current.type == EventType.MouseUp) {
								ShowContextMenu3(eventData, property, i, ItemAtt, property.serializedObject.targetObject);
							}
							/*if(eventData.eventList[i].actionEvent == null) {
								eventData.eventList.RemoveAt(i);
								i--;
								continue;
							}*/
							string s = "";
							if(eventData.editIndex == i + 1) {
								GUI.backgroundColor = Color.green;
							}
							if(i == 0) {
								if(eventData.blocks.Count == 1) {
									s = ")";
								} else {
									s = " &&";
								}
								if(GUI.Button(eventPos, new GUIContent("if(<size=11>" + eventData.blocks[i].displayName + "</size>" + s, uNodeEditorUtility.RemoveHTMLTag(eventData.blocks[i].toolTip)), uNodeGUIStyle.ButtonRichText)) {
									eventData.editIndex = i + 1;
								}
							} else if(i == eventData.blocks.Count - 1) {
								if(GUI.Button(eventPos, new GUIContent("<size=11>" + eventData.blocks[i].displayName + ")</size>", uNodeEditorUtility.RemoveHTMLTag(eventData.blocks[i].toolTip)), uNodeGUIStyle.ButtonRichText)) {
									eventData.editIndex = i + 1;
								}
							} else {
								if(eventData.blocks[i + 1].eventType != EventActionData.EventType.Or) {
									s = " &&";
								}
								if(GUI.Button(eventPos, new GUIContent("<size=11>" + eventData.blocks[i].displayName + "</size>" + s, uNodeEditorUtility.RemoveHTMLTag(eventData.blocks[i].toolTip)), uNodeGUIStyle.ButtonRichText)) {
									eventData.editIndex = i + 1;
								}
							}
							GUI.backgroundColor = Color.white;
						} else {
							if(i == eventData.blocks.Count - 1) {
								GUI.contentColor = Color.red;
							} else if(eventData.blocks[i + 1].eventType == EventActionData.EventType.Or) {
								GUI.contentColor = Color.red;
							} else if(i == 0) {
								GUI.contentColor = Color.red;
							} else if(eventData.blocks[i - 1].eventType == EventActionData.EventType.Or) {
								GUI.contentColor = Color.red;
							} else if(eventData.useLevelValidation) {
								if(i >= 0 && eventData.blocks[i - 1].levelValidation < eventData.blocks[i].levelValidation) {
									GUI.contentColor = Color.red;
								} else if(i < eventData.blocks.Count && eventData.blocks[i + 1].levelValidation < eventData.blocks[i].levelValidation) {
									GUI.contentColor = Color.red;
								}
							}

							if(eventData.editIndex == i + 1) {
								GUI.backgroundColor = Color.green;
							}
							GUIStyle style = uNodeGUIStyle.ButtonRichText;
							style.alignment = TextAnchor.MiddleCenter;
							if(GUI.Button(eventPos, new GUIContent("||", "The Conditional OR operator"), style)) {
								eventData.editIndex = i + 1;
							}
							style.alignment = TextAnchor.MiddleLeft;
							GUI.contentColor = Color.white;
							GUI.backgroundColor = Color.white;
						}
					}
					eventRect.y += 2;
				} else if(eventData.blocks != null && eventData.blocks.Count > 0 && isActionEvent) {
					for(var i = 0; i < eventData.blocks.Count; i++) {
						eventRect.height = EditorGUIUtility.singleLineHeight;
						eventRect.y += EditorGUIUtility.singleLineHeight;
						Rect eventPos = eventRect;
						if(eventData.blocks[i].block is MissingEvent) {
							MissingEvent missingEvent = eventData.blocks[i].block as MissingEvent;
							if(!missingEvent.hasShowLog) {
								Debug.LogWarning(eventData.blocks[i].TypeName + " Type not found", property.serializedObject.targetObject);
								missingEvent.hasShowLog = true;
							}
							if(eventPos.Contains(Event.current.mousePosition) && (Event.current.button == 0 && Event.current.clickCount == 2 || Event.current.button == 1 && Event.current.type == EventType.MouseUp)) {
								FilterAttribute filter = new FilterAttribute(typeof(Block));
								filter.OnlyGetType = true;
								EventActionData actionData = eventData.blocks[i];
								ItemSelector.ShowWindow(null, filter, delegate (MemberData value) {
									System.Type RType = value.type;
									if(RType != null && (RType.IsSubclassOf(typeof(Block)) || RType == typeof(Block))) {
										uNodeEditorUtility.RegisterUndo(property.serializedObject.targetObject, "Change Event Type: " + RType.FullName);
										actionData.TypeName = RType.FullName;
										actionData.OnAfterDeserialize();
									}
								}, true).ChangePosition(eventPos.ToScreenRect()).usingNamespaces.Add("MaxyGames.Events");
							}
							GUI.contentColor = Color.red;
							if(eventData.editIndex == i + 1) {
								GUI.backgroundColor = Color.green;
							}
							if(GUI.Button(eventPos, new GUIContent("Missing Block", eventData.blocks[i].TypeName +
								"\n\nRight Click to select correct type"), uNodeGUIStyle.ButtonRichText)) {
								eventData.editIndex = i + 1;
							}
							GUI.contentColor = Color.white;
							GUI.backgroundColor = Color.white;
							continue;
						}
						if(eventData.blocks[i].eventType == EventActionData.EventType.Event) {
							if(eventPos.Contains(Event.current.mousePosition) && Event.current.button == 0 && Event.current.clickCount == 2) {
								if(eventData.blocks[i].block == null) {
									FilterAttribute filter = new FilterAttribute(typeof(Block));
									filter.OnlyGetType = true;
									EventActionData actionData = eventData.blocks[i];
									ItemSelector.ShowWindow(null, filter, delegate (MemberData value) {
										System.Type RType = value.type;
										if(RType != null && (RType.IsSubclassOf(typeof(Block)) || RType == typeof(Block))) {
											uNodeEditorUtility.RegisterUndo(property.serializedObject.targetObject, "Change Event Type: " + RType.FullName);
											actionData.TypeName = RType.FullName;
											actionData.OnAfterDeserialize();
										}
									}, true).ChangePosition(eventPos.ToScreenRect()).usingNamespaces.Add("MaxyGames.Events");
								} else {
									FieldsEditorWindow window = FieldsEditorWindow.ShowWindow();
									window.titleContent = new GUIContent(eventData.blocks[i].block.GetType().Name);
									window.propertyPath = property.propertyPath;
									window.actionIndex = i;
									window.targetObject = property.serializedObject.targetObject;
									window.targetField = eventData.blocks[i].block;
								}
							}
							if(eventPos.Contains(Event.current.mousePosition) && Event.current.button == 1 && Event.current.type == EventType.MouseUp) {
								ShowContextMenu3(eventData, property, i, ItemAtt, property.serializedObject.targetObject);
							}
							/*if(eventData.eventList[i].actionEvent == null) {
								eventData.eventList.RemoveAt(i);
								i--;
								continue;
							}*/
							if(eventData.editIndex == i + 1) {
								GUI.backgroundColor = Color.green;
							}
							if(GUI.Button(eventPos, new GUIContent(eventData.blocks[i].displayName, uNodeEditorUtility.RemoveHTMLTag(eventData.blocks[i].toolTip)), uNodeGUIStyle.ButtonRichText)) {
								eventData.editIndex = i + 1;
							}
							GUI.backgroundColor = Color.white;
						} else {
							eventData.blocks.RemoveAt(i);
							i--;
						}
					}
					eventRect.y += 2;
				} else {
					eventRect.height = EditorGUIUtility.singleLineHeight;
					eventRect.y += EditorGUIUtility.singleLineHeight;
					GUI.Label(eventRect, "No Event");
				}
				Rect toolRect = eventRect;
				toolRect.height = EditorGUIUtility.singleLineHeight - 2;
				toolRect.y += EditorGUIUtility.singleLineHeight;
				toolRect.width = 40;
				EditorGUI.BeginDisabledGroup(eventData.editIndex == 0);
				EditorGUI.BeginDisabledGroup(eventData.editIndex == eventData.blocks.Count);
				if(GUI.Button(toolRect, new GUIContent("Down", "Move Down Selected Event"), EditorStyles.miniButton)) {
					var temp = eventData.blocks[eventData.editIndex - 1];
					eventData.blocks.RemoveAt(eventData.editIndex - 1);
					eventData.blocks.Insert(eventData.editIndex, temp);
					eventData.editIndex++;
				}
				EditorGUI.EndDisabledGroup();
				toolRect.x += 40;
				EditorGUI.BeginDisabledGroup(eventData.editIndex <= 1);
				if(GUI.Button(toolRect, new GUIContent("Up", "Move Up Selected Event"), EditorStyles.miniButton)) {
					var temp = eventData.blocks[eventData.editIndex - 1];
					eventData.blocks.RemoveAt(eventData.editIndex - 1);
					eventData.blocks.Insert(eventData.editIndex - 2, temp);
					eventData.editIndex--;
				}
				EditorGUI.EndDisabledGroup();
				toolRect.x += 40;
				toolRect.width = 25;
				if(GUI.Button(toolRect, new GUIContent("+", "Duplicate Selected Event"), EditorStyles.miniButton)) {
					property.FindPropertyRelative("eventList").InsertArrayElementAtIndex(eventData.editIndex - 1);
					property.serializedObject.ApplyModifiedProperties();
				}
				toolRect.x += 25;
				toolRect.width = 25;
				if(GUI.Button(toolRect, new GUIContent("-", "Remove Selected Event"), EditorStyles.miniButton)) {
					property.FindPropertyRelative("eventList").DeleteArrayElementAtIndex(eventData.editIndex - 1);
					property.serializedObject.ApplyModifiedProperties();

					if(eventData.blocks.Count < eventData.editIndex) {
						eventData.editIndex = eventData.blocks.Count;
					}
				}
				int decreaseW = 0;
				if(eventData.useLevelValidation && eventData != null) {
					toolRect.x += 25;
					toolRect.width = 25;
					EditorGUI.BeginDisabledGroup(eventData.editIndex == 0 || eventData.blocks[eventData.editIndex - 1].levelValidation == 0);
					if(GUI.Button(toolRect, new GUIContent("<", ""), EditorStyles.miniButton)) {
						eventData.blocks[eventData.editIndex - 1].levelValidation--;
					}
					EditorGUI.EndDisabledGroup();
					toolRect.x += 25;
					toolRect.width = 25;
					if(GUI.Button(toolRect, new GUIContent(">", ""), EditorStyles.miniButton)) {
						eventData.blocks[eventData.editIndex - 1].levelValidation++;
					}
					decreaseW = 50;
				}
				EditorGUI.EndDisabledGroup();
				toolRect.width = position.width - 140 - decreaseW;
				toolRect.x += 25;
				if(GUI.Button(toolRect, new GUIContent("Add New", "Add new event"), EditorStyles.miniButton)) {
					GenericMenu menu = new GenericMenu();
					ShowContextMenu((a, b) => {
						clickHandler(property, b, a);
					}, menu, ItemAtt);
					Rect r = uNodeGUIUtility.GUIToScreenRect(toolRect);
					r.y -= 20;
					if(customMenu != null) {
						foreach(var c in customMenu) {
							if(c == null) continue;
							if(c.isValidationMenu && ItemAtt.eventType != EventData.EventType.Condition) continue;
							if(c.isSeparator) {
								menu.AddSeparator(c.menuName);
							} else {
								if(c.isItemSelector) {
									menu.AddItem(new GUIContent(c.menuName), false, delegate () {
										ItemSelector.ShowWindow(property.serializedObject.targetObject, c.filter,
											delegate (MemberData m) {
												var instance = property.serializedObject.targetObject;
												if(!(m.instance is UnityEngine.Object) && instance != null && !m.isStatic) {
													Type startType = m.startType;
													if(startType != null) {
														if(instance.GetType().IsCastableTo(startType)) {
															m.instance = instance;
														} else if(m.IsTargetingUNode) {
															m.instance = instance;
														} else if(instance is Component) {
															if(startType == typeof(GameObject)) {
																m.instance = (instance as Component).gameObject;
															} else if(startType.IsSubclassOf(typeof(Component))) {
																m.instance = (instance as Component).GetComponent(startType);
															}
														} else if(instance is GameObject) {
															if(startType == typeof(GameObject)) {
																m.instance = instance as GameObject;
															} else if(startType.IsSubclassOf(typeof(Component))) {
																m.instance = (instance as GameObject).GetComponent(startType);
															}
														}
														if(m.instance == null && ReflectionUtils.CanCreateInstance(startType)) {
															m.instance = ReflectionUtils.CreateInstance(startType);
														}
													}
													if(m.instance == null) {
														m.instance = MemberData.none;
													}
												}
												EventActionData vaEvent = c.onClickItem(m);
												property.FindPropertyRelative("eventList").InsertArrayElementAtIndex(property.FindPropertyRelative("eventList").arraySize);
												property.serializedObject.ApplyModifiedProperties();
												eventData.blocks[eventData.blocks.Count - 1] = vaEvent;
												uNodeGUIUtility.GUIChanged(property.serializedObject.targetObject);
											}).ChangePosition(r.ToScreenRect());
									});
								} else if(c.onClickMenu != null) {
									c.onClickMenu();
									uNodeGUIUtility.GUIChanged(property.serializedObject.targetObject);
								}
							}
						}
					}
					menu.ShowAsContext();
				}
			}
			EditorGUI.indentLevel = oldIndent;
		}


		static List<BlockMenuAttribute> _menuItems;
		public static List<BlockMenuAttribute> FindAllMenu() {
			if(_menuItems != null) {
				return _menuItems;
			}
			List<BlockMenuAttribute> menuItems = new List<BlockMenuAttribute>();

			foreach(System.Reflection.Assembly assembly in EditorReflectionUtility.GetAssemblies()) {
				foreach(System.Type type in EditorReflectionUtility.GetAssemblyTypes(assembly)) {
					if(type.GetCustomAttributes(typeof(BlockMenuAttribute), false).Length > 0) {
						BlockMenuAttribute menuItem = (BlockMenuAttribute)type.GetCustomAttributes(typeof(BlockMenuAttribute), false)[0];
						menuItem.type = type;
						menuItems.Add(menuItem);
					}
				}
			}
			menuItems.Sort((x, y) => string.Compare(x.category + x.name, y.category + y.name, StringComparison.OrdinalIgnoreCase));
			_menuItems = menuItems;
			return menuItems;
		}
	}
}