using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// Stores a triggerable module that is mounted on one of the vehicle's module mounts.
    /// </summary>
	public class MountedTriggerable 
	{
        // The triggerable
		public Triggerable triggerable;

        // The trigger groups data for this triggerable
        public List<int> triggerValuesByGroup;
	
		public MountedTriggerable(Triggerable triggerable)
		{
			this.triggerable = triggerable;
			triggerValuesByGroup = new List<int>();
		}
	}
	
	
    /// <summary>
    /// Manages a group of triggerables (such as on a vehicle), including adding/removing/modifying trigger groups.
    /// </summary>
	public class TriggerablesManager : ModuleManager
	{
        
        [Header("Settings")]

        [Tooltip("Create triggers to specify how triggerables should be fired at a particular index.")]
        [SerializeField]
        protected List<Trigger> triggers = new List<Trigger>();

        [Tooltip("Any Triggerable components that will not be added automatically.")]
        [SerializeField]
        protected List<Triggerable> startingTriggerables = new List<Triggerable>();

        // All of the triggerables that are being controlled by this Triggerables component
		[HideInInspector]
		public List<MountedTriggerable> mountedTriggerables = new List<MountedTriggerable>();

        // A list of all the trigger indexes currently being triggered
        private List<int> triggeredIndexes = new List<int>();

        [SerializeField]
        protected bool triggeringEnabled = true;
        public bool TriggeringEnabled
        {
            get { return triggeringEnabled; }
            set
            {
                triggeringEnabled = value;
                if (!triggeringEnabled)
                {
                    StopTriggeringAll();
                }
            }
        }

        // The number of trigger groups stored on this Triggerable
        private int numGroups;
        public int NumGroups { get { return numGroups; } }

        // The selected trigger group index
        private int selectedTriggerGroupIndex = -1;
        public int SelectedTriggerGroupIndex { get { return selectedTriggerGroupIndex; } }



        protected override void Awake()
		{

            AddTriggerGroup();

            base.Awake();

            for (int i = 0; i < startingTriggerables.Count; ++i)
            {
                if (startingTriggerables[i] != null)
                {
                    CreateMountedTriggerable(startingTriggerables[i]);
                }
            }
        }


        /// <summary>
        /// Set the selected trigger group.
        /// </summary>
        /// <param name="index">The index of the newly selected trigger group.</param>
        public void SetSelectedTriggerGroup(int index)
        {
            selectedTriggerGroupIndex = Mathf.Clamp(index, -1, numGroups - 1);
        }


        /// <summary>
        /// Get a mounted triggerable class instance that has been created for a specified triggerable module.
        /// </summary>
        /// <param name="triggerable">A reference to the triggerable module.</param>
        /// <returns>The MountedTriggerable instance.</returns>
        private MountedTriggerable GetMountedTriggerable(Triggerable triggerable)
        {
            foreach (MountedTriggerable mountedTriggerable in mountedTriggerables)
            {
                if (mountedTriggerable.triggerable == triggerable)
                {
                    return mountedTriggerable;
                }
            }
            return null;
        }


        protected virtual void CreateMountedTriggerable(Triggerable triggerable)
        {
            // Create a new mounted triggerable instance for the new triggerable
            MountedTriggerable newMountedTriggerable = new MountedTriggerable(triggerable);
            mountedTriggerables.Add(newMountedTriggerable);

            // Add the default trigger to all the trigger groups.
            for (int i = 0; i < numGroups; ++i)
            {
                newMountedTriggerable.triggerValuesByGroup.Add(newMountedTriggerable.triggerable.DefaultTriggerIndex);
            }
        }

        
        /// <summary>
        /// Event called when a new module is mounted on one of the module mounts, to store it if it's a triggerable module.
        /// </summary>
        /// <param name="moduleMount">The module mount where the new module was loaded.</param>
        protected override void OnModuleMounted (Module module)
		{
            
            // Get a reference to a Triggerable component on the new module.
            Triggerable triggerable = module.GetComponent<Triggerable>();
            
            // Store the triggerable found on the new module mounted on the module mount.
            if (triggerable != null && !triggerable.ManagedLocally)
			{
                
                // Check if it's already referenced
                for (int i = 0; i < mountedTriggerables.Count; ++i)
                {
                    if (mountedTriggerables[i].triggerable == triggerable)
                    {
                        return;
                    }
                }

                CreateMountedTriggerable(triggerable);
            }
		}

        /// <summary>
        /// Called when a module is unmounted from a module mount.
        /// </summary>
        /// <param name="moduleMount">The module mount where the module was unmounted.</param>
        /// <param name="module">The module that was unmounted.</param>
        protected override void OnModuleUnmounted(Module module)
        {
            // Get the module's triggerable if it exists.
            Triggerable triggerable = module.GetComponent<Triggerable>();
            if (triggerable == null) return;
            
            // Remove any reference to it
            for (int i = 0; i < mountedTriggerables.Count; ++i)
            {
                if (mountedTriggerables[i].triggerable == triggerable)
                {
                    mountedTriggerables.RemoveAt(i);
                    break;
                }
            }
        }

        public override void DeactivateModuleManager()
        {
            base.DeactivateModuleManager();
            StopTriggeringAll();
        }

        /// <summary>
        /// Trigger all the triggerable modules at a particular trigger index.
        /// </summary>
        /// <param name="triggerIndex">The trigger index that is being triggered.</param>
        public void StartTriggeringAtIndex(int triggerIndex)
		{

            if (!triggeringEnabled) return;

            // If no trigger group is selected, exit
            if (selectedTriggerGroupIndex == -1) return;

            bool fireSequentially = false;
            for (int i = 0; i < triggers.Count; ++i)
            {
                if (triggers[i].triggerIndex == triggerIndex)
                {
                    fireSequentially = true;
                    triggers[i].isTriggering = true;
                }
            }

            if (!fireSequentially)
            {
                // Start triggering all triggerables at the specified trigger index.
                for (int i = 0; i < mountedTriggerables.Count; ++i)
                {
                    if (mountedTriggerables[i].triggerValuesByGroup[selectedTriggerGroupIndex] == triggerIndex)
                    {
                        mountedTriggerables[i].triggerable.StartTriggering();
                    }
                }
            }

            // Add the trigger index to the list of triggered indexes
            if (!triggeredIndexes.Contains(triggerIndex))
            {
                triggeredIndexes.Add(triggerIndex);
            }
		}


		/// <summary>
        /// Stop triggering all the triggerable modules at a particular trigger index.
        /// </summary>
        /// <param name="triggerIndex">The trigger index to stop triggering.</param>
		public void StopTriggeringAtIndex(int triggerIndex)
		{

            // If no trigger group is selected, exit
            if (selectedTriggerGroupIndex == -1) return;

            // Stop triggering all triggerables at the specified trigger index.
            for (int i = 0; i < mountedTriggerables.Count; ++i)
			{
				if (mountedTriggerables[i].triggerValuesByGroup[selectedTriggerGroupIndex] == triggerIndex) mountedTriggerables[i].triggerable.StopTriggering();
			}

            for (int i = 0; i < triggers.Count; ++i)
            {
                if (triggers[i].triggerIndex == triggerIndex)
                {
                    triggers[i].isTriggering = false;
                }
            }

            // Remove the trigger index from the list of triggered indexes
            if (triggeredIndexes.Contains(triggerIndex))
            {
                triggeredIndexes.RemoveAt(triggeredIndexes.IndexOf(triggerIndex));
            }
        }

        /// <summary>
        /// Stop triggering all of the triggerables.
        /// </summary>
        public void StopTriggeringAll()
        {
            // If no trigger group is selected, exit
            if (selectedTriggerGroupIndex == -1) return;

            // Stop triggering all triggerables
            for (int i = 0; i < mountedTriggerables.Count; ++i)
            {
                mountedTriggerables[i].triggerable.StopTriggering();
            }

            for (int i = 0; i < triggers.Count; ++i)
            {
                triggers[i].isTriggering = false;
            }

            // Remove all triggered indexes
            triggeredIndexes.Clear();

        }


        public virtual void TriggerOnce(int triggerIndex)
        {

            if (!triggeringEnabled) return;

            // If no trigger group is selected, exit
            if (selectedTriggerGroupIndex == -1) return;

            // Start triggering all triggerables at the specified trigger index.
            for (int i = 0; i < mountedTriggerables.Count; ++i)
            {
                if (mountedTriggerables[i].triggerValuesByGroup[selectedTriggerGroupIndex] == triggerIndex)
                {
                    mountedTriggerables[i].triggerable.TriggerOnce();
                }
            }
        }


        /// <summary>
        /// Get whether or not a particular trigger index is currently being triggered.
        /// </summary>
        /// <param name="triggerIndex">The trigger index being queried.</param>
        /// <returns>Whether the trigger index is currently being triggered.</returns>
        public bool IsTriggering(int triggerIndex)
        {
            return (triggeredIndexes.Contains(triggerIndex));
        }


		/// <summary>
        /// Set the trigger index for a triggerable module.
        /// </summary>
        /// <param name="groupIndex">The trigger group index for the newly assigned value.</param>
        /// <param name="mountedTriggerableIndex">The module index for the newly assigned value.</param>
        /// <param name="newTriggerValue">The new trigger index.</param>
		public void SetTriggerValue(int groupIndex, int mountedTriggerableIndex, int newTriggerValue)
		{		
			mountedTriggerables[mountedTriggerableIndex].triggerValuesByGroup[groupIndex] = newTriggerValue;
		}
	
	
		/// <summary>
        /// Get the trigger index of a triggerable module in a trigger group.
        /// </summary>
        /// <param name="groupIndex">The index of the trigger group to look in.</param>
        /// <param name="mountedTriggerableIndex">The mounted triggerable index.</param>
        /// <returns>The trigger index.</returns>
		public int GetTriggerValue(int groupIndex, int mountedTriggerableIndex)
		{			
			return (mountedTriggerables[mountedTriggerableIndex].triggerValuesByGroup[groupIndex]);
		}
	
	
		/// <summary>
        /// Get an array of trigger index values for a trigger group (each one corresponds to a triggerable module in that group).
        /// </summary>
        /// <param name="groupIndex">The trigger group index.</param>
        /// <returns>An array of trigger index values for that group.</returns>
		public int[] GetTriggerValues(int groupIndex)
		{
            // Create an array for the trigger index values
			int[] results = new int[mountedTriggerables.Count];
			
            // Add the trigger index values to the array
			for (int i = 0; i < mountedTriggerables.Count; ++i)
			{
				results[i] = mountedTriggerables[i].triggerValuesByGroup[groupIndex];
			}

			return results;
			
		}
	
	
		/// <summary>
        /// Add a new trigger group with default values.
        /// </summary>
		public void AddTriggerGroup()
		{
			
            // For each triggerable module, add a new group with its default trigger index. 
			for (int i = 0; i < mountedTriggerables.Count; ++i)
			{
				mountedTriggerables[i].triggerValuesByGroup.Add(mountedTriggerables[i].triggerable.DefaultTriggerIndex);
			}

            // Update the number of groups.
			numGroups += 1;
			
            // If no trigger group was selected, select this new one.
			if (selectedTriggerGroupIndex == -1)
				selectedTriggerGroupIndex = 0;
		}
	
	
		/// <summary>
        /// Remove a trigger group at a specified index.
        /// </summary>
        /// <param name="removeIndex">The index of the trigger group to remove.</param>
		public void RemoveTriggerGroup(int removeIndex)
		{

            // For all of the mounted triggerables, remove the trigger group entry
			for (int i = 0; i < mountedTriggerables.Count; ++i)
			{
				mountedTriggerables[i].triggerValuesByGroup.RemoveAt(removeIndex);
			}

            // Update the number of groups
			numGroups -= 1;

            // Update the selected trigger group index
			selectedTriggerGroupIndex = Mathf.Clamp(selectedTriggerGroupIndex, -1, numGroups - 1);
		}


        private void Update()
        {
            for(int i = 0; i < triggers.Count; ++i)
            {
                if (triggers[i].isTriggering)
                {
                    if (Time.time - triggers[i].lastTriggeredTime > triggers[i].triggerInterval)
                    {
                        int index = triggers[i].lastTriggeredIndex;
                        for(int j = 0; j < mountedTriggerables.Count; ++j)
                        {
                            index += 1;
                            if (index >= mountedTriggerables.Count)
                            {
                                index = 0;
                            }
                            if (mountedTriggerables[index].triggerValuesByGroup[selectedTriggerGroupIndex] == triggers[i].triggerIndex)
                            {
                                mountedTriggerables[index].triggerable.TriggerOnce();
                                triggers[i].lastTriggeredIndex = index;
                                triggers[i].lastTriggeredTime = Time.time;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
