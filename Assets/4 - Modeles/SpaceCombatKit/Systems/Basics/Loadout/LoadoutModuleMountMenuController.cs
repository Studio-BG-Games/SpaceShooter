using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat 
{

    /// <summary>
    /// This class manages the module mount selection UI within the loadout menu.
    /// </summary>
	public class LoadoutModuleMountMenuController : ButtonsListManager 
	{

        /// <summary>
        /// Update the module mount selection UI when a new vehicle is selected.
        /// </summary>
        /// <param name="moduleMounts">The list of module mounts.</param>
        public void UpdateButtons(List<ModuleMount> moduleMounts)
		{

            // Update the number of weapon mount buttons
            SetNumButtons(moduleMounts.Count);
	
			// Label and activate all the mount buttons
			for (int i = 0; i < moduleMounts.Count; ++i)
			{
				buttonControllers[i].SetText(moduleMounts[i].Label);
				buttonControllers[i].gameObject.SetActive(true);
			}
		}
	}
}
