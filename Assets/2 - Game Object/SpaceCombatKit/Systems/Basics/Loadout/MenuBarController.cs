using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages a single bar indicator in the UI.
    /// </summary>
	public class MenuBarController : MonoBehaviour 
	{
	
		[SerializeField]
		protected GameObject barParent;

		[SerializeField]
		protected Image bar;
	
		
        /// <summary>
        /// Make the bar visible.
        /// </summary>
		public void EnableBar()
		{
			barParent.SetActive(true);
		}
	

        /// <summary>
        /// Hide the bar.
        /// </summary>
		public void DisableBar()
		{
			barParent.SetActive(false);
		}
	

        /// <summary>
        /// Set the fill value in the bar.
        /// </summary>
        /// <param name="val">The new fill value for the bar.</param>
		public void SetValue(float val)
		{
			bar.fillAmount = val;
		}		
	}
}
