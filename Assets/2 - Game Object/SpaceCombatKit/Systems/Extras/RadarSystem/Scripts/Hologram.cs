using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    public class Hologram : MonoBehaviour
    {
        [SerializeField]
        protected Bounds bounds = new Bounds(Vector3.zero, new Vector3(10, 10, 10));
        public Bounds Bounds { get { return bounds; } }

        protected Material[] materials;
        public Material[] Materials { get { return materials; } }

        protected void Awake()
        {
            Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
            materials = new Material[renderers.Length];
            for(int i = 0; i < renderers.Length; ++i)
            {
                materials[i] = renderers[i].material;
            }
        }

        // Show the bounding box visually in the editor scene view
        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(bounds.center), transform.rotation, transform.lossyScale);
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.zero, bounds.size);
        }
    }
}
