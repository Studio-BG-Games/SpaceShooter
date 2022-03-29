using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Base class for a weapon unit that makes up part of a weapon.
    /// </summary>
    public class WeaponUnit : MonoBehaviour
    {
        /// <summary>
        /// Start triggering the weapon unit.
        /// </summary>
        public virtual void StartTriggering() { }


        /// <summary>
        /// Stop triggering the weapon unit.
        /// </summary>
        public virtual void StopTriggering() { }


        /// <summary>
        /// Check if the weapon unit can be triggered.
        /// </summary>
        public virtual bool CanTrigger
        {
            get { return true; }
        }


        /// <summary>
        /// Trigger the weapon unit once.
        /// </summary>
        public virtual void TriggerOnce() { }


        /// <summary>
        /// Get the damage for this weapon unit (typically in DPS - Damage Per Second).
        /// </summary>
        /// <param name="healthType">The health type to get damage for.</param>
        /// <returns>The damage done for a particular health type.</returns>
        public virtual float Damage(HealthType healthType)
        {
            return 0;
        }


        /// <summary>
        /// Get the weapon unit speed.
        /// </summary>
        public virtual float Speed
        {
            get { return 0; }
        }


        /// <summary>
        /// Get the range of this weapon unit.
        /// </summary>
        public virtual float Range
        {
            get { return 0; }
        }


        /// <summary>
        /// Aim the weapon unit at a world position.
        /// </summary>
        /// <param name="aimPosition">The world position to aim the weapon unit at.</param>
        public virtual void Aim(Vector3 aimPosition) { }


        /// <summary>
        /// Clear any aiming currently implemented on this weapon unit.
        /// </summary>
        public virtual void ClearAim() { }
    }
}
