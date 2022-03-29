using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    // This class represents an instance of a powered subsystem - a subsystem that can be configured to store, draw and recharge power
    /// <summary>
    /// This class represents an instance of a powered subsystem, and is used by the Power subsystem class to 
    /// store settings for the different subsystems in the vehicle.
    /// </summary>
    [System.Serializable]
    public class PoweredSubsystem
    {

        public PoweredSubsystemType type;

        public SubsystemPowerConfiguration powerConfiguration;

        public float independentPowerOutput;

        public float fixedPowerFraction = 0.1f;

        public float defaultDistributablePowerFraction = 0.33f;

        public float distributablePowerFraction;

        public float directPowerFraction = 0.5f;

        public float rechargePowerFraction = 0.5f;

        public float maxRechargeRate = 100;

        public float storageCapacity = 1000;

        public float currentStorageValue;

    }
}