using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{
    public class RadarAudioController : MonoBehaviour
    {

        [Header("Component References")]

        [SerializeField]
        protected List<Tracker> trackers = new List<Tracker>();

        [Header("Hostile Alarm")]

        [SerializeField]
        protected List<Team> hostileTeams = new List<Team>();

        [SerializeField]
        protected AudioSource hostileTeamDetectedAudio;

        [SerializeField]
        protected float hostileAlarmDelay = 0.25f;

        protected int numHostilesTracked = 0;


        private void Awake()
        {
            for(int i = 0; i < trackers.Count; ++i)
            {
                trackers[i].onStartedTracking.AddListener(OnStartedTrackingTarget);
                trackers[i].onStoppedTracking.AddListener(OnStoppedTrackingTarget);
            }
        }

        /// <summary>
        /// Called when a new target is tracked.
        /// </summary>
        /// <param name="newTarget">The new target.</param>
        public virtual void OnStartedTrackingTarget(Trackable target)
        {
            if (target == null) return;

            if (hostileTeams.IndexOf(target.Team) != -1)
            {
                // If a hostile is not currently detected, raise the alarm
                if (numHostilesTracked == 0)
                {
                    if (hostileTeamDetectedAudio != null && hostileTeamDetectedAudio.gameObject.activeInHierarchy)
                    {
                        hostileTeamDetectedAudio.PlayDelayed(hostileAlarmDelay);
                    }
                }

                numHostilesTracked += 1;
            }
        }


        /// <summary>
        /// Called when a target stops being tracked.
        /// </summary>
        /// <param name="newTarget">The untracked target</param>
        public void OnStoppedTrackingTarget(Trackable target)
        {

            if (target == null) return;

            // If the untracked target is hostile, reduce the count of hostiles being tracked
            if (target.Team != null && hostileTeams.Contains(target.Team))
            {
                numHostilesTracked -= 1;
            }
        }
    }
}
