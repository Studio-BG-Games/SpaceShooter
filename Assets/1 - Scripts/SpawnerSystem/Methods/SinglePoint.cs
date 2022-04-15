using System.Collections.Generic;
using UnityEngine;

namespace SpawnerSystem
{
    public class SinglePoint : ISpawnMethod
    {
        public Transform Point;

        public float GizmosSize = 5;


        public void DrawGizmos()
        {
            if (!Point) return;
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(Point.position, GizmosSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Point.position, GizmosSize+5);
        }

        public void Spawn(ICreateEntity spawnPrefab, List<IAddActionSpawn> addsActions)
        {
            var newobj = spawnPrefab.Create(Point.position).gameObject;
            addsActions.ForEach(x=>x.Action(newobj, Point.position));
        }
    }
}