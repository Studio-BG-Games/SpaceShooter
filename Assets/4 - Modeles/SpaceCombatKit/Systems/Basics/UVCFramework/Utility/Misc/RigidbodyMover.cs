using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class moves a rigidbody at a constant velocity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyMover : MonoBehaviour
    {

        [SerializeField]
        protected Vector3 localVelocity;
        public Vector3 LocalVelocity 
        { 
            get { return localVelocity; }
            set { localVelocity = value; }
        }

        private Rigidbody rBody;
        public Rigidbody Rigidbody
        {
            get { return rBody; }
        }


        private void Awake()
        {
            rBody = GetComponent<Rigidbody>();
        }

        // Called when the component is first added to a gameobject, or reset in the inspector
        private void Reset()
        {
            // Initialize rigidbody with good values
            Rigidbody r = GetComponent<Rigidbody>();
            r.useGravity = false;
            r.drag = 0;
        }


        private void Update()
        {
            // Set velocity according to facing direction
            rBody.velocity = transform.TransformDirection(localVelocity);
        }
    }
}