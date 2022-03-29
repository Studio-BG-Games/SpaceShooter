using System.Collections.Generic;
using System.Linq;
using ConsoleModul.PartConsole;
using JetBrains.Annotations;
using UnityEngine;
using Object = System.Object;

namespace ConsoleModul
{
    public class Console
    {
        private Dictionary<string, IPartConsole> _consoles = new Dictionary<string, IPartConsole>();
        
        public void Add(IPartConsole partConsole, string id)
        {
            if (_consoles.TryGetValue(id, out var value))
                return;
            _consoles.Add(id, partConsole);
        }

        [CanBeNull]
        public IPartConsole Get(string id)
        {
            _consoles.TryGetValue(id, out var value);
            return value;
        }

        public T Get<T>(string id) where T : class, IPartConsole => Get(id) as T;

        public void Remove(string id)
        {
            _consoles.Remove(id);
        }
    }
}