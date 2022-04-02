using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class GunTurret : Turret
    {
        [Header("Fire Control")]

        [Tooltip("The minimum time between firing bursts.")]
        [SerializeField]
        protected float minFiringInterval = 0.5f;

        [Tooltip("The maximum time between firing bursts.")]
        [SerializeField]
        protected float maxFiringInterval = 2;

        [Tooltip("The minimum length of the firing burst.")]
        [SerializeField]
        protected float minFiringPeriod = 1;

        [Tooltip("The maximum length of the firing burst.")]
        [SerializeField]
        protected float maxFiringPeriod = 2;

        [Tooltip("The maximum angle to target where the turret will fire.")]
        [SerializeField]
        protected float minFiringAngle = 5;

        [Tooltip("Whether the turret rotates back to the center when no target is present.")]
        [SerializeField]
        protected bool noTargetReturnToCenter = true;

        protected float firingStateStartTime;
        protected bool firing = false;
        protected float nextFiringStatePeriod;


        protected override void TurretControlUpdate()
        {
            base.TurretControlUpdate();

            // If no target, return to idle
            if (target == null)
            {
                if (firing) StopFiring();

                // Return the turret to center
                if (noTargetReturnToCenter) gimbalController.ResetGimbal(false);
            }
            else
            {
                // Track the target
                TrackTarget();

                // Fire
                UpdateFiring();
            }
        }

        // Update the firing of the turret
        protected virtual void UpdateFiring()
        {
            bool canFire = true;

            // If angle to target is too large, can't fire
            if (firingAngle > minFiringAngle)
            {
                canFire = false;
            }

            if (canFire)
            {
                // Switch firing states
                if (Time.time - firingStateStartTime > nextFiringStatePeriod)
                {
                    if (firing)
                    {
                        StopFiring();
                    }
                    else
                    {
                        StartFiring();
                    }
                }
            }
            else
            {
                if (firing)
                {
                    StopFiring();
                }
            }
        }


        // Start firing the turret
        protected virtual void StartFiring()
        {
            firing = true;
            nextFiringStatePeriod = Random.Range(minFiringPeriod, maxFiringPeriod);
            firingStateStartTime = Time.time;
            weapon.Triggerable.StartTriggering();
        }


        // Stop firing the turret
        protected virtual void StopFiring()
        {
            firing = false;
            nextFiringStatePeriod = Random.Range(minFiringInterval, maxFiringInterval);
            firingStateStartTime = Time.time;
            weapon.Triggerable.StopTriggering();
        }
    }
}

