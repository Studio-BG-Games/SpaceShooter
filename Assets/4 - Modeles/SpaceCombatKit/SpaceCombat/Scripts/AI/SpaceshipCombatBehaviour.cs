using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class provides an example of AI combat behavior for a spaceship.
    /// </summary>
	public class SpaceshipCombatBehaviour : AISpaceshipBehaviour 
	{

        public enum CombatState
        {
            Attacking,
            Evading
        }

        [Header("Attacking")]

        [Tooltip("The attack behaviour for this AI.")]
        [SerializeField]
        protected AISpaceshipBehaviour attackBehaviour;

        [Tooltip("The minimum distance to target within which the AI will engage the target, rather than move away from it.")]
        [SerializeField]
        protected float minEngageDistance = 100;

        [Tooltip("The maximum distance to target within which the AI will engage the target, rather than close distance with it.")]
        [SerializeField]
        protected float maxEngageDistance = 500;

        [Tooltip("The minimum time that the AI will be in attack mode during any given amount of time that this behaviour is running.")]
        [SerializeField]
        protected float minAttackTime = 10;

        [Tooltip("The maximum time that the AI will be in attack mode during any given amount of time that this behaviour is running.")]
        [SerializeField]
        protected float maxAttackTime = 20;

        [Header("Evading")]

        [Tooltip("The evade behaviour for this AI.")]
        [SerializeField]
        protected AISpaceshipBehaviour evadeBehaviour;

        [Tooltip("The minimum time that the AI will be in evade mode during any given amount of time that this behaviour is running.")]
        [SerializeField]
        protected float minEvadeTime = 5;

        [Tooltip("The maximum time that the AI will be in evade mode during any given amount of time that this behaviour is running.")]
        [SerializeField]
        protected float maxEvadeTime = 10;

        protected float combatBehaviourStartTime = 0;
        protected float currentCombatBehaviourPeriod = 0;

        protected CombatState combatState;

        protected Weapons weapons;


        protected override bool Initialize(Vehicle vehicle)
        {

            if (!base.Initialize(vehicle)) { return false; }

            weapons = vehicle.GetComponent<Weapons>();
            if (weapons == null) return false;

            attackBehaviour.SetVehicle(vehicle);
            evadeBehaviour.SetVehicle(vehicle);

            return true;

        }

        public override void StopBehaviour()
        {
            base.StopBehaviour();
            attackBehaviour.StopBehaviour();
            evadeBehaviour.StopBehaviour();
        }

        protected virtual void StartAttack()
        {
            evadeBehaviour.StopBehaviour();
            attackBehaviour.StartBehaviour();

            combatState = CombatState.Attacking;
            combatBehaviourStartTime = Time.time;
            currentCombatBehaviourPeriod = Random.Range(minAttackTime, maxAttackTime);
        }

        protected virtual void StartEvade()
        {
            evadeBehaviour.StartBehaviour();
            attackBehaviour.StopBehaviour();

            combatState = CombatState.Evading;
            combatBehaviourStartTime = Time.time;
            currentCombatBehaviourPeriod = Random.Range(minEvadeTime, maxEvadeTime);
        }

        public override bool BehaviourUpdate()
        {

            if (!base.BehaviourUpdate()) return false;

            if (weapons.WeaponsTargetSelector == null || weapons.WeaponsTargetSelector.SelectedTarget == null) return false;

            switch (combatState)
            {
                case CombatState.Attacking:
                    
                    // Change state if it's been long enough
                    if (Time.time - combatBehaviourStartTime > currentCombatBehaviourPeriod)
                    {
                        StartEvade();
                    }
                    else if (Vector3.Distance(vehicle.transform.position, weapons.WeaponsTargetSelector.SelectedTarget.transform.position) < minEngageDistance)
                    {
                        StartEvade();
                    }
                    else
                    {
                        attackBehaviour.BehaviourUpdate();
                    }

                    break;

                case CombatState.Evading:

                    // Change state if it's been long enough
                    if (Time.time - combatBehaviourStartTime > currentCombatBehaviourPeriod)
                    {
                        StartAttack();
                    }
                    else
                    {
                        evadeBehaviour.BehaviourUpdate();
                    }

                    break;
            }

            return true;
        }
	}
}
