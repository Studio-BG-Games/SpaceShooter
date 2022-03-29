using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Base class for a component that manages components on modules loaded onto the vehicle.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class ModuleManager : MonoBehaviour
    {

        [Header("Module Manager")]

        [Tooltip("Whether to load modules that are in the hierarchy but not mounted at a module mount.")]
        [SerializeField]
        protected bool loadUnmountedModules = true;
        protected Module[] unmountedModules;

        protected virtual void Awake()
        {
            // Get all the module mounts on the vehicle
            ModuleMount[] moduleMounts = GetComponentsInChildren<ModuleMount>();
            foreach (ModuleMount moduleMount in moduleMounts)
            {
                moduleMount.onModuleMounted.AddListener(OnModuleMounted);
                moduleMount.onModuleUnmounted.AddListener(OnModuleUnmounted);
            }

            unmountedModules = new Module[0];
            if (loadUnmountedModules)
            {
                unmountedModules = GetComponentsInChildren<Module>();
            }
        }

        protected virtual void Start()
        {
            foreach (Module module in unmountedModules)
            {
                OnModuleMounted(module);
            }
        }

        /// <summary>
        /// Called when a new module mount is added to the vehicle.
        /// </summary>
        /// <param name="moduleMount">The new module mount.</param>
        public virtual void OnModuleMountAdded(ModuleMount moduleMount)
        {

            if (moduleMount.MountedModule() != null)
            {
                OnModuleMounted(moduleMount.MountedModule());
            }

            moduleMount.onModuleMounted.AddListener(OnModuleMounted);
            moduleMount.onModuleUnmounted.AddListener(OnModuleUnmounted);
        }

        /// <summary>
        /// Called when a module mount is removed from the vehicle.
        /// </summary>
        /// <param name="moduleMount">The new module mount.</param>
        public virtual void OnModuleMountRemoved(ModuleMount moduleMount)
        {

            if (moduleMount.MountedModule() != null)
            {
                OnModuleUnmounted(moduleMount.MountedModule());
            }

            moduleMount.onModuleMounted.RemoveListener(OnModuleMounted);
            moduleMount.onModuleUnmounted.RemoveListener(OnModuleUnmounted);
        }

        // Called when a module is mounted on one of the vehicle's module mounts
        protected virtual void OnModuleMounted(Module module) { }

        // Called when a module is unmounted from one of the vehicle's module mounts
        protected virtual void OnModuleUnmounted(Module module) { }

        public virtual void ActivateModuleManager() { }

        public virtual void DeactivateModuleManager() { }

    }
}