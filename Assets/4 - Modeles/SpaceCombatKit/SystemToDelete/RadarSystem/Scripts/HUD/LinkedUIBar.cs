using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Manages a UI fill bar to show a value from 0 to 1, reading from a LinkableVariable.
    /// </summary>
    public class LinkedUIBar : MonoBehaviour
    {

        [SerializeField]
        protected Image barImage;

        [SerializeField]
        protected string key;

        [SerializeField]
        protected bool disableIfValueMissing = true;


        /// <summary>
        /// Set the bar fill amount based on a linked variable from a Trackable component.
        /// </summary>
        /// <param name="trackable">The trackable where the linked variable is stored.</param>
        public virtual void Set(Trackable trackable)
        {
            Set(trackable.variablesDictionary);
        }

        /// <summary>
        /// Set the bar fill amount based on a linked variable from a linked variables dictionary/
        /// </summary>
        /// <param name="variablesDictionary">The dictionary of linked variables.</param>
        public virtual void Set(Dictionary<string, LinkableVariable> variablesDictionary)
        {
            if (variablesDictionary.ContainsKey(key))
            {
                gameObject.SetActive(true);
                Set(variablesDictionary[key].FloatValue);
            }
            else
            {
                if (disableIfValueMissing) gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Set the bar fill amount based on a float value from 0-1;
        /// </summary>
        /// <param name="fillAmount">The fill amount.</param>
        public virtual void Set(float fillAmount)
        {
            barImage.fillAmount = fillAmount;
        }
    }
}
