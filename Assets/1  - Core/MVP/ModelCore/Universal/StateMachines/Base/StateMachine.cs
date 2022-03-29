using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ModelCore.Universal.StateMachines
{
    public abstract class StateMachine<T> : Model
    {
        [JsonIgnore] public override string IdModel => $"{Prefics}{Alias}";
        public override string Prefics => "SM_";

        [JsonProperty] private Dictionary<T, State<T>> _states = new Dictionary<T, State<T>>();
        [JsonProperty] public State<T> CurrentState { get; private set; }
        [JsonProperty] public string Alias { get; private set; } 
        
        public delegate void StateHasChanged<TD>(TD oldState, TD newState);
        public event StateHasChanged<T> Changed;
        
        public StateMachine(string alias)
        {
            Alias = alias;
        }

        protected override void FinalRenane(string newName) => Alias = newName;

        public void Add(T states)
        {
            if(!_states.ContainsKey(states))
                _states.Add(states, Create(states));
        }

        public State<T> Get(T alias)
        {
            if(!_states.TryGetValue(alias, out var r))
                Root.Logger.LogWarning($"Запрос несуществуещего состояния, вернулся null. State - {alias}");
            return r;
        }
        
        public void Enter(T value)
        {
            var newState = Get(value);
            if (newState == null)
            {
                Root.Logger.LogError($"Нет состояния {value}. Состояние не поменялось");
                return;
            }
            if(newState==CurrentState)
                return;

            T oldValue = CurrentState != null ? CurrentState.Alias : default(T);
            CurrentState?.Off();
            CurrentState = newState;
            CurrentState.On();
            T newValue = CurrentState.Alias;
            Changed?.Invoke(oldValue, newValue);
        }

        protected abstract State<T> Create(T aliasState);
    }
}