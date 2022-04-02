using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class PoweredProjectileLauncher : ProjectileWeaponUnit, IPowerConsumer
    {
        [Header("Power")]

        public bool usePower = true;

        protected Power power;
        public Power Power { set { power = value; } }

        public float powerDrawPerLaunch;

        public override void TriggerOnce()
        {
            if (usePower && power == null) return;

            if (usePower)
            {
                if (power.HasStoredPower(PoweredSubsystemType.Weapons, powerDrawPerLaunch))
                {
                    power.DrawStoredPower(PoweredSubsystemType.Weapons, powerDrawPerLaunch);
                    base.TriggerOnce();
                }
            }
            else
            {
                base.TriggerOnce();
            }
        }
    }
}
