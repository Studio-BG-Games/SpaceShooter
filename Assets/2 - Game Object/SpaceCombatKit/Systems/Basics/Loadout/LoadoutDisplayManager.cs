using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages the display of actual vehicles and modules in the loadout menu
    /// (not the menu graphics).
    /// </summary>
	public class LoadoutDisplayManager : MonoBehaviour 
	{

        [SerializeField]
        protected LoadoutManager loadoutManager;

		[Header("Vehicle")]

		[SerializeField]
		protected Transform vehicleDisplayParent;

		[SerializeField]
        protected float vehicleDisplayRotationSpeed = 0f;
	
		[SerializeField]
        protected float vehicleDropTime = 0.5f;

		[SerializeField]
        protected float maxVehicleDropDistance = 0.175f;

		[SerializeField]
        protected float maxVehicleDropRotation = 3f;

		[SerializeField]
        protected AnimationCurve vehicleDropInCurve;

        protected Coroutine dropvehicleCoroutine;

		List<Vehicle> vehicles = new List<Vehicle>();

        protected Transform focusedVehicleTransform = null;
        protected Transform focusedMount = null;


		/// <summary>
        /// Called to make a vehicle do a drop animation into the loadout menu.
        /// </summary>
		public void DropVehicle()
		{

			if (dropvehicleCoroutine != null) StopCoroutine (dropvehicleCoroutine);
	
			vehicleDisplayParent.localPosition = Vector3.zero;
			vehicleDisplayParent.localRotation = Quaternion.identity;
	
			dropvehicleCoroutine = StartCoroutine(DoVehicleDrop());
			
		}

        /// <summary>
        /// Coroutine executing a drop animation for a vehicle in the loadout menu.
        /// </summary>
        /// <returns>Null.</returns>
        IEnumerator DoVehicleDrop()
		{
	
			float startTime = Time.time;
			while (Time.time - startTime < vehicleDropTime)
			{
				float timeFraction = (Time.time - startTime)/vehicleDropTime;
	
				float nextOffsetY = vehicleDropInCurve.Evaluate(timeFraction) * maxVehicleDropDistance;
				vehicleDisplayParent.localPosition = new Vector3(0f, nextOffsetY, 0f);
	
				float nextRotX = vehicleDropInCurve.Evaluate(timeFraction) * maxVehicleDropRotation;
				vehicleDisplayParent.localRotation = Quaternion.Euler(new Vector3(nextRotX, 0f, 0f));
	
				yield return null;
			}
		}

	
        /// <summary>
        /// Create all of the vehicles that will be displayed in the Loadout menu.
        /// </summary>
        /// <param name="vehiclePrefabs">A list of all the vehicle prefabs.</param>
        /// <param name="itemManager">A prefab containing references to all the vehicles and modules available in the menu. </param>
        /// <returns>A list of all the created vehicles. </returns>
		public List<Vehicle> AddDisplayVehicles(List<Vehicle> vehiclePrefabs, PlayerItemManager itemManager)
		{

			// Add ships
			for (int i = 0; i < vehiclePrefabs.Count; ++i)
			{
	
				// Instantiate and position the vehicle

				GameObject newVehicleGameObject = (GameObject)Instantiate(vehiclePrefabs[i].gameObject, Vector3.zero, Quaternion.identity);	
				Transform newVehicleTransform = newVehicleGameObject.transform;				

				newVehicleTransform.SetParent(vehicleDisplayParent);
				newVehicleTransform.localPosition = Vector3.zero;
				newVehicleTransform.localRotation = Quaternion.identity;
				newVehicleTransform.localScale = new Vector3(1f, 1f, 1f);


				// Add the vehicle to display list
				Vehicle createdVehicle = newVehicleGameObject.GetComponent<Vehicle>();
				vehicles.Add(createdVehicle);	

				createdVehicle.CachedRigidbody.isKinematic = true;
                for (int j = 0; j < createdVehicle.ModuleMounts.Count; ++j)
                {
                    createdVehicle.ModuleMounts[j].createDefaultModulesAtStart = false;
                }

				// Mount modules
				foreach (ModuleMount moduleMount in createdVehicle.ModuleMounts)
				{

					// Clear anything that's already been loaded onto the prefab as a mountable module
					moduleMount.UnmountAllModules();
					
					// Add mountable modules at this mount for all compatible modules
					foreach (Module modulePrefab in itemManager.modulePrefabs)
					{
						if (modulePrefab != null)
						{
							if (moduleMount.MountableTypes.Contains(modulePrefab.ModuleType))
							{
                                Module createdModule = (Module)GameObject.Instantiate(modulePrefab, null);

								moduleMount.AddModule(createdModule);
							}
						}
					}					
				}

				// Get the loadout configuration
				List<int> moduleIndexesByMount = PlayerData.GetModuleLoadout(i, itemManager);

				// Mount modules on each module mount
				for (int j = 0; j < createdVehicle.ModuleMounts.Count; ++j)
				{
					// If no selection has been saved...
					if (moduleIndexesByMount[j] == -1)
					{

						// If there is no module loaded already
						if (createdVehicle.ModuleMounts[j].MountedModuleIndex == -1)
						{
							int firstSelectableIndex = Mathf.Clamp(0, -1, createdVehicle.ModuleMounts[j].Modules.Count - 1);
							if (firstSelectableIndex != -1)
							{
								createdVehicle.ModuleMounts[j].MountModule(firstSelectableIndex);
										
							}
						}
					}
					else
					{
						// Load the module according to the saved configuration
						for (int k = 0; k < createdVehicle.ModuleMounts[j].Modules.Count; ++k)
						{
							if (createdVehicle.ModuleMounts[j].Modules[k].ID == itemManager.modulePrefabs[moduleIndexesByMount[j]].ID)
							{
								createdVehicle.ModuleMounts[j].MountModule(k);
								break;
							}
						}
		
						
					}
				}	

				// Deactivate the vehicle
				newVehicleGameObject.SetActive(false);
				
			}

			return vehicles;
		}

        /// <summary>
        /// Event called when a vehicle is selected in the loadout menu.
        /// </summary>
        /// <param name="newSelectionIndex">The index of the newly selected vehicle.</param>
        /// <param name="previousSelectionIndex">The index of the previously selected vehicle.</param>
        /// <param name="itemManager">A prefab containing references to all the vehicles and modules available in the menu. </param>
        public void OnVehicleSelection(int newSelectionIndex, int previousSelectionIndex, PlayerItemManager itemManager)
		{

			// Disable the last ship
			if (previousSelectionIndex != -1)
			{ 
				vehicles[previousSelectionIndex].CachedGameObject.SetActive(false);
			}
			
			// Activate the new ship
			vehicles[newSelectionIndex].CachedGameObject.SetActive(true);

			// Do the drop animation
			DropVehicle();

			focusedVehicleTransform = vehicles[newSelectionIndex].transform;

			
		}


		/// <summary>
        /// Event called when a new module is selected in the module selection part of the loadout menu, to mount
        /// the new module on the display vehicle.
        /// </summary>
        /// <param name="vehicleIndex">The index of the vehicle on which to mount the new module.</param>
        /// <param name="mountIndex">The index of the module mount at which to load the new module.</param>
        /// <param name="moduleIndex">The index of the newly selected module.</param>
		public void OnModuleSelection(int vehicleIndex, int mountIndex, int moduleIndex)
		{
			
			if (vehicleIndex == -1 || mountIndex == -1)
				return;

			if (moduleIndex != -1)
			{
				vehicles[vehicleIndex].ModuleMounts[mountIndex].MountModule(moduleIndex);
			}
		}


        /// <summary>
        /// Event called when a different module mount is focused on in the loadout menu.
        /// </summary>
        /// <param name="moduleMount">The new module mount to focus on.</param>
        public void FocusModuleMount(ModuleMount moduleMount)
		{
			if (moduleMount != null)
			{
				focusedMount = moduleMount.transform;
			}
			else
			{
				focusedMount = null;
			}
		}


        // Called every frame
        void Update()
        {
            // Rotate the vehicle if not focusing on module mount
            if (loadoutManager.MenuState == LoadoutManager.LoadoutMenuState.ModuleSelection)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else
            {
                transform.Rotate(new Vector3(0f, vehicleDisplayRotationSpeed * Time.deltaTime, 0f));
            }
        }
	}
}
