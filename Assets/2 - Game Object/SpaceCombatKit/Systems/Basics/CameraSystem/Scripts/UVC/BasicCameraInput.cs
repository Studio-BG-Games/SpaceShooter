using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.CameraSystem;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Camera controls script.
    /// </summary>
    public class BasicCameraInput : GeneralInput
    {

        [Header("Settings")] 

        [SerializeField]
        protected CameraEntity cameraEntity;

        [Header("Cycle Camera View")]

        [SerializeField]
        protected CustomInput cycleViewForwardInput = new CustomInput("Camera Controls", "Cycle camera view forward.", KeyCode.RightBracket);

        [SerializeField]
        protected CustomInput cycleViewBackwardInput = new CustomInput("Camera Controls", "Cycle camera view backward.", KeyCode.LeftBracket);

        [Header("Select Camera View")]

        [SerializeField]
        protected List<CameraViewInput> cameraViewInputs = new List<CameraViewInput>();

        [Header("Free Look Mode")]

        [SerializeField]
        protected float freeLookSpeed = 1;

        [SerializeField]
        protected CustomInput freeLookModeInput = new CustomInput("Camera Controls", "Free look mode.", KeyCode.LeftShift);

        [SerializeField]
        protected CustomInput lookHorizontalInput = new CustomInput("Camera Controls", "Free look horizontal.", "Mouse X");

        [SerializeField]
        protected CustomInput lookVerticalInput = new CustomInput("Camera Controls", "Free look vertical.", "Mouse Y");

        protected GimbalController cameraGimbalController;


        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        protected override bool Initialize()
        {          
            if (cameraEntity != null)
            {
      
                cameraGimbalController = cameraEntity.GetComponent<GimbalController>();
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void InputUpdate()
        {
            
            // Cycle camera view
            if (cameraEntity != null)
            {
                if (cycleViewForwardInput.Down())
                {
                    cameraEntity.CycleCameraView(true);
                }
                else if (cycleViewBackwardInput.Down())
                {
                    cameraEntity.CycleCameraView(false);
                }
            }

            // Select camera view
            for (int i = 0; i < cameraViewInputs.Count; ++i)
            {
                if (cameraViewInputs[i].input.Down())
                {
                    cameraEntity.SetView(cameraViewInputs[i].view);
                }
            }

            // Free look mode
            if (cameraGimbalController != null)
            {
                if (freeLookModeInput.Pressed())
                {
                    cameraGimbalController.Rotate(lookHorizontalInput.FloatValue() * freeLookSpeed,
                                                            -lookVerticalInput.FloatValue() * freeLookSpeed);
                }
                else if (freeLookModeInput.Up())
                {
                    cameraGimbalController.ResetGimbal(true);
                }
            }
        }
    }
}