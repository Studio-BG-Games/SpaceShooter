using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{
	
	/// <summary>
    /// This class controls an indicator bar on the power management menu that shows the ratio of fixed vs distributable power.
    /// </summary>
	public class SubsystemPowerInfoController : MonoBehaviour 
	{
	
		[SerializeField]
        protected LayoutElement fixedPowerBar;
	
		[SerializeField]
        protected LayoutElement distributablePowerBar;
	
		[SerializeField]
        protected RectTransform barsParent;
	
		[SerializeField]
        protected Text subsystemPowerText;


        /// <summary>
        /// Update the indicator.
        /// </summary>
        /// <param name="subsystemTotalPower">The total power available to the subsystem.</param>
        /// <param name="shipTotalPower">The total power available to the ship.</param>
        /// <param name="subsystemDistributablePowerValue">The amount of distributable power assigned to the subsystem.</param>
        public void SetPowerValues (float subsystemTotalPower, float shipTotalPower, float subsystemDistributablePowerValue){
	
			float maxWidth = barsParent.sizeDelta.x;
			float adjustableFraction = subsystemDistributablePowerValue / subsystemTotalPower;
			distributablePowerBar.preferredWidth = adjustableFraction * maxWidth * (subsystemTotalPower / shipTotalPower);
			fixedPowerBar.preferredWidth = (1 - adjustableFraction) * maxWidth * (subsystemTotalPower / shipTotalPower);
	
			subsystemPowerText.text = Mathf.RoundToInt(subsystemTotalPower).ToString() + " kW";
		}
	}
}
