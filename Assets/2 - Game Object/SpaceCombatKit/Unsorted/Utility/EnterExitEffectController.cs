using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Manages the enter/exit effect by controlling shader variables.
    /// </summary>
    public class EnterExitEffectController : MonoBehaviour
    {

        [SerializeField]
        protected MeshRenderer enterExitRenderer;

        [SerializeField]
        protected float effectSpeed = 1;

        // Called every frame
        private void Update()
        {
            if (enterExitRenderer != null)
            {
                enterExitRenderer.sharedMaterial.SetFloat("_UVOffsetY", Time.realtimeSinceStartup * effectSpeed);
            }
        }
    }
}