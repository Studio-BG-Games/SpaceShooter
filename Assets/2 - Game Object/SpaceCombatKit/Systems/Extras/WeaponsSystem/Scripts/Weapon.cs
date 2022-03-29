using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Base class for a weapon.
    /// </summary>
    public class Weapon : MonoBehaviour, IAimer
    {
        [Tooltip("The Triggerable component that controls this weapon.")]
        [SerializeField]
        protected Triggerable triggerable;
        public virtual Triggerable Triggerable
        {
            get { return triggerable; }
        }

        protected Module module;

        [Tooltip("The weapon units that make up this weapon. Typically these would all be multiples of exactly the same weapon unit.")]
        [SerializeField]
        protected List<WeaponUnit> weaponUnits = new List<WeaponUnit>();
        public List<WeaponUnit> WeaponUnits { get { return weaponUnits; } }


        [Tooltip("Add any boolean functions here that have to return True for the weapon to fire.")]
        [SerializeField]
        protected Conditions firingConditions;
        public virtual Conditions FiringConditions
        {
            get { return firingConditions; }
            set { firingConditions = value; }
        }

        [Header ("Multi Weapon Settings")]

        [Tooltip("How the weapon behaves when it is made up of multiple weapon units.")]
        [SerializeField]
        protected MultiWeaponFiringMode multiWeaponFiringMode;
        public virtual MultiWeaponFiringMode MultiWeaponFiringMode
        {
            get { return multiWeaponFiringMode; }
            set { multiWeaponFiringMode = value; }
        }

        [Tooltip("Whether the weapon should always start firing from a specific weapon unit when weapon units are fired sequentially.")]
        [SerializeField]
        protected bool specifySequentialFiringStartIndex = false;
        public virtual bool SpecifySequentialFiringStartIndex
        {
            get { return specifySequentialFiringStartIndex; }
            set { specifySequentialFiringStartIndex = value; }
        }

        [Tooltip("The index in the Weapon Units list of the weapon that begins firing when firing sequentially.")]
        [SerializeField]
        protected int sequentialFiringStartIndex = 0;
        public virtual int SequentialFiringStartIndex
        {
            get { return sequentialFiringStartIndex; }
            set { sequentialFiringStartIndex = value; }
        }

        [Header("Resource Usage")]

        [Tooltip("Whether resource changes should be applied each time a weapon unit is fired (rather than each time the weapon is fired as a whole).")]
        [SerializeField]
        protected bool applyResourceUsagePerWeaponUnit = true;

        [Tooltip("All the resources that this weapon uses or creates.")]
        [SerializeField]
        protected List<ResourceHandler> resourceHandlers = new List<ResourceHandler>();
        public List<ResourceHandler> ResourceHandlers { get { return resourceHandlers; } }

        protected int nextTriggerIndex = -1;

        protected int numWeaponUnits = 0;

        protected bool triggering = false;



        /// <summary>
        /// Get the damage for this weapon for a specific health type.
        /// </summary>
        /// <param name="healthType">The Health Type to get weapon damage for.</param>
        /// <returns>The weapon damage.</returns>
        public virtual float Damage(HealthType healthType)
        {
            float damage = 0;
            for(int i = 0; i < weaponUnits.Count; ++i)
            {
                damage += weaponUnits[i].Damage(healthType);
            }

            return damage * FireRate;
        }


        /// <summary>
        /// Get the speed of this weapon.
        /// </summary>
        public virtual float Speed
        {
            get
            {
                return weaponUnits.Count > 0 ? weaponUnits[0].Speed : 0;
            }
        }


        /// <summary>
        /// Get the range of this weapon.
        /// </summary>
        public virtual float Range
        {
            get
            {
                return weaponUnits.Count > 0 ? weaponUnits[0].Range : 0;
            }
        }


        /// <summary>
        /// Get the fire rate of this weapon (shots/second).
        /// </summary>
        public virtual float FireRate
        {
            get
            {
                return (triggerable.TriggerModeSetting == Triggerable.TriggerMode.Automatic ? 1 / triggerable.ActionInterval : 1);
            }
        }


        // Called when the component is first added to a gameobject, or reset in the inspector.
        protected virtual void Reset()
        {
            // Add a Module component
            module = GetComponentInChildren<Module>();
            if (module == null)
            {
                gameObject.AddComponent<Module>();
            }

            // Get/add a Triggerable component
            triggerable = GetComponentInChildren<Triggerable>();
            if (triggerable == null)
            {
                triggerable = gameObject.AddComponent<Triggerable>();
            }

            // Get all weapon units in hierarchy
            weaponUnits = new List<WeaponUnit>(transform.GetComponentsInChildren<WeaponUnit>());
        }


        // Called before the scene starts, or when the object is instantiated
        protected virtual void Awake()
        {
            // Set up to stop firing when module is deactivated
            module = GetComponentInChildren<Module>();
            
            // Connect to triggerable component
            triggerable.onStartTriggering.AddListener(StartTriggering);
            triggerable.onStopTriggering.AddListener(StopTriggering);
            triggerable.onAction.AddListener(TriggerOnce);

            numWeaponUnits = weaponUnits.Count;
          
            firingConditions.Initialize();
        }


        // Called when the scene starts, or after the object is instantiated
        protected virtual void Start()
        {
            // Initialize the next trigger index for sequential firing
            nextTriggerIndex = numWeaponUnits == 0 ? -1 : 0;
        }


        /// <summary>
        /// Aim the weapon at a specific world position.
        /// </summary>
        /// <param name="aimPosition">The world space aim position.</param>
        public virtual void Aim(Vector3 aimPosition)
        {
            for(int i = 0; i < weaponUnits.Count; ++i)
            {
                weaponUnits[i].Aim(aimPosition);
            }
        }


        /// <summary>
        /// Remove any aiming of the weapon.
        /// </summary>
        public virtual void ClearAim()
        {
            for (int i = 0; i < weaponUnits.Count; ++i)
            {
                weaponUnits[i].ClearAim();
            }
        }


        // Start triggering this weapon
        protected virtual void StartTriggering()
        {
            // Check if the weapon can be triggered
            if (!CanTriggerWeapon()) return;

            switch (multiWeaponFiringMode)
            {
                case MultiWeaponFiringMode.Sequential:

                    // Start triggering the next weapon unit
                    if (nextTriggerIndex < numWeaponUnits)
                    {
                        StartTriggeringWeaponUnit(nextTriggerIndex);
                    }

                    // Iterate weapon units here if the weapon operates continuously rather than in a set of individual actions
                    if (triggerable.TriggerModeSetting == Triggerable.TriggerMode.OnOff)
                    {
                        IterateWeaponUnits();
                    }

                    break;

                default:

                    // Start triggering all weapon units
                    for(int i = 0; i < numWeaponUnits; ++i)
                    {
                        StartTriggeringWeaponUnit(i);
                    }

                    break;
            }

            triggering = true;
        }


        // Stop triggering this weapon
        protected virtual void StopTriggering()
        {
            // Stop triggering all weapon units
            for (int i = 0; i < numWeaponUnits; ++i)
            {
                StopTriggeringWeaponUnit(i);
            }

            triggering = false;
        }


        // Trigger this weapon once
        protected virtual void TriggerOnce()
        {
            // Check if this weapon can be triggered
            if (!CanTriggerWeapon()) return;

            switch (multiWeaponFiringMode)
            {
                case MultiWeaponFiringMode.Sequential:
                    
                    // Trigger the next weapon unit once
                    if (nextTriggerIndex < numWeaponUnits)
                    {
                        TriggerWeaponUnitOnce(nextTriggerIndex);
                    }

                    // Move to the next weapon for sequential firing
                    IterateWeaponUnits();

                    break;

                default:

                    // Trigger all weapon units once
                    for (int i = 0; i < numWeaponUnits; ++i)
                    {
                        TriggerWeaponUnitOnce(i);
                    }

                    break;
            }

            // Use resources
            for (int i = 0; i < resourceHandlers.Count; ++i)
            {
                if (!resourceHandlers[i].perSecond)
                {
                    resourceHandlers[i].Implement((multiWeaponFiringMode == MultiWeaponFiringMode.Simultaneous && applyResourceUsagePerWeaponUnit) ? weaponUnits.Count : 1);
                }
            }
        }


        // Start triggering a specific weapon unit
        protected virtual void StartTriggeringWeaponUnit(int index) 
        {
            weaponUnits[index].StartTriggering();
        }


        // Stop triggering a specific weapon unit
        protected virtual void StopTriggeringWeaponUnit(int index) 
        {
            weaponUnits[index].StopTriggering();

            // When weapon stops firing, go to the specified start index again for sequential firing
            if (specifySequentialFiringStartIndex)
            {
                nextTriggerIndex = sequentialFiringStartIndex;
            }
        }


        // Trigger a specific weapon unit once
        protected virtual void TriggerWeaponUnitOnce(int index) 
        {
            weaponUnits[index].TriggerOnce();
        }


        // Whether the weapon can be triggered
        protected virtual bool CanTriggerWeapon()
        {
            if (module != null && !module.IsActivated) return false;

            // Check firing conditions
            if (!firingConditions.ConditionsMet) return false;

            // Check if weapon units can be triggered
            if (multiWeaponFiringMode == MultiWeaponFiringMode.Simultaneous)
            {
                for (int i = 0; i < weaponUnits.Count; ++i)
                {
                    if (!weaponUnits[i].CanTrigger) return false;
                }
            }
            else
            {
                if (nextTriggerIndex != -1 && !weaponUnits[nextTriggerIndex].CanTrigger) return false;
            }

            // Check if required resources are available
            for (int i = 0; i < resourceHandlers.Count; ++i)
            {
                if (!resourceHandlers[i].Ready((multiWeaponFiringMode == MultiWeaponFiringMode.Simultaneous && applyResourceUsagePerWeaponUnit) ? weaponUnits.Count : 1)) return false;
            }

            return true;
        }


        // Called every frame that the weapon is triggering
        protected virtual void OnWeaponTriggering() 
        {
            // If the weapon suddenly cannot be triggered any more, stop firing
            if (triggering && !CanTriggerWeapon())
            {
                StopTriggering();
            }
            else
            {
                // Use resources every frame that the weapon is firing
                for(int i = 0; i < resourceHandlers.Count; ++i)
                {
                    if (resourceHandlers[i].perSecond)
                    {
                        resourceHandlers[i].Implement((multiWeaponFiringMode == MultiWeaponFiringMode.Simultaneous && applyResourceUsagePerWeaponUnit) ? weaponUnits.Count : 1);
                    }
                }
            }
        }


        // Iterate through weapons for sequential firing of a weapon made up of multiple weapon units.
        protected virtual void IterateWeaponUnits()
        {
            nextTriggerIndex++;

            if (nextTriggerIndex >= numWeaponUnits)
            {
                nextTriggerIndex = 0;
            }
        }


        // Called every frame
        protected virtual void Update()
        {
            if (triggering)
            {
                OnWeaponTriggering();
            }
        }
    }
}
