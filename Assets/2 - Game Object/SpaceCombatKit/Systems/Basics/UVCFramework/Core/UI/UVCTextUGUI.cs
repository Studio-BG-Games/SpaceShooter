using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Derived class for UGUI text
    /// </summary>
    public class UVCTextUGUI : UVCText
    {
        [SerializeField]
        protected Text textUGUI;

        /// <summary>
        /// Get/set the text.
        /// </summary>
        public override string text
        {
            get { return textUGUI.text; }
            set { textUGUI.text = value; }
        }

        /// <summary>
        /// Get/set the color.
        /// </summary>
        public override Color color
        {
            get { return textUGUI.color; }
            set { textUGUI.color = value; }
        }

        protected virtual void Reset()
        {
            textUGUI = GetComponent<Text>();
        }
    }
}
