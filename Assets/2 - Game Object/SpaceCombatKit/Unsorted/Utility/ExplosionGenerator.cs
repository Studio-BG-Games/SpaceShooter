using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.Pooling;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Create an explosion on an event.
    /// </summary>
    public class ExplosionGenerator : MonoBehaviour
    {
        [Tooltip("Whether to use the pool manager for spawning the explosion, rather than instantiating a new one.")]
        [SerializeField]
        protected bool usePoolManager;

        [SerializeField]
        protected GameObject explosionPrefab;


        private void Start()
        {
            if (usePoolManager && PoolManager.Instance == null)
            {
                Debug.LogWarning("No pool manager found in scene, please add one to pool explosions.");
                usePoolManager = false;
            }
        }

        /// <summary>
        /// Create an explosion at this transform's position.
        /// </summary>
        public virtual void Explode()
        {
            if (usePoolManager)
            {
                PoolManager.Instance.Get(explosionPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}
