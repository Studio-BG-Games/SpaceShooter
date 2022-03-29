using System;

namespace ConsoleModul.PartConsole.ComandConsoless
{
    public class Command : CommandBase
    {
        private readonly Action _callback;

        public Command(string description, string format, Action callback) : base(description, format)
        {
            _callback = callback;
        }

        public void Invoke() => _callback?.Invoke();
    }
}