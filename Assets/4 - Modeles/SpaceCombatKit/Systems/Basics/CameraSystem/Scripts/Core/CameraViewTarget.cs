using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace VSX.CameraSystem
{

    /// <summary>
    /// A camera view target is a transform that represents the position and rotation target for the camera to follow, with additional customizable settings.
    /// </summary>
    public class CameraViewTarget : MonoBehaviour
    {

        [Header("General")]

        [Tooltip("Determines the order that camera view targets will be accessed, affecting cycling backward/forward of camera views.")]
        [SerializeField]
        protected int sortingIndex = 0;
        public int SortingIndex { get { return sortingIndex; } }

        [Tooltip("The Camera View for this camera view target.")]
        [SerializeField]
        protected CameraView cameraView;
        public CameraView CameraView { get { return cameraView; } }

        [Tooltip("Whether to parent the camera to this camera view target when the view is selected.")]
        [SerializeField]
        protected bool parentCameraOnSelected;
        public bool ParentCameraOnSelected { get { return parentCameraOnSelected; } }

        [Header("Position Settings")]

        [Tooltip("Whether to lock the camera position to this camera view target when it is selected.")]
        [SerializeField]
        protected bool lockPosition;
        public bool LockPosition { get { return lockPosition; } }

        [Tooltip("How strongly the camera follows the position of this transform.")]
        [SerializeField]
        protected float positionFollowStrength = 0.4f;
        public float PositionFollowStrength { get { return positionFollowStrength; } }

        [Tooltip("How much of a sideways offset occurs for the camera in proportion to the roll angular velocity of the camera target.")]
        [SerializeField]
        protected float spinOffsetCoefficient = 1f;
        public float SpinOffsetCoefficient { get { return spinOffsetCoefficient; } }

        [SerializeField]
        protected float yawLateralOffset = 1f;
        public float YawLateralOffset { get { return yawLateralOffset; } }

        [Header("Rotation Settings")]

        [Tooltip("Whether to lock the camera's orientation to the forward axis of this transform.")]
        [SerializeField]
        protected bool lockCameraForwardVector = true;
        public bool LockCameraForwardVector { get { return lockCameraForwardVector; } }

        [Tooltip("Whether to lock the rotation of the camera to this transform.")]
        [SerializeField]
        protected bool lockRotation;
        public bool LockRotation { get { return lockRotation; } }

        [Tooltip("How strongly the camera follows the rotation of this camera view target.")]
        [SerializeField]
        protected float rotationFollowStrength = 0.08f;
        public float RotationFollowStrength { get { return rotationFollowStrength; } }

        [Tooltip("Keep the camera upright (relative to the world Y axis) at all times.")]
        [SerializeField]
        protected bool lockCameraUpright = false;
        public bool LockCameraUpright { get { return lockCameraUpright; } }

        [Header("Events")]

        public UnityEvent onSelected;

        

        protected virtual void Awake()
        {
            if (cameraView == null)
            {
                Debug.LogWarning("Camera View Target component should have a Camera View assigned, please assign one in the inspector.");
            }
        }

        /// <summary>
        /// Called when this camera view target is selected.
        /// </summary>
        public void OnSelected()
        {
            onSelected.Invoke();
        }
    }
}
