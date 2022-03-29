using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    
	/// <summary>
    /// This class provides an example of AI combat behavior for a spaceship.
    /// </summary>
	public class CapitalShipCombatBehaviour : AISpaceshipBehaviour 
	{

        /// <summary>
        /// This class holds data that is used by the AI to make decisions during combat.
        /// </summary>
        public class CombatDecisionInfo
        {

            public Vector3 toTargetVector;

            public Vector3 targetPosition;

            /// <summary>
            /// How much the AI is facing the target (-1 to 1, using the dot product of the forward vectors)
            /// </summary>
            public float facingTargetAmount;

            public float angleToTarget;

            /// <summary>
            /// How much the target is facing the AI (-1 to 1, using the dot product of the forward vectors)
            /// </summary>
            public float targetFacingAmount;

            public float distanceToTarget;

            /// <summary>
            /// A 0-1 value for evaluating the primary firing solution quality.
            /// </summary>
            public float primaryFiringSolutionQuality;

            /// <summary>
            /// A 0-1 value for evaluating the secondary firing solution quality.
            /// </summary>
            public float secondaryFiringSolutionQuality;

        }

        [Header("Combat Parameters")]
	
        [Tooltip("The minimum (x-value) and maximum (y-value) distance in which the AI will engage a target rather than move away.")]
		[SerializeField]
		private Vector2 minMaxEngageDistance = new Vector2(100, 500);

        [Tooltip("The minimum (x-value) and maximum (y-value) duration of firing the primary weapon before starting a pause.")]
        [SerializeField]
		private Vector2 minMaxPrimaryFiringPeriod = new Vector2(1,3);

        [Tooltip("The minimum (x-value) and maximum (y-value) duration of the pause in between firing the primary weapon.")]
        [SerializeField]
		private Vector2 minMaxPrimaryFiringInterval = new Vector2(0.5f, 2);

        [Tooltip("The maximum angle to target within which gimballed weapons will fire at the target.")]
        [SerializeField]
		private float maxFiringAngle = 15f;
	
		private float primaryWeaponActionStartTime = 0;
		private float primaryWeaponActionPeriod = 0f;
		private bool isFiringPrimary = false;

        [Tooltip("The distance to target below which the ship will become fully broadside-on to the target.")]
        [SerializeField]
        float broadsideDistanceToTarget = 500;

		
		[Header("Behaviour Characteristics")]	
		
		protected CombatDecisionInfo decisionInfo;

        protected Weapons weapons;
        protected TriggerablesManager triggerablesManager;
        protected VehicleEngines3D engines;
        protected Rigidbody rBody;




        protected override void Awake()
		{
			decisionInfo = new CombatDecisionInfo();
		}


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

            return (engines != null);

        }

        
        // Called when the gameobject is disabled.
		private void OnDisable()
		{
			StopAllCoroutines();
		}

        public override void StopBehaviour()
        {
            base.StopBehaviour();
            if (triggerablesManager != null)
            {
                triggerablesManager.StopTriggeringAll();
            }
        }

        // Update the data that is used to calculate decisions
        private void UpdateDecisionInfo()
		{
			
			decisionInfo.targetPosition = weapons.WeaponsTargetSelector.SelectedTarget.transform.position;
				
		
			decisionInfo.toTargetVector = decisionInfo.targetPosition - vehicle.transform.position;
	
			decisionInfo.distanceToTarget = Vector3.Distance(vehicle.transform.position, decisionInfo.targetPosition);
            decisionInfo.angleToTarget = Vector3.Angle(vehicle.transform.forward, decisionInfo.toTargetVector);
			decisionInfo.facingTargetAmount = Vector3.Dot(vehicle.transform.forward, decisionInfo.toTargetVector.normalized);

            decisionInfo.targetFacingAmount = Vector3.Dot(weapons.WeaponsTargetSelector.SelectedTarget.transform.forward, -(decisionInfo.toTargetVector).normalized);

		}
	
	
		// Get a 0-1 value that describes how good of a firing position the primary weapons are in
		float GetPrimaryFiringSolutionQuality()
		{
            float primaryFiringSolutionQuality = 1;
		
            // Zero if target out of engage range
			primaryFiringSolutionQuality *= decisionInfo.distanceToTarget < minMaxEngageDistance.y ? 1f : 0f;
            
            if (weapons.Turrets.Count > 0)
            {
                float angleToTarget;
                float val = 0;
                foreach (Turret turret in weapons.Turrets)
                {
                    
                    turret.GimbalController.TrackPosition(decisionInfo.targetPosition, out angleToTarget, false);

                    if (angleToTarget < maxFiringAngle)
                    {
                        val += 1;
                    }
                }
                
                val /= weapons.Turrets.Count;
                primaryFiringSolutionQuality *= val;
            }
            
            return primaryFiringSolutionQuality;
		}


		// Update whether or not the AI can fire this frame
		private void UpdateFiring()
		{
            
            float primaryFiringSolutionQuality = GetPrimaryFiringSolutionQuality();
            
            bool canFirePrimary = primaryFiringSolutionQuality > 0.5f;
            
            if (canFirePrimary)
			{ 
				
				// If weapon can fire but has not been firing, check if the cooling off period has finished before firing it again
				if (!isFiringPrimary)
				{
                    // If hasn't finished cooling off period, don't fire
                    if (Time.time - primaryWeaponActionStartTime < primaryWeaponActionPeriod)
					{
						canFirePrimary = false;
					}
					else
					{
						primaryWeaponActionStartTime = Time.time;
						primaryWeaponActionPeriod = Random.Range(minMaxPrimaryFiringPeriod.x, minMaxPrimaryFiringPeriod.y);
						isFiringPrimary = true;
					}
				}
				// If weapon can fire and has been firing, check if it has finished the firing period
				else
				{
					// If weapon has been firing long enough, stop firing
					if (Time.time - primaryWeaponActionStartTime > primaryWeaponActionPeriod)
					{
                        canFirePrimary = false;
						primaryWeaponActionStartTime = Time.time;
						primaryWeaponActionPeriod = Random.Range(minMaxPrimaryFiringInterval.x, minMaxPrimaryFiringInterval.y);
                        isFiringPrimary = false;
					}
				}
			}
            
	        if (canFirePrimary && primaryFiringSolutionQuality > 0.5f)
            {
                triggerablesManager.StartTriggeringAtIndex(0);
            }
            else
            {
                triggerablesManager.StopTriggeringAtIndex(0);
            }
		}
	
		
		/// <summary>
        /// Called every frame to update this behavor when it is running.
        /// </summary>
		public override bool BehaviourUpdate()
		{
            
            if (weapons.WeaponsTargetSelector == null || weapons.WeaponsTargetSelector.SelectedTarget == null) return false;

			UpdateDecisionInfo();
            
			UpdateFiring();

            // Attack

            Vector3 perpendicularTargetDirection = Quaternion.Euler(0f, 90f, 0f) * decisionInfo.toTargetVector;

            float broadsideAmount = Mathf.Clamp(2 - (decisionInfo.distanceToTarget / broadsideDistanceToTarget), 0, 1);

            // Update the steering target
            Vector3 steeringTarget = vehicle.transform.position + (broadsideAmount * perpendicularTargetDirection + (1 - broadsideAmount) * decisionInfo.toTargetVector);
                    
            // Used to make the ship slow down as it approaches the target
			float amountOfEngageDistance = (decisionInfo.distanceToTarget - minMaxEngageDistance.x) / (minMaxEngageDistance.y - minMaxEngageDistance.x);

            // Set the throttle values
            float forwardThrottleAmount = Mathf.Clamp(amountOfEngageDistance, 0.25f, 1);

            Maneuvring.TurnToward(vehicle.transform, steeringTarget, maxRotationAngles, shipPIDController.steeringPIDController);
            engines.SetSteeringInputs(shipPIDController.steeringPIDController.GetControlValues());
            engines.SetMovementInputs(new Vector3(0f, 0f, forwardThrottleAmount));

            return true;

		}
	}
}
