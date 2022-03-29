using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat.Radar
{
   
    /// <summary>
    /// Unity event for running functions when the HUD radar widget is updated.
    /// </summary>
    [System.Serializable]
    public class OnHUDRadarWidgetUpdatedEventHandler : UnityEvent <Trackable> { }

    /// <summary>
    /// Controls a widget displayed on the Radar of a HUD
    /// </summary>
    public class HUDRadarWidget : MonoBehaviour
    {
        [Header("Settings")]

        [SerializeField]
        protected UIColorManager colorManager;

        [SerializeField]
        protected RectTransform foot;
        protected bool hasFoot = false;

        [SerializeField]
        protected RectTransform leg;
        protected bool hasLeg = false;

        [SerializeField]
        protected List<RectTransform> selectedUIObjects = new List<RectTransform>();

        [Header("Events")]

        // Widget updated event
        public OnHUDRadarWidgetUpdatedEventHandler onUpdated;


        private void Awake()
        {
            hasFoot = foot != null;
            hasLeg = leg != null;
        }

        /// <summary>
        /// Set the color of the radar widget.
        /// </summary>
        /// <param name="newColor">The new color of the radar widget.</param>
        public virtual void SetColor(Color newColor)
        {
            // Update the color manager
            if (colorManager != null) colorManager.SetColor(newColor);
        }

        /// <summary>
        /// Set whether this radar widget is selected.
        /// </summary>
        /// <param name="isSelected">Whether this radar widget is selected.</param>
        public virtual void SetSelected(bool isSelected)
        {
            // Activate/deactivate all the selected UI objects
            for (int i = 0; i < selectedUIObjects.Count; ++i)
            {
                selectedUIObjects[i].gameObject.SetActive(isSelected);
            } 
        }

        /// <summary>
        /// Set whether the radar widget is clamped to the border of the radar.
        /// </summary>
        /// <param name="isClampedToBorder">Whether the widget is clamped to the border.</param>
        public virtual void SetIsClampedToBorder(bool isClampedToBorder)
        {
            // Activate/deactivate the leg and foot
            if (hasLeg) leg.gameObject.SetActive(!isClampedToBorder);
            if (hasFoot) foot.gameObject.SetActive(!isClampedToBorder);
        }

        /// <summary>
        /// Update the widget with a target reference.
        /// </summary>
        /// <param name="trackable">The target component.</param>
        public virtual void UpdateRadarWidget(Trackable trackable)
        {
            // Update the foot
            if (hasFoot) foot.localPosition = new Vector3(0f, 0f, -1 * transform.localPosition.z);

            // Update the leg 
            if (hasLeg)
            {
                leg.localPosition = new Vector3(0f, 0f, -0.5f * transform.localPosition.z);
                leg.sizeDelta = new Vector2(leg.sizeDelta.x, Mathf.Abs(transform.localPosition.z));
            }  
        }
    }
}
