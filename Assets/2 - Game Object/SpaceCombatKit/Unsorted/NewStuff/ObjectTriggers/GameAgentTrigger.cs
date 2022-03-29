using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class GameAgentTrigger : ObjectTrigger
    {

        [Header("Game Agent Trigger")]

        [Tooltip("The vehicle that this game agent is in will be used as the trigger object. If this value is not set, and a Game Agent Manager is in the scene, the Focused Game Agent will be used as the trigger game agent.")]
        [SerializeField]
        protected GameAgent triggerGameAgent;

        // Called when scene starts
        protected virtual void Start()
        {
            // Set up the trigger game agent
            if (triggerGameAgent != null)
            {
                SetTriggerGameAgent(triggerGameAgent);
            }
            else
            {
                if (GameAgentManager.Instance != null)
                {
                    SetTriggerGameAgent(GameAgentManager.Instance.FocusedGameAgent);

                    GameAgentManager.Instance.onFocusedGameAgentChanged.AddListener(SetTriggerGameAgent);
                }
            }
        }

        // Set the trigger game agent
        protected void SetTriggerGameAgent(GameAgent newTriggerGameAgent)
        {
            // Disconnect previous trigger game agent
            if (this.triggerGameAgent != null)
            {
                this.triggerGameAgent.onEnteredVehicle.RemoveListener(OnTriggerGameAgentEnteredVehicle);
                triggerObject = null;
            }

            // Update the trigger game agent
            this.triggerGameAgent = newTriggerGameAgent;
            if (triggerGameAgent == null)
            {
                return;
            }
            else
            {
                if (triggerGameAgent.IsInVehicle)
                {
                    triggerObject = triggerGameAgent.Vehicle.transform;
                }

                triggerGameAgent.onEnteredVehicle.AddListener(OnTriggerGameAgentEnteredVehicle);
            }
        }

        // Called when the trigger game agent enters a vehicle
        protected virtual void OnTriggerGameAgentEnteredVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                triggerObject = null;
            }
            else
            {
                triggerObject = vehicle.transform;
            }
        }

    }

}
