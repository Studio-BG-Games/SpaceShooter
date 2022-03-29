using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using VSX.CameraSystem;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Mobile controls for a space fighter.
    /// </summary>
    public class MobileSpacefighterControls : VehicleInput
    {

        [Header("General")]

        [Tooltip("Whether to yaw when rolling. Traces out a circle with the nose rather than rolling on a point.")]
        [SerializeField]
        protected bool linkYawAndRoll = false;

        [Tooltip("The amount to yaw when rolling.")]
        [SerializeField]
        protected float yawRollRatio = 1f;


        [Header("Pitch")]

        [Tooltip("Whether to use axis input for pitch.")]
        [SerializeField]
        protected bool useAxisForPitch = true;

        [Tooltip("The name of the cross platform input axis for pitch.")]
        [SerializeField]
        protected string pitchAxisName = "Vertical";

        [Tooltip("Whether to invert the pitch (nose up/down).")]
        [SerializeField]
        protected bool invertPitchAxis = false;


        [Header("Yaw")]

        [Tooltip("Whether to use axis input for yaw.")]
        [SerializeField]
        protected bool useAxisForYaw = true;

        [Tooltip("The name of the cross platform input axis for yaw.")]
        [SerializeField]
        protected string yawAxisName = "Horizontal";

        [Tooltip("Whether to invert the yaw (nose left/right).")]
        [SerializeField]
        protected bool invertYawAxis = false;


        [Header("Roll")]

        [Tooltip("Whether to use axis input for roll.")]
        [SerializeField]
        protected bool useAxisForRoll = false;

        [Tooltip("The name of the cross platform input axis for roll.")]
        [SerializeField]
        protected string rollAxisName = "Horizontal";

        [Tooltip("Whether to invert the roll.")]
        [SerializeField]
        protected bool invertRollAxis = false;

        [Tooltip("Whether to start the throttle at the setting that it is found at upon entering the vehicle or when this script is enabled (if False, throttle will start at zero).")]
        [SerializeField]
        protected bool initializeThrottleToVehicle = true;

        [SerializeField]
        protected Slider throttleSlider;

        [Header("Camera")]

        [SerializeField]
        protected CameraEntity cameraEntity;
        
        protected Vector3 steeringInputs;
        protected Vector3 movementInputs;
        protected Vector3 boostInputs;

        protected VehicleEngines3D engines;

        protected TriggerablesManager triggerablesManager;


        // Initialize the vehicle input script with a vehicle
        protected override bool Initialize(Vehicle vehicle)
        {
            
            if (!base.Initialize(vehicle)) return false;

            engines = vehicle.GetComponent<VehicleEngines3D>();
            if (engines == null)
            {
                if (debugInitialization)
                {
                    Debug.LogWarning(GetType().Name + " failed to initialize - the required " + engines.GetType().Name + " component was not found on the vehicle.");
                }
                return false;
            }


            triggerablesManager = vehicle.GetComponent<TriggerablesManager>();
            if (triggerablesManager == null)
            {
                if (debugInitialization)
                {
                    Debug.LogWarning(GetType().Name + " failed to initialize - the required " + triggerablesManager.GetType().Name + " component was not found on the vehicle.");
                }
                return false;
            }

            if (initializeThrottleToVehicle && throttleSlider != null)
            {
                throttleSlider.value = engines.MovementInputs.z;
            }

            return true;
        }

        private void OnEnable()
        {
            if (initialized && initializeThrottleToVehicle && throttleSlider != null)
            {
                throttleSlider.value = engines.MovementInputs.z;
            }
        }

        /// <summary>
        /// Set the pitch (nose up/down) input value.
        /// </summary>
        /// <param name="pitchValue">The pitch value.</param>
        public void Pitch(float pitchValue)
        {
            steeringInputs.x = pitchValue;
        }

        /// <summary>
        /// Set the yaw (nose left/right) input value.
        /// </summary>
        /// <param name="yawValue">The yaw value.</param>
        public void Yaw(float yawValue)
        {
            steeringInputs.y = yawValue;
        }

        /// <summary>
        /// Set the roll input value.
        /// </summary>
        /// <param name="rollValue">The roll value.</param>
        public void Roll(float rollValue)
        {
            steeringInputs.z = rollValue;
        }

        /// <summary>
        /// Set the forward/back movement input value.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        public void SetForwardMovementInput(float inputValue)
        {
            movementInputs.z = inputValue;
        }

        /// <summary>
        /// Set the horizontal movement input value.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        public void SetHorizontalMovementInput(float inputValue)
        {
            movementInputs.x = inputValue;
        }

        /// <summary>
        /// Set the vertical movement input value.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        public void SetVerticalMovementInput(float inputValue)
        {
            movementInputs.y = inputValue;
        }

        /// <summary>
        /// Set the forward boost value.
        /// </summary>
        /// <param name="boostValue">The forward boost value.</param>
        public void SetForwardBoost(float boostValue)
        {
            boostInputs.z = boostValue;
        }

        /// <summary>
        /// Start triggering at a trigger index.
        /// </summary>
        /// <param name="triggerIndex">The trigger index.</param>
        public void StartTriggeringAtIndex(int triggerIndex)
        {
            if (triggerablesManager != null) triggerablesManager.StartTriggeringAtIndex(triggerIndex);
        }

        /// <summary>
        /// Stop triggering at a trigger index.
        /// </summary>
        /// <param name="triggerIndex">The trigger index.</param>
        public void StopTriggeringAtIndex(int triggerIndex)
        {
            if (triggerablesManager != null) triggerablesManager.StopTriggeringAtIndex(triggerIndex);
        }


        public void CycleCameraView(bool forward)
        {
            cameraEntity.CycleCameraView(forward);
        }


        // Called every frame that this input script is active.
        protected override void InputUpdate()
        {
            // Pitch axis input
            if (useAxisForPitch)
            {
                Pitch(-CrossPlatformInputManager.GetAxis(pitchAxisName) * (invertPitchAxis ? -1 : 1));
            }

            // Yaw axis input
            if (useAxisForYaw)
            {
                Yaw(CrossPlatformInputManager.GetAxis(yawAxisName) * (invertYawAxis ? -1 : 1));
            }

            // Roll axis input
            if (useAxisForRoll)
            {
                Roll(CrossPlatformInputManager.GetAxis(rollAxisName) * (invertRollAxis ? -1 : 1));
            }

            // Linked yaw a roll
            if (linkYawAndRoll)
            {
                Yaw(steeringInputs.z * yawRollRatio);
            }

            // Set steering, movement and boost input on the engines.
            if (engines != null)
            {
                engines.SetSteeringInputs(steeringInputs);
                engines.SetMovementInputs(movementInputs);
                engines.SetBoostInputs(boostInputs);
            }
        }
    }
}