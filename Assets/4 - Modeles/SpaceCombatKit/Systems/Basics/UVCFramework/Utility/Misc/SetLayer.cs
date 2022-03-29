using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class SetLayer : MonoBehaviour
    {
        [SerializeField]
        protected string layerName;

        [SerializeField]
        protected bool includeHierarchy = true;

        private void Awake()
        {
            int layer = LayerMask.NameToLayer(layerName);

            if (layer != -1)
            {
                if (includeHierarchy)
                {
                    SetLayerRecursive(transform, layer);
                }
                else
                {
                    transform.gameObject.layer = layer;
                }
            }
            else
            {
                Debug.LogError("Cannot set gameobject layer to '" + layerName + "' because it doesn't exist. Add this layer to your project before running the scene.");
            }
        }

        void SetLayerRecursive(Transform t, int layer)
        {
            t.gameObject.layer = layer;

            foreach (Transform child in t)
            {
                SetLayerRecursive(child, layer);
            }
        }
    }
}

