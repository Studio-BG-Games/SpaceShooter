using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class PoweredVehicleEngines3D : VehicleEngines3D
    {

        [Header("Power")]

        [Tooltip("The Power component on this vehicle.")]
        [SerializeField]
        protected Power power;

        [Tooltip("The coefficients that are multiplied by the available 'direct' power to the engines to determine the rotation (steering) forces.")]
        [SerializeField]
        protected Vector3 powerToRotationForceCoefficients = new Vector3(0.1f, 0.1f, 0.2f);

        [Tooltip("The coefficients that are multiplied by the available 'direct' power to the engines to determine the translation (thrust) forces.")]
        [SerializeField]
        protected Vector3 powerToTranslationForceCoefficients = new Vector3(0.1f, 0.1f, 0.2f);

        [Header("Limits")]

        [SerializeField]
        Vector3 steeringForceLimits = new Vector3(30, 30, 50);

        [SerializeField]
        Vector3 movementForceLimits = new Vector3(2000, 2000, 2000);


        protected Vector3 defaultSteeringForces;
        public Vector3 DefaultSteeringForces
        {
            get { return defaultSteeringForces; }
        }

        protected Vector3 defaultMovementForces;
        public Vector3 DefaultMovementForces
        {
            get { return defaultMovementForces; }
        }


        protected override void Reset()
        {
            base.Reset();
            power = GetComponent<Power>();
        }

        protected override void Awake()
        {
            base.Awake();
            defaultSteeringForces = maxSteeringForces;
            defaultMovementForces = maxMovementForces;
        }

        protected override void Update()
        {
            if (power == null) return;

            base.Update();

            // Calculate the current available pitch, yaw and roll torques
            if (power.GetPowerConfiguration(PoweredSubsystemType.Engines) != SubsystemPowerConfiguration.Unpowered)
            {
                maxSteeringForces = power.GetSubsystemTotalPower(PoweredSubsystemType.Engines) * powerToRotationForceCoefficients;
            }
            else
            {
                maxSteeringForces = defaultSteeringForces;
            }

            // Clamp below maximum limits
            maxSteeringForces.x = Mathf.Min(maxSteeringForces.x, steeringForceLimits.x);
            maxSteeringForces.y = Mathf.Min(maxSteeringForces.y, steeringForceLimits.y);
            maxSteeringForces.z = Mathf.Min(maxSteeringForces.z, steeringForceLimits.z);

            // Calculate the currently available thrust
            if (power.GetPowerConfiguration(PoweredSubsystemType.Engines) != SubsystemPowerConfiguration.Unpowered)
            {
                maxMovementForces = power.GetSubsystemTotalPower(PoweredSubsystemType.Engines) * powerToTranslationForceCoefficients;
            }
            else
            {
                maxMovementForces = defaultMovementForces;
            }

            // Keep the thrust below the maximum limit
            maxMovementForces.x = Mathf.Min(maxMovementForces.x, movementForceLimits.x);
            maxMovementForces.y = Mathf.Min(maxMovementForces.y, movementForceLimits.y);
            maxMovementForces.z = Mathf.Min(maxMovementForces.z, movementForceLimits.z);
            
        }
    }
}