using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    public class HUDVehicleEnterExit : MonoBehaviour
    {

        [SerializeField]
        protected VehicleEnterExitManager vehicleEnterExitManager;

        [Tooltip("The prompt that appears when the occupant can enter or exit the vehicle.")]
        [SerializeField]
        protected UVCText promptText;

        protected string enterPrompt;
        protected string exitPrompt;

        [SerializeField]
        protected bool overrideEnterExitPrompt = false;

        [Tooltip("The default message for the prompt that appears when the occupant can enter the vehicle.")]
        [SerializeField]
        protected string enterPromptOverride;

        [Tooltip("The default message for the prompt that appears when the occupant can exit the vehicle.")]
        [SerializeField]
        protected string exitPromptOverride;


        protected virtual void Reset()
        {
            vehicleEnterExitManager = GetComponentInChildren<VehicleEnterExitManager>();
        }


        public virtual void SetPrompts(string enterPrompt, string exitPrompt)
        {
            if (!overrideEnterExitPrompt)
            {
                this.enterPrompt = enterPrompt;
                this.exitPrompt = exitPrompt;
            }
        }


        protected virtual void Update()
        {
            if (overrideEnterExitPrompt)
            {
                enterPrompt = enterPromptOverride;
                exitPrompt = exitPromptOverride;
            }

            bool activated = vehicleEnterExitManager.Vehicle.Occupants.Count > 0 && vehicleEnterExitManager.Vehicle.Occupants[0].IsPlayer;
            if (activated && promptText != null)
            {
                if (vehicleEnterExitManager.EnterableVehicles.Count > 0)
                {
                    promptText.text = enterPrompt;
                }
                else if (vehicleEnterExitManager.CanExitToChild())
                {
                    promptText.text = exitPrompt;
                }
                else
                {
                    promptText.text = "";
                }
            }
        }
    }
}