using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class CopyTransform : MonoBehaviour
    {
        [Tooltip("The other transform that this transform will be following.")]
        [SerializeField]
        protected Transform copyTarget;

        [Tooltip("Whether to copy the position 'x' value of the target transform.")]
        [SerializeField]
        protected bool copyPositionX = true;

        [Tooltip("Whether to copy the position 'y' value of the target transform.")]
        [SerializeField]
        protected bool copyPositionY = true;

        [Tooltip("Whether to copy the position 'z' value of the target transform.")]
        [SerializeField]
        protected bool copyPositionZ = true;

        [Tooltip("Whether to copy the rotation of the target transform.")]
        [SerializeField]
        protected bool copyRotation = true;

        [Tooltip("Whether to return to zero world space rotation every frame. Useful if this is a child of a transform and you want its rotation to not be affected.")]
        [SerializeField]
        protected bool clearRotation = false;

        [Tooltip("Whether to copy the target transform when in the Unity Editor.")]
        [SerializeField]
        protected bool enableInEditor = false;


        protected void OnValidate()
        {
            if (enableInEditor)
            {
                UpdateTransform();
            }
        }



        protected virtual void UpdateTransform()
        {
            if (copyTarget != null)
            {
                // Copy position
                Vector3 nextPosition = new Vector3(copyPositionX ? copyTarget.position.x : transform.position.x,
                                                copyPositionY ? copyTarget.position.y : transform.position.y,
                                                copyPositionZ ? copyTarget.position.z : transform.position.z);

                transform.position = nextPosition;

                // Copy rotation
                if (copyRotation) transform.rotation = copyTarget.rotation;
            }

            // Clear rotation
            if (clearRotation) transform.rotation = Quaternion.identity;
        }


        protected void LateUpdate()
        {
            UpdateTransform();
        }
    }
}