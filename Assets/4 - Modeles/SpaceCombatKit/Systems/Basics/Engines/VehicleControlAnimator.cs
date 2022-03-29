using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.CameraSystem;

namespace VSX.UniversalVehicleCombat
{
    public class VehicleControlAnimator : MonoBehaviour, ICameraEntityUser
    {

        [SerializeField]
        protected bool specifyCameraViews = false;

        [SerializeField]
        protected List<CameraView> cameraViews = new List<CameraView>();

        [SerializeField]
        protected Rigidbody m_Rigidbody;

        [SerializeField]
        protected float animationLerpSpeed = 0.5f;

        [SerializeField]
        protected bool activated = true;

        [Header("Roll")]

        [SerializeField]
        protected float sideRotationToRoll = 20;

        [SerializeField]
        protected float sideMovementToRoll = -0.15f;

        [Header("Turn Following")]

        [SerializeField]
        protected float verticalTurnFollowing = 8;

        [SerializeField]
        protected float sideTurnFollowing = 5;

        protected CameraEntity cameraEntity;





        public void SetCameraEntity(CameraEntity entity)
        {
            // Disconnect event from previous camera
            if (this.cameraEntity != null)
            {
                this.cameraEntity.onCameraViewTargetChanged.RemoveListener(OnCameraViewChanged);
            }

            // Set new camera
            this.cameraEntity = entity;

            // Connect to event on new camera
            if (this.cameraEntity != null)
            {
                OnCameraViewChanged(cameraEntity.CurrentViewTarget);
                this.cameraEntity.onCameraViewTargetChanged.AddListener(OnCameraViewChanged);
            }
        }


        void OnCameraViewChanged(CameraViewTarget newCameraViewTarget)
        {
            ResetTransform();

            activated = false;

            ResetTransform();

            // Check camera view
            if (specifyCameraViews)
            {
                if (newCameraViewTarget == null) return;

                if (cameraViews.IndexOf(newCameraViewTarget.CameraView) == -1)
                {
                    return;
                }
            }

            activated = true;
        }


        protected void ResetTransform()
        {
            transform.localRotation = Quaternion.identity;
        }


        // Update is called once per frame
        void FixedUpdate()
        {
            if (activated)
            {
                Vector3 localAngularVelocity = m_Rigidbody.transform.InverseTransformDirection(m_Rigidbody.angularVelocity);

                Vector3 localVelocity = m_Rigidbody.transform.InverseTransformDirection(m_Rigidbody.velocity);

                float targetRoll = sideRotationToRoll * -localAngularVelocity.y + sideMovementToRoll * localVelocity.x;
                float targetPitch = verticalTurnFollowing * localAngularVelocity.x;
                float targetYaw = sideTurnFollowing * localAngularVelocity.y;

                // Yaw to roll
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(targetPitch, targetYaw, targetRoll), animationLerpSpeed);
            }
        }
    }
}
