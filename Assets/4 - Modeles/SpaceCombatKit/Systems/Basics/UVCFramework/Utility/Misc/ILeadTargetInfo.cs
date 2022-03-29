using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Interface for a component that stores lead target position information
    /// </summary>
    public interface ILeadTargetInfo
    {
        Transform Target { get; }

        Vector3[] LeadTargetPositions { get; }
    }
}

