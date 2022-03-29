using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Controls a group item in the controls menu.
    /// </summary>
    public class ControlsMenuGroupItem : MonoBehaviour
    {
        [SerializeField]
        protected UVCText groupLabelText;

        [SerializeField]
        protected bool toUpper = true;

        /// <summary>
        /// Set the parameters of this group item.
        /// </summary>
        /// <param name="groupLabel">The label for this group item.</param>
        public void Set(string groupLabel)
        {
            groupLabelText.text = toUpper ? groupLabel.ToUpper() : groupLabel;
        }
    }
}