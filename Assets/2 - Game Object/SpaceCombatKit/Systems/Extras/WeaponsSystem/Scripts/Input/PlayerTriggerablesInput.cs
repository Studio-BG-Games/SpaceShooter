using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Player input class that exposes indexed customisable trigger inputs in the inspector, and implements triggering
    /// of triggerable modules mounted on a vehicle.
    /// </summary>
    public class PlayerTriggerablesInput : VehicleInput
    {

        [Header("Trigger Inputs")]

        [SerializeField]
        protected List<TriggerInput> triggerInputs = new List<TriggerInput>();

        protected TriggerablesManager triggerablesManager;


        /// <summary>
        /// Initialize the vehicle input with a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle reference.</param>
        /// <returns>Whether initialization succeeded.</returns>
        protected override bool Initialize(Vehicle vehicle)
        {

            // Clear dependencies
            triggerablesManager = null;
            
            // Make sure the vehicle has a Triggerables component
            triggerablesManager = vehicle.GetComponent<TriggerablesManager>();
            if (triggerablesManager == null)
            {
                return false;
            }

            return true;

        }

        /// <summary>
        /// Stop receivng input.
        /// </summary>
        public override void DisableInput()
        {
            base.DisableInput();
            if (triggerablesManager != null)
            {
                triggerablesManager.StopTriggeringAll();
            }
        }

   
        // Called to begin triggering at a specified index.
        public void TriggerDown(int triggerIndex)
        {
            triggerablesManager.StartTriggeringAtIndex(triggerIndex);
        }

        // Called to stop triggering at a specified index.
        public void TriggerUp(int triggerIndex)
        {
            triggerablesManager.StopTriggeringAtIndex(triggerIndex);
        }

        protected override void OnInputUpdateFailed()
        {
            base.OnInputUpdateFailed();
            if (triggerablesManager != null) triggerablesManager.StopTriggeringAll();
        }

        // Called every frame
        protected override void InputUpdate()
        {
            for (int i = 0; i < triggerInputs.Count; ++i)
            {
                // Trigger down
                if (triggerInputs[i].inputSettings.Down()) TriggerDown(triggerInputs[i].triggerIndex);

                // Trigger up
                if (triggerInputs[i].inputSettings.Up()) TriggerUp(triggerInputs[i].triggerIndex);
            }
        }
    }
}