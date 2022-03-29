using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{
    public class SpaceshipAttackBehaviour : AISpaceshipBehaviour
    {

        [Header("Primary Weapons")]

        [Tooltip("This is the minimum random amount of time the primary weapons will be firing continuously before a pause, when the target is in the sights.")]
        [SerializeField]
        protected float minPrimaryFiringPeriod = 1;

        [Tooltip("This is the maximum random amount of time the primary weapons will be firing continuously before a pause, when the target is in the sights.")]
        [SerializeField]
        protected float maxPrimaryFiringPeriod = 3;

        [Tooltip("This is the minimum random amount of time the primary weapons will pause before firing again, when the target is in the sights.")]
        [SerializeField]
        protected float minPrimaryFiringPause = 0.5f;

        [Tooltip("This is the maximum random amount of time the primary weapons will pause before firing again, when the target is in the sights.")]
        [SerializeField]
        protected float maxPrimaryFiringPause = 2;

        [Tooltip("This is the maximum angle to target (relative to the ship's forward vector) within which the AI will fire the primary weapons.")]
        [SerializeField]
        protected float maxFiringAngle = 15f;

        [Tooltip("This is the maximum distance to target within which the AI will fire the primary weapons.")]
        [SerializeField]
        protected float maxFiringDistance = 600;

        protected float primaryWeaponActionStartTime = 0;
        protected float primaryWeaponActionPeriod = 0f;
        protected bool primaryWeaponFiring = false;

        [Header("Secondary Weapons")]

        [Tooltip("This is the minimum (x-value) and maximum (y-value) random interval between firing of the secondary weapons (missiles).")]
        [SerializeField]
        protected Vector2 minMaxSecondaryFiringInterval = new Vector2(10, 25);

        protected float secondaryWeaponActionStartTime = 0;
        protected float secondaryWeaponActionPeriod = 0f;
        protected bool secondaryWeaponFiring = false;

        protected Weapons weapons;
        protected TriggerablesManager triggerablesManager;
        protected VehicleEngines3D engines;
        protected Rigidbody rBody;
        

        protected override bool Initialize(Vehicle vehicle)
        {
            if (!base.Initialize(vehicle)) return false;

            weapons = vehicle.GetComponent<Weapons>();
            if (weapons == null) return false;
            
            triggerablesManager = vehicle.GetComponent<TriggerablesManager>();
            if (triggerablesManager == null) return false;

            engines = vehicle.GetComponent<VehicleEngines3D>();
            if (engines == null) return false;

            rBody = vehicle.GetComponent<Rigidbody>();
            if (rBody == null) return false;

            return true;

        }

        public override void StopBehaviour()
        {
            if (initialized) triggerablesManager.StopTriggeringAll();
        }
        
        protected virtual void SetPrimaryWeaponAction(bool fire)
        {
            if (fire)
            {
                triggerablesManager.StartTriggeringAtIndex(0);
                primaryWeaponFiring = true;

                primaryWeaponActionStartTime = Time.time;
                primaryWeaponActionPeriod = Random.Range(minPrimaryFiringPeriod, maxPrimaryFiringPeriod);
            }
            else
            {
                triggerablesManager.StopTriggeringAtIndex(0);
                primaryWeaponFiring = false;

                primaryWeaponActionStartTime = Time.time;
                primaryWeaponActionPeriod = Random.Range(minPrimaryFiringPause, maxPrimaryFiringPause);
            }
        }

        /// <summary>
        /// Update the behaviour.
        /// </summary>
        public override bool BehaviourUpdate()
        {

            if (!base.BehaviourUpdate()) return false;

            if (weapons.WeaponsTargetSelector == null || weapons.WeaponsTargetSelector.SelectedTarget == null)
            {
                return false;
            }

            Vector3 targetPos = weapons.GetAverageLeadTargetPosition(weapons.WeaponsTargetSelector.SelectedTarget.transform.position,
                                                                        weapons.WeaponsTargetSelector.SelectedTarget.Rigidbody.velocity);

            Vector3 toTargetVector = targetPos - vehicle.transform.position;

            // Do primary weapons
            bool canFirePrimary = Vector3.Angle(vehicle.transform.forward, toTargetVector) < maxFiringAngle && toTargetVector.magnitude < maxFiringDistance;
            if (canFirePrimary)
            {
                // Fire in bursts
                if (Time.time - primaryWeaponActionStartTime > primaryWeaponActionPeriod)
                {
                    SetPrimaryWeaponAction(!primaryWeaponFiring);
                }
            }
            else
            {
                SetPrimaryWeaponAction(false);
            }

            // Do the secondary weapons
            if (weapons.MissileWeapons.Count > 0)
            {
                TargetLocker targetLocker = weapons.MissileWeapons[0].GetComponent<TargetLocker>();

                if (targetLocker != null && targetLocker.LockState == LockState.Locked)
                {
                    if (Time.time - secondaryWeaponActionStartTime > secondaryWeaponActionPeriod)
                    {
                        triggerablesManager.TriggerOnce(1);
                        secondaryWeaponActionPeriod = Random.Range(minMaxSecondaryFiringInterval.x, minMaxSecondaryFiringInterval.y);
                        secondaryWeaponActionStartTime = Time.time;
                    }
                }
            }

            // Turn toward target
            Maneuvring.TurnToward(vehicle.transform, targetPos, maxRotationAngles, shipPIDController.steeringPIDController);
            engines.SetSteeringInputs(shipPIDController.steeringPIDController.GetControlValues());

            Maneuvring.TranslateToward(rBody, targetPos, shipPIDController.movementPIDController);

            Vector3 movementInputs = shipPIDController.movementPIDController.GetControlValues();
            engines.SetMovementInputs(new Vector3(0, 0, movementInputs.z));

            return true;

        }
    }
}