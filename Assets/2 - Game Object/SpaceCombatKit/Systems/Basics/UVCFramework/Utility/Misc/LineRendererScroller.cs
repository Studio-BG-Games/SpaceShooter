using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Scrolls the UV position of a line renderer (e.g. for a beam).
    /// </summary>
    public class LineRendererScroller : MonoBehaviour
    {
        [SerializeField]
        protected LineRenderer lineRenderer;

        [SerializeField]
        protected float scrollSpeedX = -5;

        [SerializeField]
        protected float tiling = 0.005f;


        protected void Reset()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        // Called every frame
        private void Update()
        {

            float length = Vector3.Distance(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1));
            float scrollSpeed = scrollSpeedX;
            float nextTiling = tiling * length;

            lineRenderer.material.SetTextureOffset("_MainTex", new Vector2((Time.time * scrollSpeed) % 1.0f, 0f));
            lineRenderer.material.SetTextureScale("_MainTex", new Vector2(nextTiling, 1));

        }
    }
}