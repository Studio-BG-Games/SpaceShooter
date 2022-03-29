using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace VSX.UniversalVehicleCombat 
{ 

	/// <summary>
    /// Manages a label for a triggerable module in the trigger groups menu.
    /// </summary>
	public class TriggerGroupsMenuLabelItemController : MonoBehaviour 
	{
        [Tooltip("The Text field for this label item in the trigger groups menu.")]
		[SerializeField]
		protected Text itemText;

		/// <summary>
        /// Set the triggerable module label in the menu.
        /// </summary>
        /// <param name="newValue">The new label.</param>
		public void SetLabel(string newValue)
		{
			itemText.text = newValue;
		}
	}
}