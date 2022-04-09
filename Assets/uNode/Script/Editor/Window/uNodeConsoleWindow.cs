using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using MaxyGames.uNode;
using MaxyGames.uNode.Editors;

namespace MaxyGames.uNodeLogger {
	public class uNodeConsoleWindow : EditorWindow, uNodeLoggerEditor.ILoggerWindow {
		public static uNodeConsoleWindow window;
		public const string KEY_OpenConsole = "(See uNode Console)";

		[MenuItem("Tools/uNode/Show Console")]
		static public void ShowLogWindow() {
			var window = (uNodeConsoleWindow)GetWindow(typeof(uNodeConsoleWindow), false);
			window.autoRepaintOnSceneChange = true;
			window.wantsMouseMove = true;
			window.minSize = new Vector2(200, 150);
			window.CurrentTopPaneHeight = window.position.height / 2;
			window.Show();
		}

		public void OnLogChange(LogInfo logInfo) {
			Dirty = true;
			// Repaint();
		}


		void OnInspectorUpdate() {
			if(Dirty) {
				Repaint();
			}
		}

		void OnEnable() {
			window = this;
			// Connect to or create the backend
			if(!EditorLogger) {
				EditorLogger = Logger.GetLogger<uNodeLoggerEditor>();
				if(!EditorLogger) {
					EditorLogger = uNodeLoggerEditor.Create();
				}
			}

			Logger.AddLogger(EditorLogger);
			EditorLogger.AddWindow(this);
			titleContent.text = "uNode Console";
			ClearSelectedMessage();

			SmallErrorIcon = EditorGUIUtility.FindTexture("d_console.erroricon.sml");
			SmallWarningIcon = EditorGUIUtility.FindTexture("d_console.warnicon.sml");
			SmallMessageIcon = EditorGUIUtility.FindTexture("d_console.infoicon.sml");
			ErrorIcon = SmallErrorIcon;
			WarningIcon = SmallWarningIcon;
			MessageIcon = SmallMessageIcon;
			Dirty = true;
			Repaint();
		}

		string ExtractLogListToString() {
			string result = "";
			foreach(CountedLog log in RenderLogs) {
				LogInfo logInfo = log.Log;
				result += logInfo.GetRelativeTimeStampAsString() + ": " + logInfo.Severity + ": " + logInfo.Message + "\n";
			}
			return result;
		}

		string ExtractLogDetailsToString() {
			string result = "";
			if(RenderLogs.Count > 0 && SelectedRenderLog >= 0) {
				var countedLog = RenderLogs[SelectedRenderLog];
				var log = countedLog.Log;

				for(int c1 = 0; c1 < log.Callstack.Count; c1++) {
					var frame = log.Callstack[c1];
					var methodName = frame.GetFormattedMethodName();
					result += methodName + "\n";
				}
			}
			return result;
		}

		void HandleCopyToClipboard() {
			const string copyCommandName = "Copy";

			Event e = Event.current;
			if(e.type == EventType.ValidateCommand && e.commandName == copyCommandName) {
				e.Use();
			} else if(e.type == EventType.ExecuteCommand && e.commandName == copyCommandName) {
				string result = ExtractLogListToString();

				result += "\n";
				result += ExtractLogDetailsToString();

				GUIUtility.systemCopyBuffer = result;
			}
		}

