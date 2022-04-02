using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Associates a custom input with a trigger index.
    /// </summary>
    [System.Serializable]
    public class TriggerInput
    {
        public int triggerIndex;
        public CustomInput inputSettings;
    }
}
