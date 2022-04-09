﻿using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace MaxyGames.uNode.Editors {
	public static class ConsoleActivityWatcher {
		[InitializeOnLoadMethod]
		static void Initialize() {
			//Delay some frame to avoid lag on editor startup.
			uNodeThreadUtility.ExecuteAfter(100, () => {
				consoleWindowType = "UnityEditor.ConsoleWindow".ToType();
				fieldActiveText = consoleWindowType.GetField("m_ActiveText", MemberData.flags);
				fieldInstanceID = consoleWindowType.GetField("m_ActiveInstanceID", MemberData.flags);
				fieldCallStack = consoleWindowType.GetField("m_CallstackTextStart", MemberData.flags);
				EditorApplication.update += WatchConsoleActivity;
				// Debug.Log("Ready");
			});
		}

		private static Type consoleWindowType;
		private static FieldInfo fieldActiveText;
		private static FieldInfo fieldInstanceID;
		private static FieldInfo fieldCallStack;
		private static EditorWindow consoleWindow;

		private static float time;

		private static void WatchConsoleActivity() {
			// if (entryChanged == null) return;
			if (consoleWindow == null) {
				if (time < uNodeThreadUtility.time) {
					var windows = Resources.FindObjectsOfTypeAll(consoleWindowType);
					for(int i=0;i<windows.Length;i++) {
						var console = windows[i] as EditorWindow;
						var root = console.rootVisualElement;
						var watcher = root.Q<ConsoleWatcher>();
						if(watcher == null) {
							watcher = new ConsoleWatcher(console);
							root.Add(watcher);
							watcher.StretchToParentSize();
						}
						consoleWindow = console;
					}
					//Find console window every 2 second
					time = uNodeThreadUtility.time + 2;
				}
			}
		}

		#region Classes
		private struct ActivityData {
			public int line;
			public string path;
			public uNodeEditor.EditorScriptInfo info;

			public ActivityData(int line, string path, uNodeEditor.EditorScriptInfo info) {
				this.line = line;
				this.path = path;
				this.info = info;
			}
		}

		class ConsoleWatcher : VisualElement {
			private readonly EditorWindow consoleWindow;
			private IMGUIContainer container;
			private string lastActiveText;

			public ConsoleWatcher(EditorWindow consoleWindow) {
				this.consoleWindow = consoleWindow;
				UpdateContainer();
			}

			void UpdateContainer() {
				if(consoleWindow != null) {
					if(consoleWindow.rootVisualElement?.parent?.childCount > 0) {
						container = consoleWindow.rootVisualElement.parent[0] as IMGUIContainer;
					}
					if(container == null) {
						this.pickingMode = PickingMode.Ignore;
					} else {
						this.pickingMode = PickingMode.Position;
					}
				}
			}

			public override void HandleEvent(EventBase evt) {
				if(evt is AttachToPanelEvent) {
					UpdateContainer();
				}
				if(consoleWindow != null) {
					if(evt is MouseDownEvent mouseEvent && mouseEvent.button == 0) {
						var activeText = (string)fieldActiveText.GetValue(consoleWindow);
						var activeInstanceID = (int)fieldInstanceID.GetValue(consoleWindow);
						if(mouseEvent.clickCount >= 2) {
							lastActiveText = activeText;
							if(!ConsoleActivityChanged(activeText, activeInstanceID)) {
								container.HandleEvent(evt);
							}
							return;
						}
						container.HandleEvent(evt);
						if(lastActiveText != activeText || mouseEvent.modifiers == EventModifiers.Shift || mouseEvent.modifiers == EventModifiers.Control) {
							lastActiveText = activeText;
							if(fieldCallStack != null) {
								fieldCallStack.SetValue(consoleWindow, 0);
							}
							ConsoleActivityChanged(activeText, activeInstanceID);
						}
					} else {
						container.HandleEvent(evt);
						if(fieldCallStack != null) {
							fieldCallStack.SetValue(consoleWindow, 0);
						}
					}
				}
			}
		}

		struct MenuData {
			public string menu;
			public GenericMenu.MenuFunction action;
		}
		#endregion

		private static bool ConsoleActivityChanged(string text, int instanceID) {
			if(string.IsNullOrEmpty(text)) return false;
			var strs = text.Split('\n');
			List<MenuData> menus = new List<MenuData>();
			if(uNodeLogger.uNodeConsoleWindow.window == null && text.Contains(uNodeLogger.uNodeConsoleWindow.KEY_OpenConsole)) {
				menus.Add(new MenuData() {
					menu = "Open uNode Console",
					action = uNodeLogger.uNodeConsoleWindow.ShowLogWindow
				});
			}
			foreach(var txt in strs) {
				var idx = txt.IndexOf(uNodeException.KEY_REFERENCE);
				if(idx >= 0) {
					string str = null;
					for(int i = idx + uNodeException.KEY_REFERENCE.Length; i < txt.Length; i++) {
						if(str == null || char.IsNumber(txt[i])) {
							str += txt[i];
						} else {
							break;
						}
					}
					if(int.TryParse(str, out var id)) {
						var reference = EditorUtility.InstanceIDToObject(id);
						if(reference != null && reference is INode<uNodeRoot>) {
							if(reference is NodeComponent) {
								menus.Add(new MenuData() {
									menu = $"Highlight Node:{(reference as NodeComponent).GetNodeName()} from {(reference as NodeComponent).GetOwner().DisplayName}",
									action = () => uNodeEditor.HighlightNode(reference as NodeComponent)
								});
							} else {
								menus.Add(new MenuData() {
									menu = $"Highlight Node: {(reference as Component).gameObject.name} from {(reference as INode<uNodeRoot>).GetOwner().DisplayName}",
									action = () => uNodeEditor.Open(reference as INode<uNodeRoot>)
								});
							}
							continue;
						}
					}
					continue;
				}
				List<ActivityData> datas = new List<ActivityData>();
				foreach(var info in uNodeEditor.SavedData.scriptInformations) {
					string path = info.path.Replace("\\", "/");
					int index = txt.IndexOf("at " + path + ":");
					if(index < 0) {
						try {
							path = path.Remove(0, System.IO.Directory.GetCurrentDirectory().Length + 1);
							index = txt.IndexOf("at " + path + ":");
						}
						catch { }
					}
					if(index >= 0) {
						datas.Add(new ActivityData(index, path, info));
					}
				}
				// datas.Sort((x, y) => CompareUtility.Compare(x.number, y.number));
				ActivityData lastData = default;
				int line = 0;
				foreach(var data in datas.OrderByDescending(x => x.line)) {
					string num = "";
					for(int i = data.line + data.path.Length + 4; i < txt.Length; i++) {
						var c = txt[i];
						if(char.IsNumber(c)) {
							num += c;
						} else {
							break;
						}
					}
					if(int.TryParse(num, out line)) {
						line--;
						lastData = data;
					}
				}
				if(lastData.info != null && uNodeEditor.CanHighlightNode(lastData.info, line)) {
					menus.Add(new MenuData() {
						menu = $"{lastData.path.Replace('/', '\\')}:{lastData.line}",
						action = () => uNodeEditor.HighlightNode(lastData.info, line)
					});
					continue;
				}
				var uObj = EditorUtility.InstanceIDToObject(instanceID);
				if(uObj != null && uObj is INode<uNodeRoot>) {
					if(uObj is NodeComponent) {
						menus.Add(new MenuData() {
							menu = $"Highlight Node:{(uObj as NodeComponent).GetNodeName()} from {(uObj as NodeComponent).GetOwner().DisplayName}",
							action = () => uNodeEditor.HighlightNode(uObj as NodeComponent)
						});
					} else {
						menus.Add(new MenuData() {
							menu = $"Highlight Node: {(uObj as Component).gameObject.name} from {(uObj as INode<uNodeRoot>).GetOwner().DisplayName}",
							action = () => uNodeEditor.Open(uObj as INode<uNodeRoot>)
						});
					}
				}
			}
			if(menus.Count > 0) {
				if(menus.Count == 1) {
					menus[0].action();
				} else {
					GenericMenu menu = new GenericMenu();
					for(int i=0;i<menus.Count;i++) {
						menu.AddItem(new GUIContent(menus[i].menu), false, menus[i].action);
					}
					menu.ShowAsContext();
					Event.current.Use();
				}
				return true;
			}
			return false;
		}
	}
}