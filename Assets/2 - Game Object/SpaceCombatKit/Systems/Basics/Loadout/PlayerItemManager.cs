using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace VSX.UniversalVehicleCombat
{
	/// <summary>
    /// This class holds references to vehicles and modules that are shown on the loadout menu.
    /// </summary>
	public class PlayerItemManager : MonoBehaviour 
	{
		public List<Vehicle> vehicles = new List<Vehicle>();

        public List<Module> modulePrefabs = new List<Module>();
    }
}