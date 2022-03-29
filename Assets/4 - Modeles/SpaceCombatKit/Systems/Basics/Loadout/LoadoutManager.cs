using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using VSX.UniversalVehicleCombat;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages the loadout menu scene.
    /// </summary>
    public class LoadoutManager : MonoBehaviour
    {

        [Header("General")]

        [SerializeField]
        protected PlayerItemManager itemManager;

        protected List<Vehicle> displayVehicles = new List<Vehicle>();
        public List<Vehicle> DisplayVehicles { get { return displayVehicles; } }

        public enum LoadoutMenuState
        {
            VehicleSelection,
            ModuleSelection
        }

        protected LoadoutMenuState menuState = LoadoutMenuState.VehicleSelection;
        public LoadoutMenuState MenuState { get { return menuState; } }

        [SerializeField]
        protected GameObject moduleSelectionUI;

        [SerializeField]
        protected GameObject shipSelectionUI;

        [SerializeField]
        protected GameObject blackout;

        [SerializeField]
        protected string missionSceneName = "SCK_SpaceCombat";

        [Header("Menu Controllers")]

        [SerializeField]
        protected LoadoutDisplayManager displayManager;

        [SerializeField]
        protected LoadoutModuleMenuController moduleMenuController;
        protected int focusedModuleItemIndex;

        [SerializeField]
        protected LoadoutModuleMountMenuController mountMenuController;
        protected int selectedModuleMountIndex;
        public int SelectedModuleMountIndex { get { return selectedModuleMountIndex; } }

        [SerializeField]
        protected VehicleStatsController vehicleStatsController;

        [SerializeField]
        protected ModuleStatsController moduleStatsController;

        protected int selectedVehicleIndex = -1;
        public int SelectedVehicleIndex { get { return selectedVehicleIndex; } }

        protected List<int> selectableModuleIndexes = new List<int>();

        [Header("Audio")]

        [SerializeField]
        protected AudioSource menuClickAudio;



        private void Start()
        {
            InitializeMenu();
            OpenMenu();
        }

        protected void InitializeMenu()
        {
            
            // Create the display vehicles
            displayVehicles = displayManager.AddDisplayVehicles(itemManager.vehicles, itemManager);

            // Disable the blackout screen
            if (blackout != null) blackout.SetActive(false);

            // Set the starting menu state
            SetMenuState(LoadoutMenuState.VehicleSelection);

            mountMenuController.onButtonSelected.AddListener(SelectModuleMount);
      
            moduleMenuController.UpdateButtons(itemManager.modulePrefabs);
            moduleMenuController.onButtonSelected.AddListener(SelectModule);

            vehicleStatsController.VehiclesList = itemManager.vehicles;
            moduleStatsController.ModulesList = itemManager.modulePrefabs;
            
        }

        public void OpenMenu()
        {
            // Select a ship
            if (displayVehicles.Count > 0)
            {
                int ind = PlayerData.GetSelectedVehicleIndex(itemManager);
                if (ind >= 0 && displayVehicles.Count > ind)
                {
                    SelectVehicle(ind);
                }
                else
                {
                    SelectVehicle(0);
                }
            }
        }

      
        /// <summary>
        /// Select a vehicle in the loadout menu.
        /// </summary>
        /// <param name="index">The index of the newly selected vehicle in the display vehicles list.</param>
        void SelectVehicle (int index)
        {
            
            if (index == selectedVehicleIndex) return;

            int previousVehicleIndex = selectedVehicleIndex;
            selectedVehicleIndex = index;
            PlayerData.SaveSelectedVehicleIndex(selectedVehicleIndex);
            
            // Update the vehicle display
            displayManager.OnVehicleSelection(selectedVehicleIndex, previousVehicleIndex, itemManager);

            // Show ship stats
            vehicleStatsController.ShowStats(displayVehicles[selectedVehicleIndex]);

            // Update the module mount menu
            mountMenuController.UpdateButtons(displayVehicles[selectedVehicleIndex].ModuleMounts);

            // Focus on the first module mount
            if(displayVehicles[selectedVehicleIndex].ModuleMounts.Count > 0)
            {
                SelectModuleMount(0);
            }
        }


        /// <summary>
        /// Toggle between the vehicle and module selection modes in the loadout menu.
        /// </summary>
        /// <param name="newStateIndex">The index of the new menu state in the enum.</param>
        public void SetMenuState(int newStateIndex)
        {
            
            LoadoutMenuState state = (LoadoutMenuState)newStateIndex;

            SetMenuState(state);

        }


        public void SetMenuState(LoadoutMenuState newState)
        {

            menuState = newState;

            if (menuState == LoadoutMenuState.VehicleSelection)
            {
                shipSelectionUI.SetActive(true);
                moduleSelectionUI.SetActive(false);
                displayManager.FocusModuleMount(null);
            }
            else
            {
                shipSelectionUI.SetActive(false);
                moduleSelectionUI.SetActive(true);

                ModuleMount focusedMount = selectedModuleMountIndex == -1 ? null : displayVehicles[selectedVehicleIndex].ModuleMounts[selectedModuleMountIndex];
                displayManager.FocusModuleMount(focusedMount);
            }
        }
        

        /// <summary>
        /// Cycle the loadout menu state.
        /// </summary>
        /// <param name="forward">Whether to cycle forward (or backward).</param>
        public void CycleMenuState(bool forward)
        {
            int index = (int)menuState;
            if (forward)
            {
                index += 1;
            }
            else
            {
                index -= 1;
            }

            index = Mathf.Clamp(index, 0, System.Enum.GetValues(typeof(LoadoutManager.LoadoutMenuState)).Length - 1);

            if (index != (int)menuState)
            {
                SetMenuState(index);
            }
        }


        public void CycleModuleMount(bool forward, bool wrap = false)
        {
            if (menuState == LoadoutManager.LoadoutMenuState.ModuleSelection)
            {

                int newModuleMountIndex = selectedModuleMountIndex;

                if (forward)
                {
                    newModuleMountIndex += 1;
                }
                else
                {
                    newModuleMountIndex -= 1;
                }

                if (wrap)
                {
                    if (newModuleMountIndex >= displayVehicles[selectedVehicleIndex].ModuleMounts.Count)
                    {
                        newModuleMountIndex = 0;
                    }
                    else if (newModuleMountIndex < 0)
                    {
                        newModuleMountIndex = displayVehicles[selectedVehicleIndex].ModuleMounts.Count - 1;
                    }
                }
                else
                {
                    newModuleMountIndex = Mathf.Clamp(newModuleMountIndex, -1, displayVehicles[selectedVehicleIndex].ModuleMounts.Count - 1);
                }

                if (newModuleMountIndex == -1)
                {
                    SelectModuleMount(-1);
                }
                else
                {
                    SelectModuleMount(newModuleMountIndex);
                } 
            }
        }

        /// <summary>
        /// Called when the player clicks on a button to focus on a different module mount in the loadout menu.
        /// </summary>
        /// <param name="newMountIndex">The index of the new module mount.</param>
        public void SelectModuleMount(int moduleMountIndex)
        {

            selectedModuleMountIndex = moduleMountIndex;

            // Update the module mount
            mountMenuController.SetButtonSelected(selectedModuleMountIndex);

            // Update the module options menu
            selectableModuleIndexes = new List<int>();
            int mountedIndex = -1;
            for (int i = 0; i < displayVehicles[selectedVehicleIndex].ModuleMounts[selectedModuleMountIndex].Modules.Count; ++i)
            {

                int index = -1;
                foreach(Module itemManagerModule in itemManager.modulePrefabs)
                {
                    if (itemManagerModule.ID == displayVehicles[selectedVehicleIndex].ModuleMounts[selectedModuleMountIndex].Modules[i].ID)
                    {
                        index = itemManager.modulePrefabs.IndexOf(itemManagerModule);
                        break;
                    }
                }
                
                selectableModuleIndexes.Add(index);

                if (displayVehicles[selectedVehicleIndex].ModuleMounts[selectedModuleMountIndex].MountedModuleIndex == i)
                {
                    mountedIndex = index;
                    
                    SelectModule(mountedIndex);
                }
            }

            moduleMenuController.SetVisibleButtons(selectableModuleIndexes);
            moduleMenuController.SetButtonSelected(mountedIndex);

            if (menuState == LoadoutMenuState.ModuleSelection)
            {
                ModuleMount focusedMount = selectedModuleMountIndex == -1 ? null : displayVehicles[selectedVehicleIndex].ModuleMounts[selectedModuleMountIndex];
                displayManager.FocusModuleMount(focusedMount);
            }
        }


        /// <summary>
        /// Called when the player clicks on a button to clear the current selection of a module at a module mount (mount nothing).
        /// </summary>
        public void ClearModuleMount()
        {
            SelectModule(-1);
        }


        public void CycleModule (bool forward, bool wrap = false)
        {
            if (menuState == LoadoutManager.LoadoutMenuState.ModuleSelection)
            {

                int newModuleIndexInList = selectableModuleIndexes.IndexOf(focusedModuleItemIndex);
                
                if (forward)
                {
                    newModuleIndexInList += 1;
                }
                else
                {
                    newModuleIndexInList -= 1;
                }

                if (wrap)
                {
                    if (newModuleIndexInList >= selectableModuleIndexes.Count)
                    {
                        newModuleIndexInList = 0;
                    }
                    else if (newModuleIndexInList < 0)
                    {
                        newModuleIndexInList = selectableModuleIndexes.Count - 1;
                    }
                }
                else
                {
                    newModuleIndexInList = Mathf.Clamp(newModuleIndexInList, -1, selectableModuleIndexes.Count - 1);
                }
                
                if (newModuleIndexInList != -1) SelectModule(selectableModuleIndexes[newModuleIndexInList]);
            }
        }

        /// <summary>
        /// Called when the player clicks on a module item in the loadout menu.
        /// </summary>
        /// <param name="newModuleIndex">The index of the newly selected module in the menu</param>
        public void SelectModule(int newModuleIndex)
        {

            focusedModuleItemIndex = newModuleIndex;

            // Update the module menu 
            moduleMenuController.SetButtonSelected(newModuleIndex);

            // If no module selected, clear the module mount
            if (newModuleIndex == -1)
            {
                displayVehicles[selectedVehicleIndex].ModuleMounts[selectedModuleMountIndex].MountModule(-1);
                return;
            }

            // Mount the module and get a reference to it
            Module module = null;
            for (int i = 0; i < displayVehicles[selectedVehicleIndex].ModuleMounts[selectedModuleMountIndex].Modules.Count; ++i)
            {
                // If the module exists
                if (displayVehicles[selectedVehicleIndex].ModuleMounts[selectedModuleMountIndex].Modules[i].ID ==
                    itemManager.modulePrefabs[newModuleIndex].ID)
                {
                    // Mount the module
                    displayVehicles[selectedVehicleIndex].ModuleMounts[selectedModuleMountIndex].MountModule(i);
                    module = displayVehicles[selectedVehicleIndex].ModuleMounts[selectedModuleMountIndex].MountedModule();
                }
            }
            
            // Show the module stats
            moduleStatsController.ShowStats(module);

        }


        // Cycle the vehicle selection in the menu forward or backward.
        public void CycleVehicleSelection(bool cycleForward)
        {
            
            if (displayVehicles.Count < 2) return;

            // Cycle up or down
            int nextIndex;
            if (cycleForward)
            {
                nextIndex = selectedVehicleIndex + 1;
            }
            else
            {
                nextIndex = selectedVehicleIndex - 1;
            }

            // Wrap the new index to the number of vehicles
            if (nextIndex < 0)
            {
                nextIndex = displayVehicles.Count - 1;
            }
            else if (nextIndex >= displayVehicles.Count)
            {
                nextIndex = 0;
            }
                
            // If selected vehicle index has changed
            SelectVehicle(nextIndex);
            
        }


        /// <summary>
        /// Exit the loadout menu and begin the game.
        /// </summary>
        public void StartMission()
        {

            for (int i = 0; i < displayVehicles.Count; ++i)
            {
                PlayerData.SaveModuleLoadout(displayVehicles[i], i, itemManager);
            }

            blackout.SetActive(true);

            SceneManager.LoadScene(missionSceneName);

        }
    }
}