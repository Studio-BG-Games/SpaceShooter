using System.Collections.Generic;
using UnityEngine;

namespace SpawnerSystem
{
    public class RandomList : ICreateEntity
    {
        public List<Container> _containers;
        
        public Transform Create(Vector3 position)
        {
            var r =_containers[Random.Range(0, _containers.Count)].TryGet().transform;
            r.position = position;
            return r;
        }

        [System.Serializable]
        public class Container
        {
            [Range(0,1f)] public float Chance;
            public GameObject prefab;

            public GameObject TryGet()
            {
                if (Random.Range(0, 1f) < Chance) return Object.Instantiate(prefab);
                else return new GameObject("Empty");
            }
        }
    }
}