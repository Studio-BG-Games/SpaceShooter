using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class PoweredDamageable : Damageable, IPowerConsumer
    {

        protected Power power;
        public Power Power { set { power = value; } }

        [SerializeField]
        protected float rechargeRate;

        protected float rechargeWaitStartTime;
        protected float nextRechargeWaitTime;

        
        public virtual void StartWaitBeforeRecharge(float waitTime)
        {

            float timeRemaining = Mathf.Max(nextRechargeWaitTime - (Time.time - rechargeWaitStartTime), 0);

            if (timeRemaining < waitTime)
            {
                nextRechargeWaitTime = waitTime;
                rechargeWaitStartTime = Time.time;
            }
        }

        protected virtual void Update()
        {
            if (power == null) return;

            if (destroyed && !canHealAfterDestroyed) return;

            if (Time.time - rechargeWaitStartTime < nextRechargeWaitTime) return;

            if (!Mathf.Approximately(currentHealth, startingHealth))
            {
                float diff = startingHealth - currentHealth;
                float recharge = rechargeRate * Time.deltaTime;
                recharge = Mathf.Min(recharge, diff);
                recharge = Mathf.Min(recharge, power.GetStoredPower(PoweredSubsystemType.Health));

                power.DrawStoredPower(PoweredSubsystemType.Health, recharge);

                if (destroyed)
                {
                    Restore(false);
                }

                currentHealth += recharge;
                currentHealth = Mathf.Clamp(currentHealth, 0, startingHealth);
            }
        }
    }
}