using QueueSystem;
using UnityEngine;

namespace SpawnerSystem
{
    public class SetQueue : IAddActionSpawn
    {
        public Queue QueueOfSpawner;
        
        public void Action(GameObject obj, Vector3 point)
        {
            if (QueueOfSpawner == null)
            {
                Debug.LogWarning("Спаунеру не установлен очередь для хаспауненых объектов", obj);
                return;
            }
            if(obj.TryGetComponent<Queue>(out var queue)) queue.CopyFrom(QueueOfSpawner);
        }
    }
}