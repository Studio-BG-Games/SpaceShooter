using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// A spawn point for a member of a wave.
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        [Header("Spawner")]

        protected bool usePoolManager = false;

        [SerializeField]
        protected bool spawnOnEnable = false;

        public virtual bool Destroyed { get { return false; } }

        public UnityEvent onSpawned;


        protected virtual void OnEnable()
        {
            if (spawnOnEnable)
            {
                Spawn();
            }
        }

        /// <summary>
        /// Spawn the object.
        /// </summary>
        public virtual void Spawn()
        {
            onSpawned.Invoke();
        }
    }
}
