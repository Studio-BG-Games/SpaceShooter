using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class AIVehicleBehaviour : MonoBehaviour
    {

        [Header("Vehicle Behaviour")]

        [Tooltip("The vehicle to be controlled by this AI behaviour.")]
        [SerializeField]
        protected Vehicle vehicle;

        [Tooltip("Whether to update this behaviour every frame. Leave unchecked if another script, such as a BehaviourSelector component, is managing the behaviours.")]
        [SerializeField]
        protected bool updateEveryFrame = false;
        public bool UpdateEveryFrame
        {
            get { return updateEveryFrame; }
            set { updateEveryFrame = value; }
        }

        protected bool initialized = false;
        public bool Initialized { get { return initialized; } }

        protected VehicleBehaviourState state;
        public VehicleBehaviourState State { get { return state; } }


        protected virtual void Awake()
        {
            if (vehicle != null) SetVehicle(vehicle);
        }

        /// <summary>
        /// Set the vehicle for this AI behaviour.
        /// </summary>
        /// <param name="vehicle">The vehicle for this AI behaviour.</param>
        public virtual void SetVehicle(Vehicle vehicle)
        {
            initialized = Initialize(vehicle);
        }


        /// <summary>
        /// Initialize with a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>Whether this behaviour successfully initialized.</returns>
        protected virtual bool Initialize(Vehicle vehicle)
        {
            if (vehicle != null)
            {
                this.vehicle = vehicle;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Start this behaviour.
        /// </summary>
        public virtual void StartBehaviour()
        {
            if (!initialized) return;

            state = VehicleBehaviourState.Started;
        
        }

        /// <summary>
        /// Stop this behaviour.
        /// </summary>
        public virtual void StopBehaviour()
        {
            state = VehicleBehaviourState.Stopped;

        }
   
        
        /// <summary>
        /// Update this behaviour.
        /// </summary>
        /// <returns>Whether the behaviour updated successfully.</returns>
        public virtual bool BehaviourUpdate()
        {
            if (!initialized) return false;

            if (state == VehicleBehaviourState.Stopped) StartBehaviour();

            return true;
        } 
        
        protected virtual void Update()
        {
            if (updateEveryFrame) BehaviourUpdate();
        }
    }
}