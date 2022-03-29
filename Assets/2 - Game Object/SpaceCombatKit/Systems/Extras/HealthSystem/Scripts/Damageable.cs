using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// UnityEvent to run functions when a damageable is damaged.
    /// </summary>
    [System.Serializable]
    public class OnDamageableDamagedEventHandler : UnityEvent<float, Vector3, HealthModifierType, Transform> { }

    /// <summary>
    /// UnityEvent to run functions when a damageable is healed.
    /// </summary>
    [System.Serializable]
    public class OnDamageableHealedEventHandler : UnityEvent<float, Vector3, HealthModifierType, Transform> { }

    /// <summary>
    /// UnityEvent to run functions when a damageable is destroyed.
    /// </summary>
    [System.Serializable]
    public class OnDamageableDestroyedEventHandler : UnityEvent { }

    /// <summary>
    /// UnityEvent to run functions when a damageable is restored after being destroyed.
    /// </summary>
    [System.Serializable]
    public class OnDamageableRestoredEventHandler : UnityEvent { }


    /// <summary>
    /// Makes an object damageable and healable.
    /// </summary>
    public class Damageable : MonoBehaviour
    {

        [Header("General")]

        [SerializeField]
        protected string damageableID;
        public string DamageableID { get { return damageableID; } }

        // The health type of this damageable
        [SerializeField]
        protected HealthType healthType;
        public HealthType HealthType { get { return healthType; } }

        // The maximum health value for the container
        [SerializeField]
        protected float healthCapacity = 100;
        public virtual float HealthCapacity
        {
            get { return healthCapacity; }
            set
            {
                healthCapacity = value;
                healthCapacity = Mathf.Max(healthCapacity, 0);
                currentHealth = Mathf.Min(currentHealth, healthCapacity);
            }
        }

        // The health value of the container when the scene starts
        [SerializeField]
        protected float startingHealth = 100;
        public virtual float StartingHealth { get { return startingHealth; } }

        // The current health value of the container
        protected float currentHealth;
        public virtual float CurrentHealth { get { return currentHealth; } }
        public virtual float CurrentHealthFraction { get { return currentHealth / startingHealth; } }

        public virtual void SetHealth(float newHealthValue)
        {
            currentHealth = Mathf.Clamp(newHealthValue, 0, healthCapacity);
        }

        // Enable/disable damage
        [SerializeField]
        protected bool isDamageable = true;

        // Enable/disable healing
        [SerializeField]
        protected bool isHealable = true;

        [SerializeField]
        protected bool canHealAfterDestroyed = false;

        [Header("Collisions")]

        // The coefficient multiplied by the collision relative velocity magnitude to get the damage value
        [SerializeField]
        protected float collisionRelativeVelocityToDamageFactor = 2.5f;

        [SerializeField]
        protected HealthModifierType collisionHealthModifierType;

        [Header("Events")]

        // Damageable damaged event
        public OnDamageableDamagedEventHandler onDamaged;

        // Damageable healed event
        public OnDamageableHealedEventHandler onHealed;

        // Damageable destroyed event
        public OnDamageableDestroyedEventHandler onDestroyed;

        // Damageable restored event
        public OnDamageableRestoredEventHandler onRestored;

        // Whether this damageable is currently destroyed
        protected bool destroyed = false;
        public bool Destroyed { get { return destroyed; } }



        /// <summary>
        /// Restore when object is enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            Restore(true);
        }

        /// <summary>
        /// Toggle whether this damageable is damageable.
        /// </summary>
        /// <param name="damageable">Whether this damageable is to be damageable.</param>
        public virtual void SetDamageable(bool isDamageable)
        {
            this.isDamageable = isDamageable;
        }


        /// <summary>
        /// Toggle whether this damageable is healable.
        /// </summary>
        /// <param name="healable">Whether this damageable is to be healable.</param>
        public void SetHealable(bool healable)
        {
            this.isHealable = healable;
        }


        /// <summary>
        /// Called when a collision happens to check if it involves a one of this damageable's colliders (if so, damages it).
        /// </summary>
        /// <param name="collision">The collision information.</param>
        public virtual void OnCollision(Collision collision)
        {
            for (int i = 0; i < collision.contacts.Length; ++i)
            {
                Damage(collision.relativeVelocity.magnitude * collisionRelativeVelocityToDamageFactor, collision.contacts[i].point, collisionHealthModifierType, null);
            }          
        }


        /// <summary>
        /// Damage this damageable.
        /// </summary>
        /// <param name="damage">The damage amount.</param>
        /// <param name="hitPoint">The world position where the damage occurred.</param>
        public virtual void Damage(float damage, Vector3 hitPoint, HealthModifierType healthModifierType, Transform damageSourceRootTransform)
        {
            if (destroyed) return;
            
            if (isDamageable)
            {                
                // Reduce the health
                currentHealth -= damage;

                // Destroy
                if (currentHealth <= 0)
                {
                    currentHealth = 0;
                    Destroy();
                }
                // Damage
                else
                {
                    // Call the damage event
                    onDamaged.Invoke(damage, hitPoint, healthModifierType, damageSourceRootTransform);
                }
            }
        }

        
        /// <summary>
        /// Heal this damageable.
        /// </summary>
        /// <param name="healing">The healing amount.</param>
        /// <param name="hitPoint">The world position where the healing occurred.</param>
        public virtual void Heal(float healing, Vector3 hitPoint, HealthModifierType healthModifierType, Transform damageSourceRootTransform)
        {

            if (destroyed) return;

            if (isHealable)
            {
                // Add the health
                currentHealth = Mathf.Clamp(currentHealth + healing, 0, healthCapacity);

                // Update the container status
                if (currentHealth > 0 && destroyed && canHealAfterDestroyed)
                    Restore(false);
            
                onHealed.Invoke(healing, hitPoint, healthModifierType, damageSourceRootTransform);
            }
        }


        /// <summary>
        /// Destroy this damageable.
        /// </summary>
        public void Destroy()
        {

            // If already in the correct state, return
            if (this.destroyed) return;

            destroyed = true;

            // Call the destroyed event
            onDestroyed.Invoke();

        }

        /// <summary>
        /// Restore this damageable.
        /// </summary>
        /// <param name="reset">Whether to reset to starting conditions.</param>
        public void Restore(bool reset = true)
        {

            destroyed = false;

            if (reset)
            {
                currentHealth = healthCapacity;
            }
            
            // Call the event
            onRestored.Invoke();
            
        }


        public virtual void SetColliderActivation(bool activate)
        {
            
        }
    }
}