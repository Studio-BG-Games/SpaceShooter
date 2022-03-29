using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Stores all the trackables in the scene for easy access by Tracker components.
    /// </summary>
    public class TrackableSceneManager : MonoBehaviour
    {
        
        // A list of all the trackables in the scene
        List<Trackable> trackables = new List<Trackable>();
        public List<Trackable> Trackables { get { return trackables; } }

        // Keep a running tab of the next ID to assign to a trackable
        int nextID = 0;

        // Trackable registered event 
        public TrackableEventHandler onTrackableRegistered;

        // Trackable unregistered event 
        public TrackableEventHandler onTrackableUnregistered;

        // Singleton reference
        public static TrackableSceneManager Instance;



        private void Awake()
        {
            // Enforce the singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Register a trackable in the scene.
        /// </summary>
        /// <param name="trackable">The trackable.</param>
        public void Register(Trackable trackable)
        {

            if (trackables.Count == 0)
            {
                trackables.Add(trackable);
            }
            else
            {
                for (int i = 0; i < trackables.Count; ++i)
                {
                    if (trackables[i].RegistrationOrder >= trackable.RegistrationOrder)
                    {
                        trackables.Insert(i, trackable);
                        break;
                    }
                    else
                    {
                        if (i == trackables.Count - 1)
                        {
                            trackables.Add(trackable);
                            break;
                        }
                    }
                }
            }

            trackable.SetTrackableID(nextID);
            nextID += 1;

            onTrackableRegistered.Invoke(trackable);
        }


        /// <summary>
        /// Called to unregister a trackable in the scene.
        /// </summary>
        /// <param name="trackable">The trackable to be unregistered.</param>
        public void Unregister(Trackable trackable)
        {
            trackables.Remove(trackable);
            onTrackableUnregistered.Invoke(trackable);
        }


        /// <summary>
        /// Get a trackable using its ID.
        /// </summary>
        /// <param name="trackableID">The trackable's ID.</param>
        /// <returns>The trackable with the specified ID.</returns>
        public Trackable GetTrackableByID(int trackableID)
        {
            for (int i = 0; i < trackables.Count; ++i)
            {
                if (trackables[i].TrackableID == trackableID)
                {
                    return trackables[i];
                }
            }

            return null;
        }

        
        /// <summary>
        /// Get all trackables that can be tracked by a specified Tracker component.
        /// </summary>
        /// <param name="tracker">The tracker component.</param>
        public void GetTrackables(Tracker tracker)
        {

            // Reference to the last used index in the list to update, for use when trimming excess off the end
            int usedIndex = -1;

            for (int i = 0; i < trackables.Count; ++i)
            {
                
                if (trackables[i].Equals(null)) continue;

                if (!trackables[i].gameObject.activeSelf) continue;

                if (!trackables[i].gameObject.activeInHierarchy) continue;

                if (!trackables[i].Activated) continue;
                
                if (tracker.IsTrackable(trackables[i]))
                {
                    
                    usedIndex += 1;

                    if (usedIndex >= tracker.Targets.Count)
                    {
                        tracker.Targets.Add(trackables[i]);

                    }
                    else
                    {
                        tracker.Targets[usedIndex] = trackables[i];
                    }
                }
            }

            // Remove excess references
            if (tracker.Targets.Count > usedIndex + 1)
            {
                int removeAmount = tracker.Targets.Count - (usedIndex + 1);
                tracker.Targets.RemoveRange(usedIndex + 1, removeAmount);
            }
        }
    }
}
