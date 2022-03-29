using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.CameraSystem
{
    /// <summary>
    /// Simple demo script for changing the camera view.
    /// </summary>
    public class DemoCameraViewControls : MonoBehaviour
    {
        [Tooltip("The camera entity that these controls will operate on.")]
        [SerializeField]
        protected CameraEntity cameraEntity;

        private void Update()
        {
            // Cycle camera view back
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                cameraEntity.CycleCameraView(false);
            }
            // Cycle camera view forward
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                cameraEntity.CycleCameraView(true);
            }
        }
    }
}

