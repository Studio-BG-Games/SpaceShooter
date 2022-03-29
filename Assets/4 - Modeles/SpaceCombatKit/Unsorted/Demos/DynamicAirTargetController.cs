using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Controller for a moving air target in the demo.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class DynamicAirTargetController : MonoBehaviour
    {

        [Tooltip("The minimum random speed for this air target.")]
        [SerializeField]
        protected float minSpeed = 50;

        [Tooltip("The maximum random speed for this air target.")]
        [SerializeField]
        protected float maxSpeed = 150;

        [Tooltip("The minimum x position for this air target.")]
        [SerializeField]
        protected float minX = -250;

        [Tooltip("The maximum x position for this air target.")]
        [SerializeField]
        protected float maxX = 250;

        protected float posY;
        protected float posZ;

        protected Rigidbody rBody;


        protected void Awake()
        {
            rBody = GetComponent<Rigidbody>();

            posY = transform.localPosition.y;
            posZ = transform.localPosition.z;

            int startingDirection = 1 + Random.Range(0, 2) * -2;
            rBody.velocity = Vector3.right * startingDirection * Random.Range(minSpeed, maxSpeed);
        }


        // Called every frame
        protected void Update()
        {
            // Prevent rotation
            rBody.rotation = Quaternion.identity;
            rBody.angularVelocity = Vector3.zero;

            // Set the velocity
            Vector3 velocity = rBody.velocity;
            if (transform.localPosition.x < minX && rBody.velocity.x < 0)
            {
                velocity = Vector3.right * Random.Range(minSpeed, maxSpeed);
            }
            else if (transform.localPosition.x > maxX && rBody.velocity.x > 0)
            {
                velocity = -Vector3.right * Random.Range(minSpeed, maxSpeed);
            }

            velocity.y = posY - transform.localPosition.y;
            velocity.z = posZ - transform.localPosition.z;

            rBody.velocity = velocity;

        }
    }
}