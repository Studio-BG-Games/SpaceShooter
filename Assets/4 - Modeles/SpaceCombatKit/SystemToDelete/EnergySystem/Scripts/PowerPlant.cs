using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class represents a powerplant module that can be loaded onto a vehicle.
    /// </summary>
	public class PowerPlant : MonoBehaviour
	{

        [SerializeField]
        protected float output = 1000;
		public float Output { get { return output; } }
        
	}
}
