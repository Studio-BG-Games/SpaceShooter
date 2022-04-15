using UnityEngine;

namespace SpawnerSystem
{
    public interface ICreateEntity
    {
        Transform Create(Vector3 position);
    }
}