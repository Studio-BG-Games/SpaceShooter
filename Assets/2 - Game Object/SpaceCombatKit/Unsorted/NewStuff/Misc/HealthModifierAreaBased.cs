using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Emits damage to damage receivers, either at a raycast hit point, or to all damage receivers in the vicinity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class HealthModifierAreaBased : MonoBehaviour, IRootTransformUser
    {

        [SerializeField]
        protected bool triggerOnDamageReceiverDetected = true;

        [SerializeField]
        protected HealthModifier healthModifierSettings;

        [SerializeField]
        protected float maxEffectDistance = 2000;

        [SerializeField]
        protected AnimationCurve effectFalloffCurve = AnimationCurve.Linear(0, 1, 1, 1);

        [SerializeField]
        protected float effectMultiplier = 1;

        [SerializeField]
        protected Transform rootTransform;
        public Transform RootTransform
        {
            set { rootTransform = value; }
        }

        [Header("Damage Receiver Scanning")]

        [SerializeField]
        protected SphereCollider scannerTriggerCollider;
        public SphereCollider ScannerTriggerCollider { get { return scannerTriggerCollider; } }

        // All of the damageables currently within the trigger collider.
        protected List<DamageReceiver> damageReceiversInRange = new List<DamageReceiver>();
        public List<DamageReceiver> DamageReceiversInRange { get { return damageReceiversInRange; } }

        public UnityEvent onDamageReceiverDetected;



        // Called when a collider enters a trigger collider.
        protected virtual void OnTriggerEnter(Collider other)
        {

            DamageReceiver damageReceiver = other.GetComponent<DamageReceiver>();

            if (damageReceiver != null)
            {
                // Ignore if outside the designated trigger collider's radius
                if (scannerTriggerCollider != null)
                {
                    Vector3 colliderWorldPos = scannerTriggerCollider.transform.TransformPoint(scannerTriggerCollider.center);
                    float distanceToTarget = Vector3.Distance(damageReceiver.GetClosestPoint(colliderWorldPos), colliderWorldPos);
                    if (distanceToTarget < scannerTriggerCollider.radius)
                    {

                        if (!damageReceiversInRange.Contains(damageReceiver))
                        {
                            // Add the damageable to the list and invoke the event
                            damageReceiversInRange.Add(damageReceiver);
                        }

                        onDamageReceiverDetected.Invoke();

                        if (triggerOnDamageReceiverDetected)
                        {
                            Trigger();
                        }
                    }
                }
            }
        }


        // Called when a collider exits the trigger collider
        protected virtual void OnTriggerExit(Collider other)
        {
            // Get a reference to the damageable
            DamageReceiver damageReceiver = other.GetComponent<DamageReceiver>();

            if (damageReceiver != null)
            {
                // Ignore if still inside the designated trigger collider's radius
                if (scannerTriggerCollider != null)
                {
                    Vector3 colliderWorldPos = scannerTriggerCollider.transform.TransformPoint(scannerTriggerCollider.center);
                    float distanceToTarget = Vector3.Distance(damageReceiver.GetClosestPoint(colliderWorldPos), colliderWorldPos);
                    if (distanceToTarget > scannerTriggerCollider.radius)
                    {
                        // Remove the damageable from the list
                        int index = damageReceiversInRange.IndexOf(damageReceiver);
                        if (index != -1)
                        {
                            damageReceiversInRange.RemoveAt(index);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Called when the component is first added to a gameobject, or reset in the inspector.
        /// </summary>
        protected virtual void Reset()
        {
            rootTransform = transform;

            scannerTriggerCollider = GetComponentInChildren<SphereCollider>();

#if UNITY_EDITOR
#if UNITY_2018_3_OR_NEWER
            if (UnityEditor.PrefabUtility.GetPrefabAssetType(transform.root) != UnityEditor.PrefabAssetType.NotAPrefab)
            {
                rootTransform = transform.root;
            }
#else
                    if (UnityEditor.PrefabUtility.GetPrefabType(transform.root) != UnityEditor.PrefabType.None) {
                        rootTransform = transform.root;
                    }
#endif
#endif
        }


        public void SetEffectMultiplier(float multiplier)
        {
            this.effectMultiplier = multiplier;
        }


        /// <summary>
        /// Emit damage from a point in world space to any damage receivers in range.
        /// </summary>
        /// <param name="damageEmissionPoint">The world space point from which to emit damage.</param>
        public void Trigger()
        {
            // Go through all the detected damageables and damage them according to the damage falloff parameters
            for (int i = 0; i < damageReceiversInRange.Count; ++i)
            {

                if (damageReceiversInRange[i].RootTransform == rootTransform) return;

                // Get the distance from the damage emitter to the damageable's closest point
                Vector3 closestPoint = damageReceiversInRange[i].GetClosestPoint(transform.position);
                float distance = Vector3.Distance(transform.position, closestPoint);

                // Damage
                float damageValue = healthModifierSettings.GetDamage(damageReceiversInRange[i].HealthType) * effectFalloffCurve.Evaluate(distance) * effectMultiplier;
                if (damageValue != 0)
                {
                    damageReceiversInRange[i].Damage(damageValue, closestPoint, healthModifierSettings.HealthModifierType, rootTransform);
                }

                // Healing
                float healingValue = healthModifierSettings.GetHealing(damageReceiversInRange[i].HealthType) * effectFalloffCurve.Evaluate(distance) * effectMultiplier;
                if (healingValue != 0)
                {
                    damageReceiversInRange[i].Heal(healingValue, closestPoint, healthModifierSettings.HealthModifierType, rootTransform);
                }
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Color originalColor = Gizmos.color;
            Gizmos.color = new Color(1, 0.5f, 0);
            Gizmos.DrawWireSphere(transform.position, maxEffectDistance);
            Gizmos.color = originalColor;
        }
    }
}