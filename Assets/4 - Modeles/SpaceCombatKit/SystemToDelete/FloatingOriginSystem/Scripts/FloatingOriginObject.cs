using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.FloatingOriginSystem
{
 
    /// <summary>
    /// Add this to any object that needs to be shifted when the floating origin shifts.
    /// </summary>
    public class FloatingOriginObject : MonoBehaviour
    {
        [Tooltip("Whether the trail renderers in the hierarchy of this object will be managed during a floating origin shift to prevent stretching. Make sure all trail renderers are enabled on the object.")]
        [SerializeField]
        protected bool manageTrailRenderers = true;

        protected List<TrailRenderer> trailRenderers = new List<TrailRenderer>();   // Stored trail renderers found in hierarchy
        protected List<List<Vector3>> trailPositions = new List<List<Vector3>>();   // Trail positions stored before an origin shift

        // Called before an origin shift to e.g. store state values
        public UnityEvent onPreOriginShift;

        // Called after an origin shift to e.g. implement state values
        public UnityEvent onPostOriginShift;

        // The stored world position of this object
        protected Vector3 storedPosition;

        protected bool activated = true;


        // Get the floating position of this object.
        public Vector3 FloatingPosition
        {
            get { return (transform.position - FloatingOriginManager.Instance.transform.position); }
        }

        // Use this for initialization
        void Awake()
        {
            // Get all the trail renderers in the hierarchy
            trailRenderers = new List<TrailRenderer>(transform.GetComponentsInChildren<TrailRenderer>());
            foreach(TrailRenderer trailRenderer in trailRenderers)
            {
                trailPositions.Add(new List<Vector3>());
            }

            FloatingOriginObject[] childObjects = transform.GetComponentsInChildren<FloatingOriginObject>();
            foreach (FloatingOriginObject childObject in childObjects)
            {
                if (childObject.transform != transform)
                {
                    childObject.SetActivation(false);
                }
            }
        }

        protected virtual void OnEnable()
        {
            Register();
        }

        protected virtual void OnDisable()
        {
            Deregister();
        }


        public void SetActivation(bool setActivated)
        {
            activated = setActivated;
        }


        public void Register()
        {
            if (FloatingOriginManager.Instance != null)
            {
                // Register this floating origin object
                FloatingOriginManager.Instance.Register(this);
            }
        }

        public void Deregister()
        {
            if (FloatingOriginManager.Instance != null)
            {
                // Register this floating origin object
                FloatingOriginManager.Instance.Deregister(this);
            }
        }

        /// <summary>
        /// Called before the floating origin shifts.
        /// </summary>
        public virtual void OnPreOriginShift()
        {
            if (!activated) return;

            // Store this object's position
            storedPosition = transform.position;

            // Store the local trail positions
            for (int i = 0; i < trailRenderers.Count; ++i)
            {
                trailPositions[i].Clear();
                for (int j = 0; j < trailRenderers[i].positionCount; ++j)
                {
                    trailPositions[i].Add(trailRenderers[i].transform.InverseTransformPoint(trailRenderers[i].GetPosition(j)));
                }
            }

            // Call the event
            onPreOriginShift.Invoke();

        }

        /// <summary>
        /// Called after the floating origin shifts.
        /// </summary>
        public virtual void OnPostOriginShift(Vector3 offset)
        {

            if (!activated) return;

            // Implement this object's position
            transform.position = storedPosition + offset;

            // Implement the stored trail positions
            for (int i = 0; i < trailRenderers.Count; ++i)
            {
                for (int j = 0; j < trailPositions[i].Count; ++j)
                {
                    trailPositions[i][j] = trailRenderers[i].transform.TransformPoint(trailPositions[i][j]);
                }

                trailRenderers[i].SetPositions(trailPositions[i].ToArray());

            }

            // Call the event
            onPostOriginShift.Invoke();

        }
    }
}