		Vector2 DrawPos;
		public void OnGUI() {
			Color defaultLineColor = GUI.backgroundColor;
			GUIStyle unityLogLineEven = null;
			GUIStyle unityLogLineOdd = null;
			GUIStyle unitySmallLogLine = null;

			foreach(var style in GUI.skin.customStyles) {
				if(style.name == "CN EntryBackEven")
					unityLogLineEven = style;
				else if(style.name == "CN EntryBackOdd")
					unityLogLineOdd = style;
				else if(style.name == "CN StatusInfo")
					unitySmallLogLine = style;
			}

			EntryStyleBackEven = new GUIStyle(unitySmallLogLine);

			EntryStyleBackEven.normal = unityLogLineEven.normal;
			EntryStyleBackEven.margin = new RectOffset(0, 0, 0, 0);
			EntryStyleBackEven.border = new RectOffset(0, 0, 0, 0);
			EntryStyleBackEven.fixedHeight = 0;

			EntryStyleBackOdd = new GUIStyle(EntryStyleBackEven);
			EntryStyleBackOdd.normal = unityLogLineOdd.normal;
			// EntryStyleBackOdd = new GUIStyle(unityLogLine);


			SizerLineColour = new Color(defaultLineColor.r * 0.5f, defaultLineColor.g * 0.5f, defaultLineColor.b * 0.5f);

			// GUILayout.BeginVertical(GUILayout.Height(topPanelHeaderHeight), GUILayout.MinHeight(topPanelHeaderHeight));
			ResizeTopPane();
			DrawPos = Vector2.zero;
			DrawToolbar();

			float logPanelHeight = position.height - CurrentTopPaneHeight - DrawPos.y;
			logPanelHeight = Mathf.Clamp(logPanelHeight, 100, position.height - 100);

			if(Dirty) {
				CurrentLogList = EditorLogger.CopyLogInfo();
			}
			DrawLogList(logPanelHeight);

			DrawPos.y += DividerHeight;

			DrawLogDetails();

			HandleCopyToClipboard();

			//If we're dirty, do a repaint
			Dirty = false;
			if(MakeDirty) {
				Dirty = true;
				MakeDirty = false;
				Repaint();
			}
		}

		bool ButtonClamped(string text, GUIStyle style, out Vector2 size) {
			var content = new GUIContent(text);
			size = style.CalcSize(content);
			var rect = new Rect(DrawPos, size);
			return GUI.Button(rect, text, style);
		}

		bool ToggleClamped(bool state, string text, GUIStyle style, out Vector2 size) {
			var content = new GUIContent(text);
			return ToggleClamped(state, content, style, out size);
		}

		bool ToggleClamped(bool state, GUIContent content, GUIStyle style, out Vector2 size) {
			size = style.CalcSize(content);
			Rect drawRect = new Rect(DrawPos, size);
			return GUI.Toggle(drawRect, state, content, style);
		}

		void LabelClamped(string text, GUIStyle style, out Vector2 size) {
			var content = new GUIContent(text);
			size = style.CalcSize(content);

			Rect drawRect = new Rect(DrawPos, size);
			GUI.Label(drawRect, text, style);
		}

		void DrawToolbar() {
			var toolbarStyle = EditorStyles.toolbarButton;

			var errorToggleContent = new GUIContent(EditorLogger.NoErrors.ToString(), SmallErrorIcon);
			var warningToggleContent = new GUIContent(EditorLogger.NoWarnings.ToString(), SmallWarningIcon);
			var messageToggleContent = new GUIContent(EditorLogger.NoMessages.ToString(), SmallMessageIcon);

			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(100));
			if(GUILayout.Button(new GUIContent("Clear"), EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(15))) {
				EditorLogger.Clear();
			}
			var collapse = GUILayout.Toggle(Collapse, "Collapse", EditorStyles.toolbarButton, GUILayout.Width(65), GUILayout.Height(15));
			if(collapse != Collapse) {
				MakeDirty = true;
				Collapse = collapse;
				SelectedRenderLog = -1;
			}
			EditorLogger.ClearOnPlay = GUILayout.Toggle(EditorLogger.ClearOnPlay, "Clear On Play", EditorStyles.toolbarButton, GUILayout.Width(85), GUILayout.Height(15));
			EditorLogger.PauseOnError = GUILayout.Toggle(EditorLogger.PauseOnError, "Error Pause", EditorStyles.toolbarButton, GUILayout.Width(75), GUILayout.Height(15));
			GUILayout.FlexibleSpace();
			Vector2 size = EditorStyles.toolbarButton.CalcSize(messageToggleContent);
			var showMessages = GUILayout.Toggle(ShowMessages, messageToggleContent, EditorStyles.toolbarButton, GUILayout.Width(size.x), GUILayout.Height(15));
			size = EditorStyles.toolbarButton.CalcSize(warningToggleContent);
			var showWarnings = GUILayout.Toggle(ShowWarnings, warningToggleContent, EditorStyles.toolbarButton, GUILayout.Width(size.x), GUILayout.Height(15));
			size = EditorStyles.toolbarButton.CalcSize(errorToggleContent);
			var showErrors = GUILayout.Toggle(ShowErrors, errorToggleContent, EditorStyles.toolbarButton, GUILayout.Width(size.x), GUILayout.Height(15));
			GUILayout.EndHorizontal();
			DrawPos.y += 18;
			//var showTimes = ToggleClamped(ShowTimes, "Times", EditorStyles.toolbarButton, out elementSize);
			//if(showTimes!=ShowTimes)
			//{
			//    MakeDirty = true;
			//    ShowTimes = showTimes;
			//}
			//DrawPos.x += elementSize.x;

