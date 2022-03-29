using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Animate an Image color based on damage amount.
    /// </summary>
    public class HitEffectImageController : ModuleManager
    {

        [Header("Settings")]

        [SerializeField]
        protected Color defaultColor = Color.white;

        protected Color m_Color;

        [SerializeField]
        protected float maxEffectDamageValue = 1000;

        [SerializeField]
        protected AnimationCurve damageEffectCurve;

        [SerializeField]
        protected AnimationCurve colorAmountAnimationCurve;

        [SerializeField]
        protected float animationTime;

        [SerializeField]
        protected Image hitEffectImage;

        protected float nextValue = 0;


        protected override void Start()
        {
            base.Start();

            SetColorAmount(0);

            Damageable[] damageables = transform.GetComponentsInChildren<Damageable>();
            for(int i = 0; i < damageables.Length; ++i)
            {
                Connect(damageables[i]);
            }
        }

        /// <summary>
        /// Called when a module is mounted on a module mount.
        /// </summary>
        /// <param name="module">The newly mounted module.</param>
        protected override void OnModuleMounted(Module module)
        {
            // Connect any damageables on the module
            Damageable[] damageables = module.GetComponentsInChildren<Damageable>();
            for (int i = 0; i < damageables.Length; ++i)
            {
                Connect(damageables[i]);
            }
        }

        /// <summary>
        /// Called when a module is unmounted at a module mount.
        /// </summary>
        /// <param name="module">The newly unmounted module.</param>
        protected override void OnModuleUnmounted(Module module)
        {
            // Disconnect any damageables on the module
            Damageable[] damageables = module.GetComponentsInChildren<Damageable>();
            for (int i = 0; i < damageables.Length; ++i)
            {
                Disconnect(damageables[i]);
            }
        }

        /// <summary>
        /// Connect a damageable module to animate the effect when damaged.
        /// </summary>
        /// <param name="damageable">The Damageable module to connect.</param>
        protected virtual void Connect(Damageable damageable)
        {
            Disconnect(damageable);

            damageable.onDamaged.AddListener((damage, hitPoint, healthModifierType, damageSourceRootTransform) => 
                                                ShowEffect(damageable.HealthType, damage, hitPoint, healthModifierType, damageSourceRootTransform));
        }

        /// <summary>
        /// Disconnect a damageable module to stop animating the effect when damaged.
        /// </summary>
        /// <param name="damageable">The Damageable module to disconnect.</param>
        protected virtual void Disconnect(Damageable damageable)
        {
            damageable.onDamaged.RemoveListener((damage, hitPoint, healthModifierType, damageSourceRootTransform) =>
                                                ShowEffect(damageable.HealthType, damage, hitPoint, healthModifierType, damageSourceRootTransform));
        }


        /// <summary>
        /// Animate the damage.
        /// </summary>
        /// <param name="healthType">The health type of the Damageable that got damaged.</param>
        /// <param name="damage">The damage amount.</param>
        /// <param name="hitPoint">The hit position.</param>
        /// <param name="healthModifierType">The type of damage source.</param>
        /// <param name="damageSourceRootTransform">The root transform of the damage source.</param>
        protected virtual void ShowEffect(HealthType healthType, float damage, Vector3 hitPoint, HealthModifierType healthModifierType, Transform damageSourceRootTransform)
        {
            m_Color = damageEffectCurve.Evaluate(damage / maxEffectDamageValue) * healthType.Color;

            Animate();
        }

        /// <summary>
        /// Set the image color amount;
        /// </summary>
        /// <param name="amount">The color amount.</param>
        protected virtual void SetColorAmount(float amount)
        {
            hitEffectImage.color = amount * m_Color;
        }

        /// <summary>
        /// Animate the effect.
        /// </summary>
        public virtual void Animate()
        {
            if (gameObject.activeInHierarchy) StartCoroutine(AnimationCoroutine());
        }

        // The animation coroutine
        protected virtual IEnumerator AnimationCoroutine()
        {

            float startTime = Time.time;

            while (true)
            {
                float amount = (Time.time - startTime) / animationTime;

                if (amount > 1)
                {
                    nextValue = Mathf.Max(nextValue, colorAmountAnimationCurve.Evaluate(1));
                    break;
                }

                nextValue = Mathf.Max(nextValue, colorAmountAnimationCurve.Evaluate(amount));

                yield return null;
            }
        }

        protected virtual void LateUpdate()
        {
            SetColorAmount(nextValue);
            nextValue = 0;
        }
    }
}
