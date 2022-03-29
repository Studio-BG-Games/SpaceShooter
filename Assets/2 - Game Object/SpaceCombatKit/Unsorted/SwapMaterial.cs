using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    [System.Serializable]
    public class SwapMaterialItem
    {
        public Material originalMaterial;
        public Material targetMaterial;
    }

    /// <summary>
    /// Exposes a function to swap (or revert) materials, which can be used in Unity Events.
    /// </summary>
    public class SwapMaterial : MonoBehaviour
    {

        [Tooltip("A list of material pairs to swap from the first material to the second one.")]
        [SerializeField]
        protected List<SwapMaterialItem> swapMaterialItems = new List<SwapMaterialItem>();

        protected MeshRenderer[] meshRenderers;


        protected virtual void Awake()
        {
            meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();
        }

        /// <summary>
        /// Swap between the materials (first item to second item).
        /// </summary>
        public virtual void Swap()
        {
            for (int i = 0; i < meshRenderers.Length; ++i)
            {
                Material[] materials = meshRenderers[i].materials;
                for (int j = 0; j < materials.Length; ++j)
                {
                    for (int k = 0; k < swapMaterialItems.Count; ++k)
                    {
                        if (materials[j].name.Contains(swapMaterialItems[k].originalMaterial.name))
                        {
                            materials[j] = swapMaterialItems[k].targetMaterial;
                        }
                    }
                }
                meshRenderers[i].materials = materials;
            }
        }

        /// <summary>
        /// Revert between the materials (second item to first item).
        /// </summary>
        public virtual void Revert()
        {
            for (int i = 0; i < meshRenderers.Length; ++i)
            {
                Material[] materials = meshRenderers[i].materials;
                for (int j = 0; j < materials.Length; ++j)
                {
                    for (int k = 0; k < swapMaterialItems.Count; ++k)
                    {
                        if (materials[j].name.Contains(swapMaterialItems[k].targetMaterial.name))
                        {
                            materials[j] = swapMaterialItems[k].originalMaterial;
                        }
                    }
                }

                meshRenderers[i].materials = materials;

            }
        }
    }
}
