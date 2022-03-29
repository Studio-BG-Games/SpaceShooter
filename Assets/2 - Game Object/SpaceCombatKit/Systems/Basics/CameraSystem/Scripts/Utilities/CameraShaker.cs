using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.CameraSystem
{
    /// <summary>
    /// Shake the camera.
    /// </summary>
    public class CameraShaker : MonoBehaviour
    {

        [Header("General")]

        [Tooltip("The transform to be shaken.")]
        [SerializeField]
        public Transform cameraTransform;

        [Header("Shake Parameters")]

        [Tooltip("The maximum shake vector length that describes the angle for the shake relative to a unit forward vector.")]
        [SerializeField]
        protected float maxShakeVectorLength = 0.05f;


        protected virtual void Awake()
        {
            StartCoroutine(ResetRotationCoroutine());
        }

        // Shake the camera once.
        public virtual void SingleFrameShake(float shakeStrength)
        {
            
            // Get a random vector on the xy plane
            Vector3 localShakeVector = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f).normalized;

            // Scale according to desired shake magnitude
            localShakeVector *= shakeStrength * maxShakeVectorLength;

            // Calculate the look target 
            Vector3 shakeLookTarget = cameraTransform.TransformPoint(Vector3.forward + localShakeVector);
            
            // Look at the target
            cameraTransform.LookAt(shakeLookTarget, transform.up);

        }      

        // Reset the rotation at the end of the frame
        IEnumerator ResetRotationCoroutine()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                cameraTransform.localRotation = Quaternion.identity;
            }
        }
    }
}