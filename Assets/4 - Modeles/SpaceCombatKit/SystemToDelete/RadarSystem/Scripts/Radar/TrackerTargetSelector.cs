using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Target selector for targets being tracked by a Tracker component;
    /// </summary>
    public class TrackerTargetSelector : TargetSelector
    {
        [Header("Tracker")]

        [SerializeField]
        protected Tracker tracker;
            
        protected override void Reset()
        {
            base.Reset();
            tracker = GetComponent<Tracker>();
        }

        protected virtual void Awake()
        {
            // Set the starting tracker
            SetTracker(tracker);
        }

        public void SetTracker(Tracker tracker)
        {
            if (this.tracker != null)
            {
                selectedTarget = null;
                trackables = null;
                this.tracker.onStoppedTracking.RemoveListener(OnStoppedTracking);
            }

            this.tracker = tracker;

            // Set the list of targets
            if (this.tracker != null)
            {
                trackables = this.tracker.Targets;
                this.tracker.onStoppedTracking.AddListener(OnStoppedTracking);
            }
        }
    }
}