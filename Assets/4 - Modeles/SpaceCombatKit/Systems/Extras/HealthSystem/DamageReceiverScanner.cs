using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Unity event for running functions when a damage receiver is found.
    /// </summary>
    [System.Serializable]
    public class OnDamageReceiverDetectedEventHandler : UnityEvent<DamageReceiver> { }

    /// <summary>
    /// Unity event for running functions when a damage receiver stops being scanned.
    /// </summary>
    [System.Serializable]
    public class OnDamageReceiverUndetectedEventHandler : UnityEvent<DamageReceiver> { }

    /// <summary>
    /// Stores damage receivers that are inside a trigger collider.
    /// </summary>
    public class DamageReceiverScanner : MonoBehaviour
    {

        [Header("Damage Receiver Scanner")]
        [SerializeField]
        protected SphereCollider scannerTriggerCollider;
        public SphereCollider ScannerTriggerCollider { get { return scannerTriggerCollider; } }

        // All of the damageables currently within the trigger collider.
        protected List<DamageReceiver> damageReceiversInRange = new List<DamageReceiver>();
        public List<DamageReceiver> DamageReceiversInRange { get { return damageReceiversInRange; } }

        // Damage receiver found event
        public OnDamageReceiverDetectedEventHandler onDamageReceiverDetected;

        // Damage receiver exited event
        public OnDamageReceiverUndetectedEventHandler onDamageReceiverUndetected;


        protected virtual void Reset()
        {
            scannerTriggerCollider = GetComponent<SphereCollider>();
        }

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
                            onDamageReceiverDetected.Invoke(damageReceiver);
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
                            onDamageReceiverUndetected.Invoke(damageReceiver);
                        }
                    }
                }  
            }
        }
    }
}