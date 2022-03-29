using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Manages a single item on th controls menu.
    /// </summary>
    public class ControlsMenuItem : MonoBehaviour
    {
        [SerializeField]
        protected UVCText actionText;

        [SerializeField]
        protected UVCText inputText;

        /// <summary>
        /// Set the parameters for this controls menu item. 
        /// </summary>
        /// <param name="action">The action for this control input.</param>
        /// <param name="input">The control input.</param>
        public void Set(string action, string input)
        {
            actionText.text = action;
            inputText.text = input;
        }
    }
}