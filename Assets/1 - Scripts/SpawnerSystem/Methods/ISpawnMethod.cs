using System.Collections.Generic;
using UnityEngine;

namespace SpawnerSystem
{
    public interface ISpawnMethod
    {
        void DrawGizmos();
        void Spawn(ICreateEntity spawnPrefab, List<IAddActionSpawn> addsActions);
    }
}