			//var showChannels = ToggleClamped(ShowChannels, "Channels", EditorStyles.toolbarButton, out elementSize);
			//if (showChannels != ShowChannels)
			//{
			//    MakeDirty = true;
			//    ShowChannels = showChannels;
			//}
			//DrawPos.x += elementSize.x;

			//If the errors/warning to show has changed, clear the selected message
			if(showErrors != ShowErrors || showWarnings != ShowWarnings || showMessages != ShowMessages) {
				ClearSelectedMessage();
				MakeDirty = true;
			}
			ShowWarnings = showWarnings;
			ShowMessages = showMessages;
			ShowErrors = showErrors;
		}

		bool ShouldShowLog(System.Text.RegularExpressions.Regex regex, LogInfo log) {
			//if(log.Channel == CurrentChannel || CurrentChannel == "All" || (CurrentChannel == "No Channel" && String.IsNullOrEmpty(log.Channel))) {
				if((log.Severity == LogSeverity.Message && ShowMessages)
				   || (log.Severity == LogSeverity.Warning && ShowWarnings)
				   || (log.Severity == LogSeverity.Error && ShowErrors)) {
					if(regex == null || regex.IsMatch(log.Message)) {
						return true;
					}
				}
			//}

			return false;
		}

		GUIContent GetLogLineGUIContent(LogInfo log, bool showTimes, bool showChannels) {
			var showMessage = log.Message;

			//Make all messages single line
			//showMessage = showMessage.Replace(MaxyGames.uNodeLogger.Logger.NewLine, " ");

			showMessage = showMessage.Split('\n')[0];

			// Format the message as follows:
			//     [channel] 0.000 : message  <-- Both channel and time shown
			//     0.000 : message            <-- Time shown, channel hidden
			//     [channel] : message        <-- Channel shown, time hidden
			//     message                    <-- Both channel and time hidden
			var showChannel = showChannels && !string.IsNullOrEmpty(log.Channel);
			var channelMessage = showChannel ? string.Format("[{0}]", log.Channel) : "";
			var channelTimeSeparator = (showChannel && showTimes) ? " " : "";
			var timeMessage = showTimes ? string.Format("{0}", log.GetRelativeTimeStampAsString()) : "";
			var prefixMessageSeparator = (showChannel || showTimes) ? " : " : "";
			showMessage = string.Format("{0}{1}{2}{3}{4}",
					channelMessage,
					channelTimeSeparator,
					timeMessage,
					prefixMessageSeparator,
					showMessage
				);

			var content = new GUIContent(showMessage, GetIconForLog(log));
			return content;
		}

