using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Links shield modules mounted at a module mount with damage receivers etc.
    /// </summary>
    public class ShieldModuleLinker : DamageableModuleLinker
    {

        [SerializeField]
        protected MeshRenderer energyShieldMeshRenderer;

        /// <summary>
        /// Called when a module is mounted at the module mount.
        /// </summary>
        /// <param name="module">The mounted module.</param>
        protected override void OnModuleMounted(Module module)
        {

            base.OnModuleMounted(module);

            EnergyShieldController energyShieldController = module.GetComponent<EnergyShieldController>();
            if (energyShieldController != null)
            {
                energyShieldController.EnergyShieldMeshRenderer = energyShieldMeshRenderer;
            }
        }

        /// <summary>
        /// Called when a module is unmounted at the module mount.
        /// </summary>
        /// <param name="module">The unmounted module.</param>
        protected override void OnModuleUnmounted(Module module)
        {
            base.OnModuleUnmounted(module);

            EnergyShieldController energyShieldController = module.GetComponent<EnergyShieldController>();
            if (energyShieldController != null)
            {
                energyShieldController.EnergyShieldMeshRenderer = null;
            }
        }
    }
}
