using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.CameraSystem;

namespace VSX.FloatingOriginSystem
{
    /// <summary>
    /// This class provides a floating origin for managing large-scale distances. When the focused transform (e.g. player) gets too far from the scene center,
    /// the error in floating point position calculations increases and things start to jitter and shake. With the Floating Origin Manager, when the focused transform gets a 
    /// specified distance from the origin, it is returned to the center and any object that holds a Floating Origin Object component is then
    /// shifted to maintain the same relative position to the focused transform.
    /// </summary>
    public class FloatingOriginManager : MonoBehaviour
    {
        // Flag for if the floating origin is activated
        protected bool activated = true;
        public bool Activated
        {
            get { return activated; }
            set { activated = value; }
        }

        [Tooltip("A reference to the object that the floating origin keeps near the center of the scene.")]
        [SerializeField]
        protected Transform focusedTransform;
        public Transform FocusedTransform
        {
            get { return focusedTransform; }
            set { focusedTransform = value; }
        }

        [SerializeField]
        protected bool useFirstCameraEntityInScene = true;

        [Tooltip("The maximum distance the focused transform (e.g. player) can reach from the center before the origin is shifted.")]
        [SerializeField]
        protected float maxDistanceFromCenter = 1000;

        // A list of all the floating origin objects in the scene.
        protected List<FloatingOriginObject> floatingOriginObjects = new List<FloatingOriginObject>();

        // The current position of the floating origin
        protected Vector3 floatingOriginPosition = Vector3.zero;
        public Vector3 FloatingOriginPosition { get { return floatingOriginPosition; } }

        public Vector3 FocusedTransformFloatingPosition { get { return focusedTransform.position - floatingOriginPosition; } }

        // Singleton reference to the floating origin manager
        public static FloatingOriginManager Instance;


        protected virtual void Reset()
        {
            FocusedTransform = Camera.main.transform;
        }

        // Called when scene starts
        protected virtual void Awake()
        {
            // Create the singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }


        protected virtual void Start()
        {
            if (focusedTransform == null && useFirstCameraEntityInScene)
            {
                CameraEntity cameraEntity = FindObjectOfType<CameraEntity>();
                if (cameraEntity != null) focusedTransform = cameraEntity.transform;
            }
        }


        /// <summary>
        /// Register a new floating origin object.
        /// </summary>
        /// <param name="floatingOriginObject">The new floating origin object.</param>
        public virtual void Register(FloatingOriginObject floatingOriginObject)
        {
            // Add the new floating origin object to the list.
            if (floatingOriginObjects.IndexOf(floatingOriginObject) == -1)
            {
                floatingOriginObjects.Add(floatingOriginObject);
            }
        }


        /// <summary>
        /// Deregister a floating origin object to prevent it being shifted.
        /// </summary>
        /// <param name="floatingOriginObject">The floating origin object to deregister.</param>
        public virtual void Deregister(FloatingOriginObject floatingOriginObject)
        {
            floatingOriginObjects.Remove(floatingOriginObject);
        }


        /// <summary>
        /// Shift the floating origin.
        /// </summary>
        protected virtual void ShiftScene()
        {
            // Call the pre-shift event on scene origin object so they can prepare anything that needs preparing
            for (int i = 0; i < floatingOriginObjects.Count; ++i)
            {
                floatingOriginObjects[i].OnPreOriginShift();
            }

            // Store the offset
            Vector3 focusedTransformOffset = focusedTransform.position;

            // Update the floating origin position
            floatingOriginPosition -= focusedTransformOffset;

            // Put the focused transform in the center
            focusedTransform.position = Vector3.zero;

            // Call the post-shift event on scene origin objects
            for (int i = 0; i < floatingOriginObjects.Count; ++i)
            {
                floatingOriginObjects[i].OnPostOriginShift(-focusedTransformOffset);
            }
        }


        // Called every frame
        protected virtual void Update()
        {
            if (!activated || focusedTransform == null) return;

            // If focused transform is too far from center, shift the scene and place it at (0,0,0)
            if (focusedTransform.position.magnitude > maxDistanceFromCenter)
            {
                ShiftScene();
            }
        }
    }
}