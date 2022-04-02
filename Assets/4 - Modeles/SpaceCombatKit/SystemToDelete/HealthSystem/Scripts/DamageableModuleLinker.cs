using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Links/unlinks damageable modules mounted at a module mount with damage receivers.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class DamageableModuleLinker : MonoBehaviour
    {

        [SerializeField]
        protected ModuleMount moduleMount;

        [SerializeField]
        protected List<DamageReceiver> damageReceivers = new List<DamageReceiver>();

        [Header("Events")]

        // Damageable damaged event
        public OnDamageableDamagedEventHandler onDamageableModuleDamaged;

        // Damageable healed event
        public OnDamageableHealedEventHandler onDamageableModuleHealed;

        // Damageable destroyed event
        public OnDamageableDestroyedEventHandler onDamageableModuleDestroyed;

        // Damageable restored event
        public OnDamageableRestoredEventHandler onDamageableModuleRestored;


        protected virtual void Reset()
        {
            moduleMount = GetComponent<ModuleMount>();
        }

        protected virtual void Awake()
        {
            moduleMount.onModuleMounted.AddListener(OnModuleMounted);
            moduleMount.onModuleUnmounted.AddListener(OnModuleUnmounted);
        }

        // Called when a module is mounted on the module mount
        protected virtual void OnModuleMounted(Module module)
        {
            Damageable damageable = module.GetComponent<Damageable>();
            if (damageable != null)
            {
                // Link this
                damageable.onDamaged.AddListener(OnDamageableModuleDamaged);
                damageable.onHealed.AddListener(OnDamageableModuleHealed);
                damageable.onDestroyed.AddListener(OnDamageableModuleDestroyed);
                damageable.onRestored.AddListener(OnDamageableModuleRestored);

                for (int i = 0; i < damageReceivers.Count; ++i)                {
                    // Link damage receivers
                    damageReceivers[i].SetDamageable(damageable);
                }
            }
        }


        // Called when a module is unmounted on the module mount
        protected virtual void OnModuleUnmounted(Module module)
        {
            Damageable damageable = module.GetComponent<Damageable>();
            if (damageable != null)
            {

                // Unlink this
                damageable.onDamaged.RemoveListener(OnDamageableModuleDamaged);
                damageable.onHealed.RemoveListener(OnDamageableModuleHealed);
                damageable.onDestroyed.RemoveListener(OnDamageableModuleDestroyed);
                damageable.onRestored.RemoveListener(OnDamageableModuleRestored);

                for (int i = 0; i < damageReceivers.Count; ++i)
                {
                    damageReceivers[i].SetDamageable(null);
                }
            }
        }

        protected virtual void OnDamageableModuleDamaged(float damage, Vector3 hitPoint, HealthModifierType healthModifierType, Transform damageSourceRootTransform)
        {
            onDamageableModuleDamaged.Invoke(damage, hitPoint, healthModifierType, damageSourceRootTransform);
        }


        protected virtual void OnDamageableModuleHealed(float healing, Vector3 hitPoint, HealthModifierType healthModifierType, Transform damageSourceRootTransform)
        {
            onDamageableModuleHealed.Invoke(healing, hitPoint, healthModifierType, damageSourceRootTransform);
        }

        protected virtual void OnDamageableModuleDestroyed()
        {
            onDamageableModuleDestroyed.Invoke();
        }

        protected virtual void OnDamageableModuleRestored()
        {
            onDamageableModuleRestored.Invoke();
        }
    }
}
