using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// A 3-dimensional (X-Y-Z) PID Controller.
    /// </summary>
    [System.Serializable]
    public class PIDController3D
    {
        [Tooltip("The PID controller for the X axis.")]
        public PIDController controllerXAxis = new PIDController();

        [Tooltip("The PID controller for the Y axis.")]
        public PIDController controllerYAxis = new PIDController();

        [Tooltip("The PID controller for the Z axis.")]
        public PIDController controllerZAxis = new PIDController();

        /// <summary>
        /// An enum for each axis of the PID controller.
        /// </summary>
        public enum Axis
        {
            X,
            Y,
            Z
        }

        /// <summary>
        /// Set the error for a specific axis of the PID controller.
        /// </summary>
        /// <param name="axis">The axis that the error pertains to.</param>
        /// <param name="error">The error value.</param>
        /// <param name="errorChangeRate">How fast the error is changing.</param>
        public void SetError(Axis axis, float error, float errorChangeRate)
        {
            switch (axis)
            {
                case Axis.X:
                    controllerXAxis.SetError(error, errorChangeRate);
                    break;
                case Axis.Y:
                    controllerYAxis.SetError(error, errorChangeRate);
                    break;
                case Axis.Z:
                    controllerZAxis.SetError(error, errorChangeRate);
                    break;
            }
        }

        /// <summary>
        /// Set the integral influence for a specified axis of the PID controller.
        /// </summary>
        /// <param name="axis">The axis that the integral influence is being set for.</param>
        /// <param name="influence">The integral influence.</param>
        public void SetIntegralInfluence(PIDController3D.Axis axis, float influence)
        {
            switch (axis)
            {
                case Axis.X:
                    controllerXAxis.SetIntegralInfluence(influence);
                    break;
                case Axis.Y:
                    controllerYAxis.SetIntegralInfluence(influence);
                    break;
                case Axis.Z:
                    controllerZAxis.SetIntegralInfluence(influence);
                    break;
            }
        }

        /// <summary>
        /// Set the integral influence for all three axes simultaneously.
        /// </summary>
        /// <param name="influence">Th integral influence value.</param>
        public void SetIntegralInfluence(float influence)
        {
            controllerXAxis.SetIntegralInfluence(influence);
            controllerYAxis.SetIntegralInfluence(influence);
            controllerZAxis.SetIntegralInfluence(influence);
        }

        /// <summary>
        /// Get the control value (the PID function return value) for a specific axis.
        /// </summary>
        /// <param name="axis">The axis to get the control value for.</param>
        /// <returns>The control value for the specified axis.</returns>
        public float GetControlValue(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return controllerXAxis.GetControlValue();
                case Axis.Y:
                    return controllerYAxis.GetControlValue();
                default:    // Z
                    return controllerZAxis.GetControlValue();
            }
        }

        /// <summary>
        /// Get the control values for all three axes simultaneously in a Vector3 format.
        /// </summary>
        /// <returns>The control value for each axis in Vector3 format.</returns>
        public Vector3 GetControlValues()
        {
            return new Vector3(controllerXAxis.GetControlValue(), controllerYAxis.GetControlValue(), controllerZAxis.GetControlValue());
        }
    }
}