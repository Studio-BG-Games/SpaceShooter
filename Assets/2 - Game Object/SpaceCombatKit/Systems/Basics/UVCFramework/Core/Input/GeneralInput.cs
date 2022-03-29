using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Base class for vehicle input components.
    /// </summary>
    public class GeneralInput : MonoBehaviour
    {

        [Header("General Input")]

        // Whether this input component has everything it needs to run
        protected bool initialized = false;
        public bool Initialized { get { return initialized; } }

        [SerializeField]
        protected bool activateInputAtStart = true;

        // Whether this input component is currently activated
        protected bool inputEnabled = true;
        public virtual bool InputEnabled { get { return inputEnabled; } }

        [SerializeField]
        protected Conditions inputUpdateConditions = new Conditions();

        [SerializeField]
        protected bool debugInitialization = false;

        protected virtual void Awake()
        {
            inputUpdateConditions.Initialize();
        }

        protected virtual void Start()
        {
            if (activateInputAtStart)
            {
                initialized = Initialize();
            }
        }

        /// <summary>
        /// Start running this input script.
        /// </summary>
        public virtual void EnableInput()
        {
            inputEnabled = true;
        }

        /// <summary>
        /// Stop running this input script.
        /// </summary>
        public virtual void DisableInput()
        {
            inputEnabled = false;
        }


        /// <summary>
        /// Attempt to initialize the input component.
        /// </summary>
        /// <returns> Whether initialization was successful. </returns>
        protected virtual bool Initialize()
        {
            return true;
        }

        /// <summary>
        /// Put all your input code in an override of this method.
        /// </summary>
        protected virtual void InputUpdate() { }


        /// <summary>
        /// Called every frame that this input script was not able to run.
        /// </summary>
        protected virtual void OnInputUpdateFailed() { }
        
       
        protected virtual void Update()
        {
            if (initialized && inputEnabled && inputUpdateConditions.ConditionsMet)
            {
                InputUpdate();
            }
            else
            {
                OnInputUpdateFailed();
            }
        }
    }
}