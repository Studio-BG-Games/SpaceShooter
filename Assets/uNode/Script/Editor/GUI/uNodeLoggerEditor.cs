#pragma warning disable
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace MaxyGames.uNodeLogger {
	[System.Serializable]
	public class uNodeLoggerEditor : ScriptableObject, ILogger {
		List<LogInfo> ListLogInfo = new List<LogInfo>();
		HashSet<string> Channels = new HashSet<string>();

		public bool PauseOnError = false;
		public bool ClearOnPlay = true;
		public bool WasPlaying = false;
		public int NoErrors;
		public int NoWarnings;
		public int NoMessages;

		static public uNodeLoggerEditor Create() {
			var editorDebug = FindObjectOfType<uNodeLoggerEditor>();

			if(editorDebug == null) {
				editorDebug = CreateInstance<uNodeLoggerEditor>();
			}

			editorDebug.NoErrors = 0;
			editorDebug.NoWarnings = 0;
			editorDebug.NoMessages = 0;

			return editorDebug;
		}

		public void OnEnable() {
			EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;

			//Make this scriptable object persist between Play sessions
			hideFlags = HideFlags.HideAndDontSave;
		}

		public void ProcessOnStartClear() {
			if(!WasPlaying && EditorApplication.isPlayingOrWillChangePlaymode) {
				if(ClearOnPlay) {
					Clear();
				}
			}
			WasPlaying = EditorApplication.isPlayingOrWillChangePlaymode;
		}

		void OnPlaymodeStateChanged() {
			ProcessOnStartClear();
		}

		public interface ILoggerWindow {
			void OnLogChange(LogInfo logInfo);
		}
		List<ILoggerWindow> Windows = new List<ILoggerWindow>();

		public void AddWindow(ILoggerWindow window) {
			if(!Windows.Contains(window)) {
				Windows.Add(window);
			}
		}

		public void Log(LogInfo logInfo) {
			lock(this) {
				if(!ListLogInfo.Contains(logInfo)) {
					if(!String.IsNullOrEmpty(logInfo.Channel) && !Channels.Contains(logInfo.Channel)) {
						Channels.Add(logInfo.Channel);
					}
					ListLogInfo.Add(logInfo);
				} else {
					return;
				}
			}

			if(logInfo.Severity == LogSeverity.Error) {
				NoErrors++;
			} else if(logInfo.Severity == LogSeverity.Warning) {
				NoWarnings++;
			} else {
				NoMessages++;
			}

			foreach(var window in Windows) {
				window.OnLogChange(logInfo);
			}

			if(logInfo.Severity == LogSeverity.Error && PauseOnError) {
				UnityEngine.Debug.Break();
			}
		}

		public void Clear() {
			lock(this) {
				ListLogInfo.Clear();
				Channels.Clear();
				NoWarnings = 0;
				NoErrors = 0;
				NoMessages = 0;

				foreach(var window in Windows) {
					window.OnLogChange(null);
				}
			}
		}

		public List<LogInfo> CopyLogInfo() {
			lock(this) {
				return new List<LogInfo>(ListLogInfo);
			}
		}

		public HashSet<string> CopyChannels() {
			lock(this) {
				return new HashSet<string>(Channels);
			}
		}

	}
}