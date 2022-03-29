using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{

    // Unity event for running functions when the triggerable's trigger level is set
    [System.Serializable]
    public class OnSetTriggerLevelEventHandler : UnityEvent<float> { }


    /// <summary>
    /// Provides a way to make a module triggerable by connecting trigger inputs to actions.
    /// </summary>
    public class Triggerable : MonoBehaviour
    {

        [Header("General")]

        [Tooltip("Whether this Triggerable should be ignored by the TriggerablesManager on a vehicle (set to true when using an auto turret).")]
        [SerializeField]
        protected bool managedLocally = false;
        public bool ManagedLocally { get { return managedLocally; } }

        [Tooltip("The default trigger index to which this Triggerable is assigned.")]
        [SerializeField]
        protected int defaultTriggerIndex = 0;
        public int DefaultTriggerIndex { 
            get { return defaultTriggerIndex; }
            set { defaultTriggerIndex = value; }
        }

        // Different triggering modes
        public enum TriggerMode
        {
            Single,
            Burst,
            Automatic,
            OnOff
        }

        [Tooltip("The trigger mode for this triggerable.")]
        [SerializeField]
        protected TriggerMode triggerMode = TriggerMode.Automatic;
        public TriggerMode TriggerModeSetting
        {
            get { return triggerMode; }
            set { triggerMode = value; }
        }

        protected bool triggering = false;
        protected float triggerChangeStartTime;

        [Header("Auto/Burst")]

        [Tooltip("The interval between actions during automatic mode, or during a burst.")]
        [SerializeField]
        protected float actionInterval = 0.15f;
        public float ActionInterval
        {
            get { return actionInterval; }
            set { actionInterval = value; }
        }

        protected Coroutine triggeringCoroutine;            // Coroutine for automatic/burst fire
        protected bool coroutineRunning = false;

        [Tooltip("The number of actions in a burst.")]
        [SerializeField]
        protected int burstSize = 3;

        [Tooltip("Whether to repeat the burst while the triggering remains on.")]
        [SerializeField]
        protected bool repeatBurst = false;

        [Tooltip("The interval between bursts when Repeat Burst is enabled.")]
        [SerializeField]
        protected float burstInterval = 0.5f;

        [Header("Events")]

        // Start triggering event
        public UnityEvent onStartTriggering;

        // Stop triggering event
        public UnityEvent onStopTriggering;

        // Set trigger level event
        public OnSetTriggerLevelEventHandler onSetTriggerLevel;

        // Action event
        public UnityEvent onAction;


        /// <summary>
        /// Start triggering the triggerable.
        /// </summary>
        public virtual void StartTriggering()
        {
            // Don't trigger if disabled
            if (!gameObject.activeInHierarchy) return;

            // If coroutine is still running, do nothing
            if (coroutineRunning) return;

            // Start triggering
            triggering = true;
            triggerChangeStartTime = Time.time;

            triggeringCoroutine = StartCoroutine(TriggeringCoroutine());

            // Call the event
            onStartTriggering.Invoke();
        }


        /// <summary>
        /// Stop triggering the triggerable.
        /// </summary>
        public virtual void StopTriggering()
        {
            triggering = false;
            triggerChangeStartTime = Time.time;

            onStopTriggering.Invoke();
        }


        protected virtual void OnDisable()
        {
            triggering = false;
            coroutineRunning = false;
        }

        // Triggering coroutine
        protected virtual IEnumerator TriggeringCoroutine()
        {

            coroutineRunning = true;

            switch (triggerMode)
            {
                case TriggerMode.Single:

                    Action(1);
                    break;

                case TriggerMode.Burst:
                    
                    while (true)
                    {

                        if (!triggering) break;

                        // Burst
                        for (int i = 0; i < burstSize; ++i)
                        {
                            Action(1);
                            yield return new WaitForSeconds(actionInterval);
                        }

                        // If not repeating burst, or is repeating but triggering has stopped, finish
                        if (!repeatBurst || !triggering) break;

                        // Wait between bursts
                        yield return new WaitForSeconds(burstInterval);
                    }

                    break;

                case TriggerMode.Automatic:

                    while (true)
                    {
                        Action(1);

                        yield return new WaitForSeconds(actionInterval);

                        if (!triggering) break;
                    }

                    break;
            }

            coroutineRunning = false;

        }


        /// <summary>
        /// Called to implement the action that the triggerable triggers
        /// </summary>
        public virtual void Action(float actionLevel)
        {
            onAction.Invoke();
        }

        
        /// <summary>
        /// Trigger the triggerable once.
        /// </summary>
        public virtual void TriggerOnce()
        {
            StartTriggering();
            StopTriggering();
        }

        /// <summary>
        /// Set the triggerable's trigger level.
        /// </summary>
        /// <param name="level">The trigger level.</param>
        public virtual void SetTriggerLevel(float level)
        {
            onSetTriggerLevel.Invoke(level);
        }
    }
}