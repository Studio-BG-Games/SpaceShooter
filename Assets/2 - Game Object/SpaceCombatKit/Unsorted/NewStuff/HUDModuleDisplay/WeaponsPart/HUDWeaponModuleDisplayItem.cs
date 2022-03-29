using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class HUDWeaponModuleDisplayItem : HUDModuleDisplayItem
    {
        [Header("Weapon")]

        [Tooltip("The Resource displays for the weapon module.")]
        [SerializeField]
        protected List<UIResourceDisplayItem> resourceDisplayItems = new List<UIResourceDisplayItem>();

        [Tooltip("Whether to disable the resource display if no resource container is found for it.")]
        [SerializeField]
        protected bool disableResourceDisplayIfNotFound = true;

        /// <summary>
        /// Display a module.
        /// </summary>
        /// <param name="module">The module to be displayed.</param>
        public override void DisplayModule(Module module)
        {

            Weapon weapon = module.GetComponent<Weapon>();
            if (weapon == null) return;

            base.DisplayModule(module);

            foreach(UIResourceDisplayItem item in resourceDisplayItems)
            {
                bool found = false;
                if (item.resourceType != null)
                {
                    foreach(ResourceHandler resourceHandler in weapon.ResourceHandlers)
                    {
                        if (resourceHandler.resourceContainer.ResourceType == item.resourceType)
                        {
                            found = true;
                            item.toggleObject.SetActive(true);
                            item.resourceContainer = resourceHandler.resourceContainer;
                            break;
                        }
                    }
                }

                if (!found && disableResourceDisplayIfNotFound)
                {
                    item.toggleObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Update the module display.
        /// </summary>
        public override void UpdateDisplay()
        {
            base.UpdateDisplay();

            for(int i = 0; i < resourceDisplayItems.Count; ++i)
            {
                resourceDisplayItems[i].UpdateDisplay();
            }
        }

        /// <summary>
        /// Check if a specific module can be displayed.
        /// </summary>
        /// <param name="module">The module to check if can be displayed.</param>
        /// <returns>Whether the module can be displayed.</returns>
        public override bool CanDisplayModule(Module module)
        {

            if (!base.CanDisplayModule(module)) return false;

            // Check if the module has a Weapon component
            Weapon weapon = module.GetComponent<Weapon>();
            return (weapon != null);
        }
    }
}

