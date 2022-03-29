using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using UnityEditor;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Tracks targets in the scene according to specified tracking parameters.
    /// </summary>
    public class Tracker : ModuleManager, IHUDCameraUser
    {

        [SerializeField]
        protected bool assignToModuleTargetSelectors = true;

        [Header("Tracking Parameters")]
        [Tooltip("The reference transform for tracking targets.")]
        [SerializeField]
        protected Transform referenceTransform;
        public Transform ReferenceTransform { get { return referenceTransform; } }

        [Tooltip("The tracking range.")]
        [SerializeField]
        protected float range = 5000;
        public float Range { get { return range; } }

        [Tooltip("Whether this tracker will only track specified teams. Leaving this unchecked means all teams will be tracked.")]
        [SerializeField]
        protected bool specifyTrackableTeams = false;

        [Tooltip("The teams that this tracker can track.")]
        [SerializeField]
        protected List<Team> trackableTeams = new List<Team>();

        [Tooltip("Whether this tracker will only track specified trackable types. Leaving this unchecked means all trackable types will be tracked.")]
        [SerializeField]
        protected bool specifyTrackableTypes = false;

        // The trackable types that this tracker can track
        [Tooltip("The types of trackable that this tracker can track.")]
        [SerializeField]
        protected List<TrackableType> trackableTypes = new List<TrackableType>();

        [Tooltip("Whether to update targets every frame. Can be left unchecked for manually updating for performance reasons.")]
        [SerializeField]
        protected bool updateTargetsEveryFrame = true;

        [SerializeField]
        protected bool trackOnScreenTargetsOnly = false;

        [SerializeField]
        protected Camera m_HUDCamera;
        public Camera HUDCamera { set { m_HUDCamera = value; } }

        [Tooltip("The root transform of the tracker. Used to prevent tracking of self.")]
        [SerializeField]
        protected Transform rootTransform;
        public Transform RootTransform
        {
            get { return rootTransform; }
            set { rootTransform = value; }
        }

        // A tracking state for determining when a trackable starts being tracked or stops being tracked by this tracker.
        protected enum TrackingState
        {
            Tracked,
            WasTracked,
            NotTracked
        }

        // The trackables currently being tracked by this tracker
        protected List<Trackable> targets = new List<Trackable>();
        public List<Trackable> Targets { get { return targets; } }

        // A list of tracking states for all the trackables in the scene (index corresponds to trackable ID)
        protected List<TrackingState> trackingStatesByID = new List<TrackingState>();


        [Header("Line Of Sight")]

        [SerializeField]
        protected bool lineOfSight = false;

        [SerializeField]
        protected LayerMask lineOfSightMask = ~4;

        [SerializeField]
        protected Transform lineOfSightRefTransform;
        public Transform LineOfSightRefTransform
        {
            get { return lineOfSightRefTransform; }
            set { lineOfSightRefTransform = value; }
        }

        protected Rigidbody m_Rigidbody;


        [Header("Stored Component References")]

        [Tooltip("Whether component references (e.g. target selectors, target lockers, target leaders) in the hierarchy will be stored here.")]
        [SerializeField]
        protected bool getComponentReferencesInHierarchy = true;

        // Target selectors

        protected List<TargetSelector> targetSelectors = new List<TargetSelector>();
        public List<TargetSelector> TargetSelectors { get { return targetSelectors; } }

        // Target lockers

        protected List<TargetLocker> targetLockers = new List<TargetLocker>();
        public List<TargetLocker> TargetLockers { get { return targetLockers; } }


        [Header("Events")]

        // Trackables updated event
        [HideInInspector]
        public UnityEvent onTrackablesListUpdated;

        // Started tracking trackable event
        [HideInInspector]
        public TrackableEventHandler onStartedTracking;

        // Stopped tracking trackable event
        [HideInInspector]
        public TrackableEventHandler onStoppedTracking;


        [HideInInspector]
        public TargetSelectorEventHandler onTargetSelectorAdded;

        [HideInInspector]
        public TargetSelectorEventHandler onTargetSelectorRemoved;


        [HideInInspector]
        public TargetLockerEventHandler onTargetLockerAdded;

        [HideInInspector]
        public TargetLockerEventHandler onTargetLockerRemoved;


        // Called when the component is first added to a gameobject or reste in the inspector
        protected virtual void Reset()
        {

            this.referenceTransform = transform;

            this.rootTransform = transform.root;

            lineOfSightRefTransform = transform;

        }


        protected override void Awake()
        {

            base.Awake();

            m_Rigidbody = GetComponent<Rigidbody>();

            if (TrackableSceneManager.Instance == null)
            {
                Debug.LogWarning("No TrackableSceneManager component found in scene, please add one to enable this radar to track targets.");
            }
            else
            {
                for (int i = 0; i < TrackableSceneManager.Instance.Trackables.Count; ++i)
                {
                    OnTrackableRegistered(TrackableSceneManager.Instance.Trackables[i]);
                }

                // Listen for new trackables being registered in the scene
                TrackableSceneManager.Instance.onTrackableRegistered.AddListener(OnTrackableRegistered);

                // Listen for trackables being unregistered in the scene
                TrackableSceneManager.Instance.onTrackableUnregistered.AddListener(OnTrackableUnregistered);
            }
        }


        protected override void Start()
        {
            base.Start();

            // Store target selectors in hierarchy
            if (getComponentReferencesInHierarchy)
            {
                TargetSelector[] targetSelectors = transform.GetComponentsInChildren<TargetSelector>();
                foreach (TargetSelector targetSelector in targetSelectors)
                {
                    AddTargetSelector(targetSelector);
                }
            }

            // Store target lockers in hierarchy
            if (getComponentReferencesInHierarchy)
            {
                TargetLocker[] targetLockers = transform.GetComponentsInChildren<TargetLocker>();
                foreach (TargetLocker targetLocker in targetLockers)
                {
                    AddTargetLocker(targetLocker);
                }
            }
        }


        // Called when a trackable is registered in the scene
        protected virtual void OnTrackableRegistered(Trackable trackable)
        {
            // Keep track of its tracking state
            trackingStatesByID.Add(TrackingState.NotTracked);
        }


        // Called when a trackable is destroyed in the scene
        protected virtual void OnTrackableUnregistered(Trackable trackable)
        {
            if (trackingStatesByID[trackable.TrackableID] != TrackingState.NotTracked)
            {
                int index = Targets.IndexOf(trackable);
                if (index != -1)
                {
                    Targets.RemoveAt(index);
                    OnStoppedTracking(trackable);
                }
            }
        }


        // Called to check if a target is trackable by this radar
        public virtual bool IsTrackable(Trackable target)
        {
            // Check distance
            if (!target.IgnoreTrackingDistance && Vector3.Distance(target.transform.position, transform.position) > range) return false;

            // Check if it's trying to track itself
            if (this.rootTransform != null && target.RootTransform == this.rootTransform)
            {
                return false;
            }

            // Check if on screen
            if (trackOnScreenTargetsOnly && m_HUDCamera != null)
            {
                Vector3 viewportPos = m_HUDCamera.WorldToViewportPoint(target.transform.position);
                bool onScreen = (viewportPos.z > 0 &&
                                (viewportPos.x > 0 && viewportPos.x < 1) &&
                                (viewportPos.y > 0 && viewportPos.y < 1));

                if (!onScreen) return false;
            }

            // Check if this team can be tracked
            if (specifyTrackableTeams)
            {
                bool found = false;
                for (int i = 0; i < trackableTeams.Count; ++i)
                {
                    if (trackableTeams[i] == target.Team)
                    {
                        found = true;
                    }
                }
                if (!found) return false;
            }

            // Check if this type can be tracked
            if (specifyTrackableTypes)
            {
                // Check if this target can select the type
                bool found = false;
                for (int i = 0; i < trackableTypes.Count; ++i)
                {
                    if (trackableTypes[i] == target.TrackableType)
                    {
                        found = true;
                    }
                }
                if (!found) return false;
            }
            
            // Check line of sight
            if (lineOfSight)
            {
                Vector3 targetCenter = target.transform.TransformPoint(target.TrackingBounds.center);

                RaycastHit[] hits = Physics.RaycastAll(lineOfSightRefTransform.position, (targetCenter - lineOfSightRefTransform.position).normalized, range, lineOfSightMask);
                System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
                for (int i = 0; i < hits.Length; ++i)
                {
                    if (hits[i].collider.attachedRigidbody != null)
                    {
                        if (hits[i].collider.attachedRigidbody.transform == target.RootTransform)
                        {
                            return true;
                        }

                        if (hits[i].collider.attachedRigidbody == m_Rigidbody)
                        {
                            continue;
                        }

                        return false;

                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;

        }


        // Called to update the list of tracked targets
        protected virtual void UpdateTrackedTargetsList()
        {
            // Update targets list
            if (TrackableSceneManager.Instance != null) TrackableSceneManager.Instance.GetTrackables(this);

            // Look for targets that weren't being tracked before
            for (int i = 0; i < targets.Count; ++i)
            {

                if (trackingStatesByID[targets[i].TrackableID] == TrackingState.NotTracked)
                {
                    // Call the function to do something with the newly tracked target
                    OnStartedTracking(targets[i]);
                }

                // Update the tracking state
                trackingStatesByID[targets[i].TrackableID] = TrackingState.Tracked;

            }
            
            // Look for targets that aren't being tracked any more
            for (int i = 0; i < trackingStatesByID.Count; ++i)
            {
                if (trackingStatesByID[i] == TrackingState.WasTracked)
                {
                    // Call the function to do something with the target that isn't being tracked anymore
                    OnStoppedTracking(TrackableSceneManager.Instance.GetTrackableByID(i));
                    trackingStatesByID[i] = TrackingState.NotTracked;
                }
                // Push the state of newly tracked trackables into past tense
                else if (trackingStatesByID[i] == TrackingState.Tracked)
                {
                    trackingStatesByID[i] = TrackingState.WasTracked;
                }
            }
            
            onTrackablesListUpdated.Invoke();
        }

        // Called when this tracker first starts tracking a target
        protected virtual void OnStartedTracking(Trackable trackable)
        {
            onStartedTracking.Invoke(trackable);
        }

        // Called when this tracker stops tracking a target
        protected virtual void OnStoppedTracking(Trackable trackable)
        {
            onStoppedTracking.Invoke(trackable);
        }

        // Called every frame
        protected virtual void Update()
        {

            if (updateTargetsEveryFrame)
            {
                UpdateTrackedTargetsList();
            }
        }


        public virtual void AddTargetSelector(TargetSelector targetSelector)
        {
            if (targetSelector != null)
            {
                if (targetSelectors.IndexOf(targetSelector) == -1)
                {
                    targetSelectors.Add(targetSelector);
                    onTargetSelectorAdded.Invoke(targetSelector);
                }
            }
        }

        public virtual void RemoveTargetSelector(TargetSelector targetSelector)
        {
            if (targetSelector != null)
            {
                if (targetSelectors.IndexOf(targetSelector) != -1)
                {
                    targetSelectors.RemoveAt(targetSelectors.IndexOf(targetSelector));
                    onTargetSelectorRemoved.Invoke(targetSelector);
                }
            }
        }

        public virtual void AddTargetLocker(TargetLocker targetLocker)
        {
            if (targetLocker != null)
            {
                if (targetLockers.IndexOf(targetLocker) == -1)
                {
                    targetLockers.Add(targetLocker);
                    onTargetLockerAdded.Invoke(targetLocker);
                }
            }
        }

        public virtual void RemoveTargetLocker(TargetLocker targetLocker)
        {
            if (targetLocker != null)
            {
                if (targetLockers.IndexOf(targetLocker) != -1)
                {
                    targetLockers.RemoveAt(targetLockers.IndexOf(targetLocker));
                    onTargetLockerRemoved.Invoke(targetLocker);
                }
            }
        }

        protected override void OnModuleMounted(Module module)
        {
            base.OnModuleMounted(module);

            // Store target selector
            AddTargetSelector(module.GetComponent<TargetSelector>());

            // Store target locker
            AddTargetLocker(module.GetComponent<TargetLocker>());

            if (assignToModuleTargetSelectors)
            {
                TrackerTargetSelector[] targetSelectors = module.GetComponentsInChildren<TrackerTargetSelector>();
                for (int i = 0; i < targetSelectors.Length; ++i)
                {
                    targetSelectors[i].SetTracker(this);
                }
            }
        }

        protected override void OnModuleUnmounted(Module module)
        {
            base.OnModuleUnmounted(module);


            // Remove target selector
            RemoveTargetSelector(module.GetComponent<TargetSelector>());

            // Remove target locker
            RemoveTargetLocker(module.GetComponent<TargetLocker>());


            if (assignToModuleTargetSelectors)
            {
                TrackerTargetSelector[] targetSelectors = module.GetComponentsInChildren<TrackerTargetSelector>();
                for (int i = 0; i < targetSelectors.Length; ++i)
                {
                    targetSelectors[i].SetTracker(null);
                }
            }
        }
    }
}
