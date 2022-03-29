using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class SetCameraLayerMask : MonoBehaviour
    {
        [SerializeField]
        protected Camera m_Camera;

        protected enum LayerMaskSettingMode
        {
            Overwrite,
            Include,
            Exclude
        }

        [SerializeField]
        protected LayerMaskSettingMode settingMode;

        [SerializeField]
        protected List<string> layerNames = new List<string>();


        protected void Reset()
        {
            m_Camera = GetComponent<Camera>();
        }

        private void Awake()
        {

            for(int i = 0; i < layerNames.Count; ++i)
            {
                int layer = LayerMask.NameToLayer(layerNames[i]);
                if (layer == -1)
                {
                    Debug.LogError("Cannot add layer '" + layerNames[i] + "' to the camera culling mask because it doesn't exist. Please add this layer to your project before running the scene.");
                }
            }

            int layerMask;
            if (settingMode == LayerMaskSettingMode.Overwrite)
            {
                layerMask = LayerMask.GetMask(layerNames.ToArray());
            }
            else if (settingMode == LayerMaskSettingMode.Include)
            {
                layerMask = m_Camera.cullingMask;
                foreach (string layerName in layerNames)
                {
                    int includedLayer = LayerMask.NameToLayer(layerName);
                    layerMask = layerMask | (1 << includedLayer);
                }
            }
            else
            {
                layerMask = m_Camera.cullingMask;
                foreach(string layerName in layerNames)
                {
                    int excludedLayer = LayerMask.NameToLayer(layerName);
                    layerMask = layerMask & ~(1 << excludedLayer);
                }
            }

            m_Camera.cullingMask = layerMask; 

        }
    }
}

