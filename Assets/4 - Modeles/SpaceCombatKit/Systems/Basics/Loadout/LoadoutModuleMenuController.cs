using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat 
{

    /// <summary>
    /// This class manages the module selection part of the loadout menu.
    /// </summary>
    public class LoadoutModuleMenuController : ButtonsListManager 
	{

        /// <summary>
        /// Update the module mount selection UI when a new vehicle is selected.
        /// </summary>
        /// <param name="moduleMounts">The list of module mounts.</param>
        public void UpdateButtons(List<Module> modules)
        {

            // Update the number of weapon mount buttons
            SetNumButtons(modules.Count);

            // Label and activate all the mount buttons
            for (int i = 0; i < modules.Count; ++i)
            {
                buttonControllers[i].SetText(modules[i].Label);
                buttonControllers[i].SetIcon(modules[i].Sprites.Count > 0 ? modules[i].Sprites[0] : null);
                buttonControllers[i].gameObject.SetActive(true);
            }
        }
    }
}
