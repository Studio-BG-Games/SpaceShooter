using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using VSX.UniversalVehicleCombat;

namespace VSX.CameraSystem
{
    [System.Serializable]
    public class OnCameraTargetChangedEventHandler : UnityEvent<CameraTarget> { }

    [System.Serializable]
    public class OnCameraViewTargetChangedEventHandler : UnityEvent<CameraViewTarget> { }

    public class CameraEntity : MonoBehaviour
    {

        [Header("General")]

        [Tooltip("The camera target that this camera will follow when the scene starts.")]
        [SerializeField]
        protected CameraTarget startingCameraTarget;
        
        protected CameraTarget cameraTarget;
        public CameraTarget CameraTarget { get { return cameraTarget; } }

        [Tooltip("Reference to the main camera.")]
        [SerializeField]
        protected Camera mainCamera;
        public Camera MainCamera { get { return mainCamera; } }

        [SerializeField]
        protected bool cameraControlEnabled = true;
        public virtual bool CameraControlEnabled
        {
            get { return cameraControlEnabled; }
            set
            {
                cameraControlEnabled = value;
                for (int i = 0; i < cameraControllers.Count; ++i)
                {
                    cameraControllers[i].ControllerEnabled = value;
                }
            }
        }
        
        [Tooltip("Whether the camera will react to obstacles (staying within the line of sight to the focused object). Useful for third person view.")]
        [SerializeField]
        protected bool cameraCollisionEnabled = true;
        public bool CameraCollisionEnabled
        {
            get { return cameraCollisionEnabled; }
            set { cameraCollisionEnabled = value; }
        }

        [SerializeField]
        protected float defaultFieldOfView;
        public float DefaultFieldOfView
        {
            get { return defaultFieldOfView; }
            set { defaultFieldOfView = value; }
        }

        [Tooltip("A list of all the secondary cameras which must conform to this camera's state.")]
        [SerializeField]
        protected List<SecondaryCamera> secondaryCameras = new List<SecondaryCamera>();
        public List<SecondaryCamera> SecondaryCameras { get { return secondaryCameras; } }


        [Header("Default Camera Controller")]

        [Tooltip("The camera view that is selected upon switching to a new camera target.")]
        [SerializeField]
        protected CameraView startingView;

        protected bool controllerOverriding = false;

        protected RaycastHitComparer raycastHitComparer;    // Used to compare raycast distances for camera collision

        protected Coroutine cameraCollisionCoroutine;   

        // List of all the camera controllers in the hierarchy
        protected List<CameraController> cameraControllers = new List<CameraController>();

        protected CameraViewTarget currentViewTarget;
        public CameraViewTarget CurrentViewTarget { get { return currentViewTarget; } }

        protected bool hasCameraViewTarget;
        public bool HasCameraViewTarget { get { return hasCameraViewTarget; } }

        public CameraView CurrentView { get { return hasCameraViewTarget ? currentViewTarget.CameraView : null; } }

        [Header("Events")]
        
        public OnCameraTargetChangedEventHandler onCameraTargetChanged;

        public OnCameraViewTargetChangedEventHandler onCameraViewTargetChanged;



        // Called when the component is first added to a gameobject or the component is reset
        protected virtual void Reset()
        {
            // Look for a camera in the hierarchy
            mainCamera = transform.parent.GetComponentInChildren<Camera>();

            // If none found, look for a camera tagged 'MainCamera'
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            
            // If found, initialize the default field of view.
            if (mainCamera != null)
            {
                defaultFieldOfView = Camera.main.fieldOfView;
            }
        }


        protected virtual void Awake()
		{
            raycastHitComparer = new RaycastHitComparer();  

            // Get all the camera controllers in the hierarchy
            cameraControllers = new List<CameraController>(transform.GetComponentsInChildren<CameraController>());

            CameraControlEnabled = cameraControlEnabled;

            foreach (CameraController cameraController in cameraControllers)
            {
                cameraController.SetCamera(this);
            }

            foreach (SecondaryCamera secondaryCamera in secondaryCameras)
            {
                secondaryCamera.CameraEntity = this;
            }
        }


        // Called at the start
        protected virtual void Start()
        {
            // Start targeting the starting camera target
            if (startingCameraTarget != null)
            {
                SetCameraTarget(startingCameraTarget);
            }
        }

        /// <summary>
        /// Called when this gameobject is activated
        /// </summary>
        protected virtual void OnEnable()
        {
            // Start a new collision coroutine
            cameraCollisionCoroutine = StartCoroutine(CameraCollisionUpdate());
        }

        /// <summary>
        /// Called when this gameobject is deactivated
        /// </summary>
        protected virtual void OnDisable()
        {
            // Stop any collision coroutine that is running
            if (cameraCollisionCoroutine != null)
            {
                StopCoroutine(cameraCollisionCoroutine);
            }
        }

