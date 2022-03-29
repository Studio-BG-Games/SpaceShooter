using System;
using ConsoleModul.Logger;
using UnityEngine;
using ILogger = ConsoleModul.Logger.ILogger;

namespace ConsoleModul.PartConsole.LoggerConsoles
{
    public class LoggerObserver
    {
        private CircleBuffer<ConsoleMessage> _bufferMessage;
        private ILogger _logger;

        public event Action<ConsoleMessage> NewLog;

        public LoggerObserver(ILogger logger, int size)
        {
            logger.HasLog += OnLog;
            _logger = logger;
            Debug.Assert(size>2);
            _bufferMessage = new CircleBuffer<ConsoleMessage>(size);
        }

        public void Clear() => _bufferMessage.Clear();

        public ConsoleMessage[] GetAllMessage() => _bufferMessage.GetElements();
            
        private void OnLog(ConsoleMessage obj)
        {
            _bufferMessage.Add(obj);
            NewLog?.Invoke(obj);
        }

        public void ChangeSize(int newSize)
        {
            Clear();
            _bufferMessage.ChangeSize(newSize);
        }
    }
}