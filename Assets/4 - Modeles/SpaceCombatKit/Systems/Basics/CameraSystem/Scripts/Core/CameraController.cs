using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.CameraSystem
{
    /// <summary>
    /// Base class for a script that controls the camera.
    /// </summary>
    public class CameraController : MonoBehaviour
    {

        [Header("General")]

        // A reference to the camera this controller is controlling
        protected CameraEntity cameraEntity;
        public virtual void SetCamera(CameraEntity cameraEntity)
        {
            this.cameraEntity = cameraEntity;
            this.cameraEntity.onCameraViewTargetChanged.AddListener(OnCameraViewTargetChanged);
        }

        [Header("Starting Values")]

        [Tooltip("The camera view that is selected upon switching to a new camera target.")]
        [SerializeField]
        protected CameraView startingView;

        [Tooltip("Whether to default to the first available view, if the startingView value is not set.")]
        [SerializeField]
        protected bool defaultToFirstAvailableView = true;

        // Whether this camera controller is currently activated
        protected bool controllerEnabled = false;
        public bool ControllerEnabled 
        { 
            get { return controllerEnabled; }
            set { controllerEnabled = value; }
        }

        // Whether this camera controller is ready to be activated
        protected bool initialized = false;
        public bool Initialized { get { return initialized; } }


        /// <summary>
        /// Called when the camera target that the camera is currently following changes.
        /// </summary>
        /// <param name="target">The new camera target.</param>
        /// <param name="startController">Whether to start the controller immediately.</param>
        public virtual void OnCameraTargetChanged(CameraTarget target)
        {

            initialized = false;

            if (target == null) return;

            if (Initialize(target))
            {
                initialized = true;

                if (startingView != null)
                {
                    cameraEntity.SetView(startingView);
                }
                else
                {
                    if (defaultToFirstAvailableView && target.CameraViewTargets.Count > 0)
                    {
                        cameraEntity.SetCameraViewTarget(target.CameraViewTargets[0]);
                    }
                    else
                    {
                        cameraEntity.SetView(null);
                    }
                }
            }
        }

        /// <summary>
        /// Initialize this camera controller with a camera target.
        /// </summary>
        /// <param name="target">The camera target.</param>
        /// <returns>Whether initialization was successful.</returns>
        protected virtual bool Initialize(CameraTarget target)
        {
            return true;
        }

        protected virtual void OnCameraViewTargetChanged(CameraViewTarget newTarget)
        {
            if (newTarget != null)
            {
                cameraEntity.transform.position = newTarget.transform.position;
                cameraEntity.transform.rotation = newTarget.transform.rotation;
            }
        }


        protected virtual void FixedUpdate()
        {
            // If not activated or no camera view target selected, exit.
            if (!initialized || !controllerEnabled || cameraEntity.CameraTarget == null) return;

            CameraControllerFixedUpdate();
        }


        // Put controller code for physics-driven camera targets here
        protected virtual void CameraControllerFixedUpdate() 
        {

            if (cameraEntity.CurrentViewTarget == null || cameraEntity.CameraTarget.Rigidbody == null) return;

            // Calculate the target position for the camera
            Vector3 targetPosition = cameraEntity.CurrentViewTarget.transform.position;

            // Update position
            cameraEntity.transform.position = (1 - cameraEntity.CurrentViewTarget.PositionFollowStrength) * cameraEntity.transform.position +
                                        cameraEntity.CurrentViewTarget.PositionFollowStrength * targetPosition;

            // Update rotation
            cameraEntity.transform.rotation = Quaternion.Slerp(cameraEntity.transform.rotation, cameraEntity.CurrentViewTarget.transform.rotation,
                                                    cameraEntity.CurrentViewTarget.RotationFollowStrength);

        }


        protected virtual void Update()
        {
            // If not activated or no camera view target selected, exit.
            if (!initialized || !controllerEnabled || cameraEntity.CameraTarget == null) return;

            CameraControllerUpdate();
        }


        // Put controller code for non-physics-driven camera targets here
        protected virtual void CameraControllerUpdate() 
        
        {
            if (cameraEntity.CurrentViewTarget == null || cameraEntity.CameraTarget.Rigidbody != null) return;

            // Calculate the target position for the camera
            Vector3 targetPosition = cameraEntity.CurrentViewTarget.transform.position;

            // Update position
            cameraEntity.transform.position = (1 - cameraEntity.CurrentViewTarget.PositionFollowStrength) * cameraEntity.transform.position +
                                        cameraEntity.CurrentViewTarget.PositionFollowStrength * targetPosition;

            // Update rotation
            cameraEntity.transform.rotation = Quaternion.Slerp(cameraEntity.transform.rotation, cameraEntity.CurrentViewTarget.transform.rotation,
                                                    cameraEntity.CurrentViewTarget.RotationFollowStrength);
        }
        
        
        protected virtual void LateUpdate()
        {
            // If not activated or no camera view target selected, exit.
            if (!initialized || !controllerEnabled || cameraEntity.CameraTarget == null) return;

            CameraControllerLateUpdate();
        }


        // Put controller code here that needs to occur after all Update methods in the game are completed for the frame.
        protected virtual void CameraControllerLateUpdate() 
        {
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