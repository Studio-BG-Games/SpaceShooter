using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{
    public class SpaceshipEvadeBehaviour : AISpaceshipBehaviour
    {
        [Tooltip("This is the maximum angle that will be randomly added to the path directly away from the target, when evading a target.")]
        [SerializeField]
        protected float maxEvadeAngleOffset = 90f;

        [Tooltip("This is how fast the weave goes from side to side during evasion.")]
        [SerializeField]
        protected float weaveSpeed = 1;

        [Tooltip("This is the size (width) of the weave path during evasion.")]
        [SerializeField]
        protected float weaveRadius = 5;

        [Tooltip("This value is multiplied by the distance from the center of the scene to determine the amount (0-1) that the ship will return to center rather than follow its evasion path.")]
        [SerializeField]
        protected float returnToCenterFactor = 0.0005f;

        protected Vector3 evadeDirection = Vector3.forward;

        protected Weapons weapons;
        protected VehicleEngines3D engines;


        protected override bool Initialize(Vehicle vehicle)
        {

            if (!base.Initialize(vehicle)) return false; 

            weapons = vehicle.GetComponent<Weapons>();
            if (weapons == null) { return false; }
            
            engines = vehicle.GetComponent<VehicleEngines3D>();
            if (engines == null) { return false; }
            
            return true;

        }


        public override void StartBehaviour()
        {
            base.StartBehaviour();

            if (state == VehicleBehaviourState.Started)
            {
                UpdateEvadeDirection();
            }
        }


        public virtual void UpdateEvadeDirection()
        {

            if (weapons.WeaponsTargetSelector == null || weapons.WeaponsTargetSelector.SelectedTarget == null) return;

            // Get the direction to the target
            Vector3 toTargetDirection = (weapons.WeaponsTargetSelector.SelectedTarget.transform.position - vehicle.transform.position).normalized;

            // Get a new evade path direction
            evadeDirection = (Quaternion.Euler(new Vector3(0f, Random.Range(-maxEvadeAngleOffset, maxEvadeAngleOffset), 0f)) * -toTargetDirection).normalized;

            Vector3 targetPos = vehicle.transform.position + evadeDirection.normalized * 1000;
            targetPos.y = 0;

            evadeDirection = (targetPos - vehicle.transform.position).normalized;


            // Get the vector from the vehicle to the scene center
            Vector3 toCenterVec = -vehicle.transform.position;

            // Blend with a return to center vector depending on distance from center
            float returnToCenterStrength = Mathf.Clamp(toCenterVec.magnitude * returnToCenterFactor, 0f, 1f);
            evadeDirection = (returnToCenterStrength * toCenterVec.normalized + (1 - returnToCenterStrength) * evadeDirection.normalized).normalized;

        }

    
        // Add a weave to the path of the vehicle
        protected virtual Vector3 Weave(Vector3 pathDirection, float weaveSpeed, float weaveRadius)
        {

            // Add relative horizontal offset
            float offsetX = (Mathf.PerlinNoise(Time.time * weaveSpeed, 0f) - 0.5f) * 2f;

            // Add relative vertical offset
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * weaveSpeed) - 0.5f) * 2f;

            // Create the offset vector
            Vector3 offsetVec = new Vector3(offsetX, offsetY, 1).normalized * weaveRadius;
            Vector3 offset = new Vector3(offsetVec.x, offsetVec.y, 1).normalized;

            pathDirection = Quaternion.FromToRotation(Vector3.forward, pathDirection) * offset;

            return pathDirection;

        }


        public override bool BehaviourUpdate()
        {
            if (!base.BehaviourUpdate()) return false;
            
            // Add a weave to the evade path
            Vector3 targetDirection = Weave(evadeDirection, weaveSpeed, weaveRadius);
            
            Maneuvring.TurnToward(vehicle.transform, vehicle.transform.position + targetDirection, maxRotationAngles, shipPIDController.steeringPIDController);
            engines.SetSteeringInputs(shipPIDController.steeringPIDController.GetControlValues());

            engines.SetMovementInputs(new Vector3(0, 0, 1));

            return true;
            
        }
    }
}