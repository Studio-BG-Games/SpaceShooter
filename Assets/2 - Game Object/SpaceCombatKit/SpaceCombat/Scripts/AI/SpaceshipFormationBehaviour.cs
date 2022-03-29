using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class SpaceshipFormationBehaviour : AISpaceshipBehaviour
    {

        protected Rigidbody rBody;
        protected VehicleEngines3D engines;

        [Tooltip("The transform that this ship will get into formation with.")]
        [SerializeField]
        protected Transform formationTarget;
        protected Transform FormationTarget
        {
            set { formationTarget = value; }
        }

        [Tooltip("The position offset (in local space of the formation target) that this ship will maintain during formation.")]
        [SerializeField]
        protected Vector3 formationOffset;
        public Vector3 FormationOffset
        {
            set { formationOffset = value; }
        }

        [Tooltip("The distance away from the target position beyond which this ship will use steering rather than throttle to move toward it. This prevents the ship from turning, and enables it to instead adjust the throttle, if it overshoots the formation position.")]
        [SerializeField]
        protected float turnTowardDistance = 50;


        protected override bool Initialize(Vehicle vehicle)
        {

            if (!base.Initialize(vehicle)) return false;

            rBody = vehicle.GetComponent<Rigidbody>();
            if (rBody == null) return false;

            engines = vehicle.GetComponent<VehicleEngines3D>();
            if (engines == null) return false;

            return true;
           
        }

        public override bool BehaviourUpdate()
        {

            if (!base.BehaviourUpdate()) return false;

            if (formationTarget == null) return false;

            // Get the target position
            Vector3 targetPos = formationTarget.TransformPoint(formationOffset);

            // Calculate the target to orient the ship toward. As the ship approaches the formation target
            // position, it must orient itself toward a point far ahead in space to prevent oscillation.
            float turnTowardAmount = Mathf.Clamp(Vector3.Distance(rBody.position, targetPos) / turnTowardDistance, 0, 1);
            Vector3 steeringTargetPos = turnTowardAmount * targetPos + (1 - turnTowardAmount) * (targetPos + formationTarget.forward * 1000);
            
            // Steer
            Maneuvring.TurnToward(rBody.transform, steeringTargetPos, maxRotationAngles, shipPIDController.steeringPIDController);
            engines.SetSteeringInputs(shipPIDController.GetSteeringControlValues());

            // Move
            Maneuvring.TranslateToward(rBody, targetPos, shipPIDController.movementPIDController);
            engines.SetMovementInputs(shipPIDController.GetMovementControlValues());

            return true;
            
        }
    }
}
