using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Display module info on the HUD.
    /// </summary>
    [DefaultExecutionOrder(-30)]    // Must be called before the module mount is set up
    public class HUDModuleDisplay : MonoBehaviour
    {

        [Tooltip("The module to display. Leave empty if the module will be mounted at the module mount.")]
        [SerializeField]
        protected Module module;

        [Tooltip("The module mount where the module will be displayed. Leave empty if the 'Module' field is set.")]
        [SerializeField]
        protected ModuleMount moduleMount;

        [Header ("Display Modules")]

        [Tooltip("The display controller for the module.")]
        [SerializeField]
        protected HUDModuleDisplayItem moduleDisplayItem;

        [Header("Display Settings")]

        [Tooltip("Whether only specific module types may be displayed.")]
        [SerializeField]
        protected bool specifyModuleTypes = false;

        [Tooltip("The module types that can be displayed, if 'Specify Module Types' is checked.")]
        [SerializeField]
        protected List<ModuleType> displayableModuleTypes = new List<ModuleType>();

        

        protected virtual void Awake()
        {
            // Get notified when a module is mounted at the module mount
            if (moduleMount != null)
            {
                moduleMount.onModuleMounted.AddListener(OnModuleMounted);
                moduleMount.onModuleUnmounted.AddListener(OnModuleUnmounted);
            }
        }


        public virtual void SetModuleMount(ModuleMount moduleMount)
        {
            if (this.moduleMount != null)
            {
                this.moduleMount.onModuleMounted.RemoveListener(OnModuleMounted);
                this.moduleMount.onModuleUnmounted.RemoveListener(OnModuleUnmounted);
            }

            this.moduleMount = moduleMount;

            if (this.moduleMount != null)
            {
                this.moduleMount.onModuleMounted.AddListener(OnModuleMounted);
                this.moduleMount.onModuleUnmounted.AddListener(OnModuleUnmounted);

                OnModuleMounted(this.moduleMount.MountedModule());
            }
        }


        // Called when a module is mounted at the module mount
        protected virtual void OnModuleMounted(Module module)
        {
            if (CanDisplayModule(module))
            {
                moduleDisplayItem.DisplayModule(module);
            }
        }

        // Called when a module is unmounted at the module mount
        protected virtual void OnModuleUnmounted(Module module)
        {
            moduleDisplayItem.gameObject.SetActive(false);
        }

        // Checks if a module can be displayed. Override this function when creating derived scripts for specific module types.
        protected virtual bool CanDisplayModule(Module module)
        {
            if (module == null) return false;

            // Check if the module type can be displayed
            if (specifyModuleTypes)
            {
                return (displayableModuleTypes.IndexOf(module.ModuleType) != -1);
            }

            // Check if the display item can display the module
            if (!moduleDisplayItem.CanDisplayModule(module))
            {
                return false;
            }

            return true;
        }

        // Called every frame
        protected virtual void Update()
        {
            // Update the display
            moduleDisplayItem.UpdateDisplay();
        }
    }
}

