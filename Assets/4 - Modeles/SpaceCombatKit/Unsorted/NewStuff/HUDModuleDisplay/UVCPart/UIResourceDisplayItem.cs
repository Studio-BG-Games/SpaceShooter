using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Display information about a resource for a resource container.
    /// </summary>
    [System.Serializable]
    public class UIResourceDisplayItem
    {
        [Tooltip("The resource type to display information for.")]
        public ResourceType resourceType;

        [Tooltip("The object to activate/deactivate to display/hide the resource info.")]
        public GameObject toggleObject;

        [Tooltip("The text to display resource information. Leave empty if not desired.")]
        public UVCText resourceDisplayText;

        [Tooltip("The display bar to display resource amount information. Leave empty if not desired.")]
        public UIFillBarController resourceDisplayBar;

        [Tooltip("The resource container to display resource information for.")]
        public ResourceContainerBase resourceContainer;

        /// <summary>
        /// Update the resource information.
        /// </summary>
        public virtual void UpdateDisplay()
        {
            if (resourceContainer != null)
            { 
                // Display resource info on text
                if (resourceDisplayText != null)
                {
                    resourceDisplayText.text = resourceContainer.CurrentAmountInteger.ToString() + "/" + resourceContainer.CapacityInteger.ToString();
                }

                // Display resource info on bar
                if (resourceDisplayBar != null)
                {
                    resourceDisplayBar.SetFillAmount(resourceContainer.CurrentAmountFloat / resourceContainer.CapacityFloat);
                }
            }
        }
    }
}

