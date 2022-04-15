using UnityEngine;

namespace SpawnerSystem
{
    public class SimpleObject : ICreateEntity
    {
        public GameObject Prefab;
        
        public Transform Create(Vector3 position)
        {
            var t = Object.Instantiate(Prefab).transform;
            t.position = position;
            return t;
        }
    }
}