using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.CameraSystem;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Base class for a script that controls the camera for a specific type of vehicle.
    /// </summary>
    public class VehicleCameraController : CameraController
    {

        [Header("General")]

        [Tooltip("Whether to specify the vehicle classes that this camera controller is for.")]
        [SerializeField]
        protected bool specifyCompatibleVehicleClasses;

        [Tooltip("The vehicle classes that this camera controller is compatible with.")]
        [SerializeField]
        protected List<VehicleClass> compatibleVehicleClasses = new List<VehicleClass>();

        protected VehicleCamera vehicleCamera;
        public override void SetCamera(CameraEntity cameraEntity)
        {
            base.SetCamera(cameraEntity);
            vehicleCamera = cameraEntity.GetComponent<VehicleCamera>();
            if (vehicleCamera == null)
            {
                Debug.LogError("Cannot use a Vehicle Camera Controller with the Camera Entity component. Must be used with Vehicle Camera instead.");
            }
        }


        protected override bool Initialize(CameraTarget newTarget)
        {

            Vehicle vehicle = newTarget.GetComponent<Vehicle>();
            if (vehicle == null)
            {
                return false;
            }

            // If compatible vehicle classes are specified, check that the list contains this vehicle's class.
            if (specifyCompatibleVehicleClasses)
            {
                if (compatibleVehicleClasses.IndexOf(vehicle.VehicleClass) != -1)
                {
                    return (base.Initialize(newTarget));
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return (base.Initialize(newTarget));
            }
        }
    }
}