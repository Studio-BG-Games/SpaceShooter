using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// This component enables a Tracker to notify a target when that target starts or stops being tracked.
    /// </summary>
    public class TrackerNotificationEmitter : MonoBehaviour
    {
        [Tooltip("The vehicle that is tracking targets.")]
        [SerializeField]
        protected Vehicle vehicle;

        [Tooltip("The tracker that is tracking the targets.")]
        [SerializeField]
        protected Tracker tracker;


        protected virtual void Awake()
        {
            // Subscribe to Tracker events
            tracker.onStartedTracking.AddListener(OnStartedTracking);
            tracker.onStoppedTracking.AddListener(OnStoppedTracking);
        }

        /// <summary>
        /// Called when a target starts being tracked by the Tracker.
        /// </summary>
        /// <param name="target">The target that started being tracked.</param>
        protected virtual void OnStartedTracking(Trackable target)
        {
            // Get a reference to the notification receiver on the target
            TrackableNotificationReceiver receiver = target.GetComponent<TrackableNotificationReceiver>();
            if (receiver == null) return;

            // Send notifications
            if (vehicle != null && vehicle.Occupants.Count > 0)
            {
                receiver.OnStartedTracking(vehicle.Occupants[0]);
            }
            else
            {
                receiver.OnStartedTracking(null);
            }
        }

        /// <summary>
        /// Called when a target stops being tracked by the Tracker.
        /// </summary>
        /// <param name="target">The target that stopped being tracked.</param>
        protected virtual void OnStoppedTracking(Trackable target)
        {
            // Get a reference to the notification receiver on the target
            TrackableNotificationReceiver receiver = target.GetComponent<TrackableNotificationReceiver>();
            if (receiver == null) return;

            // Send notifications
            if (vehicle != null && vehicle.Occupants.Count > 0)
            {
                receiver.OnStoppedTracking(vehicle.Occupants[0]);
            }
            else
            {
                receiver.OnStoppedTracking(null);
            }
        }
    }
}
