using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Manages a text field that displays a value from a dictionary of LinkedVariables, based on a specified key.
    /// </summary>
    public class LinkedUIText : MonoBehaviour
    {

        [SerializeField]
        protected UVCText text;

        [SerializeField]
        protected string key;

        [SerializeField]
        protected string valueNotFoundDisplay = "";

        /// <summary>
        /// Set the text from the linked variable of specified key found in the Trackable's linked variable dictionary.
        /// </summary>
        /// <param name="trackable">The Trackable component.</param>
        public virtual void Set(Trackable trackable)
        {
            if (trackable != null)
            {
                Set(trackable.variablesDictionary);
            }
        }

        /// <summary>
        /// Set the text from the linked variable of specified key found in the provided linked variable dictionary.
        /// </summary>
        /// <param name="variablesDictionary">The variables dictionary.</param>
        public virtual void Set(Dictionary<string, LinkableVariable> variablesDictionary)
        {
            // Look for the variable
            if (variablesDictionary.ContainsKey(key))
            {
                Set(variablesDictionary[key].StringValue);
            }
            // Empty the text
            else
            {
                Set(valueNotFoundDisplay);
            }
        }

        /// <summary>
        /// Set the text contents.
        /// </summary>
        /// <param name="value">The text contents string.</param>
        public virtual void Set(string value)
        {
            text.text = value;
        }
    }
}
