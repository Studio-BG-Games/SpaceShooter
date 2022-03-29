using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Instantiate one or more objects in the scene.
    /// </summary>
    public class ObjectSpawner : MonoBehaviour
    {

        [Tooltip("Whether to spawn the objects when this object is enabled.")]
        [SerializeField]
        protected bool spawnOnEnable = true;

        [Tooltip("Whether to use object pooling or instantiate a new instance every time.")]
        [SerializeField]
        protected bool usePoolManager;

        [Tooltip("A list of gameobjects to be spawned.")]
        [SerializeField]
        protected List<GameObject> objectsToSpawn = new List<GameObject>();

        [Tooltip("The transform representing the position and rotation to spawn objects with.")]
        [SerializeField]
        protected Transform spawnTransform;

        [SerializeField]
        protected bool parentToSpawnTransform = false;


        protected virtual void Reset()
        {
            spawnTransform = transform;
        }


        protected virtual void OnEnable()
        {
            if (spawnOnEnable)
            {
                SpawnAll();
            }
        }


        /// <summary>
        /// Spawn all the objects in the list.
        /// </summary>
        public virtual void SpawnAll()
        {
            for(int i = 0; i < objectsToSpawn.Count; ++i)
            {
                SpawnObject(objectsToSpawn[i]);
            }
        }

        /// <summary>
        /// Spawn an object at a specified index in the list.
        /// </summary>
        /// <param name="index">The list index.</param>
        public virtual void SpawnByIndex(int index)
        {
            if (objectsToSpawn.Count >= index)
            {
                SpawnObject(objectsToSpawn[index]);
            }
        }

        /// <summary>
        /// Spawn an object.
        /// </summary>
        /// <param name="objectToSpawn"></param>
        protected virtual void SpawnObject(GameObject objectToSpawn)
        {
            GameObject obj = Instantiate(objectToSpawn, spawnTransform.position, spawnTransform.rotation);
            if (parentToSpawnTransform)
            {
                obj.transform.SetParent(spawnTransform);
            }
        }
    }
}

