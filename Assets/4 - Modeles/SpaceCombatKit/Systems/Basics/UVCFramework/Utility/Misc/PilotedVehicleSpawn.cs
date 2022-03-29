using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Spawns a pilot (Game Agent) and a vehicle in the scene.
    /// </summary>
    public class PilotedVehicleSpawn : MonoBehaviour
    {
        [Header("General")]

        [Tooltip("Whether to create the pilot and vehicle as soon as the scene starts.")]
        [SerializeField]
        protected bool createOnStart = true;
        protected bool created = false;

        [Tooltip("Whether the pilot should enter the vehicle as soon as it is created.")]
        [SerializeField]
        protected bool enterVehicleOnCreated = true;

        [Tooltip("The transform that represents the position and rotation of the vehicle when it is spawned.")]
        [SerializeField]
        protected Transform spawnTransform;
        public virtual Transform SpawnTransform { get { return transform; } }


        [Header("Pilot")]

        [Tooltip("A reference to the pilot of the vehicle. May be a prefab or already present in the scene.")]
        [SerializeField]
        protected GameAgent pilotReference;

        [Tooltip("Whether to instantiate the pilot. Be sure to set this to True if the pilot reference is a prefab.")]
        [SerializeField]
        protected bool instantiatePilot = true;

        protected GameAgent spawnedPilot;
        public GameAgent Pilot { get { return spawnedPilot; } }


        [Header("Vehicle")]

        [Tooltip("A reference to the vehicle. May be a prefab or already present in the scene.")]
        [SerializeField]
        protected Vehicle vehicleReference;

        [Tooltip("Whether to instantiate the vehicle. Be sure to set this to True if the vehicle reference is a prefab.")]
        [SerializeField]
        protected bool instantiateVehicle = true;

        protected Vehicle spawnedVehicle;
        public Vehicle Vehicle { get { return spawnedVehicle; } }

        protected bool spawned = false;
        public bool Spawned { get { return spawned; } }

        protected bool destroyed = false;
        public virtual bool Destroyed
        {
            get { return destroyed; }
        }

        [Header("Events")]

        public UnityEvent onSpawned;
        public UnityEvent onDestroyed;


        protected virtual void Reset()
        {
            // Initialize the spawn transform
            spawnTransform = transform;
        }


        protected virtual void Start()
        {
            if (createOnStart) Create();
        }


        protected virtual void OnVehicleDestroyed()
        {
            destroyed = true;
            onDestroyed.Invoke();
        }


        /// <summary>
        /// Create the pilot and the vehicle.
        /// </summary>
        public virtual void Create()
        {
            if (created) return;

            if (instantiatePilot)
            {
                spawnedPilot = Instantiate(pilotReference, transform);
                spawnedPilot.transform.localPosition = Vector3.zero;
                spawnedPilot.transform.localRotation = Quaternion.identity;
            }
            else
            {
                spawnedPilot = pilotReference;
            }

            if (instantiateVehicle)
            {
                spawnedVehicle = Instantiate(vehicleReference, transform);
                spawnedVehicle.transform.localPosition = Vector3.zero;
                spawnedVehicle.transform.localRotation = Quaternion.identity;
            }
            else
            {
                spawnedVehicle = vehicleReference;
            }

            if (enterVehicleOnCreated)
            {
                spawnedPilot.EnterVehicle(spawnedVehicle);
            }

            spawnedVehicle.onDestroyed.AddListener(OnVehicleDestroyed);

            created = true;
        }


        /// <summary>
        /// Spawn the piloted vehicle. If they have already been created, they will be revived/restored.
        /// </summary>
        public virtual void Spawn()
        {
            Create();

            if (spawned) ResetSpawn();

            spawned = true;

            onSpawned.Invoke();
        }


        /// <summary>
        /// In the case of an animated spawn, skip to the end and finish.
        /// </summary>
        public virtual void FinishSpawn() { }


        /// <summary>
        /// Reset the spawn so it is ready to spawn again.
        /// </summary>
        public virtual void ResetSpawn()
        {
            if (!spawned) return;

            if (spawnedPilot.IsDead) spawnedPilot.Revive();
            if (spawnedVehicle.Destroyed) spawnedVehicle.Restore();
            spawnedPilot.EnterVehicle(spawnedVehicle);

            spawnedVehicle.CachedRigidbody.velocity = Vector3.zero;
            spawnedVehicle.CachedRigidbody.angularVelocity = Vector3.zero;

            destroyed = false;
        }

        protected void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, 10);
        }
    }
}

