using System;
using UnityEngine;

namespace ConsoleModul.Logger
{
    public class BaseLogger : ILogger
    {
        public event Action<ConsoleMessage> HasLog;
        
        public void Log(string mes) => HasLog?.Invoke(new ConsoleMessage().SetMessage(mes).SetColor(Color.white).SetBolt(false));

        public void Log(string mes, ConsoleMessage setting) => HasLog?.Invoke(setting.SetMessage(mes));
        
        public void Log(ConsoleMessage message) => HasLog?.Invoke(message);

        public void LogError(string mes) => HasLog?.Invoke(new ConsoleMessage().SetMessage(mes).SetColor(Color.red).SetBolt(false));

        public void LogWarning(string mes) => HasLog?.Invoke(new ConsoleMessage().SetMessage(mes).SetColor(Color.yellow));
    }
}