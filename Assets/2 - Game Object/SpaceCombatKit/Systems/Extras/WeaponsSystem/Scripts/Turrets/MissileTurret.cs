using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{
    public class MissileTurret : Turret
    {
        [Header("Missile Turret")]

        [SerializeField]
        protected TargetLocker targetLocker;

        [Header("Fire Control")]

        [Tooltip("The minimum time between firing.")]
        [SerializeField]
        protected float minFiringInterval = 0.5f;

        [Tooltip("The maximum time between firing.")]
        [SerializeField]
        protected float maxFiringInterval = 2;

        [Tooltip("Whether the turret rotates back to the center when no target is present.")]
        [SerializeField]
        protected bool noTargetReturnToCenter = true;

        protected float firingStateStartTime;
        protected float nextFiringStatePeriod;

        protected override void Reset()
        {
            base.Reset();

            targetLocker = GetComponentInChildren<TargetLocker>();
            if (targetLocker == null)
            {
                targetLocker = gameObject.AddComponent<TargetLocker>();
            }
            
            if (gimbalController != null)
            {
                targetLocker.LockingReferenceTransform = gimbalController.VerticalPivot;
            }
        }

        protected override void TurretControlUpdate()
        {
            base.TurretControlUpdate();

            // If no target, return to idle
            if (target == null)
            {
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

        public override void SetTarget(Trackable target)
        {
            base.SetTarget(target);
            targetLocker.SetTarget(target);
        }


        // Update the firing of the turret
        protected virtual void UpdateFiring()
        {
            bool canFire = true;

            // If angle to target is too large, can't fire
            if (targetLocker.LockState != LockState.Locked)
            {
                canFire = false;
            }

            if (canFire)
            {
                // Switch firing states
                if (Time.time - firingStateStartTime > nextFiringStatePeriod)
                {
                    FireOnce();
                }
            }
        }


        // Start firing the turret
        protected virtual void FireOnce()
        {
            weapon.Triggerable.TriggerOnce();
            nextFiringStatePeriod = Random.Range(minFiringInterval, maxFiringInterval);
            firingStateStartTime = Time.time;
        }
    }
}

