using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages the visual effects for an energy shield.
    /// </summary>
    public class EnergyShieldController : MonoBehaviour
    {

        [Header("General")]


        [Tooltip("The mesh renderer for the shield effect.")]
        [SerializeField]
        protected MeshRenderer energyShieldMeshRenderer;
        public MeshRenderer EnergyShieldMeshRenderer
        {
            get { return energyShieldMeshRenderer; }
            set { energyShieldMeshRenderer = value; }
        }


        [Header("Hit Effect Settings")]


        [Tooltip("How long a hit effect takes to fade away.")]
        [SerializeField]
        protected float effectFadeTime = 1;

        [Tooltip("A curve that represents how the effect strength fades over the effect fade time.")]
        [SerializeField]
        protected AnimationCurve effectFadeCurve = AnimationCurve.Linear(0, 1, 1, 0);

        [Tooltip("The distance below which effects are merged together into a single effect. Set to zero to always create a new effect.")]
        [SerializeField]
        protected float mergeEffectDistance = 0f;

        [Tooltip("When there are too many effects, an effect with a value lower than this will be overriden to add a new one. Prevents almost-faded effects from taking up slots.")]
        [SerializeField]
        protected float overrideEffectThreshold = 0f;

        protected int numEffectSlots = 10;  // The max number of hit points that can be shown. Hard-coded into the shader.


        [Header("Rim Glow")]


        [Tooltip("Whether to animate the rim glow based on hits.")]
        [SerializeField]
        protected bool hitAnimatedRimGlow = true;

        [Tooltip("The amount of rim glow based on the current hit effect amount.")]
        [SerializeField]
        protected float hitAnimatedRimGlowAmount = 0.5f;


        [Header("Physics Collisions")]


        [Tooltip("Whether to detect collisions independently. If you're running effects using a health script, disable this.")]
        [SerializeField]
        protected bool detectCollisions = true;

        [Tooltip("Whether to use the collision relative velocity to drive the effect strength.")]
        [SerializeField]
        protected bool collisionVelocityBasedEffectStrength = true;

        [Tooltip("The value that is multiplied by the collision relative velocity to get the effect strength.")]
        [SerializeField]
        protected float collisionRelativeVelocityToEffectStrength = 0.1f;

        [Tooltip("The effect color for collisions.")]
        [SerializeField]
        protected Color collisionEffectColor = Color.white;
      
        protected EffectInstance[] effectInstances; // An array of effect instances (length hard-coded based on the shader).


        // Describes a single hit effect on the shield
        protected class EffectInstance
        {
            public Vector3 position;    // The position of the effect (local to the shield mesh)
            public Color color;         // The hit effect color
            public float startTime;     // When the effect was started

            /// <summary>
            /// Constructor for a new hit effect
            /// </summary>
            /// <param name="position">Effect position.</param>
            /// <param name="color">Effect color.</param>
            /// <param name="startTime">Effect start time</param>
            public EffectInstance(Vector3 position, Color color, float startTime)
            {
                this.position = position;
                this.color = color;
                this.startTime = startTime;
            }
        }


        // Called when this component is first added to a gameobject or reset in inspector
        protected virtual void Reset()
        {
            // Find the shield mesh renderer
            energyShieldMeshRenderer = GetComponent<MeshRenderer>();
        }

        protected virtual void Awake()
        {
            // Create the array of effect instances
            effectInstances = new EffectInstance[numEffectSlots];
            for (int i = 0; i < numEffectSlots; ++i)
            {
                effectInstances[i] = new EffectInstance(Vector3.zero, new Color(0, 0, 0, 0), 0);
            }

            // If set up to independently detect collisions, show a warning if no rigidbody found (otherwise OnCollisionEnter won't be called).
            if (detectCollisions && GetComponent<Rigidbody>() == null)
            {
                Debug.LogWarning("Energy Shield Controller must be on the same gameobject as a Rigidbody to detect collisions. Add one or disable collision detection.");
            }

            UpdateEffects();
        }


        // Called when a collision occurs
        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (detectCollisions)
            {
                float effectStrength = collisionVelocityBasedEffectStrength ? collisionRelativeVelocityToEffectStrength * collision.relativeVelocity.magnitude : 1;
                for(int i = 0; i < collision.contacts.Length; ++i)
                {
                    Debug.Log(effectStrength * collisionEffectColor);
                    ShowEffect(collision.contacts[i].point, effectStrength * collisionEffectColor);
                }
            }
        }

        /// <summary>
        /// Show an effect on the shield.
        /// </summary>
        /// <param name="hitPosition">The world space hit position.</param>
        /// <param name="effectColor">The effect color.</param>
        public virtual void ShowEffect(Vector3 effectPosition, Color effectColor)
        {

            if (energyShieldMeshRenderer == null) return;
            
            // Get the local hit position relative to the shield mesh
            Vector3 localHitPosition = transform.InverseTransformPoint(effectPosition);

            // The index of the effect slot that will be used.
            int index = -1;
            
            // Look for a free slot (where the effect is zero)
            for (int i = 0; i < numEffectSlots; ++i)
            {
                float existingColorAmount = ((Vector4)effectInstances[i].color).magnitude;
                if (existingColorAmount < 0.0001f)
                {
                    index = i;
                    break;
                }
            }

            // If no free slot found, see if we can override one
            if (index == -1)
            {
                for (int i = 0; i < numEffectSlots; ++i)
                {
                    // See if there's an effect close enough to merge
                    bool isInMergeDistance = (Vector3.Distance(localHitPosition, effectInstances[i].position) < mergeEffectDistance);

                    // See if an existing effect is below the override threshold
                    float existingColorAmount = ((Vector4)effectInstances[i].color).magnitude;

                    // Override if possible
                    if (isInMergeDistance || existingColorAmount < overrideEffectThreshold)
                    {
                        index = i;
                        break;
                    }
                }
            }

            // If an effect slot found, implement this effect
            if (index != -1)
            {
                // Get the existing and new color amount for merging
                float existingColorAmount = ((Vector4)effectInstances[index].color).magnitude;
                float newColorAmount = ((Vector4)effectColor).magnitude;

                // Merge the effect positions based on color strength
                if (existingColorAmount + newColorAmount > 0.0001f)
                {
                    effectInstances[index].position = (existingColorAmount / (existingColorAmount + newColorAmount)) * effectInstances[index].position +
                                                (newColorAmount / (existingColorAmount + newColorAmount)) * localHitPosition;

                    // Merge the effect colors based on color strength
                    effectInstances[index].color = (existingColorAmount / (existingColorAmount + newColorAmount)) * effectInstances[index].color +
                                                    (newColorAmount / (existingColorAmount + newColorAmount)) * effectColor;
                }
          
                // Reset the effect start time
                effectInstances[index].startTime = Time.time;

            }
        }

        
        // Called every frame to update hit visual effects.
        protected virtual void UpdateEffects()
        {

            if (energyShieldMeshRenderer == null) return;

            // Animate the rim glow based on hits
            if (hitAnimatedRimGlow)
            {
                // Calculate the current amount of hit effects based on color strength
                float amount = 0;
                for (int i = 0; i < effectInstances.Length; ++i)
                {
                    amount = hitAnimatedRimGlowAmount * Mathf.Clamp(Mathf.Max(((Vector4)effectInstances[i].color).magnitude * 0.1f, amount), 0, 1);
                }

                // Update the rim glow based on hit effects amount
                energyShieldMeshRenderer.material.SetFloat("_RimOpacity", amount);
            }

            // Update effect instances
            for (int i = 0; i < effectInstances.Length; ++i)
            {
                // Fade the effect color
                float amount = effectFadeCurve.Evaluate(Mathf.Clamp((Time.time - effectInstances[i].startTime) / effectFadeTime, 0, 1));
                effectInstances[i].color = Color.Lerp(effectInstances[i].color, new Color(0, 0, 0, 0), (1 - amount));

                // Pass the time since started to the shader so it can adjust the effect size
                Vector4 tmp = effectInstances[i].position;
                tmp.w = Time.time - effectInstances[i].startTime;   // The fourth value in the position vector is the time since started
                energyShieldMeshRenderer.material.SetVector("_EffectPosition" + i.ToString(), tmp);
            }

            // Update the effect colors in the shader
            energyShieldMeshRenderer.material.SetVector("_EffectColor0", effectInstances[0].color);
            energyShieldMeshRenderer.material.SetVector("_EffectColor1", effectInstances[1].color);
            energyShieldMeshRenderer.material.SetVector("_EffectColor2", effectInstances[2].color);
            energyShieldMeshRenderer.material.SetVector("_EffectColor3", effectInstances[3].color);
            energyShieldMeshRenderer.material.SetVector("_EffectColor4", effectInstances[4].color);
            energyShieldMeshRenderer.material.SetVector("_EffectColor5", effectInstances[5].color);
            energyShieldMeshRenderer.material.SetVector("_EffectColor6", effectInstances[6].color);
            energyShieldMeshRenderer.material.SetVector("_EffectColor7", effectInstances[7].color);
            energyShieldMeshRenderer.material.SetVector("_EffectColor8", effectInstances[8].color);
            energyShieldMeshRenderer.material.SetVector("_EffectColor9", effectInstances[9].color);

        }

        // Called every frame
        protected virtual void Update()
        {
            // Update the shield effects frame each frame
            UpdateEffects();
        }
    }
}