using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Component for making an object trackable by radar.
    /// </summary>
    public class Trackable : MonoBehaviour
    {

        [Header("General")]

        [SerializeField]
        protected string label = "Target";
        public string Label
        {
            get { return label; }
            set { label = value; }
        }

        [SerializeField]
        protected TrackableType trackableType;
        public TrackableType TrackableType { get { return trackableType; } }

        [SerializeField]
        protected Team team;
        public Team Team 
        {
            get { return team; }
            set
            {
                team = value;
                for(int i = 0; i < childTrackables.Count; ++i)
                {
                    childTrackables[i].Team = value;
                }
            }
        }

        [Tooltip("The bounds that define this object for tracking purposes. These bounds define how HUD target boxes conform to the object's shape.")]
        [SerializeField]
        protected Bounds trackingBounds = new Bounds(Vector3.zero, new Vector3(10, 10, 10));
        public Bounds TrackingBounds { get { return trackingBounds; } }

        [SerializeField]
        protected Rigidbody m_Rigidbody;
        public Rigidbody Rigidbody
        {
            get { return m_Rigidbody; }
        }

        [Header("Settings")]

        [Tooltip("Whether this is a root trackable (not a child of another trackable).")]
        [SerializeField]
        protected bool isRootTrackable = true;

        [Tooltip("Whether this trackable is activated (can be tracked).")]
        [SerializeField]
        protected bool activated = true;
        public bool Activated { get { return activated; } }

        [Tooltip("Whether trackers should be able to track this target regardless of that tracker's range, e.g. waypoints.")]
        [SerializeField]
        protected bool ignoreTrackingDistance = false;
        public bool IgnoreTrackingDistance
        {
            get { return ignoreTrackingDistance; }
            set { ignoreTrackingDistance = value; }
        }

        [Tooltip("The registration order for the TrackablesSceneManager component. Enables control of the order for next/previous target cycling.")]
        [SerializeField]
        protected int registrationOrder = 0;
        public int RegistrationOrder { get { return registrationOrder; } }

        protected int trackableID = -1;
        public int TrackableID { get { return trackableID; } }

        protected int depth = 0;
        public int Depth { get { return depth; } }

        [Tooltip("The root transform of this trackable. Used for quickly accessing important scripts, preventing self-tracking and preventing self-hits with own weapons.")]
        [SerializeField]
        protected Transform rootTransform;
        public Transform RootTransform { get { return rootTransform; } }

        protected Trackable parentTrackable = null;
        public Trackable ParentTrackable { get { return parentTrackable; } }

        [Tooltip("The child trackables of this trackable, e.g. subsystems on a capital ship. Used for quickly searching the target hierarchy.")]
        [SerializeField]
        protected List<Trackable> childTrackables = new List<Trackable>();
        public List<Trackable> ChildTrackables { get { return childTrackables; } }

        [Header("Variables")]

        [Tooltip("All the information that must be available to trackers that are tracking this target.")]
        [SerializeField]
        protected List<LinkableVariable> variables = new List<LinkableVariable>();

        public Dictionary<string, LinkableVariable> variablesDictionary = new Dictionary<string, LinkableVariable>();

        [Header("Events")]

        // Called when this trackable is selected by a target selector, e.g. for highlighting
        public UnityEvent onSelected;

        // Called when this trackable is unselected by a target selector
        public UnityEvent onUnselected;

        public virtual Vector3 Velocity
        {
            get
            {
                return m_Rigidbody != null ? m_Rigidbody.velocity : Vector3.zero;
            }
        }


        // Called when something is changed in the inspector
        protected virtual void OnValidate()
        {           
            // Check that the list of variables is in the right order
            bool reorder = false;
            for (int i = 0; i < variables.Count; ++i)
            {
                if (i > 0)
                {
                    if (variables[i].listIndex < variables[i - 1].listIndex)
                    {
                        reorder = true;
                    }
                }
            }

            // Reorder the list of variables
            if (reorder)
            {
                List<LinkableVariable> reorderedVariables = new List<LinkableVariable>();
                for (int i = 0; i < variables.Count; ++i)
                {
                    if (i == 0)
                    {
                        reorderedVariables.Add(variables[i]);
                        continue;
                    }
                    for (int j = 0; j < reorderedVariables.Count; ++j)
                    {
                        if (reorderedVariables[j].listIndex > variables[i].listIndex)
                        {
                            reorderedVariables.Insert(j, variables[i]);
                            break;
                        }
                        else
                        {
                            if (j == reorderedVariables.Count - 1)
                            {
                                reorderedVariables.Add(variables[i]);
                                break;
                            }
                        }
                    }
                }
                variables = reorderedVariables;
            }
        }


        /// <summary>
        /// Called when the component is first added to a gameobject, or reset in the inspector.
        /// </summary>
        protected virtual void Reset()
        {
            // Get the rigidbody
            m_Rigidbody = transform.GetComponent<Rigidbody>();
            if (m_Rigidbody == null)
            {
                m_Rigidbody = transform.root.GetComponent<Rigidbody>();
            }

            Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                trackingBounds = renderers[0].bounds;
                for (int i = 0; i < renderers.Length; ++i)
                {
                    trackingBounds.Encapsulate(renderers[i].bounds);
                }

                trackingBounds.center -= transform.position;
            }

            rootTransform = transform;
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


        protected virtual void Awake()
        {

            if (TrackableSceneManager.Instance == null)
            {
                Debug.LogWarning("No TrackableSceneManager component found in scene, please add one to enable this target to be tracked.");
            }

            // Update the variables dictionary
            for (int i = 0; i < variables.Count; ++i)
            {
                variables[i].InitializeLinkDelegate();
                variablesDictionary[variables[i].Key] = variables[i];
            }
        }

        // Called when scene starts
        protected virtual void Start()
        {
            // Update child trackables
            for (int i = 0; i < childTrackables.Count; ++i)
            {
                childTrackables[i].SetParentTrackable(this);
            }

            if (isRootTrackable)
            {
                SetDepth(0);
                Register();
            }
        }

        /// <summary>
        /// Set the parent trackable for this trackable.
        /// </summary>
        /// <param name="parentTrackable">The parent trackable.</param>
        public void SetParentTrackable(Trackable parentTrackable)
        {
            this.parentTrackable = parentTrackable;
            this.team = parentTrackable.Team;
            for(int i = 0; i < childTrackables.Count; ++i)
            {
                childTrackables[i].SetParentTrackable(this);
            }
        }

        /// <summary>
        /// Set the root transform for this trackable.
        /// </summary>
        /// <param name="newRootTransform">The new root transform.</param>
        public void SetRootTransform(Transform newRootTransform)
        {
            this.rootTransform = newRootTransform;
            for (int i = 0; i < childTrackables.Count; ++i)
            {
                childTrackables[i].SetRootTransform(newRootTransform);
            }
        }

        /// <summary>
        /// Add a child trackable to this trackable (e.g. a module that can be tracked as a subsystem).
        /// </summary>
        /// <param name="childTrackable">The new child trackable.</param>
        public void AddChildTrackable(Trackable childTrackable)
        {
            childTrackables.Add(childTrackable);
            childTrackable.SetRootTransform(this.rootTransform);
            childTrackable.SetParentTrackable(this);
            childTrackable.SetDepth(this.depth + 1);
        }

        /// <summary>
        /// Remove a child trackable, e.g. when a module is unmounted that was trackable.
        /// </summary>
        /// <param name="childTrackable">The child trackable to be removed.</param>
        public void RemoveChildTrackable(Trackable childTrackable)
        {
            childTrackables.Remove(childTrackable);
            childTrackable.SetParentTrackable(null);
            childTrackable.SetDepth(0);
        }


        // Register this trackable with the TrackableSceneManager
        protected void Register()
        {
            if (TrackableSceneManager.Instance != null) TrackableSceneManager.Instance.Register(this);

            // Register children as well
            for (int i = 0; i < childTrackables.Count; ++i)
            {
                childTrackables[i].Register();
            }
        }


        /// <summary>
        /// Set the hierarchy depth for this trackable.
        /// </summary>
        /// <param name="newDepth">New depth value.</param>
        public void SetDepth(int newDepth)
        {
            depth = newDepth;

            // Update children
            for (int i = 0; i < childTrackables.Count; ++i)
            {
                childTrackables[i].SetDepth(depth + 1);
            }
        }


        /// <summary>
        /// Set whether this trackable is activated (can be tracked).
        /// </summary>
        /// <param name="activated">Activated or not.</param>
        public virtual void SetActivation(bool activated)
        {
            this.activated = activated;
        }


        // Show the bounding box visually in the editor scene view
        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(trackingBounds.center), transform.rotation, transform.lossyScale);
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.zero, trackingBounds.size);
        }


        /// <summary>
        /// Set the trackable ID for this trackable.
        /// </summary>
        /// <param name="id">The new trackable ID.</param>
        public void SetTrackableID(int id)
        {
            this.trackableID = id;
        }


        /// <summary>
        /// Called when this target is selected by the player.
        /// </summary>
        public void Select()
        {
            onSelected.Invoke();
        }


        /// <summary>
        /// Called when this target is unselected by the player.
        /// </summary>
        public void Unselect()
        {
            onUnselected.Invoke();
        }
    }
}
