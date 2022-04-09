using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace MaxyGames.uNodeLogger {
	[AttributeUsage(AttributeTargets.Method)]
	public class StackTraceIgnore : Attribute { }

	[AttributeUsage(AttributeTargets.Method)]
	public class LogUnityOnly : Attribute { }

	public enum LogSeverity {
		Message,
		Warning,
		Error,
	}

	public interface ILogger {
		void Log(LogInfo logInfo);
	}

	public interface IFilter {
		bool ApplyFilter(string channel, UnityEngine.Object source, LogSeverity severity, object message);
	}

	[System.Serializable]
	public class LogStackFrame {
		public string MethodName;
		public string DeclaringType;
		public string ParameterSig;

		public int LineNumber;
		public string FileName;

		string FormattedMethodNameWithFileName;
		string FormattedMethodName;
		string FormattedFileName;

		public LogStackFrame(StackFrame frame) {
			var method = frame.GetMethod();
			MethodName = method.Name;
			DeclaringType = method.DeclaringType.FullName;

			var pars = method.GetParameters();
			for(int c1 = 0; c1 < pars.Length; c1++) {
				ParameterSig += String.Format("{0} {1}", pars[c1].ParameterType, pars[c1].Name);
				if(c1 + 1 < pars.Length) {
					ParameterSig += ", ";
				}
			}

			FileName = frame.GetFileName();
			LineNumber = frame.GetFileLineNumber();
			MakeFormattedNames();
		}

		public LogStackFrame(string message, string filename, int lineNumber) {
			FileName = filename;
			LineNumber = lineNumber;
			FormattedMethodNameWithFileName = message;
			FormattedMethodName = message;
			FormattedFileName = message;
		}

		public string GetFormattedMethodNameWithFileName() {
			return FormattedMethodNameWithFileName;
		}

		public string GetFormattedMethodName() {
			return FormattedMethodName;
		}

		public string GetFormattedFileName() {
			return FormattedFileName;
		}

		void MakeFormattedNames() {
			FormattedMethodName = String.Format("{0}.{1}({2})", DeclaringType, MethodName, ParameterSig);

			string filename = FileName;
			if(!String.IsNullOrEmpty(FileName)) {
				var startSubName = FileName.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);

				if(startSubName > 0) {
					filename = FileName.Substring(startSubName);
				}
			}
			FormattedFileName = String.Format("{0}:{1}", filename, LineNumber);

			FormattedMethodNameWithFileName = String.Format("{0} (at {1})", FormattedMethodName, FormattedFileName);
		}
	}

	[System.Serializable]
	public class LogInfo {
		[UnityEngine.SerializeField]
		private string componentType;
		[UnityEngine.SerializeField]
		private UnityEngine.Object _gameObject;
		[UnityEngine.SerializeField]
		private UnityEngine.Object _source;
		public UnityEngine.Object Source {
			get {
				if(_source == null && _gameObject != null) {
					var comp = (_gameObject as UnityEngine.GameObject).GetComponent(componentType);
					if(comp != null) {
						return comp;
					}
				}
				return _source;
			}
			set {
				try {
					if(value != null && value is UnityEngine.Component) {
						componentType = value.GetType().FullName;
						_gameObject = (value as UnityEngine.Component).gameObject;
					}
				}
				catch { }
				_source = value;
			}
		}
		public string Channel;
		public LogSeverity Severity;
		public string Message;
		public List<LogStackFrame> Callstack;
		public LogStackFrame OriginatingSourceLocation;
		public double RelativeTimeStamp;
		string RelativeTimeStampAsString;
		public DateTime AbsoluteTimeStamp;
		string AbsoluteTimeStampAsString;

		public string GetRelativeTimeStampAsString() {
			return RelativeTimeStampAsString;
		}

		public string GetAbsoluteTimeStampAsString() {
			return AbsoluteTimeStampAsString;
		}

		public LogInfo(UnityEngine.Object source, string channel, LogSeverity severity, List<LogStackFrame> callstack, LogStackFrame originatingSourceLocation, object message) {
			Source = source;
			Channel = channel;
			Severity = severity;
			Message = "";
			OriginatingSourceLocation = originatingSourceLocation;

			var messageString = message as String;
			if(messageString != null) {
				Message = messageString;
			} else {
				if(message != null) {
					Message = message.ToString();
				}
			}

			Callstack = callstack;
			RelativeTimeStamp = Logger.GetRelativeTime();
			AbsoluteTimeStamp = DateTime.UtcNow;
			RelativeTimeStampAsString = String.Format("{0:0.0000}", RelativeTimeStamp);
			AbsoluteTimeStampAsString = AbsoluteTimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
		}
	}

	public static class Logger {
		public static int MaxMessagesToKeep = 1000;
		public static string NewLine = "\n";
		public static char DirectorySeparator = '/';

		static List<ILogger> Loggers = new List<ILogger>();
		static LinkedList<LogInfo> RecentMessages = new LinkedList<LogInfo>();
		static long StartTick;
		static bool AlreadyLogging = false;
		static List<IFilter> Filters = new List<IFilter>();

		static Logger() {
			StartTick = DateTime.Now.Ticks;
		}

		static public double GetRelativeTime() {
			long ticks = DateTime.Now.Ticks;
			return TimeSpan.FromTicks(ticks - StartTick).TotalSeconds;
		}

		static public void AddLogger(ILogger logger, bool populateWithExistingMessages = true) {
			lock(Loggers) {
				if(populateWithExistingMessages) {
					foreach(var oldLog in RecentMessages) {
						logger.Log(oldLog);
					}
				}

				if(!Loggers.Contains(logger)) {
					Loggers.Add(logger);
				}
			}
		}

		static public void AddFilter(IFilter filter) {
			lock(Loggers) {
				Filters.Add(filter);
			}
		}

		static public string ConvertDirectorySeparatorsFromUnityToOS(string unityFileName) {
			return unityFileName.Replace(DirectorySeparator, System.IO.Path.DirectorySeparatorChar);
		}

		struct IgnoredUnityMethod {
			public enum Mode { Show, ShowIfFirstIgnoredMethod, Hide };
			public string DeclaringTypeName;
			public string MethodName;
			public Mode ShowHideMode;
		}

		static IgnoredUnityMethod[] IgnoredUnityMethods = new IgnoredUnityMethod[]
		{
			new IgnoredUnityMethod { DeclaringTypeName = "Application", MethodName = "CallLogCallback", ShowHideMode = IgnoredUnityMethod.Mode.Hide },

			new IgnoredUnityMethod { DeclaringTypeName = "DebugLogHandler", MethodName = null, ShowHideMode = IgnoredUnityMethod.Mode.Hide },

			new IgnoredUnityMethod { DeclaringTypeName = "Logger", MethodName = null, ShowHideMode = IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod },

			new IgnoredUnityMethod { DeclaringTypeName = "Debug", MethodName = null, ShowHideMode = IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod },

			new IgnoredUnityMethod { DeclaringTypeName = "Assert", MethodName = null, ShowHideMode = IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod  },
		};

		static IgnoredUnityMethod.Mode ShowOrHideMethod(MethodBase method) {
			foreach(IgnoredUnityMethod ignoredUnityMethod in IgnoredUnityMethods) {
				if((method.DeclaringType.Name == ignoredUnityMethod.DeclaringTypeName) && ((ignoredUnityMethod.MethodName == null) || (method.Name == ignoredUnityMethod.MethodName))) {
					return ignoredUnityMethod.ShowHideMode;
				}
			}

			return IgnoredUnityMethod.Mode.Show;
		}

		[StackTraceIgnore]
		static bool GetCallstack(ref List<LogStackFrame> callstack, out LogStackFrame originatingSourceLocation) {
			callstack.Clear();
			StackTrace stackTrace = new StackTrace(true);           // get call stack
			StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

			bool encounteredIgnoredMethodPreviously = false;

			originatingSourceLocation = null;

			for(int i = stackFrames.Length - 1; i >= 0; i--) {
				StackFrame stackFrame = stackFrames[i];

				var method = stackFrame.GetMethod();
				if(method.IsDefined(typeof(LogUnityOnly), true)) {
					return true;
				}
				if(!method.IsDefined(typeof(StackTraceIgnore), true)) {
					IgnoredUnityMethod.Mode showHideMode = ShowOrHideMethod(method);

					bool setOriginatingSourceLocation = (showHideMode == IgnoredUnityMethod.Mode.Show);

					if(showHideMode == IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod) {
						if(!encounteredIgnoredMethodPreviously) {
							encounteredIgnoredMethodPreviously = true;
							showHideMode = IgnoredUnityMethod.Mode.Show;
						} else
							showHideMode = IgnoredUnityMethod.Mode.Hide;
					}

					if(showHideMode == IgnoredUnityMethod.Mode.Show) {
						var logStackFrame = new LogStackFrame(stackFrame);

						callstack.Add(logStackFrame);

						if(setOriginatingSourceLocation)
							originatingSourceLocation = logStackFrame;
					}
				}
			}

			// Callstack has been processed backwards -- correct order for presentation
			callstack.Reverse();

			return false;
		}

		[StackTraceIgnore()]
		static public void Log(string channel, UnityEngine.Object source, LogSeverity severity, object message) {
			lock(Loggers) {
				if(!AlreadyLogging) {
					try {
						AlreadyLogging = true;

						foreach(IFilter filter in Filters) {
							if(!filter.ApplyFilter(channel, source, severity, message))
								return;
						}

						var callstack = new List<LogStackFrame>();
						LogStackFrame originatingSourceLocation;
						var unityOnly = GetCallstack(ref callstack, out originatingSourceLocation);
						if(unityOnly) {
							return;
						}

						var logInfo = new LogInfo(source, channel, severity, callstack, originatingSourceLocation, message);

						//Add this message to our history
						RecentMessages.AddLast(logInfo);

						//Make sure our history doesn't get too big
						TrimOldMessages();

						//Delete any dead loggers and pump them with the new log
						Loggers.RemoveAll(l => l == null);
						Loggers.ForEach(l => l.Log(logInfo));
					}
					finally {
						AlreadyLogging = false;
					}
				}
			}
		}

		static public T GetLogger<T>() where T : class {
			foreach(var logger in Loggers) {
				if(logger is T) {
					return logger as T;
				}
			}
			return null;
		}

		static void TrimOldMessages() {
			while(RecentMessages.Count > MaxMessagesToKeep) {
				RecentMessages.RemoveFirst();
			}
		}
	}
}
