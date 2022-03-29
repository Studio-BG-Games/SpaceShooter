using System;

namespace ConsoleModul.PartConsole.ComandConsoless
{
    public class ArgumentCommand : CommandBase
    {
        private readonly Action<string[]> _callback;

        public ArgumentCommand(string description, string format ,Action<string[]> callback) : base(description, format)
        {
            _callback = callback;
        }

        public void Invoke(string[] arg) => _callback?.Invoke(arg);
    }
}