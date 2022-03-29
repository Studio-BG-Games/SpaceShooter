using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class SpaceshipMoveToPositionBehaviour : AISpaceshipBehaviour
    {

        protected Rigidbody rBody;     
        protected VehicleEngines3D engines;

        [Tooltip("The target position to move toward.")]
        [SerializeField]
        protected Vector3 targetPosition;

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

            // Steer
            Maneuvring.TurnToward(rBody.transform, targetPosition, maxRotationAngles, shipPIDController.steeringPIDController);
            engines.SetSteeringInputs(shipPIDController.GetSteeringControlValues());

            // Move
            Maneuvring.TranslateToward(rBody, targetPosition, shipPIDController.movementPIDController);
            engines.SetMovementInputs(shipPIDController.GetMovementControlValues());

            return true;
            
        }
    }
}
