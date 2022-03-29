using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Provides information for leading a target in order to hit it when it is moving.
    /// </summary>
    public static class TargetLeader
    {
        /// <summary>
        /// Calculate the lead position to aim at to hit a moving target for the win.
        /// </summary>
        /// <param name="shooterPosition">The shooter position.</param>
        /// <param name="interceptSpeed">The intercept speed.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetVelocity">The target velocity vector.</param>
        /// <returns></returns>
        public static Vector3 GetLeadPosition(Vector3 shooterPosition, float interceptSpeed, Vector3 targetPosition, Vector3 targetVelocity)
        {

            if (interceptSpeed < 0.0001f) return targetPosition;

            // Get target direction
            Vector3 targetDirection = targetPosition - shooterPosition;

            // Get the target velocity magnitude
            float vE = targetVelocity.magnitude;

            // Note that the dot product of a and b (a.b) is also equal to |a||b|cos(theta) where theta = angle between them.
            // This saves using the components of the vectors themselves, only the magnitudes.

            // Get the length of the playerRelPos vector
            float playerRelPosMag = targetDirection.magnitude;

            // Get angle between player-target axis and the target's velocity vector
            float angPET = Vector3.Angle(targetDirection, targetVelocity) * Mathf.Deg2Rad; // Vector3.Angle returns in degrees

            // Get the cosine of this angle
            float cosPET = Mathf.Cos(angPET);

            // Get the coefficients of the quadratic equation
            float a = vE * vE - interceptSpeed * interceptSpeed;
            float b = 2 * playerRelPosMag * vE * cosPET;
            float c = playerRelPosMag * playerRelPosMag;

            // Check for solutions. If the discriminant (b*b)-(4*a*c) is:
            // >0: two real solutions - get the maximum one (the other will be a negative time value and can be discarded)
            // =0: one real solution (both solutions the same so either one will do)
            // <0; two imaginary solutions - never will hit the target
            float discriminant = (b * b) - (4 * a * c);

            if (discriminant <= 0) return targetPosition;

            // Get the intercept time by solving the quadratic equation. Note that if a = 0 then we will be 
            // trying to divide by zero. But in that case no quadratics are necessary, the equation will be first-order
            // and can simply be rearranged to get the intercept time.

            float interceptTime;
            if (a != 0)
                interceptTime = Mathf.Max(((-1 * b) - Mathf.Sqrt(discriminant)) / (2 * a), ((-1 * b) + Mathf.Sqrt(discriminant)) / (2 * a));
            else
            {
                interceptTime = -c / (2 * b);
            }

            return (targetPosition + (targetVelocity * interceptTime));
        }
    }
}