		GenericMenu contextMenu;
		public void DrawLogList(float height) {
			var oldColor = GUI.backgroundColor;


			float buttonY = 0;

			System.Text.RegularExpressions.Regex filterRegex = null;

			//if(!String.IsNullOrEmpty(FilterRegex)) {
			//	filterRegex = new Regex(FilterRegex);
			//}

			var collapseBadgeStyle = EditorStyles.miniButton;
			var logLineStyle = EntryStyleBackEven;

			// If we've been marked dirty, we need to recalculate the elements to be displayed
			if(Dirty) {
				LogListMaxWidth = 0;
				LogListLineHeight = 0;
				CollapseBadgeMaxWidth = 0;
				RenderLogs.Clear();

				//When collapsed, count up the unique elements and use those to display
				if(Collapse) {
					var collapsedLines = new Dictionary<string, CountedLog>();
					var collapsedLinesList = new List<CountedLog>();
					
					foreach(var log in CurrentLogList) {
						if(ShouldShowLog(filterRegex, log)) {
							var matchString = log.Message + "!$" + log.Severity + "!$" + log.Channel;

							CountedLog countedLog;
							if(collapsedLines.TryGetValue(matchString, out countedLog)) {
								countedLog.Count++;
							} else {
								countedLog = new CountedLog(log, 1);
								collapsedLines.Add(matchString, countedLog);
								collapsedLinesList.Add(countedLog);
							}
						}
					}

					foreach(var countedLog in collapsedLinesList) {
						var content = GetLogLineGUIContent(countedLog.Log, ShowTimes, ShowChannels);
						RenderLogs.Add(countedLog);
						var logLineSize = logLineStyle.CalcSize(content);
						LogListMaxWidth = Mathf.Max(LogListMaxWidth, logLineSize.x);
						LogListLineHeight = Mathf.Max(LogListLineHeight, logLineSize.y);

						var collapseBadgeContent = new GUIContent(countedLog.Count.ToString());
						var collapseBadgeSize = collapseBadgeStyle.CalcSize(collapseBadgeContent);
						CollapseBadgeMaxWidth = Mathf.Max(CollapseBadgeMaxWidth, collapseBadgeSize.x);
					}
				}
				//If we're not collapsed, display everything in order
				else {
					foreach(var log in CurrentLogList) {
						if(ShouldShowLog(filterRegex, log)) {
							var content = GetLogLineGUIContent(log, ShowTimes, ShowChannels);
							RenderLogs.Add(new CountedLog(log, 1));
							var logLineSize = logLineStyle.CalcSize(content);
							LogListMaxWidth = Mathf.Max(LogListMaxWidth, logLineSize.x);
							LogListLineHeight = Mathf.Max(LogListLineHeight, logLineSize.y);
						}
					}
				}

				LogListMaxWidth += CollapseBadgeMaxWidth;
			}

			var scrollRect = new Rect(DrawPos, new Vector2(position.width, height));
			float lineWidth = Mathf.Max(LogListMaxWidth, scrollRect.width);

			var contentRect = new Rect(0, 0, lineWidth, RenderLogs.Count * LogListLineHeight);
			LogListScrollPosition = GUI.BeginScrollView(scrollRect, LogListScrollPosition, contentRect);

			float logLineX = CollapseBadgeMaxWidth;

			//Render all the elements
			int firstRenderLogIndex = (int)(LogListScrollPosition.y / LogListLineHeight);
			int lastRenderLogIndex = firstRenderLogIndex + (int)(height / LogListLineHeight);

			firstRenderLogIndex = Mathf.Clamp(firstRenderLogIndex, 0, RenderLogs.Count);
			lastRenderLogIndex = Mathf.Clamp(lastRenderLogIndex, 0, RenderLogs.Count);
			buttonY = firstRenderLogIndex * LogListLineHeight;

			for(int renderLogIndex = firstRenderLogIndex; renderLogIndex < lastRenderLogIndex; renderLogIndex++) {
				var countedLog = RenderLogs[renderLogIndex];
				var log = countedLog.Log;
				logLineStyle = (renderLogIndex % 2 == 0) ? EntryStyleBackEven : EntryStyleBackOdd;
				if(renderLogIndex == SelectedRenderLog) {
					GUI.backgroundColor = new Color(0.5f, 0.5f, 1);
				} else {
					GUI.backgroundColor = Color.white;
				}

				//Make all messages single line
				var content = GetLogLineGUIContent(log, ShowTimes, ShowChannels);
				var drawRect = new Rect(logLineX, buttonY, contentRect.width, LogListLineHeight);
				if(contextMenu != null && (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)) {
					contextMenu.ShowAsContext();
					contextMenu = null;
				}
				if(GUI.Button(drawRect, content, logLineStyle)) {
					if(Event.current.button == 1) {
						contextMenu = new GenericMenu();
						if(log.Source != null) {
							if(log.Source as NodeComponent) {
								contextMenu.AddItem(new GUIContent("HighlightNode"), false, () => {
									uNodeEditor.HighlightNode(log.Source as NodeComponent);
								});
							}
							if(log.Source as INode<uNodeRoot> != null) {
								contextMenu.AddItem(new GUIContent("Open uNode"), false, () => {
									uNodeEditor.Open(log.Source as INode<uNodeRoot>);
								});
								contextMenu.AddSeparator("");
							}
							contextMenu.AddItem(new GUIContent("Find Object"), false, () => {
								if(log.Source as NodeComponent) {
									var owner = (log.Source as NodeComponent).owner;
									if(owner != null) {
										EditorGUIUtility.PingObject(owner);
									}
								} else if(log.Source as RootObject) {
									var owner = (log.Source as RootObject).owner;
									if(owner != null) {
										EditorGUIUtility.PingObject(owner);
									}
								} else {
									EditorGUIUtility.PingObject(log.Source);
								}
							});
							contextMenu.AddItem(new GUIContent("Select Object"), false, () => {
								if(log.Source as NodeComponent) {
									var owner = (log.Source as NodeComponent).owner;
									if(owner != null) {
										EditorGUIUtility.PingObject(owner);
										Selection.instanceIDs = new int[] { owner.GetInstanceID() };
									}
								} else if(log.Source as RootObject) {
									var owner = (log.Source as RootObject).owner;
									if(owner != null) {
										EditorGUIUtility.PingObject(owner);
										Selection.instanceIDs = new int[] { owner.GetInstanceID() };
									}
								} else {
									EditorGUIUtility.PingObject(log.Source);
									Selection.instanceIDs = new int[] { log.Source.GetInstanceID() };
								}
							});
							contextMenu.AddSeparator("");
						}
						contextMenu.AddItem(new GUIContent("JumpToSource"), false, () => {
							// Attempt to display source code associated with messages. Search through all stackframes,
							//   until we find a stackframe that can be displayed in source code view
							for(int frame = 0; frame < log.Callstack.Count; frame++) {
								if(JumpToSource(log.Callstack[frame]))
									break;
							}
						});
						SelectedRenderLog = renderLogIndex;
						SelectedCallstackFrame = -1;
						LastMessageClickTime = EditorApplication.timeSinceStartup;
						Repaint();
					} else {
						//Select a message, or jump to source if it's double-clicked
						if(renderLogIndex == SelectedRenderLog) {
							if(EditorApplication.timeSinceStartup - LastMessageClickTime < DoubleClickInterval) {
								LastMessageClickTime = 0;
								if(log.Source as NodeComponent) {
									uNodeEditor.HighlightNode(log.Source as NodeComponent);
								} else if(log.Source as RootObject) {
									uNodeEditor.Open(log.Source as RootObject);
								} else {
									// Attempt to display source code associated with messages. Search through all stackframes,
									//   until we find a stackframe that can be displayed in source code view
									for(int frame = 0; frame < log.Callstack.Count; frame++) {
										if(JumpToSource(log.Callstack[frame]))
											break;
									}
								}
							} else {
								LastMessageClickTime = EditorApplication.timeSinceStartup;
							}
						} else {
							SelectedRenderLog = renderLogIndex;
							SelectedCallstackFrame = -1;
							LastMessageClickTime = EditorApplication.timeSinceStartup;
						}

						//Always select the game object that is the source of this message
						if(log.Source != null) {
							if(log.Source as NodeComponent) {
								var owner = (log.Source as NodeComponent).owner;
								if(owner != null) {
									EditorGUIUtility.PingObject(owner);
								}
							} else if(log.Source as RootObject) {
								var owner = (log.Source as RootObject).owner;
								if(owner != null) {
									EditorGUIUtility.PingObject(owner);
								}
							} else {
								EditorGUIUtility.PingObject(log.Source);
								//Selection.activeObject = log.Source;
							}
						}
					}
				}

				if(Collapse) {
					var collapseBadgeContent = new GUIContent(countedLog.Count.ToString());
					var collapseBadgeSize = collapseBadgeStyle.CalcSize(collapseBadgeContent);
					var collapseBadgeRect = new Rect(0, buttonY, collapseBadgeSize.x, collapseBadgeSize.y);
					GUI.Button(collapseBadgeRect, collapseBadgeContent, collapseBadgeStyle);
				}
				buttonY += LogListLineHeight;
			}

			//If we're following the log, move to the end
			if(ScrollFollowMessages && RenderLogs.Count > 0) {
				LogListScrollPosition.y = ((RenderLogs.Count + 1) * LogListLineHeight) - scrollRect.height;
			}

			GUI.EndScrollView();
			DrawPos.y += height;
			DrawPos.x = 0;
			GUI.backgroundColor = oldColor;
		}

