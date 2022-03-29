using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.CameraSystem
{
    /// <summary>
    /// This class manages a secondary camera, which is any additional camera in your scene that must conform to changes in the settings of the main camera.
    /// For example, a background camera that shows the environment, which must show the same field of view.
    /// </summary>
    public class SecondaryCamera : MonoBehaviour
    {

        protected CameraEntity cameraEntity;
        public CameraEntity CameraEntity
        {
            set
            {
                cameraEntity = value;
                if (cameraEntity != null)
                {
                    cameraEntity.onCameraViewTargetChanged.AddListener(OnCameraViewTargetChanged);
                }
            }
        }

        [Tooltip("The settings for this camera for each Camera View.")]
        [SerializeField]
        protected List<SecondaryCameraViewSettings> viewSettings = new List<SecondaryCameraViewSettings>();

        protected Camera m_Camera;


        protected void Awake()
        {
            m_Camera = GetComponent<Camera>();
        }

        /// <summary>
        /// Called when the camera view target changes on the camera.
        /// </summary>
        /// <param name="newCameraViewTarget">The new camera view target.</param>
        public void OnCameraViewTargetChanged(CameraViewTarget cameraViewTarget) { }

        /// <summary>
        /// Called when the field of view on the main camera changes.
        /// </summary>
        /// <param name="newFieldOfView">The new field of view.</param>
        public void OnFieldOfViewChanged(float newFieldOfView)
        {
            for (int i = 0; i < viewSettings.Count; ++i)
            {
                if (viewSettings[i].view == cameraEntity.CurrentView && viewSettings[i].copyFieldOfView)
                {
                    m_Camera.fieldOfView = newFieldOfView;
                }
            }
        }
    }
}