using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModelCore
{
    public class EntityAgregator : MonoBehaviour
    {
        private List<Entity> _entityes = new List<Entity>();

        private static EntityAgregator _instance;
        public static EntityAgregator Instance => _instance??=Create();

        private static EntityAgregator Create()
        {
            var go = new GameObject(nameof(EntityAgregator));
            DontDestroyOnLoad(go);
            return go.AddComponent<EntityAgregator>();
        }

        public Entity Select(Func<Entity, bool> predict) => _entityes.FirstOrDefault(predict);

        public Entity[] SelectAll(Func<Entity, bool> predict) => _entityes.Where(predict).ToArray();

        public void Add(Entity e)
        {
            if(!_entityes.Contains(e))
                _entityes.Add(e);
        }

        public void Remove(Entity e) => _entityes.Remove(e);
    }
}