        /// <summary>
        /// Set a new camera target to follow.
        /// </summary>
        /// <param name="target">The new camera target.</param>
        public virtual void SetCameraTarget (CameraTarget target)
		{

            if (target == cameraTarget) return;

            // Clear parent
            transform.SetParent(null);
            
            // Deactivate all the camera controllers
            for (int i = 0; i < cameraControllers.Count; ++i)
            {
                cameraControllers[i].OnCameraTargetChanged(null);
            }

            controllerOverriding = false;

            // Set the following camera to null on previous target
            if (cameraTarget != null)
            {
                cameraTarget.SetCamera(null);
            }

            // Update the camera target reference
            cameraTarget = null;
            if (target != null)
            {
                cameraTarget = target;
                
            }
            
            // Activate the appropriate controller(s)
            if (cameraTarget != null)
            {

                cameraTarget.SetCamera(this);

                // If no camera view targets on camera target, issue a warning
                if (cameraTarget.CameraViewTargets.Count == 0)
                {
                    Debug.LogWarning("No Camera View Target components found in camera target object's hierarchy, please add one or more.");
                }

                // Activate the appropriate camera controller(s)
                int numControllers = 0;
                for (int i = 0; i < cameraControllers.Count; ++i)
                {
                    cameraControllers[i].OnCameraTargetChanged(cameraTarget);

                    if (cameraControllers[i].Initialized)
                    {
                        numControllers++;
                    }
                }

                if (numControllers == 0)
                {
                    // Set the starting view
                    if (startingView != null)
                    {
                        SetView(startingView);
                    }
                    else
                    {
                        if (target.CameraViewTargets.Count > 0)
                        {
                            SetCameraViewTarget(target.CameraViewTargets[0]);
                        }
                        else
                        {
                            SetView(null);
                        }
                    }
                }
                else
                {
                    controllerOverriding = true;
                }

                onCameraTargetChanged.Invoke(cameraTarget);
            }
		}


        /// <summary>
        /// Cycle the camera view forward or backward.
        /// </summary>
        /// <param name="forward">Whether to cycle forward.</param>
        public virtual void CycleCameraView(bool forward)
        {

            // If the camera target has no camera view targets, return.
            if (cameraTarget == null || cameraTarget.CameraViewTargets.Count == 0) return;

            // Get the index of the current camera view target
            int index = cameraTarget.CameraViewTargets.IndexOf(currentViewTarget);
            index += forward ? 1 : -1;

            // Wrap the index between 0 and the number of camera view targets on the camera target.
            if (index >= cameraTarget.CameraViewTargets.Count)
            {
                index = 0;
            }
            else if (index < 0)
            {
                index = cameraTarget.CameraViewTargets.Count - 1;
            }

            // Set the new camera view target
            SetCameraViewTarget(cameraTarget.CameraViewTargets[index]);

        }

        // Set the camera view target that this camera is following
        public virtual void SetCameraViewTarget(CameraViewTarget cameraViewTarget)
        {

            if (cameraViewTarget == null) return;

            // Update the current view target info
            this.currentViewTarget = cameraViewTarget;

            // Update the flag
            hasCameraViewTarget = this.currentViewTarget != null;

            if (cameraViewTarget.ParentCameraOnSelected)
            {
                transform.SetParent(cameraViewTarget.transform);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.SetParent(null);
                transform.position = cameraViewTarget.transform.position;
                transform.rotation = cameraViewTarget.transform.rotation;
            }

            cameraViewTarget.OnSelected();

            onCameraViewTargetChanged.Invoke(cameraViewTarget);
        }

        /// <summary>
        /// Select a new camera view.
        /// </summary>
        /// <param name="newView">The new camera view.</param>
        public virtual void SetView(CameraView newView)
		{

            // If no camera target or null view, set to null and exit.
            if (newView == null || cameraTarget == null)
            {
                SetCameraViewTarget(null);
                return;
            }

            // Search all camera views on camera target for desired view
            for (int i = 0; i < cameraTarget.CameraViewTargets.Count; ++i)
			{
				if (cameraTarget.CameraViewTargets[i].CameraView == newView)
				{
                    SetCameraViewTarget(cameraTarget.CameraViewTargets[i]);
                    return;
				}
			}

            // If none found, default to the first available
            if (cameraTarget.CameraViewTargets.Count > 0)
            {
                // Set the first available Camera View Target
                SetCameraViewTarget(cameraTarget.CameraViewTargets[0]);

                if (newView != null)
                {
                    // Issue a warning
                    Debug.LogWarning("No CameraViewTarget found for Camera View type " + newView.ToString() + ". Defaulting to " +
                        currentViewTarget.CameraView.ToString());
                }
            }
            else
            {
                SetView(null);

                // Issue a warning
                Debug.LogWarning("No Camera View Target found on the camera target object, camera will not work. Please add one or more CameraViewTarget components to the camera target object's hierarchy.");
            }	
		}

