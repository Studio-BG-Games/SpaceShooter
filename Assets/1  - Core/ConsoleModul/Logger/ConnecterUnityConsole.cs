using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ConsoleModul.Logger
{
    public class ConnecterUnityConsole : ILogHandler, ILogger
    {
        public static ConnecterUnityConsole Instance => _instancec??=new ConnecterUnityConsole();
        private static ConnecterUnityConsole _instancec;

        private ConnecterUnityConsole()
        {
            if(!Application.isEditor)
                Debug.unityLogger.logHandler = this;
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            var mes = string.Format(format, args);
            switch (logType)
            {
                case LogType.Assert:
                {
                    Log(new ConsoleMessage().SetMessage(mes).SetColor(new Color(1f, 0.85f, 0.89f)));
                }
                break;
                case LogType.Warning:
                {
                    Log(new ConsoleMessage().SetMessage(mes).SetColor(Color.yellow));
                }
                break;

                case LogType.Error:
                {
                    LogError("Error: "+ mes);
                }
                break;

                case LogType.Exception:
                {
                    LogError("Exception: "+ mes);
                }
                break;

                case LogType.Log:
                {
                    Log("Log: "+mes);
                } 
                break;

            }
        }

        public void LogException(Exception exception, Object context)
        {
            LogError("____");
            LogError(exception.Message);
            LogError(exception.StackTrace);
            LogError("____");
        }

        public event Action<ConsoleMessage> HasLog;

        public void Log(string mes) => HasLog?.Invoke(new ConsoleMessage().SetMessage(mes).SetColor(Color.white));

        public void Log(string mes, ConsoleMessage setting) => HasLog?.Invoke(setting.SetMessage(mes));

        public void Log(ConsoleMessage message) => HasLog?.Invoke(message);

        public void LogError(string mes) => HasLog?.Invoke(new ConsoleMessage().SetMessage(mes).SetColor(Color.red));

        public void LogWarning(string mes) => HasLog?.Invoke(new ConsoleMessage().SetMessage(mes).SetColor(Color.yellow));
    }
}