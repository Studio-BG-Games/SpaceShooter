using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{  
    /// <summary>
    /// The Trigger class determines how the Triggerables assigned to a specified trigger index will be fired when the input is pressed.
    /// </summary>
    [System.Serializable]
    public class Trigger
    {
        [Tooltip("The trigger index for this Trigger.")]
        public int triggerIndex;

        [Tooltip("Whether to trigger all the Triggerables on this Trigger sequentially (triggered simultaneously if not checked).")]
        public bool triggerSequentially = false;

        [Tooltip("The interval between triggerables fired sequentially.")]
        public float triggerInterval = 0.25f;

        [HideInInspector]
        public int lastTriggeredIndex = -1;

        [HideInInspector]
        public float lastTriggeredTime;

        [HideInInspector]
        public bool isTriggering = false;
        
    }
}