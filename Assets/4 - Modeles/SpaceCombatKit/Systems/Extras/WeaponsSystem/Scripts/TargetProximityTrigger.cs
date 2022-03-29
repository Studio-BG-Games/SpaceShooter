using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VSX.UniversalVehicleCombat.Radar;


namespace VSX.UniversalVehicleCombat
{
    
    /// <summary>
    /// Unity event for running functions when the target proximity trigger is triggered.
    /// </summary>
    [System.Serializable]
    public class OnTargetProximityTriggerTriggeredEventHandler : UnityEvent { }

    public enum TargetProximityTriggerMode
    {
        OnTargetInRange,
        OnDistanceIncrease
    }

    /// <summary>
    /// Triggers when a target enters a trigger collider, with the option to only trigger when the distance is projected to increase
    /// in the next frame.
    /// </summary>
    public class TargetProximityTrigger : MonoBehaviour
    {
        
        [Header("Settings")]

        [SerializeField]
        protected Rigidbody m_rigidbody;

        [SerializeField]
        protected Trackable target;
        public virtual Trackable Target
        {
            set 
            { 
                target = value;
                if (target != null)
                {
                    targetRigidbody = target.GetComponent<Rigidbody>();
                }
            }
        }
        protected Rigidbody targetRigidbody;

        [SerializeField]
        protected TargetProximityTriggerMode triggerMode = TargetProximityTriggerMode.OnDistanceIncrease;

        [SerializeField]
        protected float triggerDistance = 100;

        protected bool targetInsideTrigger = false;

        protected float closestDistance;    // Keep a running tab on closest distance to trigger when it increases.


        [Header("Events")]

        // Proximity trigger triggered event
        public OnTargetProximityTriggerTriggeredEventHandler onTriggered;

        protected bool triggered = false;



        protected virtual void Reset()
        {
            m_rigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void OnEnable()
        {
            triggered = false;
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (targetRigidbody != null && other.attachedRigidbody == targetRigidbody)
            {
                targetInsideTrigger = true;
            }
        }

        // Check if the trigger should be activated
        protected virtual bool CheckTrigger()
        {

            if (target == null) return false;
            
            if (targetInsideTrigger)
            {
                switch (triggerMode)
                {
                    case TargetProximityTriggerMode.OnTargetInRange:

                        return true;

                    case TargetProximityTriggerMode.OnDistanceIncrease:

                        return GetClosestDistanceToTarget(Time.deltaTime) > GetClosestDistanceToTarget(0);

                    default: return true;
                }
            }
            else
            {
                return false;
            }
        }


        // Get the closest distance to the target based on a time projection (necessary because things can move very fast and the target can
        // change position a lot in one frame).
        protected virtual float GetClosestDistanceToTarget(float timeProjection)
        {
            Vector3 targetOffset = target.Rigidbody.velocity * timeProjection;

            if (m_rigidbody != null) targetOffset -= m_rigidbody.velocity * timeProjection;

            float closestDistance = target.TrackingBounds.ClosestPoint(transform.position + targetOffset).magnitude;

            return closestDistance;
        }


        // Called every frame
        protected virtual void Update()
        {
            if (targetInsideTrigger)
            {
                if (CheckTrigger() && !triggered)
                {
                    onTriggered.Invoke();
                    triggered = true;
                }
            }
        }
    }
}