using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace QueueSystem
{
    [System.Serializable]
    public abstract class BaseQue
    { 
        [JsonProperty][Min(0)][SerializeField] private float _lifeTime;
        [ReadOnly][ShowInInspector]private float _pastTime;

        public bool IsEnd => _pastTime >= _lifeTime;
        
        public virtual void OnInit(GameObject parent) { }
        public virtual void OnStart() { }

        public void OnUpdate(float deltaTime)
        {
            _pastTime += deltaTime;
            _pastTime = Mathf.Clamp(_pastTime, 0, _lifeTime);
            Update(deltaTime);
        }
        
        public virtual void OnFinish() { }

        public virtual void Zero() => _pastTime = 0;

        protected virtual void Update(float deltaTime) { }
    }
}