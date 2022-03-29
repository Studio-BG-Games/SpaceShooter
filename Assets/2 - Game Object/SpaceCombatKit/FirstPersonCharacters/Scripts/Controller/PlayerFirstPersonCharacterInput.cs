using System;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class PlayerFirstPersonCharacterInput : VehicleInput
    {

        [Header("Settings")]
        
        [SerializeField]
        protected bool enableVerticalStrafeInput;

        [SerializeField]
        protected CustomInput walkForwardBackwardAxisInput = new CustomInput("First Person Character", "Walk", "Vertical");

        [SerializeField]
        protected CustomInput strafeHorizontalAxisInput = new CustomInput("First Person Character", "Strafe", "Horizontal");

        [SerializeField]
        protected CustomInput strafeVerticalInput = new CustomInput("First Person Character", "Strafe", "Vertical");

        [SerializeField]
        protected CustomInput runInput = new CustomInput("First Person Character", "Run", KeyCode.LeftShift);

        [SerializeField]
        protected CustomInput jumpInput = new CustomInput("First Person Character", "Jump", KeyCode.Space);

        protected FirstPersonCharacterController characterController;



        /// <summary>
        /// Initialize this input script with a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>Whether initialization succeeded</returns>
        protected override bool Initialize(Vehicle vehicle)
        {

            characterController = vehicle.GetComponent<FirstPersonCharacterController>();
            if (characterController == null)
            {
                if (debugInitialization)
                {
                    Debug.LogWarning(GetType().Name + " failed to initialize - the required " + characterController.GetType().Name + " component was not found on the vehicle.");
                }
                return false;
            }
           
            if (debugInitialization)
            {
                Debug.Log(GetType().Name + " successfully initialized.");
            }

            return true;
        }

        // Update is called once per frame
        protected override void InputUpdate()
        {

            // Moving
            float horizontal = strafeHorizontalAxisInput.FloatValue();
            float vertical = enableVerticalStrafeInput ? strafeVerticalInput.FloatValue() : 0;
            float forward = walkForwardBackwardAxisInput.FloatValue();

            characterController.SetMovementInputs(horizontal, vertical, forward);

            // Jumping
            if (jumpInput.Down())
            {
                characterController.Jump();
            }
            
            characterController.SetRunning(runInput.Pressed());

        }
    }
}
