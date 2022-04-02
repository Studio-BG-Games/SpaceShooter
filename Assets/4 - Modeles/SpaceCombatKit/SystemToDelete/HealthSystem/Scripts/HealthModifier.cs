using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Class to hold information about damage or healing properties.
    /// </summary>
    [System.Serializable]
    public class HealthModifier
    {

        [SerializeField]
        protected HealthModifierType healthModifierType;
        public HealthModifierType HealthModifierType
        {
            get { return healthModifierType; }
        }

        [Header("Damage")]

        [SerializeField]
        protected float defaultDamageValue = 100;
        public float DefaultDamageValue
        {
            get { return defaultDamageValue; }
            set { defaultDamageValue = value; }
        }

        [SerializeField]
        protected List<HealthModifierValue> damageOverrideValues = new List<HealthModifierValue>();
        public List<HealthModifierValue> DamageOverrideValues
        {
            get { return damageOverrideValues; }
            set { damageOverrideValues = value; }
        }

        [SerializeField]
        protected float damageMultiplier = 1;
        public float DamageMultiplier
        {
            get { return damageMultiplier; }
            set { damageMultiplier = value; }
        }

        [Header("Healing")]

        [SerializeField]
        protected float defaultHealingValue = 0;
        public float DefaultHealingValue
        {
            get { return defaultHealingValue; }
            set { defaultHealingValue = value; }
        }

        [SerializeField]
        protected List<HealthModifierValue> healingOverrideValues = new List<HealthModifierValue>();
        public List<HealthModifierValue> HealingOverrideValues
        {
            get { return healingOverrideValues; }
            set { healingOverrideValues = value; }
        }

        [SerializeField]
        protected float healingMultiplier = 1;
        public float HealingMultiplier
        {
            get { return healingMultiplier; }
            set { healingMultiplier = value; }
        }


        public virtual float GetDamage(HealthType healthType)
        {
            for (int i = 0; i < damageOverrideValues.Count; ++i)
            {
                if (damageOverrideValues[i].HealthType == healthType)
                {
                    return damageOverrideValues[i].Value;
                }
            }

            return defaultDamageValue;
        }

        public virtual float GetHealing(HealthType healthType)
        {
            for (int i = 0; i < healingOverrideValues.Count; ++i)
            {
                if (healingOverrideValues[i].HealthType == healthType)
                {
                    return healingOverrideValues[i].Value;
                }
            }

            return defaultHealingValue;
        }
    }
}