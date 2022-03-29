using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{

    [System.Serializable]
    public class OnVehicleSpawnedEventHandler : UnityEvent<Vehicle> { }

    public class VehicleSpawner : MonoBehaviour
    {

        [SerializeField]
        protected bool spawnAtStart = true;

        public enum VehicleSpawnType
        {
            Loadout,
            Prefab
        }

        [Header("Vehicle")]

        [SerializeField]
        protected VehicleSpawnType vehicleSpawnType;

        [SerializeField]
        protected Transform vehicleSpawnPoint;

        [SerializeField]
        protected string vehicleName = "PlayerVehicle";

        [SerializeField]
        protected VehicleEnterExitManager startingChildVehicle; // Set a starting child vehicle for the player to exit to.

        [Header("Loadout Spawn")]

        [SerializeField]
        protected PlayerItemManager itemManager;

        [Header("Prefab Spawn")]

        [SerializeField]
        protected Vehicle vehiclePrefab;

        public enum VehicleOccupantType
        {
            Scene,
            SpawnFromPrefab,
            GameAgentManagerFocusedGameAgent
        }


        [Header("Occupant")]

        [SerializeField]
        protected VehicleOccupantType startingOccupantType;

        [SerializeField]
        protected GameAgent startingOccupant;

        [Header("Events")]

        public OnVehicleSpawnedEventHandler onVehicleSpawned;   // This is the event for the vehicle spawn 


        protected virtual void Reset()
        {
            vehicleSpawnPoint = transform;
        }

        protected void Start()
        {
            if (spawnAtStart)
            {
                Spawn();
            }
        }

        /// <summary>
        /// Spawn the vehicle and enter it.
        /// </summary>
        public void Spawn()
        {
            // Create a reference to the vehicle
            Vehicle vehicle = null;

            // Create the vehicle
            switch (vehicleSpawnType)
            {
                case VehicleSpawnType.Loadout:

                    int playerVehicleIndex = PlayerData.GetSelectedVehicleIndex(itemManager);
                    if (playerVehicleIndex == -1)
                    {
                        if (itemManager != null && itemManager.vehicles.Count > 0)
                        {
                            playerVehicleIndex = 0;
                        }
                    }

                    if (playerVehicleIndex != -1)
                    {

                        // Create the vehicle
                        Transform vehicleTransform = ((GameObject)Instantiate(itemManager.vehicles[playerVehicleIndex].gameObject, vehicleSpawnPoint.position, vehicleSpawnPoint.rotation)).transform;
                        vehicle = vehicleTransform.GetComponent<Vehicle>();
                        vehicle.name = vehicleName;


                        // Get the vehicle loadout
                        List<int> selectedModuleIndexesByMount = PlayerData.GetModuleLoadout(playerVehicleIndex, itemManager);

                        bool hasLoadout = false;
                        for (int i = 0; i < selectedModuleIndexesByMount.Count; ++i)
                        {
                            if (selectedModuleIndexesByMount[i] != -1)
                            {
                                hasLoadout = true;
                            }
                        }

                        // Create the vehicle loadout
                        if (hasLoadout)
                        {
                            for (int i = 0; i < selectedModuleIndexesByMount.Count; ++i)
                            {

                                if (selectedModuleIndexesByMount[i] == -1) continue;

                                Module module = GameObject.Instantiate(itemManager.modulePrefabs[selectedModuleIndexesByMount[i]], null);

                                vehicle.ModuleMounts[i].AddModule(module, true);

                            }
                        }
                        else
                        {
                            for (int i = 0; i < vehicle.ModuleMounts.Count; ++i)
                            {
                                vehicle.ModuleMounts[i].createDefaultModulesAtStart = true;
                            }
                        }
                    }

                    break;

                case VehicleSpawnType.Prefab:

                    vehicle = Instantiate(vehiclePrefab, vehicleSpawnPoint.position, vehicleSpawnPoint.rotation);

                    break;

            }

            if (vehicle != null)
            {
                // Set child vehicle that the game agent will exit to when exiting this vehicle.
                VehicleEnterExitManager enterExitManager = vehicle.GetComponent<VehicleEnterExitManager>();
                if (startingChildVehicle != null)
                {
                    if (enterExitManager != null)
                    {
                        if (startingChildVehicle.CanEnter(enterExitManager))
                        {
                            enterExitManager.SetChild(startingChildVehicle);
                        }
                    }
                }

                switch (startingOccupantType)
                {
                    case VehicleOccupantType.Scene:

                        if (startingOccupant != null)
                        {
                            startingOccupant.EnterVehicle(vehicle);
                        }
                        break;

                    case VehicleOccupantType.SpawnFromPrefab:

                        if (startingOccupant != null)
                        {
                            GameAgent spawnedOccupant = Instantiate(startingOccupant, Vector3.zero, Quaternion.identity);
                            spawnedOccupant.EnterVehicle(vehicle);
                            break;
                        }
                        break;

                    case VehicleOccupantType.GameAgentManagerFocusedGameAgent:

                        if (GameAgentManager.Instance.FocusedGameAgent == null)
                        {
                            Debug.LogError("No focused game agent exists to enter the spawned vehicle.");
                        }
                        else
                        {
                            GameAgentManager.Instance.FocusedGameAgent.EnterVehicle(vehicle);
                        }
                        break;
                }

                // Call the event
                onVehicleSpawned.Invoke(vehicle);
            }
        }
    }
}