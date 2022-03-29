using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Represents a gimbal that is used to target weapons fire by using raycast detection to adjust weapons fire.
    /// </summary>
    public class TargetingGimbal : GimbalController
    {

        [Header("Targeting Gimbal")]

        [Tooltip("Whether to unparent the gimbal and set it to follow position of a transform. Prevents view from changing when a vehicle is rotating.")]
        [SerializeField]
        protected bool unparentAndFollowPosition = true;

        [Tooltip("The target transform to follow the position of.")]
        [SerializeField]
        protected Transform followTarget;

        [Tooltip("The targeting gimbal object, which must be un-parented from the vehicle so as to maintain rotation when the vehicle is turning.")]
        [SerializeField]
        protected Transform targetingGimbalObject;

        [Tooltip("A transform that represents the point in space where turret fire should converge.")]
        [SerializeField]
        protected Transform aimTarget;


        protected void Reset()
        {
            followTarget = transform;
        }

        private void Start()
        {
            if (unparentAndFollowPosition && targetingGimbalObject != null) targetingGimbalObject.SetParent(null);
        }
        
        protected void LateUpdate()
        {
            if (unparentAndFollowPosition && targetingGimbalObject != null)
            {
                targetingGimbalObject.position = followTarget.position;
                targetingGimbalObject.rotation = Quaternion.identity;
            }
        }
    }
}

