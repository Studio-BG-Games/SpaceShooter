using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Unity event for running functions when a child vehicle enters a parent vehicle.
    /// </summary>
    [System.Serializable]
    public class OnChildVehicleEnteredParentEventHandler : UnityEvent { }

    /// <summary>
    /// Unity event for running functions when a child vehicle exits a parent vehicle.
    /// </summary>
    [System.Serializable]
    public class OnChildVehicleExitedParentEventHandler : UnityEvent { }

    [RequireComponent(typeof(Rigidbody))]
    public class VehicleEnterExitManager : MonoBehaviour
    {

        // Child

        [SerializeField]
        protected VehicleEnterExitManager startingChildVehicle;

        protected VehicleEnterExitManager child;
        public VehicleEnterExitManager Child
        {
            get { return child; }
        }

        protected VehicleEnterExitManager parent;
        public VehicleEnterExitManager Parent { get { return parent; } }

        [SerializeField]
        protected Transform spawnPoint;
        public Transform SpawnPoint { get { return spawnPoint; } }

        // Parent

        protected List<VehicleEnterExitManager> enterableVehicles = new List<VehicleEnterExitManager>();
        public List<VehicleEnterExitManager> EnterableVehicles { get { return enterableVehicles; } }

        [SerializeField]
        protected Vehicle vehicle;
        public Vehicle Vehicle { get { return vehicle; } }

        [SerializeField]
        protected bool specifyEnterableVehicleClasses = false;

        [SerializeField]
        protected List<VehicleClass> enterableVehicleClasses = new List<VehicleClass>(); // the vehicle classes this vehicle can enter

        [SerializeField]
        protected bool disableVehicleOnEnterParent = true;


        [Header("Prompts")]

        [Tooltip("The prompt that appears when the occupant can enter or exit the vehicle.")]
        [SerializeField]
        protected UVCText promptText;

        protected string enterPrompt;
        protected string exitPrompt;

        [SerializeField]
        protected bool useEnterPromptOverrideOnTarget = false;

        [Tooltip("The default message for the prompt that appears when the occupant can enter the vehicle.")]
        [SerializeField]
        protected string enterPromptOverride;
        public string EnterPromptOverride { get { return enterPromptOverride; } }

        [SerializeField]
        protected bool overrideExitPrompt = false;

        [Tooltip("The default message for the prompt that appears when the occupant can exit the vehicle.")]
        [SerializeField]
        protected string exitPromptOverride;

        [Header("Events")]

        public OnChildVehicleEnteredParentEventHandler onChildEntered;
        public OnChildVehicleExitedParentEventHandler onChildExited;

        public OnChildVehicleEnteredParentEventHandler onEnteredParent;
        public OnChildVehicleExitedParentEventHandler onExitedParent;


        protected virtual void Reset()
        {
            vehicle = transform.root.GetComponentInChildren<Vehicle>();
            spawnPoint = transform;
        }


        protected virtual void Awake()
        {
            child = startingChildVehicle;
        }

        public virtual void SetPrompts(string enterPrompt, string exitPrompt)
        {
            if (!useEnterPromptOverrideOnTarget)
            {
                this.enterPrompt = enterPrompt;
            }
            if (!overrideExitPrompt)
            {
                this.exitPrompt = exitPrompt;
            }
        }

        public virtual void AddEnterableVehicle(VehicleEnterExitManager enterableVehicle)
        {
            if (enterableVehicle == null) return;

            if (child != null && enterableVehicle == child) return;

            if (enterableVehicles.Contains(enterableVehicle)) return;

            if (specifyEnterableVehicleClasses && !enterableVehicleClasses.Contains(enterableVehicle.Vehicle.VehicleClass))
            {
                return;
            }

            enterableVehicles.Add(enterableVehicle);
        }

        public virtual void RemoveEnterableVehicle(VehicleEnterExitManager enterableVehicle)
        {
            if (enterableVehicle == null) return;

            if (enterableVehicles.Contains(enterableVehicle))
            {
                enterableVehicles.Remove(enterableVehicle);
            }
        }

        public virtual bool CanEnter(VehicleEnterExitManager vehicleEnterExitManager)
        {
            if (specifyEnterableVehicleClasses && !enterableVehicleClasses.Contains(vehicleEnterExitManager.Vehicle.VehicleClass))
            {
                return false;
            }

            return true;

        }

        public virtual void SetChild(VehicleEnterExitManager child)
        {
            if (enterableVehicles.IndexOf(child) != -1)
            {
                enterableVehicles.Remove(child);
            }

            this.child = child;
        }

        public virtual void EnterParent(int index = 0)
        {
            if (enterableVehicles.Count > index)
            {
                parent = enterableVehicles[index];
                parent.OnChildEntered(this);
                onEnteredParent.Invoke();

                if (disableVehicleOnEnterParent)
                {
                    vehicle.gameObject.SetActive(false);
                }
            }
        }

        public virtual void OnChildEntered(VehicleEnterExitManager child)
        {
            if (enterableVehicles.IndexOf(child) != -1)
            {
                enterableVehicles.Remove(child);
            }

            this.child = child;

            onChildEntered.Invoke();
        }


        // Parent active
        public virtual bool CanExitToChild()
        {
            return (child != null);
        }

        public virtual void ExitToChild()
        {
            
            enterableVehicles.Clear();
            child.transform.position = spawnPoint.position;
            child.transform.rotation = spawnPoint.rotation;
            child.OnExitedParent();
            
            onChildExited.Invoke();
        }

        public virtual void OnExitedParent()
        {
            vehicle.gameObject.SetActive(true);
            onExitedParent.Invoke();
        }


        /// <summary>
        /// Called every frame that a collider is inside a trigger collider.
        /// </summary>
        /// <param name="other">The collider that is inside the trigger collider.</param>
        protected virtual void OnTriggerEnter(Collider other)
        {

            if (other.attachedRigidbody == null) return;

            // Get other's enter exit manager
            VehicleEnterExitManager otherEnterExitManager = other.attachedRigidbody.GetComponent<VehicleEnterExitManager>();

            // Set enterable vehicle
            if (otherEnterExitManager != null)
            {
                otherEnterExitManager.AddEnterableVehicle(this);
            }
        }

        /// <summary>
        /// Called when a collider exits a trigger collider.
        /// </summary>
        /// <param name="other">The collider that exited.</param>
        protected virtual void OnTriggerExit(Collider other)
        {

            if (other.attachedRigidbody == null) return;

            // Get other's enter exit manager
            VehicleEnterExitManager otherEnterExitManager = other.attachedRigidbody.GetComponent<VehicleEnterExitManager>();

            // Unset enterable vehicle
            if (otherEnterExitManager != null)
            {
                otherEnterExitManager.RemoveEnterableVehicle(this);
            }
        }

        protected virtual void Update()
        {
            if (useEnterPromptOverrideOnTarget && enterableVehicles.Count > 0)
            {
                enterPrompt = enterableVehicles[0].EnterPromptOverride;
            }

            if (overrideExitPrompt)
            {
                exitPrompt = exitPromptOverride;
            }

            bool activated = vehicle.Occupants.Count > 0 && vehicle.Occupants[0].IsPlayer;
            if (activated && promptText != null)
            {
                if (enterableVehicles.Count > 0)
                {
                    promptText.text = enterPrompt;
                }
                else if (CanExitToChild())
                {
                    promptText.text = exitPrompt;
                }
                else
                {
                    promptText.text = "";
                }
            }
        }
    }
}
