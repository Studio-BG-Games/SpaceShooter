using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class PoweredBeamWeaponUnit : BeamWeaponUnit, IPowerConsumer
    {
        [Header("Power")]

        public bool usePower = true;

        protected Power power;
        public Power Power { set { power = value; } }

        public float maxPowerDrawPerSecond = 400;

        public override void SetBeamLevel(float level)
        {
            if (usePower && power == null)
            {
                base.SetBeamLevel(0);
                return;
            }

            if (usePower)
            {
                
                float desiredPower = maxPowerDrawPerSecond * Time.deltaTime * level;
                float allowedPower = Mathf.Min(desiredPower, power.GetStoredPower(PoweredSubsystemType.Weapons));

                float allowedLevel = Mathf.Approximately(maxPowerDrawPerSecond * Time.deltaTime, 0) ? level : allowedPower / (maxPowerDrawPerSecond * Time.deltaTime);

                power.DrawStoredPower(PoweredSubsystemType.Weapons, allowedPower);
                
                base.SetBeamLevel(allowedLevel);
            }
        }
    }
}
