using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Recharge the health of a Damageable over time.
    /// </summary>
    public class HealthRecharger : MonoBehaviour
    {
        [Tooltip("The damageable to recharge.")]
        [SerializeField]
        protected Damageable damageable;

        [Header("Health Recharge")]

        [Tooltip("The rate (per second) to recharge the health.")]
        [SerializeField]
        protected float healthRechargeRate = 100;

        [Tooltip("How much to pause recharging after the Damageable is damaged.")]
        [SerializeField]
        protected float damageRechargePause = 2;
        protected float lastDamageTime = -1000;

        [Header("Restore After Destroyed")]

        [Tooltip("Whether to restore the Damageable after it is destroyed.")]
        [SerializeField]
        protected bool restoreAfterDestroyed = true;

        [Tooltip("Whether to restore full health to the Damageable when restoring it.")]
        [SerializeField]
        protected bool restoreFullHealthImmediately;

        [Tooltip("The delay before restoring the Damageable after it is destroyed.")]
        [SerializeField]
        protected float restoreDelay = 10;

        protected float destroyedTime;


        // Called when the component is first added to a gameobject, or is reset in the inspector
        protected virtual void Reset()
        {
            // Reference any damageable found on the gameobject
            damageable = GetComponent<Damageable>();
        }

        protected virtual void Awake()
        {
            // Listen to the damageable damaged event
            damageable.onDamaged.AddListener((damage, hitPosition, healthModifierType, hitRootTransform) => { OnDamaged(); });

            // Listen to the damageable destroyed event
            damageable.onDestroyed.AddListener(OnDestroyed);
        }

        // Called when the Damageable is damaged.
        protected virtual void OnDamaged()
        {
            lastDamageTime = Time.time;
        }

        // Called when the Damageable is destroyed
        protected virtual void OnDestroyed()
        {
            destroyedTime = Time.time;
        }

        // Recharge the Damageable
        protected virtual void Recharge()
        {
            if (damageable.Destroyed) return;

            if ((Time.time - lastDamageTime) < damageRechargePause)
            {
                return;
            }

            damageable.SetHealth(damageable.CurrentHealth + healthRechargeRate * Time.deltaTime);

        }

        // Called every frame
        protected virtual void Update()
        {
            // Restore the Damageable if necessary
            if (damageable.Destroyed && restoreAfterDestroyed && (Time.time - destroyedTime) > restoreDelay)
            {
                damageable.Restore(restoreFullHealthImmediately);
            }

            // Recharge the health
            Recharge();
        }
    }

}
