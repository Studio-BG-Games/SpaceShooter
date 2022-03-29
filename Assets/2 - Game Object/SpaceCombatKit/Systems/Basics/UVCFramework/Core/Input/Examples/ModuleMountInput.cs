using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    [System.Serializable]
    public class ModuleSelectionInput
    {
        public int moduleIndex;
        public CustomInput input;
    }

    public class ModuleMountInput : VehicleInput
    {

        [Header("Module Mount")]

        [Tooltip("The ID of the module mount to cycle modules at.")]
        [SerializeField]
        protected string moduleMountID;


        [Header("Module Cycling")]

        [Tooltip("The input that cycles forward through the modules.")]
        [SerializeField]
        protected CustomInput cycleForwardInput;

        [Tooltip("The input that cycles backward through the modules.")]
        [SerializeField]
        protected CustomInput cycleBackwardInput;


        [Header("Module Selection")]

        [Tooltip("A list of inputs for selecting modules at specified indexes.")]
        [SerializeField]
        protected List<ModuleSelectionInput> moduleSelectionInputs = new List<ModuleSelectionInput>();

        protected Vehicle vehicle;


        protected override bool Initialize(Vehicle vehicle)
        {
            if (!base.Initialize(vehicle)) return false;

            // Get a reference to the vehicle
            this.vehicle = vehicle;

            return true;
        }

        protected void Cycle(bool isForward)
        {
            // Go through all the module mounts
            for (int i = 0; i < vehicle.ModuleMounts.Count; ++i)
            {
                // Find the one(s) with the correct ID
                if (vehicle.ModuleMounts[i].ID == moduleMountID)
                {
                    // cycle forward or backward
                    vehicle.ModuleMounts[i].Cycle(isForward);
                }
            }
        }

        protected override void InputUpdate()
        {

            foreach(ModuleMount moduleMount in vehicle.ModuleMounts)
            {
                if (moduleMount.ID == moduleMountID)
                {
                    Debug.Log("Found module");
                }
            }

            // Cycle forward input
            if (cycleForwardInput.Down())
            {
                Cycle(true);
            }

            // Cycle backward input
            if (cycleBackwardInput.Down())
            {
                Cycle(false);
            }

            // Select modules
            for (int i = 0; i < moduleSelectionInputs.Count; ++i)
            {
                if (moduleSelectionInputs[i].input.Down())
                {
                    for (int j = 0; j < vehicle.ModuleMounts.Count; ++j)
                    {
                        if (vehicle.ModuleMounts[j].ID == moduleMountID)
                        {
                            vehicle.ModuleMounts[j].MountModule(moduleSelectionInputs[i].moduleIndex);
                        }
                    }
                }
            }
        }
    }
}
