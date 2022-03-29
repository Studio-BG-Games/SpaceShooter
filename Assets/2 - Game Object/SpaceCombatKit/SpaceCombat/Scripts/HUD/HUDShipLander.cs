using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    public class HUDShipLander : MonoBehaviour
    {
        [Tooltip("The ShipLander component for this ship.")]
        [SerializeField]
        protected ShipLander shipLander;

        [Tooltip("The prompt that appears to tell the player that they can take off or land.")]
        [SerializeField]
        protected UVCText promptText;

        [Tooltip("The default message to tell the player they can take off.")]
        [SerializeField]
        protected string launchPrompt;

        [Tooltip("The default message to tell the player they can land.")]
        [SerializeField]
        protected string landPrompt;

        /// <summary>
        /// Set the taking off and landing prompt messages.
        /// </summary>
        /// <param name="launchPrompt">The launch prompt message.</param>
        /// <param name="landPrompt">The landing prompt message.</param>
        public void SetPrompts(string launchPrompt, string landPrompt)
        {
            this.launchPrompt = launchPrompt;
            this.landPrompt = landPrompt;
        }


        void Update()
        {
            if (promptText != null)
            {
                switch (shipLander.CurrentState)
                {
                    case ShipLander.ShipLanderState.Landed:

                        // Show launch prompt when landed
                        promptText.text = launchPrompt;
                        break;

                    case ShipLander.ShipLanderState.Launched:

                        // Show landing prompt when launched and near a landable surface
                        if (shipLander.CheckCanLand())
                        {
                            promptText.text = landPrompt;
                        }
                        else
                        {
                            promptText.text = "";
                        }
                        break;

                    default:

                        promptText.text = "";
                        break;
                }
            }
        }
    }
}