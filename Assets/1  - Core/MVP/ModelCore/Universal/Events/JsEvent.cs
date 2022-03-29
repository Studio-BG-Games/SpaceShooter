using System;
using Newtonsoft.Json;

namespace ModelCore.Universal
{
    public class JsEvent : Model
    {
        [JsonIgnore] public override string IdModel => $"{Prefics}{Alias}";
        [JsonIgnore] public override string Prefics => "Event_";


        public event Action Event;
        [JsonProperty] public string Alias { get; private set; }
        public JsEvent(string alias) => Alias = alias;

        public void Trig() => Event?.Invoke();

        protected override void FinalRenane(string newName)
        {
            Alias = newName;
        }
    }
}