using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// The PlayerData class saves information about the player's choices in the loadout manager to PlayerPrefs 
    /// such that the vehicle and weapons can be created in a the game scene.
    /// </summary>
	public static class PlayerData 
	{
	
		/// <summary>
        /// Save the selected vehicle's index within the PlayerItemManager prefab's vehicle list.
        /// </summary>
        /// <param name="selectedIndex">The index of the selected vehicle within the PlayerItemManager's vehicle list.</param>
		public static void SaveSelectedVehicleIndex(int newIndex)
		{
			PlayerPrefs.SetInt("SelectedVehicleIndex", newIndex);
		}
	

		// Get the saved vehicle index within the player item manager prefab component
		public static int GetSelectedVehicleIndex(PlayerItemManager playerItemManager)
		{
			int val = PlayerPrefs.GetInt("SelectedVehicleIndex", -1);
			val = Mathf.Clamp(val, -1, playerItemManager.vehicles.Count - 1);
			return val;
		}
	

		// Save the module loadout of the vehicle
		public static void SaveModuleLoadout(Vehicle modifiedVehicle, int vehicleIndex, PlayerItemManager itemManager)
		{

			if (vehicleIndex < 0 || vehicleIndex >= itemManager.vehicles.Count)
			{
				Debug.LogError("Vehicle index out of range for the PlayerItemManager's vehicle list. Unable to save loadout to PlayerPrefs");
				return;
			}

			string stringVal = "";
			for (int i = 0; i < modifiedVehicle.ModuleMounts.Count; ++i)
			{
				
				if (modifiedVehicle.ModuleMounts[i].MountedModuleIndex == -1)
				{ 
					stringVal += modifiedVehicle.ModuleMounts[i].MountedModuleIndex.ToString();
	
					if (i != modifiedVehicle.ModuleMounts.Count - 1)
					{
						stringVal += " ";
					}
					continue;
				}

				string moduleID = modifiedVehicle.ModuleMounts[i].Modules[modifiedVehicle.ModuleMounts[i].MountedModuleIndex].ID;

				int index = -1;
				foreach(Module itemManagerModule in itemManager.modulePrefabs)
				{
					if (itemManagerModule.ID == moduleID)
                    {
						index = itemManager.modulePrefabs.IndexOf(itemManagerModule);
						break;
                    }
                }

				if (index == -1)
				{
					Debug.LogWarning("Module found on ModuleMount has a prefab that does not exist in the PlayerItemManager allModulePrefabs list.");
				}
				
				stringVal += index.ToString();
	
				if (i != modifiedVehicle.ModuleMounts.Count - 1)
				{
					stringVal += " ";
				}
			}
			PlayerPrefs.SetString("VehicleModuleLoadout" + vehicleIndex.ToString(), stringVal);
			
		}

	
		// Get the saved module loadout
		public static List<int> GetModuleLoadout(int vehicleIndex, PlayerItemManager itemManager)
		{

			List<int> moduleIndexesByMount = new List<int>();
			if (vehicleIndex < 0 || vehicleIndex >= itemManager.vehicles.Count)
			{
				Debug.LogError("Vehicle index out of range for the PlayerItemManager's vehicle list");
				return moduleIndexesByMount;
			}

            // Get the loadout index list from playerprefs
            Vehicle vehicle = itemManager.vehicles[vehicleIndex];
			if (vehicleIndex == -1)
			{
				for (int i = 0; i < vehicle.ModuleMounts.Count; ++i)
				{
					moduleIndexesByMount.Add(-1);
				}
                
                return moduleIndexesByMount;
			}

			string stringVal = PlayerPrefs.GetString("VehicleModuleLoadout" + vehicleIndex.ToString(), "");
			
			if (stringVal == "")
			{
                List<ModuleMount> moduleMounts = vehicle.ModuleMounts;
                
				for (int i = 0; i < moduleMounts.Count; ++i)
				{
					if (moduleMounts[i].DefaultModulePrefabs.Count > 0)
					{
						moduleIndexesByMount.Add(itemManager.modulePrefabs.IndexOf(moduleMounts[i].DefaultModulePrefabs[0]));
					}
					else
					{
						moduleIndexesByMount.Add(-1);
					}
				}
                
				return moduleIndexesByMount;
			}

			string[] splitStringVal = stringVal.Split(null);
			foreach (string element in splitStringVal)
			{
				int intVal;
				bool success = int.TryParse(element, out intVal);	
				if (success)
				{
					moduleIndexesByMount.Add(intVal);
				}		
			}
		
			
			// Verify the number of mounts
			int diff = vehicle.ModuleMounts.Count - moduleIndexesByMount.Count;
			if (diff >= 0)
			{
				for (int i = 0; i < diff; ++i)
				{
					moduleIndexesByMount.Add(-1);
				}
			}
			else
			{
				int startIndex = moduleIndexesByMount.Count + diff;
				moduleIndexesByMount.RemoveRange(startIndex, Mathf.Abs(diff));
			}


			// Verify the type of module module
			for (int i = 0; i < moduleIndexesByMount.Count; ++i)
			{
	
				if (moduleIndexesByMount[i] == -1)
					continue;
	
				// If module index is out of range, clamp inside range
				if (moduleIndexesByMount[i] >= itemManager.modulePrefabs.Count)
				{
					moduleIndexesByMount[i] = Mathf.Clamp(moduleIndexesByMount[i], -1, itemManager.modulePrefabs.Count - 1);
					Debug.LogWarning("Module index out of range for PlayerItemManager modules list, clamping to " + moduleIndexesByMount[i].ToString());
				}
				
				// If module prefab is null, set index to -1
				if (itemManager.modulePrefabs[moduleIndexesByMount[i]] == null)
				{
					Debug.LogWarning("Module prefab at index " + i + " in the PlayerItemManager is null.");
					moduleIndexesByMount[i] = -1;
					continue;
				}
	
				// If module prefab doesn't have IModule interface, set to -1
				Module module = itemManager.modulePrefabs[moduleIndexesByMount[i]];
				if (module == null)
				{ 
					moduleIndexesByMount[i] = -1;
				} 
				// If module is not compatible with the mount, set to -1
				else if (!vehicle.ModuleMounts[i].MountableTypes.Contains(module.ModuleType))
				{
					Debug.LogWarning("Attempting to load incompatible module on module mount.");
					moduleIndexesByMount[i] = -1;
				}
			}
			
			return moduleIndexesByMount;
		}	
	}
}
