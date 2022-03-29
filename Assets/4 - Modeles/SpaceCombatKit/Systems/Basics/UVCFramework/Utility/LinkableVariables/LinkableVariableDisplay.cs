using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Display a linkable variable on a UGUI Text object.
    /// </summary>
    public class LinkableVariableDisplay : MonoBehaviour
    {
        [Tooltip("The linkable variable to display.")]
        [SerializeField]
        protected LinkableVariable variable;

        [Tooltip("The text field to show the linkable variable.")]
        [SerializeField]
        protected Text text;

        protected void Awake()
        {
            variable.InitializeLinkDelegate();
        }

        protected void Update()
        {
            text.text = variable.StringValue;
        }
    }
}
