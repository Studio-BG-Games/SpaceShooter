using MaxyGames.uNode;
using MaxyGames.uNodeLogger;
using System;
using System.Text;

namespace MaxyGames {
	public static class uNodeDebug {
		[StackTraceIgnore]
		static public void Log(object message, UnityEngine.Object context) {
			Logger.Log("", context, LogSeverity.Message, message);
		}

		[StackTraceIgnore]
		static public void Log(object message) {
			Logger.Log("", null, LogSeverity.Message, message);
		}

		[StackTraceIgnore]
		static public void LogWarning(object message, UnityEngine.Object context) {
			Logger.Log("", context, LogSeverity.Warning, message);
		}

		[StackTraceIgnore]
		static public void LogWarning(object message) {
			Logger.Log("", null, LogSeverity.Warning, message);
		}

		[StackTraceIgnore]
		static public void LogError(object message, UnityEngine.Object context) {
			Logger.Log("", context, LogSeverity.Error, message);
		}

		[StackTraceIgnore]
		static public void LogError(object message) {
			Logger.Log("", null, LogSeverity.Error, message);
		}

		[StackTraceIgnore]
		static public System.Exception LogException(System.Exception exception, UnityEngine.Object context) {
			Logger.Log("", context, LogSeverity.Error, exception);
			if(exception is uNodeException) {
				return exception;
			}
			return new uNodeException(exception, context);
		}

		[StackTraceIgnore]
		static public System.Exception LogException(System.Exception exception) {
			Logger.Log("", null, LogSeverity.Error, exception);
			if(exception is uNodeException) {
				return exception;
			}
			return new uNodeException(exception, null);
		}
	}
}
