using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace QueueSystem
{
    public class GroupQue : BaseQue
    {
        [SerializeField] [SerializeReference] [JsonProperty] private List<BaseQue> _actions;

        public override void OnInit(GameObject parent) => _actions.ForEach(x=>x.OnInit(parent));

        public override void OnStart() => _actions.ForEach(x=>x.OnStart());
        
        protected override void Update(float delta) => _actions.ForEach(x=>x.OnUpdate(delta));
        
        public override void OnFinish() => _actions.ForEach(x=>x.OnFinish());
    }
}