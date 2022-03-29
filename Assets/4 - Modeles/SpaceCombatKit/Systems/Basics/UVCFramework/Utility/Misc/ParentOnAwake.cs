using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Parent an object to another object on awake.
    /// </summary>
    public class ParentOnAwake : MonoBehaviour
    {

        [Tooltip("The object to parent this object to when the scene starts.")]
        [SerializeField]
        protected Transform parentObject;

        [Tooltip("Whether to reset the local position of this object after parenting.")]
        [SerializeField]
        protected bool resetPosition = true;

        [Tooltip("Whether to reset the local rotation of this object after parenting.")]
        [SerializeField]
        protected bool resetRotation = true;

        [Tooltip("Whether to reset the local scale of this object after parenting.")]
        [SerializeField]
        protected bool resetScale = true;


        protected void Awake()
        {
            // Parent the object
            transform.SetParent(parentObject);

            // Reset the local position
            if (resetPosition)
            {
                transform.localPosition = Vector3.zero;
            }

            // Reset the local rotation
            if (resetRotation)
            {
                transform.localRotation = Quaternion.identity;
            }

            // Reset the local scale
            if (resetScale)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }
}
