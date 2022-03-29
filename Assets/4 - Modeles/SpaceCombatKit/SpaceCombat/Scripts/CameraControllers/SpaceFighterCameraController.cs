using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.CameraSystem;

namespace VSX.UniversalVehicleCombat.Space
{
    /// <summary>
    /// This class controls the camera for a space fighter.
    /// </summary>
    public class SpaceFighterCameraController : VehicleCameraController
    {
      
        [Header("Boost Effects")]

        // The field of view to use when boosting.
        [SerializeField]
        private float boostFieldOfView = 75;

        // How fast the field of view changes when boost is activated or deactivated.
        [SerializeField]
        private float boostFieldOfViewLerpSpeed = 0.1f;

        // Cached references for necessary parts of the vehicle
        private Rigidbody vehicleRigidbody;
        private Engines vehicleEngines;

        
        /// <summary>
        /// Activate this vehicle camera controller.
        /// </summary>
        protected override bool Initialize(CameraTarget newTarget)
        {

            if (!base.Initialize(newTarget)) return false;

            // Cache references
            vehicleRigidbody = newTarget.GetComponent<Rigidbody>();
            vehicleEngines = newTarget.GetComponent<Engines>();

            return true;
        }


        // Physics update
        protected override void CameraControllerFixedUpdate()
        {
            
            if (cameraEntity.CurrentViewTarget == null) return;

            float spinLateralOffset = 0, yawLateralOffset = 0;
            if (vehicleRigidbody != null)
            {
                // Calculate the lateral offset from the camera view target based on the spin.
                spinLateralOffset = cameraEntity.CurrentViewTarget.SpinOffsetCoefficient *
                                            -cameraEntity.CurrentViewTarget.transform.InverseTransformDirection(vehicleRigidbody.angularVelocity).z;

                yawLateralOffset = cameraEntity.CurrentViewTarget.YawLateralOffset *
                                            cameraEntity.CurrentViewTarget.transform.InverseTransformDirection(vehicleRigidbody.angularVelocity).y;
            }
            
            // Calculate the target position for the camera 
            Vector3 targetPosition = cameraEntity.CurrentViewTarget.transform.TransformPoint(new Vector3(spinLateralOffset + yawLateralOffset, 0f, 0f));

            // Lerp toward the target position
            cameraEntity.transform.position = (1 - cameraEntity.CurrentViewTarget.PositionFollowStrength) * cameraEntity.transform.position +
                                        cameraEntity.CurrentViewTarget.PositionFollowStrength * targetPosition;

            // Spherically interpolate between the current rotation and the target rotation.
            cameraEntity.transform.rotation = Quaternion.Slerp(transform.rotation, cameraEntity.CurrentViewTarget.transform.rotation,
                                                    cameraEntity.CurrentViewTarget.RotationFollowStrength);        
            
        }

    
        // Called every frame
        protected override void CameraControllerUpdate()
        {

            // Update boost field of view effects. 
            if (vehicleEngines != null  && vehicleEngines.EnginesActivated == true)
            {
                float targetFieldOfView = vehicleEngines.BoostInputs.z * boostFieldOfView + 
                                    (1 - vehicleEngines.BoostInputs.z) * cameraEntity.DefaultFieldOfView;

                // Set the new field of view
                cameraEntity.SetFieldOfView(Mathf.Lerp(cameraEntity.MainCamera.fieldOfView, targetFieldOfView, boostFieldOfViewLerpSpeed));
                
            }

            // If position and/or rotation are locked for the selected camera view target, the position and rotation must be updated in 
            // late update to make sure that there is no lag.
            if (cameraEntity.CurrentViewTarget != null)
            {
                if (cameraEntity.CurrentViewTarget.LockPosition)
                {
                    cameraEntity.transform.position = cameraEntity.CurrentViewTarget.transform.position;
                }
                if (cameraEntity.CurrentViewTarget.LockRotation)
                {
                    cameraEntity.transform.rotation = cameraEntity.CurrentViewTarget.transform.rotation;
                }

                if (cameraEntity.CurrentViewTarget.LockCameraForwardVector)
                {
                    // Always point the camera directly forward 
                    cameraEntity.transform.rotation = Quaternion.LookRotation(cameraEntity.CurrentViewTarget.transform.forward, cameraEntity.transform.up);
                }
            }
        }
    }
}
