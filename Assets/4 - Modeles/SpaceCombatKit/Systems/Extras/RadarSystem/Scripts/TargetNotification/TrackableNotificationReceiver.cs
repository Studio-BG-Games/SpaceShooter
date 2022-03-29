using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// This component enables a target to receive notifications about who is tracking it. Add this component to the same gameobject 
    /// as the Trackable component on the target, and use the events to drive actions.
    /// 
    /// Note that this requires the tracker (the one tracking this target) to be using a TrackerNotificationEmitter component.
    /// </summary>
    public class TrackableNotificationReceiver : MonoBehaviour
    {
        [Tooltip("The main Trackable component for this target.")]
        [SerializeField]
        protected Trackable trackable;


        public UnityEvent onPlayerStartedTracking;  // Called when the player starts tracking the referenced trackable

        public UnityEvent onPlayerStoppedTracking;  // Called when the player stops tracking the referenced trackable


        public UnityEvent onEnemyStartedTracking;   // Called when a member of an enemy team starts tracking the referenced trackable

        public UnityEvent onEnemyStoppedTracking;   // Called when a member of an enemy team stops tracking the referenced trackable


        /// <summary>
        /// Called when a game agent starts tracking this target.
        /// </summary>
        /// <param name="gameAgent">The game agent tracking this target.</param>
        public virtual void OnStartedTracking(GameAgent gameAgent)
        {
            if (gameAgent != null)
            {
                // Player tracking events
                if (gameAgent.IsPlayer)
                {
                    onPlayerStartedTracking.Invoke();
                }

                // Enemy tracking events
                if (trackable.Team != null)
                {
                    for(int i = 0; i < trackable.Team.HostileTeams.Count; ++i)
                    {
                        if (trackable.Team.HostileTeams[i] == gameAgent.Team)
                        {
                            onEnemyStartedTracking.Invoke();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when a game agent stops tracking this target.
        /// </summary>
        /// <param name="gameAgent">The game agent that just stopped tracking this target.</param>
        public virtual void OnStoppedTracking(GameAgent gameAgent)
        {
            if (gameAgent != null)
            {
                // Player tracking events
                if (gameAgent.IsPlayer)
                {
                    onPlayerStoppedTracking.Invoke();
                }

                // Enemy tracking events
                if (trackable.Team != null)
                {
                    for (int i = 0; i < trackable.Team.HostileTeams.Count; ++i)
                    {
                        if (trackable.Team.HostileTeams[i] == gameAgent.Team)
                        {
                            onEnemyStoppedTracking.Invoke();
                            break;
                        }
                    }
                }
            }
        }
    }
}