		public void DrawLogDetails() {
			var oldColor = GUI.backgroundColor;

			SelectedRenderLog = Mathf.Clamp(SelectedRenderLog, 0, CurrentLogList.Count);

			if(RenderLogs.Count > 0 && SelectedRenderLog >= 0) {
				var countedLog = RenderLogs[SelectedRenderLog];
				var log = countedLog.Log;
				var logLineStyle = EntryStyleBackEven;

				var sourceStyle = new GUIStyle(GUI.skin.textArea);
				sourceStyle.richText = true;

				var drawRect = new Rect(DrawPos, new Vector2(position.width - DrawPos.x, position.height - DrawPos.y));

				//Work out the content we need to show, and the sizes
				var detailLines = new List<GUIContent>();
				float contentHeight = 0;
				float contentWidth = 0;
				float lineHeight = 0;
				int messageLength = 0;
				var message = log.Message.Split('\n');
				for(int i = 1; i < message.Length; i++) {
					var content = new GUIContent(message[i]);
					detailLines.Add(content);

					var contentSize = logLineStyle.CalcSize(content);
					contentHeight += contentSize.y;
					lineHeight = Mathf.Max(lineHeight, contentSize.y);
					contentWidth = Mathf.Max(contentSize.x, contentWidth);
					messageLength++;
				}

				for(int c1 = 0; c1 < log.Callstack.Count; c1++) {
					var frame = log.Callstack[c1];
					var methodName = frame.GetFormattedMethodNameWithFileName();
					if(!String.IsNullOrEmpty(methodName)) {
						var content = new GUIContent(methodName);
						detailLines.Add(content);

						var contentSize = logLineStyle.CalcSize(content);
						contentHeight += contentSize.y;
						lineHeight = Mathf.Max(lineHeight, contentSize.y);
						contentWidth = Mathf.Max(contentSize.x, contentWidth);
						if(ShowFrameSource && c1 + messageLength == SelectedCallstackFrame) {
							var sourceContent = GetFrameSourceGUIContent(frame);
							if(sourceContent != null) {
								var sourceSize = sourceStyle.CalcSize(sourceContent);
								contentHeight += sourceSize.y;
								contentWidth = Mathf.Max(sourceSize.x, contentWidth);
							}
						}
					}
				}

				//Render the content
				var contentRect = new Rect(0, 0, Mathf.Max(contentWidth, drawRect.width), contentHeight);

				LogDetailsScrollPosition = GUI.BeginScrollView(drawRect, LogDetailsScrollPosition, contentRect);

				float lineY = 0;
				for(int c1 = 0; c1 < detailLines.Count; c1++) {
					var lineContent = detailLines[c1];
					if(lineContent != null) {
						logLineStyle = (c1 % 2 == 0) ? EntryStyleBackEven : EntryStyleBackOdd;
						if(c1 == SelectedCallstackFrame) {
							GUI.backgroundColor = new Color(0.5f, 0.5f, 1);
						} else {
							GUI.backgroundColor = Color.white;
						}
						var lineRect = new Rect(0, lineY, contentRect.width, lineHeight);
						LogStackFrame frame = null;
						if(messageLength <= c1) {
							frame = log.Callstack[c1 - messageLength];

							// Handle clicks on the stack frame
							if(GUI.Button(lineRect, lineContent, logLineStyle)) {
								if(c1 == SelectedCallstackFrame) {
									if(Event.current.button == 1) {
										ToggleShowSource(frame);
										Repaint();
									} else {
										if(EditorApplication.timeSinceStartup - LastFrameClickTime < DoubleClickInterval) {
											LastFrameClickTime = 0;
											JumpToSource(frame);
										} else {
											LastFrameClickTime = EditorApplication.timeSinceStartup;
										}
									}

								} else {
									SelectedCallstackFrame = c1;
									LastFrameClickTime = EditorApplication.timeSinceStartup;
								}
							}
						} else {
							if(GUI.Button(lineRect, lineContent, logLineStyle)) {
								SelectedCallstackFrame = c1;
								LastFrameClickTime = EditorApplication.timeSinceStartup;
							}
						}
						lineY += lineHeight;
						//Show the source code if needed
						if(ShowFrameSource && c1 == SelectedCallstackFrame) {
							GUI.backgroundColor = Color.white;

							if(frame != null) {
								var sourceContent = GetFrameSourceGUIContent(frame);
								if(sourceContent != null) {
									var sourceSize = sourceStyle.CalcSize(sourceContent);
									var sourceRect = new Rect(0, lineY, contentRect.width, sourceSize.y);

									GUI.Label(sourceRect, sourceContent, sourceStyle);
									lineY += sourceSize.y;
								}
							}
						}
					}
				}
				GUI.EndScrollView();
			}
			GUI.backgroundColor = oldColor;
		}

