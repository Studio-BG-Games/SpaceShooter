using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;
namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Base class for a guided missile.
    /// </summary>
    public class Missile : RigidbodyProjectile
    {
        [Header("Settings")]

        [SerializeField]
        protected float noLockLifetime = 4;

        [Header("Guidance")]

        [SerializeField]
        protected PIDController3D steeringPIDController;

        [Header("Components")]

        [SerializeField]
        protected TargetLocker targetLocker;

        [SerializeField]
        protected VehicleEngines3D engines;   

        [SerializeField]
        protected TargetProximityTrigger targetProximityTrigger;

        protected bool locked = false;


        public override float Speed
        {
            get { return engines.GetDefaultMaxSpeedByAxis(false).z; }
        }

        public override float Range
        {
            get { return targetLocker.LockingRange; }
        }

        public override float Damage(HealthType healthType)
        {
            for (int i = 0; i < healthModifier.DamageOverrideValues.Count; ++i)
            {
                if (healthModifier.DamageOverrideValues[i].HealthType == healthType)
                {
                    return healthModifier.DamageOverrideValues[i].Value;
                }
            }

            return healthModifier.DefaultDamageValue;
        }

        public override void AddVelocity(Vector3 addedVelocity)
        {
            base.AddVelocity(addedVelocity);
            m_Rigidbody.velocity += addedVelocity;
        }

        protected override void Reset()
        {

            base.Reset();

            m_Rigidbody.useGravity = false;
            m_Rigidbody.mass = 1;
            m_Rigidbody.drag = 3;
            m_Rigidbody.angularDrag = 4;

            // Add/get engines
            engines = transform.GetComponentInChildren<VehicleEngines3D>();
            if (engines == null)
            {
                engines = gameObject.AddComponent<VehicleEngines3D>();
            }

            // Add/get target locker
            targetLocker = transform.GetComponentInChildren<TargetLocker>();
            if (targetLocker == null)
            {
                targetLocker = gameObject.AddComponent<TargetLocker>();
            }

            // Add/get a target proximity trigger
            targetProximityTrigger = transform.GetComponentInChildren<TargetProximityTrigger>();
            if (targetProximityTrigger == null)
            {
                targetProximityTrigger = gameObject.AddComponent<TargetProximityTrigger>();
            }

            detonator.DetonatingDuration = 2;

            disableAfterDistanceCovered = false;
        }

        
        /// <summary>
        /// Set the target.
        /// </summary>
        /// <param name="target">The new target.</param>
        public virtual void SetTarget(Trackable target)
        {
            if (targetLocker != null)
            {
                targetLocker.SetTarget(target);
                if (target != null) targetLocker.SetLockState(LockState.Locked);
            }
            if (targetProximityTrigger != null) targetProximityTrigger.Target = target;
        }

        /// <summary>
        /// Set the lock state of the missile.
        /// </summary>
        /// <param name="lockState">The new lock state.</param>
        public virtual void SetLockState(LockState lockState)
        {
            if (targetLocker != null) targetLocker.SetLockState(lockState);

            locked = true;
        }


        protected override void Update()
        {
            base.Update();
            
            if (targetLocker.LockState == LockState.Locked)
            {
                // Steer
                Vector3 targetVelocity = targetLocker.Target.Rigidbody != null ? targetLocker.Target.Rigidbody.velocity : Vector3.zero;
                Vector3 targetPos = TargetLeader.GetLeadPosition(transform.position, Speed, targetLocker.Target.transform.position, targetVelocity);
                Maneuvring.TurnToward(transform, targetPos, new Vector3(360, 360, 0), steeringPIDController);
                engines.SetSteeringInputs(steeringPIDController.GetControlValues());
                engines.SetMovementInputs(new Vector3(0, 0, 1));
            }
            else
            {
                // Detonate after lifetime
                if (locked)
                {
                    detonator.BeginDelayedDetonation(noLockLifetime);
                    locked = false;
                }

                // Clear steering inputs
                engines.SetSteeringInputs(Vector3.zero);
                engines.SetMovementInputs(new Vector3(0, 0, 1));
            }
        }
    }
}