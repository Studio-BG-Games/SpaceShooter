using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    public class VehicleStatsController : StatsController
    {

        protected List<VehicleStatsOverrideController> vehicleStatsOverrideControllers = new List<VehicleStatsOverrideController>();


        protected List<Vehicle> vehiclesList;
        public List<Vehicle> VehiclesList
        {
            set
            {
                vehiclesList = value;

                foreach (VehicleStatsOverrideController vehicleStatsOverrideController in vehicleStatsOverrideControllers)
                {
                    vehicleStatsOverrideController.OnVehiclesListUpdated(value);
                }
            }
        }


        protected virtual void Awake()
        {
            vehicleStatsOverrideControllers = new List<VehicleStatsOverrideController>(transform.GetComponentsInChildren<VehicleStatsOverrideController>());
            foreach (VehicleStatsOverrideController statsOverride in vehicleStatsOverrideControllers)
            {
                statsOverride.StatsController = this;
            }
        }


        public void ShowStats(Vehicle vehicle)
        {
            ClearStatsInstances();

            bool found = false;
            foreach (VehicleStatsOverrideController controller in vehicleStatsOverrideControllers)
            {
                if (controller.ShowStats(vehicle))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                labelText.text = vehicle.Label;
                descriptionText.text = vehicle.Description;
            }
        }
    }
}

