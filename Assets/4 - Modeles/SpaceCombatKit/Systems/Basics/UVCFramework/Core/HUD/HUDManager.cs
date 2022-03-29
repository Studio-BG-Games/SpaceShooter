using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Manages the different components of the HUD for a vehicle.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        
        protected List<HUDComponent> hudComponents = new List<HUDComponent>();

        protected bool activated = false;

        [Tooltip("Whether to activate the HUD when the scene starts.")]
        [SerializeField]
        protected bool activateOnStart = false;

        protected IHUDCameraUser[] m_HUDCameraUsers;


        protected virtual void Awake()
        {

            hudComponents = new List<HUDComponent>(transform.GetComponentsInChildren<HUDComponent>());
            m_HUDCameraUsers = transform.GetComponentsInChildren<IHUDCameraUser>();

            Vehicle vehicle = transform.GetComponent<Vehicle>();
            if (vehicle != null)
            {
                vehicle.onDestroyed.AddListener(DeactivateHUD);
            }
        }
      
        // Called when the scene starts
        protected void Start()
        {
            if (!activated)
            {
                if (activateOnStart)
                {
                    ActivateHUD();
                }
                else
                {
                    DeactivateHUD();
                }
            }
        }

        public void SetHUDCamera(Camera hudCamera)
        {
            for (int i = 0; i < m_HUDCameraUsers.Length; ++i)
            {
                m_HUDCameraUsers[i].HUDCamera = hudCamera;
            }
        }

        /// <summary>
        /// Activate the HUD.
        /// </summary>
        public void ActivateHUD()
        {
            for (int i = 0; i < hudComponents.Count; ++i)
            {
                if (hudComponents[i] != null)
                {
                    hudComponents[i].Activate();
                }
            }

            activated = true;
        }


        /// <summary>
        /// Deactivate the HUD.
        /// </summary>
        public void DeactivateHUD()
        {
            for (int i = 0; i < hudComponents.Count; ++i)
            {
                if (hudComponents[i] != null)   // Necessary because when OnDisable is called when scene is being destroyed, not checking causes error
                {
                    hudComponents[i].Deactivate();
                }
            }

            activated = false;
        }

        public void LateUpdate()
        {
            if (activated)
            {
                for (int i = 0; i < hudComponents.Count; ++i)
                {
                    if (hudComponents[i].Activated)
                    {
                        hudComponents[i].OnUpdateHUD();
                    }
                }
            }
        }
    }
}
