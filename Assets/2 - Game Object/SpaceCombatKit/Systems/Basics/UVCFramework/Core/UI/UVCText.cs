using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Base class for UGUI and TMPro texts.
    /// </summary>
    public class UVCText : MonoBehaviour
    {
        /// <summary>
        /// Get/set the text
        /// </summary>
        public virtual string text
        {
            get { return ""; }
            set { }
        }

        /// <summary>
        /// Get/set the color
        /// </summary>
        public virtual Color color
        {
            get { return Color.black; }
            set { }
        }
    }
}
