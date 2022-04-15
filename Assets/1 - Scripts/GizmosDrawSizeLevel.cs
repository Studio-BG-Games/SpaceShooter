using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class GizmosDrawSizeLevel : MonoBehaviour
    {
        public Vector2 Min;
        public Vector2 Max;
        [Min(0)]public float Distance;

        private void OnDrawGizmos()
        {
            Vector3 sdl = transform.position + new Vector3(Min.x, Min.y, 0);
            Vector3 sul = transform.position + new Vector3(Min.x, Max.y, 0);
            Vector3 sdr = transform.position + new Vector3(Max.x, Min.y, 0);
            Vector3 sur = transform.position + new Vector3(Max.x, Max.y, 0);
            
            Vector3 fdl = transform.position + new Vector3(Min.x, Min.y, Distance);
            Vector3 ful = transform.position + new Vector3(Min.x, Max.y, Distance);
            Vector3 fdr = transform.position + new Vector3(Max.x, Min.y, Distance);
            Vector3 fur = transform.position + new Vector3(Max.x, Max.y, Distance);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(sdl, sul);
            Gizmos.DrawLine(sdr, sur);
            Gizmos.DrawLine(fdl, ful);
            Gizmos.DrawLine(fdr, fur);
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(sdl, sdr);
            Gizmos.DrawLine(sul, sur);
            Gizmos.DrawLine(fdl, fdr);
            Gizmos.DrawLine(ful, fur);
            
            Gizmos.color = Color.white;
            Gizmos.DrawLine(sdl, fdl);
            Gizmos.DrawLine(sul, ful);
            Gizmos.DrawLine(sdr, fdr);
            Gizmos.DrawLine(sur, fur);
        }
    }
}