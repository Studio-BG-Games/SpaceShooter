using System;
using ConsoleModul.Logger;
using UnityEngine;
using ILogger = ConsoleModul.Logger.ILogger;

namespace Js
{
    public class LoggerToUnity : ILogger
    {
        public event Action<ConsoleMessage> HasLog;
        public void Log(string mes) => Debug.Log(mes);

        public void Log(string mes, ConsoleMessage setting)=> Debug.Log(mes);

        public void Log(ConsoleMessage message)
        {
            Debug.Log(message.Message);
            HasLog?.Invoke(message);
        }

        public void LogError(string mes) => Debug.LogError(mes);

        public void LogWarning(string mes) => Debug.Log(mes);
    }
}