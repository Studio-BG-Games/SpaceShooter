using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class represents a blackboard of data that is shared among AI behaviours so that behaviors can be
    /// blended.
    /// </summary>
    public class BehaviourBlackboard
	{

        public GroupMember groupMember;

        public Vehicle vehicle;
	    
		public Vector3 steeringPIDCoeffs;
        
		public Vector3 throttlePIDCoeffs;
        
        public Vector3 integralSteeringVals;

        public Vector3 integralThrottleVals;

        public Vector3 maxRotationAngles;
			
		public Vector3 steeringValues;
		
		public Vector3 throttleValues;
		
		public Vector3 obstacleAvoidanceDirection;
		
        public float obstacleAvoidanceStrength; // A 0-1 value for the obstacle avoidance strength.

        public float obstacleMovingAwaySpeed;	
		
		public float obstacleAvoidanceMargin;

		public bool canFirePrimaryWeapon;

		public bool canFireSecondaryWeapon;

		public bool secondaryWeaponFired;


        /// <summary>
        /// Initialize the blackboard.
        /// </summary>
        /// <param name="steeringPIDCoeffs">The PID controller coefficients for the steering.</param>
        /// <param name="throttlePIDCoeffs">The PID controller coefficients for the throttle.</param>
        /// <param name="maxRotationAngles">The maximum rotation angles for the vehicle.</param>
		public void Initialize(Vector3 steeringPIDCoeffs, Vector3 throttlePIDCoeffs, Vector3 maxRotationAngles){

			this.steeringPIDCoeffs = steeringPIDCoeffs;

			this.throttlePIDCoeffs = throttlePIDCoeffs;
	
			this.maxRotationAngles = maxRotationAngles;
		}

        /// <summary>
        /// Set the vehicle that this blackboard refers to.
        /// </summary>
        /// <param name="newVehicle">The vehicle that this blackboard refers to.</param>
		public void SetVehicle(Vehicle vehicle)
        {
			this.vehicle = vehicle;
		}
	}
}