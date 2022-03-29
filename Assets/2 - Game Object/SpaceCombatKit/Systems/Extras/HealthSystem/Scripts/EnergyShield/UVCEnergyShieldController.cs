using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages the visual effects for an energy shield.
    /// </summary>
    public class UVCEnergyShieldController : EnergyShieldController
    {

        [Header("Health Settings")]


        [Tooltip("The Damageable that is used to drive the shield effects.")]
        [SerializeField]
        protected Damageable damageable;

        [Tooltip ("The health based gradient colors for the shield. Left is zero health and right is full health.")]
        [SerializeField]
        [GradientUsageAttribute(true)] protected Gradient healthBasedColor = new Gradient();


        [Header("Health Based Rim Color")]


        [Tooltip("Whether to use the health based color gradient to drive the rim glow color.")]
        [SerializeField]
        protected bool healthBasedRimColor = true;


        [Header("Damage Effects")]


        [Tooltip("Whether to modify the strength of the hit effect based on the damage value.")]
        [SerializeField]
        protected bool damageBasedEffectStrength = true;

        [Tooltip("The value that is multiplied by the damage value to get the effect strength.")]
        [SerializeField]
        protected float damageToEffectStrength = 0.1f;

        [Tooltip("Whether to override the color of the shield with a specific color for damage.")]
        [SerializeField]
        protected bool overrideDamageEffectColor = false;

        [Tooltip("The unique color for damage hit effects.")]
        [SerializeField]
        [ColorUsageAttribute(true, true)] protected Color damageEffectColorOverride = new Color(0.075f, 0.5f, 1f);


        [Header("Heal Effects")]


        [Tooltip("Whether to modify the strength of the hit effect based on the heal value.")]
        [SerializeField]
        protected bool healBasedEffectStrength = true;

        [Tooltip("The value that is multiplied by the healing value to get the effect strength.")]
        [SerializeField]
        protected float healToEffectStrength = 0.1f;

        [Tooltip("Whether to override the color of the shield with a specific color for healing.")]
        [SerializeField]
        protected bool overrideHealEffectColor = false;

        [Tooltip("The unique color for heal hit effects.")]
        [SerializeField]
        [ColorUsageAttribute(true, true)] protected Color healEffectColorOverride = new Color(1f, 0f, 0.5f);
      


        // Called when this component is first added to a gameobject or reset in inspector
        protected override void Reset()
        {
            base.Reset();

            // Disable the independent collision detection by default, since the Damageable will handle collision damage.
            detectCollisions = false;

            // Find a Damageable component
            damageable = GetComponent<Damageable>();
            if (damageable == null)
            {
                damageable = transform.root.GetComponentInChildren<Damageable>();
            }

            // Initialize the zero health color to an orange-red
            GradientColorKey zeroHealthColor = new GradientColorKey(new Color(1, 0.2f, 0) * 5, 0);
            GradientAlphaKey zeroHealthAlpha = new GradientAlphaKey(1, 0);

            // Initialize the zero health color to a sci-fi blue
            GradientColorKey fullHealthColor = new GradientColorKey(new Color(0, 0.5f, 1) * 5, 1);
            GradientAlphaKey fullHealthAlpha = new GradientAlphaKey(1, 1);

            // Initialize the color gradients
            healthBasedColor.SetKeys(new GradientColorKey[] { zeroHealthColor, fullHealthColor }, 
                                        new GradientAlphaKey[] { zeroHealthAlpha, fullHealthAlpha });
        }


        protected override void Awake()
        {
            base.Awake();

            // Hook up the damage and healing events to show effects
            if (damageable != null)
            {
                damageable.onDamaged.AddListener(OnDamaged);
                damageable.onHealed.AddListener(OnHealed);
            }
        }

        
        /// <summary>
        /// Called when the shield is damaged.
        /// </summary>
        public virtual void OnDamaged(float damageValue, Vector3 hitPosition, HealthModifierType healthModifierType, Transform damageSourceRootTransform)
        {
            // Calculate the color for damage
            Color c = overrideDamageEffectColor ? damageEffectColorOverride : healthBasedColor.Evaluate(damageable.CurrentHealthFraction);

            // Modify the color based on damage amount
            if (damageBasedEffectStrength) c *= damageValue * damageToEffectStrength;

            // Show the effect
            ShowEffect(hitPosition, c);
        }

        /// <summary>
        /// Called when the shield is healed;
        /// </summary>
        public virtual void OnHealed(float healValue, Vector3 hitPosition, HealthModifierType healthModifierType, Transform damageSourceRootTransform)
        {
            // Calculate the color for healing
            Color c = overrideHealEffectColor ? healEffectColorOverride : healthBasedColor.Evaluate(damageable.CurrentHealthFraction);

            // Modify the color based on heal amount
            if (healBasedEffectStrength) c *= healValue * healToEffectStrength;

            // Show the effect
            ShowEffect(hitPosition, c);
        }

        
        // Called every frame to update hit visual effects.
        protected override void UpdateEffects()
        {
            base.UpdateEffects();

            if (energyShieldMeshRenderer == null) return;

            // Adjust the rim color based on the current health
            if (healthBasedRimColor)
            {
                energyShieldMeshRenderer.material.SetColor("_RimColor", healthBasedColor.Evaluate(damageable.CurrentHealthFraction));
            }
        }
    }
}