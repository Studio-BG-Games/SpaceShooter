using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace VSX.UniversalVehicleCombat
{

    [System.Serializable]
    public class OnPowerLevelUpdatedEventHandler : UnityEvent<float> { }

    public class PoweredObject : MonoBehaviour, IPowerConsumer
    {
   
        [Header("Settings")]

        [SerializeField]
        protected Power power;
        public Power Power { set { power = value; } }

        [SerializeField]
        protected PoweredSubsystemType powerType;

        [SerializeField]
        protected float fullPowerDraw = 200;

        protected float availablePowerFraction = 0;

        [Header("Settings")]

        [Tooltip("This event is called every frame with the fraction of full power that is available (0-1) so that you can update any components that behave differently according to the available power.")]
        public OnPowerLevelUpdatedEventHandler onAvailablePowerFractionUpdated;


        protected virtual void Update()
        {
            if (power == null || fullPowerDraw == 0) return;

            float availablePower = power.GetStoredPower(powerType);
            availablePowerFraction = Mathf.Min(availablePower / fullPowerDraw, 1);

            onAvailablePowerFractionUpdated.Invoke(availablePowerFraction);
        }

        // Full power

        public virtual bool HasFullPower()
        {
            if (power == null) return false;
            return power.HasStoredPower(powerType, fullPowerDraw);
        }

        public virtual void DrawFullPower()
        {
            if (power == null) return;
            power.DrawStoredPower(powerType, fullPowerDraw);
        }

        // Full delta time power

        public virtual bool HasFullDeltaTimePower()
        {
            if (power == null) return false;
            return power.HasStoredPower(powerType, fullPowerDraw * Time.deltaTime);
        }

        public virtual void DrawFullDeltaTimePower()
        {
            if (power == null) return;
            power.DrawStoredPower(powerType, fullPowerDraw * Time.deltaTime);
        }

        // Available delta time power
        public virtual void DrawAvailableDeltaTimePower()
        {
            if (power == null) return;
            power.DrawStoredPower(powerType, availablePowerFraction * fullPowerDraw * Time.deltaTime);
        }
    }
}

