using System;
using Newtonsoft.Json;

namespace ModelCore.Universal.StateMachines
{
    public class State<T>
    {
        [JsonProperty] public T Alias { get; private set; }
        public event Action Enter;
        public event Action Exit;
        
        public State(T alias) => Alias = alias;

        public void On() => Enter?.Invoke();
        
        public void Off() => Exit?.Invoke();
    }
}