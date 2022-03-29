using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.Effects;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Adds rumbles based on a VehicleEngines3D component.
    /// </summary>
    public class VehicleEngines3DRumble : MonoBehaviour
    {

        [Tooltip("The engines component that is creating the rumbles.")]
        [SerializeField]
        protected VehicleEngines3D engines;

        protected Rigidbody rBody;
        protected float lastVelocityTime = 0;
        protected Vector3 lastVelocity = Vector3.zero;

        [Header("Boost Rumble")]

        [Tooltip("The rumble level used when boost input is at 1 (full boost).")]
        [SerializeField]
        protected float maxBoostRumble = 0.25f;


        [Header("Velocity Rumble")]

        [Tooltip("The velocity at which point the velocity rumble caps out at maximum.")]
        [SerializeField]
        protected float maxVelocityThreshold = 100;

        [Tooltip("The rumble level used when the velocity reaches the maximum threshold value.")]
        [SerializeField]
        protected float maxVelocityRumble = 0f;

        [Tooltip("The curve that describes the amount of velocity rumble applied as a function of the fraction of max velocity threshold reached (0 - 1).")]
        [SerializeField]
        protected AnimationCurve velocityRumbleCurve = AnimationCurve.Linear(0, 0, 1, 1);


        [Header("Acceleration Rumble")]

        [Tooltip("The acceleration at which point the acceleration rumble caps out at maximum.")]
        [SerializeField]
        protected float maxAccelerationThreshold = 100;

        [Tooltip("The rumble level used when the acceleration reaches the maximum threshold value.")]
        [SerializeField]
        protected float maxAccelerationRumble = 0.02f;

        [Tooltip("The curve that describes the amount of acceleration rumble applied as a function of the fraction of max acceleration threshold reached (0 - 1).")]
        [SerializeField]
        protected AnimationCurve accelerationRumbleCurve = AnimationCurve.Linear(0, 0, 1, 1);


        protected virtual void Awake()
        {
            // Get reference to the vehicle's rigidbody
            rBody = engines.GetComponent<Rigidbody>();
        }


        // Called every physics update
        protected virtual void FixedUpdate()
        {
            // If no rumble manager in scene, exit
            if (RumbleManager.Instance == null) return;


            // Boost

            // Apply boost rumble according to inputs
            RumbleManager.Instance.AddSingleFrameRumble(Mathf.Clamp(engines.BoostInputs.magnitude, 0, 1) * maxBoostRumble);


            // Velocity

            // Calculate the velocity rumble amount
            float velocityRumbleAmount = velocityRumbleCurve.Evaluate(Mathf.Clamp(rBody.velocity.magnitude / maxVelocityThreshold, 0, 1));

            // Apply the acceleration rumble
            RumbleManager.Instance.AddSingleFrameRumble(velocityRumbleAmount * maxVelocityRumble);


            // Acceleration

            // Calculate the acceleration for this physics update
            float acceleration = (rBody.velocity.magnitude - lastVelocity.magnitude) / Time.fixedDeltaTime;

            // Calculate the acceleration rumble amount
            float accelerationRumbleAmount = accelerationRumbleCurve.Evaluate(Mathf.Clamp(acceleration / maxAccelerationThreshold, 0, 1));

            // Apply the acceleration rumble
            RumbleManager.Instance.AddSingleFrameRumble(accelerationRumbleAmount * maxAccelerationRumble);

            // Store the current velocity for next frame acceleration calculation
            lastVelocity = rBody.velocity;
        }
    }
}
