using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Base class for vehicle input components.
    /// </summary>
    public class VehicleInput : GeneralInput
    {

        [Header("Vehicle Input")]

        // Vehicle to control when the scene starts
        [SerializeField]
        protected Vehicle startingVehicle;

        [SerializeField]
        protected bool specifyCompatibleVehicleClasses = false;

        [SerializeField]
        protected List<VehicleClass> compatibleVehicleClasses = new List<VehicleClass>();


        protected override void Start()
        {
            // Initialize with the starting vehicle
            if (startingVehicle != null)
            {
                SetVehicle(startingVehicle);
            }
        }

        /// <summary>
        /// Set a new vehicle for the input component.
        /// </summary>
        /// <param name="vehicle">The new vehicle</param>
        /// <param name="startInput">Whether to start input if initialization is successful.</param>
        public virtual void SetVehicle(Vehicle vehicle)
        {
            
            initialized = false;

            if (vehicle == null) return;

            initialized = Initialize(vehicle);

        }

        /// <summary>
        /// Attempt to initialize the input component with a vehicle reference.
        /// </summary>
        /// <param name="vehicle">The vehicle to attempt initialization with.</param>
        /// <returns> Whether initialization was successful. </returns>
        protected virtual bool Initialize(Vehicle vehicle)
        {
            if (specifyCompatibleVehicleClasses)
            {
                for (int i = 0; i < compatibleVehicleClasses.Count; ++i)
                {
                    if (compatibleVehicleClasses[i] == vehicle.VehicleClass)
                    {
                        return true;
                    }
                }

                if (debugInitialization)
                {
                    Debug.LogWarning(GetType().Name + " failed to initialize. Vehicle is not a compatible vehicle class.");
                }

                return false;
            }
            else
            {
                return true;
            }
        }
    }
}