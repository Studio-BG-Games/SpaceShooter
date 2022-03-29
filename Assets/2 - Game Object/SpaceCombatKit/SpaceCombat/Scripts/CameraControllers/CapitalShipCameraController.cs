using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.CameraSystem;

namespace VSX.UniversalVehicleCombat
{
    public class CapitalShipCameraController : VehicleCameraController
    {

        [Header("Boost Effects")]

        [SerializeField]
        private float boostFieldOfView = 80;

        [SerializeField]
        private float boostFieldOfViewLerpSpeed = 0.1f;
        
        protected VehicleEngines3D engines;


        protected void Reset()
        {
            // Reset the compatible vehicle class to capital ship
            specifyCompatibleVehicleClasses = true;
        }

        protected override bool Initialize(CameraTarget newTarget)
        {
            if (!base.Initialize(newTarget)) return false;

            engines = newTarget.GetComponent<VehicleEngines3D>();

            return (engines != null);
        }


        protected override void CameraControllerLateUpdate()
        {

            if (!controllerEnabled) return;
           
            // Positioning of the locked interior camera must be done in late update to make sure that there is no position lag, so that the 
            // aiming reticle lines up with the camera forward vector			
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
            }
        }


        protected override void CameraControllerUpdate()
        {

            if (!controllerEnabled) return;

            if (engines != null)
            {
                float targetFOV = engines.BoostInputs.z * boostFieldOfView +
                                    (1 -engines.BoostInputs.z) * cameraEntity.DefaultFieldOfView;

                cameraEntity.MainCamera.fieldOfView = Mathf.Lerp(cameraEntity.MainCamera.fieldOfView, targetFOV, boostFieldOfViewLerpSpeed);

            }
        }
    }
}