        /// <summary>
        /// Set the field of view for the camera.
        /// </summary>
        /// <param name="newFieldOfView">The new field of view.</param>
        public virtual void SetFieldOfView(float newFieldOfView)
        {
            mainCamera.fieldOfView = newFieldOfView;
        }


        /// <summary>
        /// Coroutine for managing camera collisions.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator CameraCollisionUpdate()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();  // Wait until the camera controller fixed update is finished

                if (cameraCollisionEnabled && cameraTarget != null)
                {
                    // Initialize the target position for the camera
                    Vector3 targetPosition = transform.position;

                    // Prepare the spherecast
                    Vector3 sphereCastStart = CameraTarget.LookTarget.position;
                    Vector3 sphereCastEnd = transform.position;
                    Vector3 sphereCastDir = (sphereCastEnd - sphereCastStart).normalized;
                    float dist = (sphereCastEnd - sphereCastStart).magnitude;

                    // Do spherecast
                    RaycastHit[] hits = Physics.SphereCastAll(sphereCastStart, 0.1f, sphereCastDir, dist);

                    // Sort hits by distance
                    System.Array.Sort(hits, raycastHitComparer);

                    for (int i = 0; i < hits.Length; ++i)
                    {
                        // Ignore trigger colliders
                        if (hits[i].collider.isTrigger) continue;

                        // Ignore hits with the camera target
                        Rigidbody hitRigidbody = hits[i].collider.attachedRigidbody;
                        if (hitRigidbody != null && CameraTarget != null && hitRigidbody.transform == CameraTarget.transform) continue;

                        // Ignore hits with camera target (necessary since Character Controller collider doesn't use attachedRigidbody)
                        if (hits[i].collider.transform == cameraTarget.transform) continue;

                        // Set the camera position to the hit point
                        targetPosition = hits[i].point;
                        break;
                    }

                    // Update the camera position
                    transform.position = targetPosition;
                }
            }
        }


        protected virtual void Update()
        {
            for (int i = 0; i < secondaryCameras.Count; ++i)
            {
                secondaryCameras[i].OnFieldOfViewChanged(mainCamera.fieldOfView);
            }

            CameraControlUpdate();

        }


        protected virtual void CameraControlUpdate()
        {

            if (controllerOverriding || !cameraControlEnabled || cameraTarget == null || cameraTarget.Rigidbody != null) return;

            // Calculate the target position for the camera
            Vector3 targetPosition = currentViewTarget.transform.position;

            // Update position
            transform.position = (1 - currentViewTarget.PositionFollowStrength) * transform.position +
                                        currentViewTarget.PositionFollowStrength * targetPosition;

            // Update rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, currentViewTarget.transform.rotation,
                                                    currentViewTarget.RotationFollowStrength);
        }


        protected virtual void FixedUpdate()
        {
            CameraControlFixedUpdate();
        }


        protected virtual void CameraControlFixedUpdate()
        {

            if (controllerOverriding || !cameraControlEnabled || cameraTarget == null || cameraTarget.Rigidbody == null) return;

            // Calculate the target position for the camera
            Vector3 targetPosition = currentViewTarget.transform.position;

            // Update position
            transform.position = (1 - currentViewTarget.PositionFollowStrength) * transform.position +
                                        currentViewTarget.PositionFollowStrength * targetPosition;

            // Update rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, currentViewTarget.transform.rotation,
                                                    currentViewTarget.RotationFollowStrength);
        }


        protected virtual void LateUpdate()
        {
            CameraControlLateUpdate();
        }


        protected virtual void CameraControlLateUpdate()
        {

            if (controllerOverriding || controllerOverriding || !cameraControlEnabled || cameraTarget == null) return;

            // If position and/or rotation are locked for the selected camera view target, the position and rotation must be updated in 
            // late update to make sure that there is no lag.
            if (currentViewTarget != null)
            {
                if (currentViewTarget.LockPosition)
                {
                    transform.position = currentViewTarget.transform.position;
                }
                if (currentViewTarget.LockRotation)
                {
                    transform.rotation = currentViewTarget.transform.rotation;
                }

                if (currentViewTarget.LockCameraForwardVector)
                {
                    // Always point the camera directly forward 
                    transform.rotation = Quaternion.LookRotation(currentViewTarget.transform.forward, transform.up);
                }

                // Lock upright if necessary
                if (currentViewTarget.LockCameraUpright)
                {
                    transform.LookAt(transform.position + transform.forward, Vector3.up);
                }
            }
        }
    }
}
