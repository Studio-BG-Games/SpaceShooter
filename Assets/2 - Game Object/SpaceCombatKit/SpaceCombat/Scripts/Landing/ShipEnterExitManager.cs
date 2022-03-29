using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class ShipEnterExitManager : VehicleEnterExitManager
    {

        [Header("Ship Enter/Exit Settings")]

        [Tooltip("The enter/exit manager for this ship.")]
        [SerializeField]
        protected ShipLander shipLander;

        [SerializeField]
        protected bool launchShipOnChildEnter = true;

        /// <summary>
        /// Whether the child vehicle that has entered this vehicle can exit.
        /// </summary>
        /// <returns></returns>
        public override bool CanExitToChild()
        {
            if (!base.CanExitToChild()) return false;

            if (shipLander == null) return true;

            // Only allow exiting if the ship has landed
            return (shipLander.CurrentState == ShipLander.ShipLanderState.Landed);

        }

        public override void OnChildEntered(VehicleEnterExitManager child)
        {
            base.OnChildEntered(child);

            if (shipLander != null && child != null && launchShipOnChildEnter)
            {
                shipLander.Launch();
            }
        }
    }
}