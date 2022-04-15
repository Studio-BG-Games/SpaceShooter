using DefaultNamespace;
using Dreamteck.Forever;
using UnityEngine;

namespace SpawnerSystem
{
    public class SetOffset : IAddActionSpawn
    {
        public void Action(GameObject obj, Vector3 point)
        {
            if (obj.TryGetComponent<Runner>(out var r))
            {
                GlobalHelp.SetOffsetProjectTile(r, point);
            }
            else
            {
                Debug.LogWarning($"У {obj.name} нет Runner для установки смещения", obj);
            }
        }
    }
}