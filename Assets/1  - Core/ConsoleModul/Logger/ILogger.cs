using System;

namespace ConsoleModul.Logger
{
    public interface ILogger
    {
        event Action<ConsoleMessage> HasLog;
        
        public void Log(string mes);
        public void Log(string mes, ConsoleMessage setting);
        public void Log(ConsoleMessage message);
        
        public void LogError(string mes);
        public void LogWarning(string mes);
    }
}