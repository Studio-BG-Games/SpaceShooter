using System.Collections.Generic;
using Infrastructure;
using UnityEngine;

namespace SpawnerSystem
{
    public class ArrayPoints : ISpawnMethod
    {
        public Transform Start;
        public Vector3 Diraction;
        [Min(0)] public float DelayDetweenSpawn;
        public float Distance;
        [Min(1)] public int Count;
        
        public float GizmosSize = 5;

        private Vector3 GetPointByIndex(int i) => Start.position + (Diraction * Distance * i);
        
        public void DrawGizmos()
        {
            for (int i = 0; i < Count; i++)
            {
                var point = GetPointByIndex(i);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(point, GizmosSize);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(point, GizmosSize+5);
            }
        }

        public void Spawn(ICreateEntity spawnPrefab, List<IAddActionSpawn> actions)
        {
            for (int i = 0; i < Count; i++)
            {
                var newObject = spawnPrefab.Create(GetPointByIndex(i)).gameObject;
                newObject.SetActive(false);
                var tempVar = i; // Dont delete tempVar or i delete you
                CorutineGame.Instance.Wait(DelayDetweenSpawn * tempVar, () =>
                {
                    newObject.SetActive(true);
                    CorutineGame.Instance.WaitFrame(1, ()=>actions.ForEach(x => x.Action(newObject, GetPointByIndex(tempVar))));
                });
            }
        }
    }
}