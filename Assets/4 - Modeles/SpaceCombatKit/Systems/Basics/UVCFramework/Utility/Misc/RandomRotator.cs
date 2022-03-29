using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Trigger a random rotation with specified limits around each axis.
    /// </summary>
    public class RandomRotator : MonoBehaviour
    {

        [SerializeField]
        protected Transform targetTransform;

        [Header("Rotation Limits X")]

        [SerializeField]
        protected float minRotationX = 0;

        [SerializeField]
        protected float maxRotationX = 0;

        [Header("Rotation Limits Y")]

        [SerializeField]
        protected float minRotationY = 0;

        [SerializeField]
        protected float maxRotationY = 0;

        [Header("Rotation Limits Z")]

        [SerializeField]
        protected float minRotationZ = 0;

        [SerializeField]
        protected float maxRotationZ = 0;


        protected virtual void Reset()
        {
            targetTransform = transform;
        }

        /// <summary>
        /// Implement a new random rotation.
        /// </summary>
        public void NewRotation()
        {

            transform.localRotation = Quaternion.Euler(Random.Range(minRotationX, maxRotationX),
                                                        Random.Range(minRotationY, maxRotationY),
                                                        Random.Range(minRotationZ, maxRotationZ));
        }
    }
}