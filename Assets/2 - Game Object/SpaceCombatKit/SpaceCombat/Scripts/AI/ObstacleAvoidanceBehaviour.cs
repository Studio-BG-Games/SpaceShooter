using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class represents an obstacle that is perceived by the AI
    /// </summary>
    [System.Serializable]
    public class ObstacleData
	{
	
		public bool hasRigidbody;
		
		public Rigidbody rigidBody;
	
		public RaycastHit raycastHit;
	
		public float raycastHitTime;
	
		public Vector3 obstacleVelocity;

		public Vector3 currentPos;

        public Vector3 currentAvoidancePosition;

		public Vector3 currentAvoidanceDirection;

        public float directionalityFactor;

        public float timeToImpactFactor;

        public float proximityFactor;

        public float memoryFadeFactor;
			
		public float riskFactor;
	
		public float movingAwaySpeed;

		public ObstacleData(RaycastHit hit, float detectionTime, Rigidbody rigidBody)
		{
	
			this.raycastHit = hit;

			this.currentPos = hit.point;
            
			this.raycastHitTime = detectionTime;
	
			if (rigidBody != null)
			{
				this.rigidBody = rigidBody;
				this.hasRigidbody = true;
				this.obstacleVelocity = rigidBody.velocity;
			} 
			else 
			{
				this.rigidBody = null;
				this.hasRigidbody = false;
				this.obstacleVelocity = Vector3.zero;
			}
		}
	}


    [System.Serializable]
    public class ObstacleSphereCast
    {
        public float radius = 20;
    }


	/// <summary>
    /// This class calculates risk factors for all the obstacles perceived by the AI, and calculates a point in space
    /// each frame that enables the AI to avoid the obstacles.
    /// </summary>
	public class ObstacleAvoidanceBehaviour : AISpaceshipBehaviour
	{

        [Header("Sphere Casts")]
        [Tooltip("A list of all the sphere casts that you would like to perform. Sphere casts originate at the vehicle origin, and are projected along the vehicle's velocity vector (or the vehicle's forward vector if not velocity dependent.)")]
        [SerializeField]
        protected List<ObstacleSphereCast> obstacleSphereCasts = new List<ObstacleSphereCast>();
        protected int nextSphereCastIndex = -1;

        [SerializeField]
        protected LayerMask obstacleMask = ~0;

        [SerializeField]
        protected bool ignoreTriggerColliders = true;

        [Header("General Settings")]

        [Tooltip("The total risk factor threshold that triggers the obstacle avoidance behaviour.")]
        [SerializeField]
        protected float riskFactorThreshold = 0.1f;

        [Tooltip("The default scan distance. Actual scan distance may be larger if time-to-impact is being factored in and the vehicle is moving very fast.")]
        [SerializeField]
        protected float defaultObstacleScanDistance = 100;

        [Tooltip("The obstacle avoidance will scan at least this far.")]
        [SerializeField]
        protected float minObstacleScanDistance = 30;

        [Tooltip("The distance from the obstacle that the vehicle will try to reach when avoiding the obstacle.")]
        [SerializeField]
        protected float obstacleAvoidanceMargin = 50;

        [Tooltip("The distance between obstacle detections below which they will be merged together as one obstacle.")]
        [SerializeField]
        protected float obstacleMergeDistance = 15f;

        [Tooltip("The maximum number of obstacles that can be recorded.")]
        [SerializeField]
        protected int maxObstacleDataPoints = 3;

        [Header ("Time-To-Impact Settings")]

        [Tooltip("Whether the vehicle's time-to-impact with an obstacle is factored into the risk factor.")]
        [SerializeField]
        protected bool includeTimeToImpactFactor = true;

        [Tooltip("When the time-to-impact reaches this value, the risk factor will begin increasing from zero.")]
        [SerializeField]
        protected float zeroRiskTimeToImpact = 3;

        [Tooltip("When the time-to-impact reaches this value, the risk factor will reach 1 (maximum).")]
        [SerializeField]
        protected float fullRiskTimeToImpact = 1;

        [Tooltip("Whether to reduce speed when avoiding obstacles.")]
        [SerializeField]
        protected bool reduceSpeedDuringAvoidance = true;

        [Header("Obstacle Proximity Settings")]

        [Tooltip("Whether the vehicle's spatial proximity to an obstacle is factored into the risk factor.")]
        [SerializeField]
        protected bool includeProximityFactor = true;

        [Tooltip("When the time-to-impact reaches this value, the risk factor will begin increasing from zero.")]
        [SerializeField]
        protected float zeroRiskProximity = 100;

        [Tooltip("When the time-to-impact reaches this value, the risk factor will reach 1 (maximum).")]
        [SerializeField]
        protected float fullRiskProximity = 30;


        [Header("Obstacle Memory")]

        [Tooltip("Whether to remember obstacles even when not detected any more (helps prevent dithering).")]
        [SerializeField]
        protected bool useMemory = true;

        [Tooltip("The time that an obstacle stays in memory once it is no longer detected.")]
        [SerializeField]
        protected float obstacleMemoryTime = 3f;

        protected List<ObstacleData> obstacleDataList = new List<ObstacleData>();
		public List<ObstacleData> ObstacleDataList { get { return obstacleDataList; } }

        protected float obstacleAvoidanceStrength = 0;
        public float ObstacleAvoidanceStrength { get { return obstacleAvoidanceStrength; } }

        protected Vector3 obstacleAvoidanceDirection = Vector3.forward;
        protected float obstacleMovingAwaySpeed = 0;

        protected VehicleEngines3D engines;


        protected override void Awake()
        {
            base.Awake();
            if (obstacleSphereCasts.Count > 0)
            {
                nextSphereCastIndex = 0;
            }
        }

        protected void Reset()
        {
            obstacleSphereCasts.Clear();
            ObstacleSphereCast sphereCast = new ObstacleSphereCast();
            sphereCast.radius = 20;
            obstacleSphereCasts.Add(sphereCast);
        }

        protected override bool Initialize(Vehicle vehicle)
        {
            if (!base.Initialize(vehicle)) return false;

            engines = vehicle.GetComponent<VehicleEngines3D>();

            return engines != null;
        }

        // Create obstacle data from raycast hits
        void UpdateObstacleData(RaycastHit[] hits)
		{
			// If no obstacles should be 'remembered' clear the obstacle data list
			if (!useMemory)
			{ 
				obstacleDataList.Clear();
			}

			// Update the risk factors for all the obstacle data instances
			for (int i = 0; i < obstacleDataList.Count; ++i)
			{
				UpdateRiskFactor(obstacleDataList[i], true);
			}

			// Add the new data to the list
			for (int i = 0; i < hits.Length; ++i)
			{
				ObstacleData newData = new ObstacleData(hits[i], Time.time, hits[i].collider.attachedRigidbody);
                if (AddObstacleData(newData))
                {
                    UpdateRiskFactor(newData, false);
                }
			}
			
			// Initialize the blackboard values
			obstacleAvoidanceStrength = 0;
            obstacleAvoidanceDirection = vehicle.transform.forward;
            obstacleMovingAwaySpeed = 0;
			float totalRiskFactor = 0;			
			
			// Get the total risk factor for calculating the influence of this instance on the final calculated direction
			// Also update the blackboard with the maximum collision risk value
			for (int i = 0; i < obstacleDataList.Count; ++i)
			{

				totalRiskFactor += obstacleDataList[i].riskFactor;

				if (obstacleDataList[i].riskFactor > obstacleAvoidanceStrength)
					obstacleAvoidanceStrength = obstacleDataList[i].riskFactor;

			}
			
			// Update blackboard data
			if (totalRiskFactor > 0.0001f)
			{
                // Update the obstacle avoidance direction
                for (int i = 0; i < obstacleDataList.Count; ++i)
				{
					obstacleAvoidanceDirection += (obstacleDataList[i].riskFactor / totalRiskFactor) * obstacleDataList[i].currentAvoidanceDirection;
					
					obstacleMovingAwaySpeed += (obstacleDataList[i].riskFactor / totalRiskFactor) * obstacleDataList[i].movingAwaySpeed;
				}
			}
		}


		/// <summary>
        /// Add another piece of data for a new obstacle.
        /// </summary>
        /// <param name="newData">The new obstacle data.</param>
		public bool AddObstacleData(ObstacleData newData)
		{
	
			Rigidbody rBody = newData.raycastHit.collider.attachedRigidbody;
			bool hasRigidbody = rBody != null;
			
			// Prevent obstacle avoidance of self
			if (hasRigidbody && (rBody == vehicle.CachedRigidbody))
				return false;
			
			// Merge obstacle data that are in close proximity
			bool merged = false;
			for (int i = 0; i < obstacleDataList.Count; ++i)
			{
				
				if (Vector3.Distance(newData.currentPos, obstacleDataList[i].currentPos) < obstacleMergeDistance)
				{
					obstacleDataList[i] = newData;
					merged = true;
					break;
				}
			}	
			
			// If obstacle has not been merged, replace any that have a lower risk
			if (!merged)
			{

				if (obstacleDataList.Count < maxObstacleDataPoints)
				{
					obstacleDataList.Add(newData);
				}
				else
				{

					for (int i = 0; i < obstacleDataList.Count; ++i)
					{
						if (newData.riskFactor > obstacleDataList[i].riskFactor)
						{
							obstacleDataList[i] = newData;
							break;
						}
					}

				}
			}

            return true;
		}


        // Update the risk factors for the obstacles
        void UpdateRiskFactor(ObstacleData obstacleData, bool show)
        {

            // Update the position
            obstacleData.currentPos = obstacleData.raycastHit.point + obstacleData.obstacleVelocity *
            (Time.time - obstacleData.raycastHitTime);

            // Update the avoid target direction
            Vector3 toObstacleVector = obstacleData.currentPos - vehicle.transform.position;

            // Get the velocity of the collision point relative to this ship
            Vector3 collisionRelVelocity = obstacleData.obstacleVelocity - vehicle.CachedRigidbody.velocity;
            float closingVelocityAmount = Vector3.Dot(collisionRelVelocity.normalized, -toObstacleVector.normalized);

            // Get the closest distance that the point obstacle will get to this ship
            float tmp = Vector3.Dot(-toObstacleVector.normalized, collisionRelVelocity.normalized);
            Vector3 nearestPointOnLine = obstacleData.currentPos + (tmp * Vector3.Magnitude(toObstacleVector) * collisionRelVelocity.normalized);

            float timeToImpact = Vector3.Distance(obstacleData.currentPos, vehicle.transform.position) / Mathf.Max(closingVelocityAmount * collisionRelVelocity.magnitude, 0.0001f);

            obstacleData.movingAwaySpeed = Vector3.Dot(obstacleData.obstacleVelocity.normalized, toObstacleVector.normalized) * obstacleData.obstacleVelocity.magnitude;

            // Calculate the avoidance target position and the direction to it
            Vector3 perpendicularAvoidDirection = (vehicle.transform.position - nearestPointOnLine).normalized;
            obstacleData.currentAvoidancePosition = obstacleData.raycastHit.point + obstacleData.raycastHit.normal * 100;//obstacleData.currentPos + perpendicularAvoidDirection * obstacleAvoidanceMargin;

            obstacleData.currentAvoidanceDirection = (obstacleData.currentAvoidancePosition - vehicle.transform.position).normalized;

            // Initialize risk factor to 1
            obstacleData.riskFactor = 1;

            // Directionality factor - 0 when 90 degrees away, increases to 1 when directly in path
            Vector3 directionReference = vehicle.CachedRigidbody.velocity.normalized;
            if (vehicle.CachedRigidbody.velocity.magnitude < 0.0001f)
            {
                directionReference = vehicle.transform.forward;
            }

            obstacleData.directionalityFactor = Mathf.Clamp(Vector3.Dot(directionReference, toObstacleVector.normalized), 0f, 1f);
            obstacleData.riskFactor *= obstacleData.directionalityFactor;
            
            // Proximity factor - 0 when at zeroRiskProximity distance, increases to 1 when at fullRiskProximity distance
            float nearestDist = Vector3.Distance(vehicle.transform.position, nearestPointOnLine);

            obstacleData.proximityFactor = nearestDist < fullRiskProximity ? 1 : 1 - Mathf.Clamp((nearestDist - fullRiskProximity) /
                (zeroRiskProximity - fullRiskProximity), 0f, 1f);

            if (includeProximityFactor) obstacleData.riskFactor *= obstacleData.proximityFactor;


            // Time-to-impact factor - 0 when time to impact is at zeroRiskTimeToImpact, increases to 1 when time to impact is at fullRiskTimeToImpact
            obstacleData.timeToImpactFactor = timeToImpact < fullRiskTimeToImpact ? 1 : 1 - Mathf.Clamp((timeToImpact - fullRiskTimeToImpact) /
                                        (zeroRiskTimeToImpact - fullRiskTimeToImpact), 0f, 1f);
            
            if (includeTimeToImpactFactor) obstacleData.riskFactor *= obstacleData.timeToImpactFactor;

            // Memory fade factor - 0 when just seen, 1 when time since seen is at memory length
            if (useMemory)
            {
                obstacleData.memoryFadeFactor = Mathf.Clamp(1 - (Time.time - obstacleData.raycastHitTime) / obstacleMemoryTime, 0f, 1f);
                obstacleData.riskFactor *= obstacleData.memoryFadeFactor;
            }
        }       


		/// <summary>
        /// Called by the control script every frame when this behaviour is running.
        /// </summary>
		public override bool BehaviourUpdate()
		{
            // Calculate scan distance
            float obstacleScanDistance = includeTimeToImpactFactor ? vehicle.CachedRigidbody.velocity.magnitude * zeroRiskTimeToImpact : defaultObstacleScanDistance;
            obstacleScanDistance = Mathf.Max(obstacleScanDistance, minObstacleScanDistance);

            // Get the next sphere cast from the list and cast it
            if (nextSphereCastIndex != -1)
            {
                // Calculate the direction to project the sphere cast
                Vector3 projectionVector = vehicle.CachedRigidbody.velocity.normalized;
                if (vehicle.CachedRigidbody.velocity.magnitude < 0.0001f)
                {
                    projectionVector = vehicle.transform.forward;
                }

                // Do sphere cast
                RaycastHit[] hits = Physics.SphereCastAll(vehicle.transform.position, obstacleSphereCasts[nextSphereCastIndex].radius, projectionVector, 
                                                            obstacleScanDistance, obstacleMask, ignoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
                UpdateObstacleData(hits);
                
                // Iterate the sphere cast list
                nextSphereCastIndex += 1;
                if (nextSphereCastIndex == obstacleSphereCasts.Count)
                {
                    nextSphereCastIndex = 0;
                }
            }

            // Return whether 
            if (obstacleAvoidanceStrength < riskFactorThreshold)
            {
                return false;
            }
            else
            {
                Debug.DrawLine(vehicle.transform.position, vehicle.transform.position + obstacleAvoidanceDirection * 100, Color.red);

                for(int i = 0; i < obstacleDataList.Count; ++i)
                {
                    Debug.DrawLine(vehicle.transform.position, obstacleDataList[i].raycastHit.point, Color.blue);
                    Debug.DrawLine(obstacleDataList[i].raycastHit.point, obstacleDataList[i].raycastHit.point + obstacleDataList[i].currentAvoidanceDirection.normalized * 100, Color.green);
                    Debug.DrawLine(obstacleDataList[i].raycastHit.point, obstacleDataList[i].raycastHit.point + obstacleDataList[i].raycastHit.normal * 100, Color.cyan);
                }

                Maneuvring.TurnToward(vehicle.transform, vehicle.transform.position + obstacleAvoidanceDirection, maxRotationAngles, 
                                        shipPIDController.steeringPIDController);

                engines.SetSteeringInputs(shipPIDController.steeringPIDController.GetControlValues());

                if (reduceSpeedDuringAvoidance)
                {
                    engines.SetMovementInputs(Vector3.forward * (1 - obstacleAvoidanceStrength));
                }
                else
                {
                    engines.SetMovementInputs(Vector3.forward);
                }                

                return true;
            }

            
		}
	}
}
