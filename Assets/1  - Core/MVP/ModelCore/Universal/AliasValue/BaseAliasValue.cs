using System;
using ModelCore.Messages;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

namespace ModelCore.Universal.AliasValue
{
    public interface IAliasValue
    {
        public Type TypeOfValue { get; }
    }
    
    public abstract class BaseAliasValue<T> : Model, IAliasValue
    {
        [JsonIgnore] public override string IdModel => $"{PrefixValue()}_{Alias}";
        [JsonIgnore] public override string Prefics => PrefixValue()+"_";

        [JsonIgnore] public Type TypeOfValue => Value.GetType();
        
        [JsonIgnore] public string Alias => _alias;
        [JsonProperty] private string _alias = "AliasName";

        public event Action<T, T> Update;
        private Func<T, T, T> _valid;
        
        [JsonIgnore] public  T Value
        {
            get => _value;
            set
            {
                var old = _value;
                if (_valid != null) _value = _valid(old, value);
                else _value = value;
                Update?.Invoke(old, _value);
            }
        }

        [JsonProperty] protected T _value = default;

        protected override void FinalRenane(string newName) => _alias = newName;

        [JsonConstructor] private BaseAliasValue(){}

        public override void CopyFrom(Model other)
        {
            if (other is BaseAliasValue<T>) Value = (other as BaseAliasValue<T>).Value;
        }

        public override void SendMessage(Message message)
        {
            if (TryCast<SetValue<T>>(message, out var r)) Value = r.Value;

            bool TryCast<X>(Message m, out X r) where X : Message
            {
                r = m as X;
                return r != null;
            }
        }

        public BaseAliasValue(string alias)
        {
            _alias = alias;
            Value = default;
        }
        
        public BaseAliasValue(string alias, T value)
        {
            _alias = alias;
            Value = value;
        }

        public void SetValid(Func<T, T, T> valid)
        {
            _valid = valid;
            Valid();
        }

        public void Valid() => Value = Value;

        protected abstract string PrefixValue();
    }
}