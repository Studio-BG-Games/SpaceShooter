using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Provides a controller for the static (bobbing) air target used in the demo.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class StaticAirTargetController : MonoBehaviour
    {
        [Tooltip("The length of the vertical bob for this target.")]
        [SerializeField]
        protected float bobLength = 1;

        [Tooltip("An animation curve to drive the bob movement, representing the vertical position over time.")]
        [SerializeField]
        protected AnimationCurve bobCurve;

        [Tooltip("The minimum random bob speed.")]
        [SerializeField]
        protected float minBobSpeed;

        [Tooltip("The maximum random bob.")]
        [SerializeField]
        protected float maxBobSpeed;

        protected float currentBobSpeed;

        // The starting position of the current bob
        protected Vector3 startingLocalPos;

        // The direction of the current bob
        protected float currentDirection;

        protected Rigidbody rBody;



        protected void Awake()
        {
            rBody = GetComponent<Rigidbody>();
        }


        protected void Start()
        {
            startingLocalPos = transform.localPosition;

            currentDirection = 1 + Random.Range(0, 2) * -2;

            currentBobSpeed = Random.Range(minBobSpeed, maxBobSpeed);
        }


        // Called every frame
        protected void Update()
        {
            
            // Lock the rotation
            rBody.rotation = Quaternion.identity;
            rBody.angularVelocity = Vector3.zero;

            // Set the position
            float posY = startingLocalPos.y + bobCurve.Evaluate((Time.time * currentBobSpeed) % 1) * bobLength;
            Vector3 nextPos = new Vector3(startingLocalPos.x, posY, startingLocalPos.z);
            Vector3 nextWorldPos = transform.parent == null ? nextPos : transform.parent.TransformPoint(nextPos);
            //Debug.Log(startingLocalPos);
            rBody.position = nextWorldPos;

        }
    }
}