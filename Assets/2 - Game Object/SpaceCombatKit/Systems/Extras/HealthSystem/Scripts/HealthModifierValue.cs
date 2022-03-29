using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Associates a health type with a float value (e.g. for weapon damage to different health types).
    /// </summary>
    [System.Serializable]
    public class HealthModifierValue
    {
        [SerializeField]
        private HealthType healthType;
        public HealthType HealthType
        {
            get { return healthType; }
            set { healthType = value; }
        }

        [SerializeField]
        private float value;
        public float Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public HealthModifierValue(HealthType healthType, float value)
        {
            this.healthType = healthType;
            this.value = value;
        }
    }
}
