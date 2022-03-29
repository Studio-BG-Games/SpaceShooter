using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Unity event for running functions when a collision occurs
    /// </summary>
    [System.Serializable]
    public class OnCollisionEnterEventHandler : UnityEvent<Collision> { }

    /// <summary>
    /// This class provides a vehicle with a Health component.
    /// </summary>
    public class Health : ModuleManager
    {

        // All the Damageables loaded onto this vehicle
        protected List<Damageable> damageables = new List<Damageable>();
        public List<Damageable> Damageables { get { return damageables; } }

        [Header("Settings")]

        [Tooltip("Whether this component should handle collision events in its OnCollisionEnter function.")]
        [SerializeField]
        protected bool handleCollisionEvents = true;

        [Header("Damageables Default Settings")]

        [SerializeField]
        protected bool overrideDamageableDefaultSettings = false;
        public bool OverrideDamageableDefaultSettings { set { overrideDamageableDefaultSettings = value; } }

        [SerializeField]
        protected bool defaultIsDamageable = true;
        public bool DefaultIsDamageable
        {
            get { return defaultIsDamageable; }
            set { defaultIsDamageable = value; }
        }

        [SerializeField]
        protected bool defaultIsHealable = true;
        public bool DefaultIsHealable
        {
            get { return defaultIsHealable; }
            set { defaultIsHealable = value; }
        }

        [Header("Events")]

        // Collision event
        public OnCollisionEnterEventHandler onCollisionEnter;

   
        protected override void Awake()
        {
            base.Awake();
            
            DamageReceiver[] damageReceivers = transform.GetComponentsInChildren<DamageReceiver>();
            foreach(DamageReceiver damageReceiver in damageReceivers)
            {
                onCollisionEnter.AddListener(damageReceiver.OnCollision);
            }

            damageables = new List<Damageable>(transform.GetComponentsInChildren<Damageable>());
            if (overrideDamageableDefaultSettings)
            {
                foreach (Damageable damageable in damageables)
                {
                    damageable.SetDamageable(defaultIsDamageable);
                    damageable.SetHealable(defaultIsHealable);
                }
            }
        }

        /// <summary>
        /// Set the damageability of all damageable components on the vehicle.
        /// </summary>
        /// <param name="isDamageable">Whether the damageables are damageable.</param>
        public void SetDamageableAll(bool isDamageable)
        {
            for(int i = 0; i < damageables.Count; ++i)
            {
                damageables[i].SetDamageable(isDamageable);
            }
        }

        /// <summary>
        /// Set the healability of all damageable components on the vehicle.
        /// </summary>
        /// <param name="isHealable">Whether the damageables are healable.</param>
        public void SetHealableAll(bool isHealable)
        {
            for (int i = 0; i < damageables.Count; ++i)
            {
                damageables[i].SetHealable(isHealable);
            }
        }

        public void DestroyAllDamageables()
        {
            for (int i = 0; i < damageables.Count; ++i)
            {
                if (!damageables[i].Destroyed)
                {
                    damageables[i].Destroy();
                }
            }
        }

        // Called when a collision occurs
        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (handleCollisionEvents)
            {
                // Call the collision event
                onCollisionEnter.Invoke(collision);
            }
        }

        /// <summary>
        /// Called every time a new module is mounted at a module mount.
        /// </summary>
        /// <param name="moduleMount">The module mount where the new module was loaded.</param>
        protected override void OnModuleMounted(Module module)
        {
            // Get a reference to any Damageable on the new module 
            Health health = module.GetComponent<Health>();
            if (health == this) return;

            if (health != null)
            {
                for (int i = 0; i < health.damageables.Count; ++i)
                {
                    damageables.Add(health.damageables[i]);
                    if (overrideDamageableDefaultSettings)
                    {
                        health.damageables[i].SetDamageable(defaultIsDamageable);
                        health.damageables[i].SetHealable(defaultIsHealable);
                    }
                }
            }
            else
            {
                Damageable[] moduleDamageables = module.GetComponentsInChildren<Damageable>();
                for (int i = 0; i < moduleDamageables.Length; ++i)
                {
                    damageables.Add(moduleDamageables[i]);
                    if (overrideDamageableDefaultSettings)
                    {
                        moduleDamageables[i].SetDamageable(defaultIsDamageable);
                        moduleDamageables[i].SetHealable(defaultIsHealable);
                    }
                }
            }
        }


        /// <summary>
        /// Called every time a module is unmounted at a module mount.
        /// </summary>
        /// <param name="moduleMount">The module mount where the new module was unmounted.</param>
        protected override void OnModuleUnmounted(Module module)
        {
            // Get a reference to any Damageable on the new module 
            Health health = module.GetComponent<Health>();
            if (health == this) return;

            if (health != null)
            {
                for (int i = 0; i < health.damageables.Count; ++i)
                {
                    if (damageables.Contains(health.damageables[i]))
                    {
                        damageables.Remove(health.damageables[i]);
                    }
                }
            }
            else
            {
                Damageable[] moduleDamageables = module.GetComponentsInChildren<Damageable>();
                for (int i = 0; i < moduleDamageables.Length; ++i)
                {
                    if (damageables.Contains(moduleDamageables[i]))
                    {
                        damageables.Remove(moduleDamageables[i]);
                    }
                }
            }
        }
        
        /// <summary>
        /// Reset the health to starting conditions.
        /// </summary>
        public virtual void ResetHealth()
        {
            // Reset all of the damageables to starting conditions.
            foreach (Damageable damageable in damageables)
            {
                damageable.Restore();
            }
        }

        /// <summary>
        /// Get the maximum health for a specified health type.
        /// </summary>
        /// <param name="healthType">The health type being queried.</param>
        /// <returns>The maximum health.</returns>
        public virtual float GetMaxHealthByType(HealthType healthType)
        {
            float maxHealth = 0;

            for (int i = 0; i < damageables.Count; ++i)
            {
                if (damageables[i].HealthType == healthType)
                {
                    maxHealth += damageables[i].HealthCapacity;
                }
            }

            return maxHealth;
        }


        /// <summary>
        /// Get the current health for a specified health type.
        /// </summary>
        /// <param name="healthType">The health type being queried.</param>
        /// <returns>The current health.</returns>
        public virtual float GetCurrentHealthByType(HealthType healthType)
        {
            float currentHealth = 0;

            for (int i = 0; i < damageables.Count; ++i)
            {
                if (damageables[i].HealthType == healthType)
                {
                    currentHealth += damageables[i].CurrentHealth;
                }
            }

            return currentHealth;
        }

        /// <summary>
        /// Get the fraction of health remaining of a specified type.
        /// </summary>
        /// <param name="healthType">The health type.</param>
        /// <returns>The health fraction remaining</returns>
        public virtual float GetCurrentHealthFractionByType(HealthType healthType)
        {

            float currentHealth = 0;
            float maxHealth = 0.00001f;

            for (int i = 0; i < damageables.Count; ++i)
            {
                if (damageables[i].HealthType == healthType)
                {
                    currentHealth += damageables[i].CurrentHealth;
                    maxHealth += damageables[i].HealthCapacity;
                }
            }

            return currentHealth / maxHealth;
        }
    }
}