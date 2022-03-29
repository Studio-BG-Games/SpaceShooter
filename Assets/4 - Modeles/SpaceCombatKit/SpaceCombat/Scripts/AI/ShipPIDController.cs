using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VSX.UniversalVehicleCombat
{
    [System.Serializable]
    public class ShipPIDController
    {
        // Steering PID
        public PIDController3D steeringPIDController = new PIDController3D();

        public void SetSteeringError(PIDController3D.Axis axis, float error, float errorChangeRate)
        {
            steeringPIDController.SetError(axis, error, errorChangeRate);
        }

        public void SetSteeringIntegralInfluence(float influence)
        {
            steeringPIDController.SetIntegralInfluence(PIDController3D.Axis.X, influence);
            steeringPIDController.SetIntegralInfluence(PIDController3D.Axis.Y, influence);
            steeringPIDController.SetIntegralInfluence(PIDController3D.Axis.Z, influence);
        }

        public Vector3 GetSteeringControlValues()
        {
            return steeringPIDController.GetControlValues();
        }


        // Movement PID
        public PIDController3D movementPIDController = new PIDController3D();

        public void SetMovementError(PIDController3D.Axis axis, float error, float errorChangeRate)
        {
            movementPIDController.SetError(axis, error, errorChangeRate);
        }

        public void SetMovementIntegralInfluence(float influence)
        {
            movementPIDController.SetIntegralInfluence(PIDController3D.Axis.X, influence);
            movementPIDController.SetIntegralInfluence(PIDController3D.Axis.Y, influence);
            movementPIDController.SetIntegralInfluence(PIDController3D.Axis.Z, influence);
        }

        public Vector3 GetMovementControlValues()
        {
            return movementPIDController.GetControlValues();
        }
    }
}