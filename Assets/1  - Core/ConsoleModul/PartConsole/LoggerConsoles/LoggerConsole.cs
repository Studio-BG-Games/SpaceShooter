using System;
using System.Collections.Generic;
using ConsoleModul.Logger;
using UnityEngine;
using ILogger = ConsoleModul.Logger.ILogger;

namespace ConsoleModul.PartConsole.LoggerConsoles
{
    public class LoggerConsole : IPartConsole
    {
        private Dictionary<string, LoggerObserver> _consoleObserver = new Dictionary<string, LoggerObserver>();
        public event Action AddedLoger;
        public event Action RemovedLoger;

        public List<string> AvaibleLoggers()
        {
            List<string> result = new List<string>();
            foreach (var keyValuePair in _consoleObserver) result.Add(keyValuePair.Key);
            return result;
        }

        public LoggerObserver GetObserver(string id)
        {
            _consoleObserver.TryGetValue(id, out var v);
            return v;
        }

        public void ChageBuffer(string consoleName, int newSize)
        {
            var observer = GetObserver(consoleName);
            if(observer==null || newSize<2)
                return;
            observer.ChangeSize(newSize);
        }
        
        public ConsoleMessage[] GetMessagesOrNull(string nameConsole)
        {
            _consoleObserver.TryGetValue(nameConsole, out var observer);
            if (observer == null)
                return null;
            return observer.GetAllMessage();
        }
        
        public void AddLoger(ILogger logger, string name)
        {
            if (!_consoleObserver.ContainsKey(name))
            {
                _consoleObserver.Add(name, new LoggerObserver(logger, 16));
                AddedLoger?.Invoke();
            }
        }

        public void RemoveLoger(string name)
        {
            _consoleObserver.Remove(name);
            RemovedLoger?.Invoke();
        }

        public void AddLoger(ILogger logger, string name,  int size)
        {
            if (!_consoleObserver.ContainsKey(name))
            {
                _consoleObserver.Add(name, new LoggerObserver(logger, size));
                AddedLoger?.Invoke();
            }
        }
    }
}