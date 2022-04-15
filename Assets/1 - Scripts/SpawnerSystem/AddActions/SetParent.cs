using UnityEngine;

namespace SpawnerSystem
{
    public class SetParent : IAddActionSpawn
    {
        public Transform ParentForSpaningObject;

        public void Action(GameObject obj, Vector3 point) => obj.transform.SetParent(ParentForSpaningObject);
    }
}