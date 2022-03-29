using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Unity event for running functions when the vehicle is entered by a game agent.
    /// </summary>
    [System.Serializable]
    public class OnVehicleEnteredEventHandler : UnityEvent <GameAgent> { };

    /// <summary>
    /// Unity event for running functions when the vehicle is exited by a game agent.
    /// </summary>
    [System.Serializable]
    public class OnVehicleExitedEventHandler : UnityEvent <GameAgent> { };


    /// <summary> 
    /// This class is a base class for all kinds of vehicles. It exposes a function for entering and exiting 
    /// the vehicle, and deals with all kinds of vehicle events.
    /// </summary>
    public class Vehicle : MonoBehaviour
	{

        [Header("General")]

        // The class of vehicle
        [SerializeField]
        protected VehicleClass vehicleClass;
        public VehicleClass VehicleClass { get { return vehicleClass; } }

        // The identifying label for this vehicle, used by the loadout menu etc. 
        [SerializeField]
        protected string label = "Vehicle";
		public virtual string Label { get { return label; } }

        // The identifying label for this vehicle, used by the loadout menu etc. 
        [TextArea]
        [SerializeField]
        protected string description = "Vehicle.";
        public virtual string Description { get { return description; } }

        // A list of all the occupants currently in the vehicle
        protected List<GameAgent> occupants = new List<GameAgent>();
        public List<GameAgent> Occupants { get { return occupants; } }

        // Efficiently get the game object
        protected GameObject cachedGameObject;
        public GameObject CachedGameObject { get { return cachedGameObject; } }

        // Efficiently get the rigidbody
        protected Rigidbody cachedRigidbody;
        public Rigidbody CachedRigidbody { get { return cachedRigidbody; } }

        protected List<ModuleMount> moduleMounts = new List<ModuleMount>();
        public List<ModuleMount> ModuleMounts
        {
            get
            {
                // If prefab, search the hierarchy
                if (gameObject.scene.rootCount == 0)
                {
                    return GetModuleMountsOnVehicle();
                }
                // If not prefab, use cached list
                else
                {
                    return moduleMounts;
                }
            }
        }

        protected List<ModuleManager> moduleManagers = new List<ModuleManager>();

        protected bool destroyed;
        public bool Destroyed { get { return destroyed; } }

        [Header("Vehicle State Events")]
        
        // Vehicle destroyed event
        public UnityEvent onDestroyed;

        // Vehicle restored event
        public UnityEvent onRestored;

        [Header("Vehicle Owner Events")]

        public UnityEvent onEnteredByPlayer;

        public UnityEvent onEnteredByAI;

        public UnityEvent onExitedAll;

        // Game agent entered event
        [HideInInspector]
        public OnVehicleEnteredEventHandler onGameAgentEntered;

        // Game Agent exited event
        [HideInInspector]
        public OnVehicleEnteredEventHandler onGameAgentExited;



        // Called when the component is first added to a gameobject or is reset in the inspector
        protected virtual void Reset()
        {
            label = "Vehicle";
        }


        protected virtual void Awake()
		{		
            cachedGameObject = gameObject;
            cachedRigidbody = GetComponent<Rigidbody>();

            moduleManagers = new List<ModuleManager>(GetComponentsInChildren<ModuleManager>());

            moduleMounts = GetModuleMountsOnVehicle();
            for (int i = 0; i < moduleMounts.Count; ++i)
            {
                moduleMounts[i].onModuleAdded.AddListener(OnModuleAdded);
                moduleMounts[i].onModuleRemoved.AddListener(OnModuleRemoved);
            }
        }

        protected virtual void OnModuleAdded(Module module)
        {
            if (occupants.Count != 0)
            {
                module.SetOwner(occupants[0]);
            }
        }

        protected virtual void OnModuleRemoved(Module module)
        {
            if (occupants.Count != 0)
            {
                module.SetOwner(null);
            }
        }

        protected virtual List<ModuleMount> GetModuleMountsOnVehicle()
        {
            List<ModuleMount> moduleMountsList = new List<ModuleMount>();
            ModuleMount[] moduleMountsArray = transform.GetComponentsInChildren<ModuleMount>();
            foreach (ModuleMount moduleMount in moduleMountsArray)
            {

                moduleMount.RootTransform = transform;

                // Find the right index to insert the module mount according to its sorting index
                int insertIndex = 0;
                for (int i = 0; i < moduleMountsList.Count; ++i)
                {
                    if (moduleMountsList[i].SortingIndex < moduleMount.SortingIndex)
                    {
                        insertIndex = i + 1;
                    }
                }

                // Insert the camera view target into the list
                moduleMountsList.Insert(insertIndex, moduleMount);
            }

            return moduleMountsList;
        }


        /// <summary>
        /// Add a module mount to the vehicle.
        /// </summary>
        /// <param name="moduleMount">The module mount to be added.</param>
        public virtual void AddModuleMount(ModuleMount moduleMount)
        {

            moduleMount.RootTransform = transform;

            // Find the right index to insert the module mount according to its sorting index
            int insertIndex = 0;
            for (int i = 0; i < moduleMounts.Count; ++i)
            {
                if (moduleMounts[i].SortingIndex < moduleMount.SortingIndex)
                {
                    insertIndex = i + 1;
                }
            }

            moduleMounts.Insert(insertIndex, moduleMount);

            for (int i = 0; i < moduleManagers.Count; ++i)
            {
                moduleManagers[i].OnModuleMountAdded(moduleMount);
            }

            moduleMount.onModuleAdded.AddListener(OnModuleAdded);
            moduleMount.onModuleRemoved.AddListener(OnModuleRemoved);

        }


        /// <summary>
        /// Remove a module mount from the vehicle.
        /// </summary>
        /// <param name="moduleMount">The module mount to be removed.</param>
        public virtual void RemoveModuleMount(ModuleMount moduleMount)
        {

            int index = moduleMounts.IndexOf(moduleMount);
            if (index == -1) return;

            moduleMounts.Remove(moduleMount);

            for (int i = 0; i < moduleManagers.Count; ++i)
            {
                moduleManagers[i].OnModuleMountRemoved(moduleMount);
            }

            moduleMount.onModuleAdded.RemoveListener(OnModuleAdded);
            moduleMount.onModuleRemoved.RemoveListener(OnModuleRemoved);

        }

        
        /// <summary>
        /// Called when a game agent enters the vehicle.
        /// </summary>
        /// <param name="newOccupant">The game agent that entered the vehicle.</param>
        public virtual void OnEntered (GameAgent newOccupant)
        {
            if (newOccupant == null) return;

            // Check if the game agent is already in the vehicle
            for (int i = 0; i < occupants.Count; ++i)
            {
                if (occupants[i] == newOccupant)
                {
                    return;
                }
            }

            // Add the new occupant
            occupants.Add(newOccupant);

            if (occupants.Count != 0)
            {
                for (int i = 0; i < moduleManagers.Count; ++i)
                {
                    moduleManagers[i].ActivateModuleManager();
                }
            }

            // Update owner for modules
            for(int i = 0; i < moduleMounts.Count; ++i)
            {
                for(int j = 0; j < moduleMounts[i].Modules.Count; ++j)
                {
                    moduleMounts[i].Modules[j].SetOwner(newOccupant);
                }
            }

            // Call the events
            onGameAgentEntered.Invoke(newOccupant);
            if (newOccupant.IsPlayer)
            {
                onEnteredByPlayer.Invoke();
            }
            else
            {
                onEnteredByAI.Invoke();
            }
        }


        /// <summary>
        /// Called when a game agent exits a vehicle.
        /// </summary>
        /// <param name="exitingOccupant">The game agent exiting.</param>
        public virtual void OnExited (GameAgent exitingOccupant)
        {
            if (exitingOccupant == null) return;

            // Find the occupant in the list and remove
            for (int i = 0; i < occupants.Count; ++i)
            {
                if (occupants[i] == exitingOccupant)
                {
                    // Remove the occupant
                    occupants.RemoveAt(i);

                    // Call the event
                    onGameAgentExited.Invoke(exitingOccupant);
                    if (occupants.Count == 0)
                    {
                        onExitedAll.Invoke();
                    }

                    break;
                }
            }

            if (occupants.Count == 0)
            {
                for(int i = 0; i < moduleManagers.Count; ++i)
                {
                    moduleManagers[i].DeactivateModuleManager();
                }

                // Set owner to null for modules
                for (int i = 0; i < moduleMounts.Count; ++i)
                {
                    for (int j = 0; j < moduleMounts[i].Modules.Count; ++j)
                    {
                        moduleMounts[i].Modules[j].SetOwner(null);
                    }
                }
            }
        }

        /// <summary>
        /// Called to destroy the vehicle (e.g. when health reaches zero).
        /// </summary>
        public virtual void Destroy()
        {
            if (!destroyed)
            {
                destroyed = true;

                // Call event
                onDestroyed.Invoke();
            }          
        }

        /// <summary>
        /// Restore the vehicle after it has been destroyed.
        /// </summary>
        public virtual void Restore()
		{
            if (destroyed)
            {
                destroyed = false;

                // Call event
                onRestored.Invoke();
            }         
		}
    }
}
