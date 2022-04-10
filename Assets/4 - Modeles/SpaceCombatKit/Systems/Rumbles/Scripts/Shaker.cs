using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.Effects
{
    /// <summary>
    /// Creates a shake effect for a Transform by rotating it randomly in small amounts.
    /// </summary>
    public class Shaker : MonoBehaviour
    {
        [Tooltip("The transform to shake.")]
        [SerializeField]
        protected Transform shakenTransform;

        [Tooltip("Whether to apply a rotation around the X axis when shaking the object.")]
        [SerializeField]
        protected bool shakeXAxis = true;

        [Tooltip("Whether to apply a rotation around the Y axis when shaking the object.")]
        [SerializeField]
        protected bool shakeYAxis = true;

        [Tooltip("Whether to apply a rotation around the Z axis when shaking the object.")]
        [SerializeField]
        protected bool shakeZAxis = true;

        [Tooltip("The shake magnitude. This is the length of the vector, perpendicular to the forward axis, that represents where the transform will 'look' when a shake is applied.")]
        [SerializeField]
        protected float maxShake = 0.01f;

        protected bool shaken = false;  // A flag that records if a shake occurred during the current frame.


        protected virtual void Awake()
        {
            if (shakenTransform.parent == null)
            {
                Debug.LogError("Shaker component must be added to a child transform that has a parent.");
            }
        }

        /// <summary>
        /// Apply a shake at a specified level (0-1).
        /// </summary>
        /// <param name="level">The shake level.</param>
        public void Shake(float level)
        {

            if (Time.timeScale < 0.0001f)
            {
                shakenTransform.localRotation = Quaternion.identity;

                return;
            }

            // Do a single shake
            // Get a random vector on the xy plane
            Vector3 shakeVector = new Vector3(shakeXAxis ? UnityEngine.Random.Range(-1, 1) : 0,
                                                shakeYAxis ? UnityEngine.Random.Range(-1, 1) : 0,
                                                shakeZAxis ? UnityEngine.Random.Range(-1, 1) : 0).normalized;

            // Scale according to desired shake magnitude
            shakeVector *= level * maxShake;

            Vector3 lookDirection, upVector;
            
            // Look at shake vector
            lookDirection = (shakenTransform.parent.TransformDirection(Vector3.forward).normalized + shakeVector).normalized;
            upVector = shakenTransform.parent.TransformDirection(Vector3.up);
            

            shakenTransform.rotation = Quaternion.LookRotation(lookDirection, upVector);

            shaken = true;

        }

        private void LateUpdate()
        {
            // If a shake occurred during this frame, keep the shake rotation.
            if (shaken)
            {
                shaken = false;
            }
            // If the shake occurred last frame, clear the shake rotation.
            else
            {
                shakenTransform.localRotation = Quaternion.identity;
            }
        }
    }
}