		Texture2D GetIconForLog(LogInfo log) {
			if(log.Severity == LogSeverity.Error) {
				return ErrorIcon;
			}
			if(log.Severity == LogSeverity.Warning) {
				return WarningIcon;
			}

			return MessageIcon;
		}

		void ToggleShowSource(LogStackFrame frame) {
			ShowFrameSource = !ShowFrameSource;
		}

		bool JumpToSource(LogStackFrame frame) {
			if(frame.FileName != null) {
				var osFileName = MaxyGames.uNodeLogger.Logger.ConvertDirectorySeparatorsFromUnityToOS(frame.FileName);
				var filename = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), osFileName);
				if(System.IO.File.Exists(filename)) {
					if(UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(filename, frame.LineNumber))
						return true;
				}
			}

			return false;
		}

		GUIContent GetFrameSourceGUIContent(LogStackFrame frame) {
			var source = GetSourceForFrame(frame);
			if(!String.IsNullOrEmpty(source)) {
				var content = new GUIContent(source);
				return content;
			}
			return null;
		}


		void DrawFilter() {
			Vector2 size;
			LabelClamped("Filter Regex", GUI.skin.label, out size);
			DrawPos.x += size.x;

			string filterRegex = null;
			bool clearFilter = false;
			if(ButtonClamped("Clear", GUI.skin.button, out size)) {
				clearFilter = true;

				GUIUtility.keyboardControl = 0;
				GUIUtility.hotControl = 0;
			}
			DrawPos.x += size.x;

			var drawRect = new Rect(DrawPos, new Vector2(position.width - DrawPos.x, size.y));
			filterRegex = EditorGUI.TextArea(drawRect, FilterRegex);

			if(clearFilter) {
				filterRegex = null;
			}
			//If the filter has changed, invalidate our currently selected message
			if(filterRegex != FilterRegex) {
				ClearSelectedMessage();
				FilterRegex = filterRegex;
				MakeDirty = true;
			}

			DrawPos.y += size.y;
			DrawPos.x = 0;
		}

		List<string> GetChannels() {
			if(Dirty) {
				CurrentChannels = EditorLogger.CopyChannels();
			}

			var categories = CurrentChannels;

			var channelList = new List<string>();
			channelList.Add("All");
			channelList.Add("No Channel");
			channelList.AddRange(categories);
			return channelList;
		}

		private void ResizeTopPane() {
			//Set up the resize collision rect
			CursorChangeRect = new Rect(0, position.height - CurrentTopPaneHeight, position.width, DividerHeight);

			var oldColor = GUI.color;
			GUI.color = SizerLineColour;
			GUI.DrawTexture(CursorChangeRect, EditorGUIUtility.whiteTexture);
			GUI.color = oldColor;
			EditorGUIUtility.AddCursorRect(CursorChangeRect, MouseCursor.ResizeVertical);

			if(Event.current.type == EventType.MouseDown && CursorChangeRect.Contains(Event.current.mousePosition)) {
				Resize = true;
			}

			//If we've resized, store the new size and force a repaint
			if(Resize) {
				CurrentTopPaneHeight = position.height - Event.current.mousePosition.y;
				CursorChangeRect.Set(CursorChangeRect.x, CurrentTopPaneHeight, CursorChangeRect.width, CursorChangeRect.height);
				Repaint();
			}

			if(Event.current.type == EventType.MouseUp)
				Resize = false;

			CurrentTopPaneHeight = Mathf.Clamp(CurrentTopPaneHeight, 100, position.height - 100);
		}

		//Cache for GetSourceForFrame
		string SourceLines;
		LogStackFrame SourceLinesFrame;

		string GetSourceForFrame(LogStackFrame frame) {
			if(SourceLinesFrame == frame) {
				return SourceLines;
			}


			if(frame.FileName == null) {
				return "";
			}

			var osFileName = MaxyGames.uNodeLogger.Logger.ConvertDirectorySeparatorsFromUnityToOS(frame.FileName);
			var filename = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), osFileName);
			if(!System.IO.File.Exists(filename)) {
				return "";
			}

			int lineNumber = frame.LineNumber - 1;
			int linesAround = 3;
			var lines = System.IO.File.ReadAllLines(filename);
			var firstLine = Mathf.Max(lineNumber - linesAround, 0);
			var lastLine = Mathf.Min(lineNumber + linesAround + 1, lines.Count());

			SourceLines = "";
			if(firstLine != 0) {
				SourceLines += "...\n";
			}
			for(int c1 = firstLine; c1 < lastLine; c1++) {
				string str = lines[c1] + "\n";
				if(c1 == lineNumber) {
					str = "<color=#ff0000ff>" + str + "</color>";
				}
				SourceLines += str;
			}
			if(lastLine != lines.Count()) {
				SourceLines += "...\n";
			}

			SourceLinesFrame = frame;
			return SourceLines;
		}

		void ClearSelectedMessage() {
			SelectedRenderLog = -1;
			SelectedCallstackFrame = -1;
			ShowFrameSource = false;
		}

		Vector2 LogListScrollPosition;
		Vector2 LogDetailsScrollPosition;

		Texture2D ErrorIcon;
		Texture2D WarningIcon;
		Texture2D MessageIcon;
		Texture2D SmallErrorIcon;
		Texture2D SmallWarningIcon;
		Texture2D SmallMessageIcon;

		[SerializeField]
		bool ShowChannels = true;
		[SerializeField]
		bool ShowTimes = false;
		[SerializeField]
		bool Collapse = false;

		bool ScrollFollowMessages = false;
		float CurrentTopPaneHeight = 200;
		bool Resize = false;
		Rect CursorChangeRect;
		int SelectedRenderLog = -1;
		bool Dirty = false;
		bool MakeDirty = false;
		float DividerHeight = 5;

		double LastMessageClickTime = 0;
		double LastFrameClickTime = 0;

		const double DoubleClickInterval = 0.3f;

		[UnityEngine.SerializeField]
		uNodeLoggerEditor EditorLogger;

		List<LogInfo> CurrentLogList = new List<LogInfo>();
		HashSet<string> CurrentChannels = new HashSet<string>();

		//Standard unity pro colours
		Color SizerLineColour;

		GUIStyle EntryStyleBackEven;
		GUIStyle EntryStyleBackOdd;
		[SerializeField]
		string FilterRegex = null;
		[SerializeField]
		bool ShowErrors = true;
		[SerializeField]
		bool ShowWarnings = true;
		[SerializeField]
		bool ShowMessages = true;
		int SelectedCallstackFrame = 0;
		bool ShowFrameSource = false;

		class CountedLog {
			public LogInfo Log = null;
			public Int32 Count = 1;
			public CountedLog(LogInfo log, Int32 count) {
				Log = log;
				Count = count;
			}
		}

		List<CountedLog> RenderLogs = new List<CountedLog>();
		float LogListMaxWidth = 0;
		float LogListLineHeight = 0;
		float CollapseBadgeMaxWidth = 0;

	}
}