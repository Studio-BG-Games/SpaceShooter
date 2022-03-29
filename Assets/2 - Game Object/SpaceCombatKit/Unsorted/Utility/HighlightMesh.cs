using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Highlight and unhighlight a mesh (e.g. when it is part of a trackable targeted by the player)
    /// </summary>
    public class HighlightMesh : MonoBehaviour
    {

        [SerializeField]
        protected Color highlightedAlbedoColor = Color.white;
        
        [SerializeField]
        protected Color highlightColor;

        protected MeshRenderer meshRenderer;
        protected Material[] materials;
        protected List<Color> originalColors = new List<Color>();


        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            materials = meshRenderer.materials;
            for (int i = 0; i < materials.Length; ++i)
            {
                originalColors.Add(materials[i].color);
            }
        }

        /// <summary>
        /// Highlight the mesh.
        /// </summary>
        public void Highlight()
        {
            for(int i = 0; i < materials.Length; ++i)
            {
                materials[i].color = highlightedAlbedoColor;
                materials[i].SetColor("_EmissionColor", highlightColor);
            }
            
        }

        /// <summary>
        /// Unhighlight the mesh.
        /// </summary>
        public void Unhighlight()
        {
            for (int i = 0; i < materials.Length; ++i)
            {
                materials[i].color = originalColors[i];
                materials[i].SetColor("_EmissionColor", Color.black);
            }
        }
    }
}
