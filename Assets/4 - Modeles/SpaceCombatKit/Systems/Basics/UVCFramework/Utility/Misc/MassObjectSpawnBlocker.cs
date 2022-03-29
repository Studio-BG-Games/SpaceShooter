using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Block an area from being spawned in (e.g. preventing asteroids from being spawned inside a station). Used by the MassObjectSpawner component.
    /// </summary>
    public class MassObjectSpawnBlocker : MonoBehaviour
    {
        [Tooltip("The distance from this transform within which an object will not be allowed to spawn.")]
        [SerializeField]
        protected float clearanceRadius = 300;


        /// <summary>
        /// Whether a world position is blocked by this spawn blocker.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <returns>Whether the position is blocked from spawning.</returns>
        public virtual bool IsBlocked(Vector3 position)
        {
            return (Vector3.Distance(position, transform.position) < clearanceRadius);
        }

        // Visualize the clearance radius
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, clearanceRadius);
        }
    }
}

