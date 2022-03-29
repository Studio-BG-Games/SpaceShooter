using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class VehicleStatsOverrideController : MonoBehaviour
    {
        [SerializeField]
        protected VehicleClass vehicleClass;
        public VehicleClass VehicleClass
        {
            get { return vehicleClass; }
        }

        protected VehicleStatsController statsController;
        public VehicleStatsController StatsController
        {
            set { statsController = value; }
        }

        public virtual void OnVehiclesListUpdated(List<Vehicle> vehicles)
        {
            // Update max stats values
        }

        public virtual bool ShowStats(Vehicle vehicle) 
        {
            return false;
        }
    }
}
