using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// The PID controller is a function that takes an error value as input and returns a control value that corrects that error smoothly.
    /// </summary>
    [System.Serializable]
    public class PIDController
    {
        public float proportionalCoefficient = 0.01f;
        public float integralCoefficient;
        public float derivativeCoefficient;

        protected float proportionalValue;
        protected float integralValue;
        protected float derivativeValue;

        public float integralInfluence = 1;

        /// <summary>
        /// Set the error input value for the PID function.
        /// </summary>
        /// <param name="error">The error value.</param>
        /// <param name="errorChangeRate">The amount that the error is changing.</param>
        public void SetError(float error, float errorChangeRate)
        {

            // Proportional
            proportionalValue = proportionalCoefficient * error;

            // Integral
            integralValue += integralInfluence * (integralCoefficient * error);
            integralValue = Mathf.Clamp(integralValue, -1, 1);

            // Derivative
            derivativeValue = derivativeCoefficient * errorChangeRate;

        }

        /// <summary>
        /// Set the influence that the integral value will have on th result.
        /// </summary>
        /// <param name="influence">The integral influnce value.</param>
        public void SetIntegralInfluence(float influence)
        {
            this.integralInfluence = influence;
        }

        /// <summary>
        /// Get the result from the PID function.
        /// </summary>
        /// <returns>The PID function result.</returns>
        public float GetControlValue()
        {
            return proportionalValue + (integralInfluence * integralValue) + derivativeValue;
        }
    }
}
