using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Trigger a random scaling along each axis.
    /// </summary>
    public class RandomScaler : MonoBehaviour
    {

        [SerializeField]
        protected Transform targetTransform;

        [Header("Scale Limits X")]

        [SerializeField]
        protected float minScaleX = 0;

        [SerializeField]
        protected float maxScaleX = 0;

        [Header("Scale Limits Y")]

        [SerializeField]
        protected float minScaleY = 0;

        [SerializeField]
        protected float maxScaleY = 0;

        [Header("Scale Limits Z")]

        [SerializeField]
        protected float minScaleZ = 0;

        [SerializeField]
        protected float maxScaleZ = 0;


        protected void Reset()
        {
            targetTransform = transform;
        }

        /// <summary>
        /// Implement a new random scale.
        /// </summary>
        public void NewScale()
        {

            transform.localScale = new Vector3(Random.Range(minScaleX, maxScaleX),
                                                        Random.Range(minScaleY, maxScaleY),
                                                        Random.Range(minScaleZ, maxScaleZ));
        }
    }
}