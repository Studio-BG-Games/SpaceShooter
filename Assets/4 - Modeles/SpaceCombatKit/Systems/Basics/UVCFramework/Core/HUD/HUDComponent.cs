using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    
    /// <summary>
    /// Base class for a managing different sections of the HUD.
    /// </summary>
    public class HUDComponent : MonoBehaviour, IHUDCameraUser
    {

        [Header("HUD Component")]

        [Tooltip("The camera that is displaying this HUD component.")]
        [SerializeField]
        protected Camera hudCamera;
        public Camera HUDCamera
        {
            get { return hudCamera; }
            set { hudCamera = value; }
        }

        [Tooltip("Whether to activate this HUD component when the scene starts.")]
        [SerializeField]
        protected bool activateOnAwake = false;

        [Tooltip("Whether to update this HUD component every frame. Used when it is not being managed by a HUD Manager component.")]
        [SerializeField]
        protected bool updateIndividuallyEveryFrame = false;

        [SerializeField]
        protected Vector3 parentToCameraOffset;

        protected bool activated = false;
        public bool Activated { get { return activated; } }

        
        protected virtual void Awake()
        {
            if (activateOnAwake)
            {
                Activate();
            }
        }

     
        /// <summary>
        /// Activate this HUD Component
        /// </summary>
        public virtual void Activate()
        {
            gameObject.SetActive(true);
            activated = true;
        }

        /// <summary>
        /// Deactivate this HUD component
        /// </summary>
        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
            activated = false;
        }

        public virtual void ParentToTransform(Transform parentTransform)
        {
            transform.SetParent(parentTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public virtual void ParentToCamera()
        {
            ParentToTransform(hudCamera.transform);
            transform.localPosition = parentToCameraOffset;
        }

        public virtual void ClearParent()
        {
            transform.parent = null;
        }

        /// <summary>
        /// Called to update this HUD Component.
        /// </summary>
        public virtual void OnUpdateHUD() { }
        
        // Called every frame
        protected virtual void Update()
        {
            if (updateIndividuallyEveryFrame && activated)
            {
                OnUpdateHUD();
            }
        }
    }
}