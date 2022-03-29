using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Interface for something that aims (for example a weapon). Used to control aiming of weapons etc.
    /// </summary>
    public interface IAimer
    {
        void Aim(Vector3 aimPosition);

        void ClearAim();
    }
}
