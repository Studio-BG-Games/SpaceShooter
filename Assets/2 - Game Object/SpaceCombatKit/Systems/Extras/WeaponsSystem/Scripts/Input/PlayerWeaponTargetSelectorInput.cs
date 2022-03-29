using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{
    public class PlayerWeaponTargetSelectorInput : VehicleInput
    {

        [Header("Settings")]
        
        public CustomInput nextTargetInput = new CustomInput("Target Selection", "Next", KeyCode.Greater);
        public CustomInput previousTargetInput = new CustomInput("Target Selection", "Previous", KeyCode.Less);
        public CustomInput nearestTargetInput = new CustomInput("Target Selection", "Nearest", KeyCode.N);
        public CustomInput frontTargetInput = new CustomInput("Target Selection", "Front", KeyCode.M);

        TargetSelector weaponTargetSelector;


        /// <summary>
        /// Initialize this input component.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>Whether initialization succeeded.</returns>
        protected override bool Initialize(Vehicle vehicle)
        {
          
            Weapons weapons = vehicle.GetComponent<Weapons>();
            if (weapons == null) return false;

            weaponTargetSelector = weapons.WeaponsTargetSelector;
            if (weaponTargetSelector == null) return false;

            return true;

        }

        protected override void InputUpdate()
        {

            // Select next target
            if (nextTargetInput.Down())
            {
                weaponTargetSelector.Cycle(true);
            }

            // Select previous target
            if (previousTargetInput.Down())
            {
                weaponTargetSelector.Cycle(false);
            }

            // Select nearest target
            if (nearestTargetInput.Down())
            {
                weaponTargetSelector.SelectNearest();
            }

            // Select front target
            if (frontTargetInput.Down())
            {
                weaponTargetSelector.SelectFront();
            }
        }
    }
}