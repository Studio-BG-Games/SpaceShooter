using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat
{
    
    /// <summary>
    /// Unity event for running functions when the ship lander launches.
    /// </summary>
    [System.Serializable]
    public class OnShipLaunchedEventHandler : UnityEvent { }

    /// <summary>
    /// Unity event for running functions when the ship lander is launching.
    /// </summary>
    [System.Serializable]
    public class OnShipLaunchingEventHandler : UnityEvent { }

    /// <summary>
    /// Unity event for running functions when the ship lander is landing.
    /// </summary>
    [System.Serializable]
    public class OnShipLandingEventHandler : UnityEvent { }

    /// <summary>
    /// Unity event for running functions when the ship lander has landed.
    /// </summary>
    [System.Serializable]
    public class OnShipLandedEventHandler : UnityEvent { }

    /// <summary>
    /// Provides a system for taking off and landing in a ship.
    /// </summary>
    public class ShipLander : MonoBehaviour
    {
        // The current state of the ship lander
        public enum ShipLanderState
        {
            Landed,
            Launched,
            Landing,
            Launching
        }

        [SerializeField]
        protected ShipLanderState currentState = ShipLanderState.Landed;
        public ShipLanderState CurrentState { get { return currentState; } }

        protected float currentStateStartTime;

        [Tooltip("The layers for objects the ship can land on.")]
        [SerializeField]
        protected LayerMask groundMask;

        [Header("Launching")]

        [Tooltip("How far the ship rises vertically when taking off.")]
        [SerializeField]
        protected float launchHeight = 20;

        [Tooltip("The height animation curve for taking off.")]
        [SerializeField]
        protected AnimationCurve launchCurve;

        [Tooltip("How long the taking off animation takes.")]
        [SerializeField]
        protected float launchTime = 3;

        [Header("Landing")]

        [Tooltip("How far above the ground the ship is when it is considered to be on the ground.")]
        [SerializeField]
        protected float landedHeight = 2.5f;

        [Tooltip("How far above the ground the landing animation can be triggered.")]
        [SerializeField]
        float minCanLandHeight = 20;

        [Tooltip("The height animation curve for landing")]
        [SerializeField]
        protected AnimationCurve landCurve;

        [Tooltip("How long the landing animation takes.")]
        [SerializeField]
        protected float landTime = 3;

        [Header("Events")]

        // Launched event
        public OnShipLaunchedEventHandler onShipLaunched;

        // Launching event
        public OnShipLaunchingEventHandler onShipLaunching;

        // Landed event
        public OnShipLandedEventHandler onShipLanded;

        // Landing event
        public OnShipLandingEventHandler onShipLanding;

        // Animation start/end parameters

        protected Vector3 startPos;
        protected Vector3 endPos;

        protected Quaternion startRot;
        protected Quaternion endRot;



        public void SetState(ShipLanderState newState)
        {
            switch (newState)
            {
                case ShipLanderState.Landed:

                    GetComponent<Rigidbody>().isKinematic = true;
                    currentState = ShipLanderState.Landed;
                    currentStateStartTime = Time.time;
                    onShipLanded.Invoke();

                    break;

                case ShipLanderState.Launched:

                    GetComponent<Rigidbody>().isKinematic = false;
                    currentState = ShipLanderState.Launched;
                    currentStateStartTime = Time.time;
                    onShipLaunched.Invoke();

                    break;

                case ShipLanderState.Landing:

                    GetComponent<Rigidbody>().isKinematic = true;
                    currentState = ShipLanderState.Landing;
                    currentStateStartTime = Time.time;

                    onShipLanding.Invoke();

                    break;

                case ShipLanderState.Launching:

                    GetComponent<Rigidbody>().isKinematic = true;

                    currentState = ShipLanderState.Launching;
                    
                    onShipLaunching.Invoke();

                    break;

            }

            currentStateStartTime = Time.time;

        }

        // Physics update
        void FixedUpdate()
        {
            switch (currentState)
            {
                // Launching animation
                case ShipLanderState.Launching:

                    float amount = (Time.time - currentStateStartTime) / launchTime;

                    if (amount >= 1)
                    {
                        transform.position = endPos;
                        transform.rotation = endRot;
                        SetState(ShipLanderState.Launched);
                    }
                    else
                    {
                        float curveAmount = launchCurve.Evaluate(amount);
                        transform.position = curveAmount * endPos + (1 - curveAmount) * startPos;
                        transform.rotation = Quaternion.Slerp(startRot, endRot, curveAmount);
                    }

                    break;

                // Landing animation
                case ShipLanderState.Landing:

                    amount = (Time.time - currentStateStartTime) / landTime;

                    if (amount >= 1)
                    {
                        transform.position = endPos;
                        transform.rotation = endRot;
                        SetState(ShipLanderState.Landed);
                    }
                    else
                    {
                        float curveAmount = landCurve.Evaluate(amount);
                        transform.position = curveAmount * endPos + (1 - curveAmount) * startPos;
                        transform.rotation = Quaternion.Slerp(startRot, endRot, curveAmount);
                    }                   

                    break;

            }
        }

        /// <summary>
        /// Check if the ship can land.
        /// </summary>
        /// <returns>Whether the ship can land.</returns>
        public bool CheckCanLand()
        {

            if (currentState == ShipLanderState.Landed) return false;

            RaycastHit hit;
            if (CheckCanLand(out hit))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Do a raycast check to see if the ship can land.
        protected bool CheckCanLand(out RaycastHit hit)
        {
            if (Physics.Raycast(transform.position, -transform.up, out hit, minCanLandHeight, groundMask))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Land the ship.
        /// </summary>
        public void Land()
        {

            if (currentState != ShipLanderState.Launched) return;

            RaycastHit hit;
            if (CheckCanLand(out hit))
            {

                // Get the start and end positions for the landing animation
                startPos = transform.position;
                endPos = hit.point + hit.normal * landedHeight;

                // Get the start/end rotations for the landing animation
                startRot = transform.rotation;

                Vector3 endForward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
                endRot = Quaternion.LookRotation(endForward, hit.normal);

                SetState(ShipLanderState.Landing);

            }
        }

        /// <summary>
        /// Launch the ship.
        /// </summary>
        public void Launch()
        {

            if (currentState != ShipLanderState.Landed) return;

            // Get the start/end positions for the launching animation
            startPos = transform.position;
            endPos = transform.position + transform.up * (launchHeight - landedHeight);

            // Get the start/end rotations for the launching animation
            startRot = transform.rotation;
            endRot = transform.rotation;

            SetState(ShipLanderState.Launching);

        }

        public void SetLanded()
        {
            SetState(ShipLanderState.Landed);
        }

        public void SetLaunched()
        {
            SetState(ShipLanderState.Launched);
        }
    }
}
