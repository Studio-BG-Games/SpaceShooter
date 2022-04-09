using System.Collections.Generic;
using ModelCore;
using Services;
using UnityEngine;

namespace Models
{
    public class C_ConnectHealthWithRef : MonoBehaviour
    {
        public List<Target> Targets;
        
        public void Connect(Entity e)
        {
            Targets.ForEach(x =>
            {
                if (!x.TrySet(e)) Debug.LogWarning($"У сущности {e.name} нет здоровья по id {x.TargetId.name}", e);
            });
        }
        
        [System.Serializable]
        public class Target
        {
            public HealthID TargetId;
            public HealthfRef Ref;

            public bool TrySet(Entity e)
            {
                var h =  e.Select<Health>(x => x.Id == TargetId);
                if (h == null) return false;
                Ref.Init(h);
                return true;
            }
        }
    }
}