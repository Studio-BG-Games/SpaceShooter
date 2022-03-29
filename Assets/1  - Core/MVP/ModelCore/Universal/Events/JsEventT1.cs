using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ModelCore.Universal
{
    public abstract class JsEventT1<T> : Model, IJsEventT
    {
        [JsonIgnore] public Type GetTypeEvent => typeof(T);
        [JsonIgnore] public override string IdModel => $"{Prefics}{Alias}";
        [JsonIgnore] public override string Prefics => "EventT_";
        [JsonProperty] public string Alias { get; private set; }
        
        public event Action<T> Event;
        
        public JsEventT1(string alias) => Alias = alias;
        
        public void Trig(T arg) => Event?.Invoke(arg);

        protected override void FinalRenane(string newName) => Alias = newName;
    }

    public interface IJsEventT
    {
        Type GetTypeEvent { get; }
    }
}