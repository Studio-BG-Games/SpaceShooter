using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class GimbalControls : VehicleInput
    {

        [Header("Settings")]
      
        [SerializeField]
        protected float gimbalRotationSpeed = 100;

        [SerializeField]
        protected CustomInput horizontalRotationInputAxis = new CustomInput("Gimballed Vehicles", "Look Horizontal", "Mouse X");

        [SerializeField]
        protected CustomInput verticalRotationInputAxis = new CustomInput("Gimballed Vehicles", "Look Vertical", "Mouse Y");

        protected GimbalController gimbalController;



        protected override bool Initialize(Vehicle vehicle)
        {
            if(!base.Initialize(vehicle)) return false;

            gimbalController = vehicle.GetComponent<GimbalController>();

            if (gimbalController == null)
            {
                if (debugInitialization)
                {
                    Debug.LogWarning(GetType().Name + " failed to initialize - the required " + gimbalController.GetType().Name + " component was not found on the vehicle.");
                }
            }
            else
            {
                if (debugInitialization)
                {
                    Debug.Log(GetType().Name + " successfully initialized.");
                }
            }

            return (gimbalController != null);

        }

        protected override void InputUpdate()
        {
            base.InputUpdate();

            gimbalController.Rotate(horizontalRotationInputAxis.FloatValue() * gimbalRotationSpeed * Time.deltaTime,
                                            -verticalRotationInputAxis.FloatValue() * gimbalRotationSpeed * Time.deltaTime);
        }

    }
}