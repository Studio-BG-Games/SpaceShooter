using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Derived class for TMPro UGUI text
    /// </summary>
    public class UVCTextTMProUGUI : UVCText
    {
        [SerializeField]
        protected TextMeshProUGUI textTMProUGUI;

        /// <summary>
        /// Get/set the text contents.
        /// </summary>
        public override string text
        {
            get { return textTMProUGUI.text; }
            set { textTMProUGUI.text = value; }
        }

        /// <summary>
        /// Get/set the color.
        /// </summary>
        public override Color color
        {
            get { return textTMProUGUI.color; }
            set { textTMProUGUI.color = value; }
        }

        protected virtual void Reset()
        {
            textTMProUGUI = GetComponent<TextMeshProUGUI>();
        }
    }
}

