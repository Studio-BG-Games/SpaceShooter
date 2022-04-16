using System.Collections.Generic;
using UnityEngine;

namespace SpawnerSystem
{
    public class OrderObjects : ICreateEntity
    {
        public List<GameObject> Objects;
        public bool ToShufle;

        private int index=0;
        
        public Transform Create(Vector3 position)
        {
            var r = Spawn();
            r.position = position;
            return r;
        }

        public Transform Spawn()
        {
            if (Objects == null || Objects.Count == 0)
                return new GameObject("Empty").transform;
            if (ToShufle) Shuffle();
            var result = Objects[index];
            index++;
            if (index >= Objects.Count) index = 0;
            return Object.Instantiate(result).transform;
        }

        private void Shuffle()
        {
            ToShufle = false;
            for (int i = 0; i < Objects.Count*3; i++)
            {
                var startIndex = i % Objects.Count;
                var nextIndex = Random.Range(0, Objects.Count);
                var tempObj = Objects[startIndex];
                Objects[startIndex] = Objects[nextIndex];
                Objects[nextIndex] = tempObj;
            }
        }
    }
}