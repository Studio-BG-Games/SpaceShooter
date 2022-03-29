using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class manages the trigger groups menu, which gives the player the ability to bind triggerable modules 
    /// to input triggers at runtime.
    /// </summary>
    public class TriggerableGroupsMenuController : SimpleMenuManager 
	{
	
        [Header("General")]

        [Tooltip("The triggerables manager where the trigger groups are being set. A vehicle will usually have one on the root transform.")]
        [SerializeField]
        protected TriggerablesManager triggerablesManager;

		[Header("Triggerable Labels")]

        [Tooltip("The prefab for a label listed on the menu for a Triggerable module.")]
		[SerializeField]
        protected TriggerGroupsMenuLabelItemController labelItemPrefab;

        [Tooltip("The object that the Triggerable labels will be parented to.")]
		[SerializeField]
        protected Transform labelItemParent;

        protected List<TriggerGroupsMenuLabelItemController> labelItems = new List<TriggerGroupsMenuLabelItemController>();

        protected int focusedTriggerableIndex = -1;
	

		[Header("Triggerable Groups")]

        [Tooltip("The prefab for an individual trigger group in the menu.")]
		[SerializeField]
        protected TriggerGroupsMenuGroupItemController groupItemPrefab;

        [Tooltip("The object that the group items will be parented to.")]
		[SerializeField]
        protected Transform groupItemParent;

        protected List<TriggerGroupsMenuGroupItemController> groupItems = new List<TriggerGroupsMenuGroupItemController>();

        protected int focusedGroupIndex = -1;


		[Header("Triggerable Items")]
	
        [Tooltip("The prefab for a single trigger item, which is a button that can be clicked to set the trigger index for a Triggerable module in a particular group.")]
		[SerializeField]
        protected TriggerGroupsMenuTriggerItemController triggerItemPrefab;



        /// <summary>
        /// Focus on the triggerables manager from a different vehicle.
        /// </summary>
        /// <param name="vehicle">The new vehicle.</param>
        public void SetVehicle(Vehicle vehicle)
        {
            if (vehicle != null)
            {
                triggerablesManager = vehicle.GetComponent<TriggerablesManager>();
            }
            else
            {
                triggerablesManager = null;
            }
        }


        /// <summary>
        /// Called to activate the trigger groups menu in the scene.
        /// </summary>
        public override void OpenMenu()
		{
            
			if (triggerablesManager == null) return;
            base.OpenMenu();

            // Update the menu
            UpdateMenu();

        }


        /// <summary>
        /// Called to deactivate the trigger groups menu in the scene.
        /// </summary>
        public override void CloseMenu()
		{
            // Clear the triggerable menu items
            for (int i = 0; i < labelItems.Count; ++i)
            {
                Destroy(labelItems[i].gameObject);
            }

            // Empty the list of references
            labelItems.Clear();

            // Get rid of all the group items
            for (int i = 0; i < groupItems.Count; ++i)
            {
                Destroy(groupItems[i].gameObject);
            }

            // Clear all references to group items
            groupItems.Clear();
            
            base.CloseMenu();
        }
 

		/// <summary>
        /// Update the menu according to the TriggerableGroupsManager component
        /// </summary>
		void UpdateMenu()
		{

			if (triggerablesManager == null) return;

            // Update labels
            for (int i = 0; i < triggerablesManager.mountedTriggerables.Count; ++i)
			{

				// If the label item hasn't been created, create it
				if (i >= labelItems.Count)
				{

                    // Create a label
					TriggerGroupsMenuLabelItemController labelItem = Instantiate(labelItemPrefab.gameObject, Vector3.zero, Quaternion.identity, 
                                                                                labelItemParent).GetComponent<TriggerGroupsMenuLabelItemController>();
                    // Position/rotate/scale the new label
					labelItem.transform.localPosition = Vector3.zero;
					labelItem.transform.localRotation = Quaternion.identity;
					labelItem.transform.localScale = new Vector3(1, 1, 1);
					
                    labelItems.Add(labelItem);

				}

                // Update triggerable item label information
                Module module = triggerablesManager.mountedTriggerables[i].triggerable.GetComponent<Module>();
                if (module == null)
                {
                    labelItems[i].SetLabel(triggerablesManager.mountedTriggerables[i].triggerable.name);
                }
                else
                {
                    labelItems[i].SetLabel(module.Label);
                }
			}

            
            // Update group items
            int diff = triggerablesManager.NumGroups - groupItems.Count;
            if (diff > 0)
            {
                for (int i = 0; i < diff; ++i)
                {
                    // Add a new group item, parent it to the group item parent, and get a reference to its controller component
                    TriggerGroupsMenuGroupItemController groupItem = Instantiate(groupItemPrefab.gameObject, Vector3.zero, Quaternion.identity,
                                                                                groupItemParent).GetComponent<TriggerGroupsMenuGroupItemController>();

                    // Position/rotate/scale the group item
                    groupItem.transform.localPosition = Vector3.zero;
                    groupItem.transform.localRotation = Quaternion.identity;
                    groupItem.transform.localScale = new Vector3(1, 1, 1);

                    groupItem.onTriggerGroupSelected.AddListener(SelectGroup);
                    groupItems.Add(groupItem);

                }
            }
            // If there are too many, get rid of the excess
            else if (diff < 0)
            {

                // Destroy unnecessary group items
                for (int i = groupItems.Count - Mathf.Abs(diff); i < groupItems.Count; ++i)
                {
                    Destroy(groupItems[i].gameObject);
                }

                // Clear the references to the destroyed group items
                groupItems.RemoveRange(groupItems.Count - Mathf.Abs(diff), Mathf.Abs(diff));

            }

            
            // Update each of the group items
            for (int i = 0; i < groupItems.Count; ++i)
            {

                // Update the group item's index
                groupItems[i].SetGroupIndex(i);

                // Update the sibling index, considering that the first item is the triggerable label items list, and
                // the last item is the add/remove group buttons.
                groupItems[i].transform.SetSiblingIndex(i + 1);

                // Make sure there are enough trigger items in the group item
                diff = triggerablesManager.mountedTriggerables.Count - groupItems[i].triggerItems.Count; 

                if (diff > 0)
                {
                    for (int j = 0; j < diff; ++j)
                    {
                        // Add a new trigger item to the group item
                        TriggerGroupsMenuTriggerItemController triggerItem = Instantiate(triggerItemPrefab.gameObject, Vector3.zero, Quaternion.identity,
                                                                                    groupItems[i].transform).GetComponent<TriggerGroupsMenuTriggerItemController>();

                        // Position/rotate/scale the trigger item
                        triggerItem.transform.localPosition = Vector3.zero;
                        triggerItem.transform.localRotation = Quaternion.identity;
                        triggerItem.transform.localScale = new Vector3(1, 1, 1);

                        // Initialize the trigger item with the correct information
                        triggerItem.Init(j, groupItems.Count - 1, triggerablesManager.GetTriggerValue(i, j));

                        triggerItem.onTriggerItemSelected.AddListener(SelectTriggerableInGroup);

                        // Add the trigger item to the group item's trigger item list
                        groupItems[i].triggerItems.Add(triggerItem);
                    }
                }
                else if (diff < 0)
                {
                    // Destroy unnecessary trigger items
                    for (int j = groupItems[i].triggerItems.Count - Mathf.Abs(diff); j < groupItems[i].triggerItems.Count; ++j)
                    {
                        Destroy(groupItems[i].triggerItems[j].gameObject);
                    }

                    // Clear the references to the destroyed trigger items
                    groupItems[i].triggerItems.RemoveRange(groupItems[i].triggerItems.Count - Mathf.Abs(diff), Mathf.Abs(diff));
                }
            }

            
            // Update the values in all of the trigger items within the group items
            for (int i = 0; i < groupItems.Count; ++i)
            {
                for (int j = 0; j < groupItems[i].triggerItems.Count; ++j)
                {
                    groupItems[i].triggerItems[j].SetTriggerValue(triggerablesManager.GetTriggerValue(i, j));
                }
            }

            // Highlight the selected trigger group
            SelectGroup(triggerablesManager.SelectedTriggerGroupIndex);
			
		}	
        

		/// <summary>
        /// Add a new trigger group.
        /// </summary>
		public void AddNewGroup()
		{

            if (!menuActivated) return;

            triggerablesManager.AddTriggerGroup();

            UpdateMenu();
		}
	
	
		/// <summary>
        /// Remove the focused trigger group.
        /// </summary>
		public void RemoveFocusedGroup()
		{

            if (!menuActivated) return;

            if (focusedGroupIndex == -1)
				return;

			triggerablesManager.RemoveTriggerGroup (focusedGroupIndex);
		
            UpdateMenu();

		}
	
        /// <summary>
        /// Cycle the selected trigger group forward or backward.
        /// </summary>
        /// <param name="forward">Whether to cycle forward or backward.</param>
        public void CycleSelectedTriggerGroup(bool forward, bool wrap = false)
        {

            if (!menuActivated) return;

            if (triggerablesManager.NumGroups == 0) return;

            int newIndex = forward ? focusedGroupIndex + 1 : focusedGroupIndex - 1;
            if (wrap)
            {
                // Wrap to beginning
                if (newIndex >= triggerablesManager.NumGroups)
                {
                    newIndex = 0;
                }
                // Wrap to end
                else if (newIndex < 0)
                {
                    newIndex = triggerablesManager.NumGroups - 1;
                }
            }
            else
            {
                // Clamp the index within the number of groups
                newIndex = Mathf.Clamp(newIndex, 0, triggerablesManager.NumGroups - 1);
            }
            
            // Update the UI
            if (newIndex != focusedGroupIndex)
            {
                SelectTriggerableInGroup(focusedTriggerableIndex, newIndex);
            }
        }


        /// <summary>
        /// Cycle through the trigger items in the focused group item.
        /// </summary>
        /// <param name="forward">Whether to cycle forward or backward.</param>
        /// <param name="wrap">Whether to wrap the selected trigger item index when it reaches the beginning or the end.</param>
        public void CycleFocusedTriggerItem(bool forward, bool wrap = false)
        {

            if (!menuActivated) return;

            if (triggerablesManager.mountedTriggerables.Count == 0) return;

            // Get the new index
            int newIndex = forward ? focusedTriggerableIndex + 1 : focusedTriggerableIndex - 1;

            // Wrap or clamp
            if (wrap)
            {
                // Wrap to beginning
                if (newIndex >= groupItems[focusedGroupIndex].triggerItems.Count)
                {
                    newIndex = 0;
                }
                // Wrap to end
                else if (newIndex < 0)
                {
                    newIndex = groupItems[focusedGroupIndex].triggerItems.Count - 1;
                }
            }
            else
            {
                // Clamp the index within the number of trigger items in the group
                newIndex = Mathf.Clamp(newIndex, 0, groupItems[focusedGroupIndex].triggerItems.Count);
            }
            
            // Update the UI
            if (newIndex != focusedTriggerableIndex)
            {
                SelectTriggerableInGroup(newIndex, focusedGroupIndex);
            }

        }

        /// <summary>
        /// Select a trigger group by index.
        /// </summary>
        /// <param name="newSelectedGroupIndex">The index of the new selected trigger group.</param>
        /// <param name="updateGroupItem">Whether to update the trigger group element UI.</param>
        public void SelectGroup(int newSelectedGroupIndex)
		{
            if (!menuActivated) return;

            if (newSelectedGroupIndex == -1) return;

            // Set the selected trigger group
            triggerablesManager.SetSelectedTriggerGroup(newSelectedGroupIndex);
			focusedGroupIndex = newSelectedGroupIndex;

			// Update the UI
			groupItems[newSelectedGroupIndex].Select(false);
	
			// Deselect all the other groups
			for (int i = 0; i < groupItems.Count; ++i)
			{
				if (i != newSelectedGroupIndex) 
					groupItems[i].Deselect();
			}
		}
	
		
		/// <summary>
        /// Select a triggerable item in a specified trigger group.
        /// </summary>
        /// <param name="newFocusedTriggerableIndex">The index of the triggerable module selected in the menu.</param>
        /// <param name="newFocusedGroupIndex">The index of the trigger group selected in the menu.</param>
		public void SelectTriggerableInGroup (int newFocusedTriggerableIndex, int newFocusedGroupIndex)
		{

            if (!menuActivated) return;

            // Update the focused triggerable index			
            focusedTriggerableIndex = newFocusedTriggerableIndex;

            // Update the focused group index
            triggerablesManager.SetSelectedTriggerGroup(newFocusedGroupIndex);
			SelectGroup(newFocusedGroupIndex);
	
			// Deselect other trigger groups
			for (int i = 0; i < groupItems.Count; ++i)
			{
				for (int j = 0; j < groupItems[i].triggerItems.Count; ++j)
				{
					// If it is not the new focused triggerable item in the new focused group, deselect
					if (!(i == focusedGroupIndex && j == focusedTriggerableIndex))
					{
						groupItems[i].triggerItems[j].Deselect();
					}
                    else
                    {
                        groupItems[i].triggerItems[j].Select(false);
                    }
				}
			}
		}
		
	
		/// <summary>
        /// Set the trigger index for the focused trigger item.
        /// </summary>
        /// <param name="newTriggerValue">The new trigger index.</param>
		public void SetTriggerGroupTriggerValue(int newTriggerValue)
		{

            if (!menuActivated) return;

			int selectedGroupIndex = triggerablesManager.SelectedTriggerGroupIndex;

			// If no group and/or triggerable is selected, return
			if (selectedGroupIndex < 0 || focusedTriggerableIndex < 0) return;

			// Set the new value on the UI as well as the player vehicle's trigger groups manager
			groupItems[selectedGroupIndex].triggerItems[focusedTriggerableIndex].SetTriggerValue(newTriggerValue);
			triggerablesManager.SetTriggerValue(selectedGroupIndex, focusedTriggerableIndex, newTriggerValue);

		}
	}
}