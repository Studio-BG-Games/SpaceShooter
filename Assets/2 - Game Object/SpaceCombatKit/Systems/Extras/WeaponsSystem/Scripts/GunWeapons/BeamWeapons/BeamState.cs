using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    ///  The different states for a beam controller.
    /// </summary>
    public enum BeamState
    {
        Off,
        FadingIn,
        FadingOut,
        Sustaining,
        Pulsing
    }
}
