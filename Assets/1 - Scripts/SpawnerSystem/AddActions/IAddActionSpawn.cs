using UnityEngine;

namespace SpawnerSystem
{
    public interface IAddActionSpawn
    {
        void Action(GameObject obj, Vector3 point);
    }
}