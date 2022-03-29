using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{
    // A behaviour selector simply tries each behaviour in the list, and when one of them succeeds, it is finished.
    public class BehaviourSelector : VehicleInput
    {

        [Header("Parameters")]

        [Tooltip("The list of behaviours to select from. Will try each one in succession until one successfully runs.")]
        [SerializeField]
        protected List<AIVehicleBehaviour> behaviours = new List<AIVehicleBehaviour>();
        int currentBehaviourIndex = -1;

        protected override void Start()
        {
            base.Start();

            foreach (AIVehicleBehaviour behaviour in behaviours)
            {
                if (behaviour.UpdateEveryFrame)
                {
                    Debug.LogError("Behaviour " + behaviour.name + " is set to 'Update Every Frame' while managed by a Behaviour Selector. Remove it from the Behaviour Selector's Behaviours list or uncheck this setting in the inspector.");
                }
            }
        }

        protected override bool Initialize(Vehicle vehicle)
        {
            VehicleEngines3D engines = vehicle.GetComponent<VehicleEngines3D>();
            if (engines == null) return false;

            // Success
            for (int i = 0; i < behaviours.Count; ++i)
            {
                behaviours[i].SetVehicle(vehicle);
            }
         
            return true;
        }

        protected virtual void SetSelection(int selectionIndex)
        {
            for (int i = 0; i < behaviours.Count; ++i)
            {
                if (i == selectionIndex)
                {
                    behaviours[i].StartBehaviour();
                    currentBehaviourIndex = i;
                }
                else
                {
                    behaviours[i].StopBehaviour();
                }
            }
        }


        protected override void InputUpdate()
        {
            for (int i = 0; i < behaviours.Count; ++i)
            {
                if (behaviours[i].BehaviourUpdate())
                {
                    if (currentBehaviourIndex != i)
                    {
                        SetSelection(i);
                    }
                    break;
                }
            }
        }
    }
}