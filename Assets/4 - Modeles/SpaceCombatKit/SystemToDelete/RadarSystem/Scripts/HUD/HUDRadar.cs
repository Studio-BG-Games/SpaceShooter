using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat.Radar
{
    
    /// <summary>
    /// Manages a 2D or 3D radar on a vehicle HUD.
    /// </summary>
    public class HUDRadar : HUDComponent
    {
     
        [Header("Target Information Sources")]

        [SerializeField]
        protected List<Tracker> trackers = new List<Tracker>();

        [Header("Widgets")]

        [SerializeField]
        protected Transform widgetParent;

        [SerializeField]
        protected HUDRadarWidget defaultRadarWidget;
        protected HUDRadarWidgetContainer defaultRadarWidgetContainer;

        [SerializeField]
        protected List<HUDRadarWidgetContainer> radarWidgetOverrides = new List<HUDRadarWidgetContainer>();

        [Header("Settings")]

        [SerializeField]
        protected float widgetScale = 1;

        [SerializeField]
        protected bool clampToBorder = false;

        [SerializeField]
        protected float equatorRadius = 0.5f;

        [SerializeField]
        protected float scaleExponent = 1f;

        protected float currentZoom = 1;
        public float CurrentZoom { get { return currentZoom; } }

        protected float radarDisplayRange;

        protected Vector3 targetRelPos;

        [SerializeField]
        protected int maxNewTargetsEachFrame = 1;
        protected int numTargetsLastFrame;
        protected int displayedTargetCount;

        [SerializeField]
        private Color defaultWidgetColor = Color.white;

        [SerializeField]
        protected List<TeamColor> teamColors = new List<TeamColor>();

        [SerializeField]
        protected bool display2D = false;

        [SerializeField]
        protected bool VerticalAxisZ = true;    // Whether the vertical axis is the Z axis rather than the Y axis

      
        // Called when something is changed in the inspector
        protected void OnValidate()
        {
            // Make sure that the assigned exponent is greater or equal to 1
            scaleExponent = Mathf.Max(scaleExponent, 1);
        }


        protected override void Awake()
        {
            if (defaultRadarWidget != null)
            {
                defaultRadarWidgetContainer = new HUDRadarWidgetContainer(defaultRadarWidget);
            }

            base.Awake();

            if (hudCamera == null) hudCamera = Camera.main;

            // Make sure that the distance exponent is at least 1
            scaleExponent = Mathf.Max(scaleExponent, 1);

            currentZoom = 0f;
        }


        /// <summary>
        /// Set the zoom.
        /// </summary>
        /// <param name="zoom">The zoom.</param>
        public virtual void SetZoom(float zoom)
        {
            // The zoom must be greater than 0
            currentZoom = Mathf.Max(zoom, 0);
        }


        // Visualize a target tracked by a Tracker component
        protected virtual void Visualize(Tracker tracker, Trackable trackable)
        {
            
            targetRelPos = tracker.ReferenceTransform.InverseTransformPoint(trackable.transform.position);

            // If target is outside the radar display range and is not clamped to the border, ignore it
            if (targetRelPos.magnitude > radarDisplayRange)
            {
                if (!clampToBorder)
                {
                    return;
                }
            }

            // Get a radar widget that matches the trackable type and display it
            HUDRadarWidget radarWidget = null;
            for (int i = 0; i < radarWidgetOverrides.Count; ++i)
            {
                for (int j = 0; j < radarWidgetOverrides[i].trackableTypes.Count; ++j)
                {
                    if (radarWidgetOverrides[i].trackableTypes[j] == trackable.TrackableType)
                    {
                        radarWidget = radarWidgetOverrides[i].GetNextAvailable(widgetParent);
                        break;
                    }
                }

                if (radarWidget != null) break;
            }

            if (radarWidget == null && defaultRadarWidgetContainer != null)
            {
                radarWidget = defaultRadarWidgetContainer.GetNextAvailable(widgetParent);
            }

            if (defaultRadarWidget == null) return;

            // Update the color of the target box
            if (trackable.Team != null)
            {
                radarWidget.SetColor(trackable.Team.DefaultColor);
                for (int i = 0; i < teamColors.Count; ++i)
                {
                    if (teamColors[i].team == trackable.Team)
                    {
                        radarWidget.SetColor(teamColors[i].color);
                    }
                }
            }
            else
            {
                radarWidget.SetColor(defaultWidgetColor);
            }

            bool isSelected = false;
            for (int i = 0; i < tracker.TargetSelectors.Count; ++i)
            {
                if (tracker.TargetSelectors[i].SelectedTarget == trackable)
                {
                    isSelected = true;
                    break;
                }
            }
            radarWidget.SetSelected(isSelected);

            Vector3 localPos;
            if (targetRelPos.magnitude > radarDisplayRange)
            {
                localPos = (targetRelPos.normalized * radarDisplayRange) * (equatorRadius / radarDisplayRange);

                radarWidget.SetIsClampedToBorder(true);
            }
            else
            {

                float amount = (targetRelPos.magnitude / radarDisplayRange);
                amount = 1 - Mathf.Pow(1 - amount, scaleExponent);
                localPos = (amount * equatorRadius) * targetRelPos.normalized;

                radarWidget.SetIsClampedToBorder(false);
            }

            if (VerticalAxisZ)
            {
                localPos = Quaternion.Euler(-90, 0, 0) * localPos;
            }

            if (display2D)
            {
                localPos.z = 0;
            }

            radarWidget.transform.localPosition = localPos;

            radarWidget.transform.localRotation = Quaternion.identity;

            radarWidget.UpdateRadarWidget(trackable);

            return;
        }

        // Late update
        void LateUpdate()
        {
            // If not activated, clear all the radar widgets and exit
            if (!activated)
            {
                for (int i = 0; i < radarWidgetOverrides.Count; ++i)
                {
                    radarWidgetOverrides[i].Begin();
                    radarWidgetOverrides[i].Finish();

                    if (defaultRadarWidgetContainer != null)
                    {
                        defaultRadarWidgetContainer.Begin();
                        defaultRadarWidgetContainer.Finish();

                    }

                }
                return;
            }

            // Update the display range
            float maxRange = 0;
            for (int i = 0; i < trackers.Count; ++i)
            {
                maxRange = Mathf.Max(maxRange, trackers[i].Range);
            }
            radarDisplayRange = (1 - currentZoom) * maxRange;

            // Begin using the radar widget containers
            if (defaultRadarWidgetContainer != null) defaultRadarWidgetContainer.Begin();
            for (int i = 0; i < radarWidgetOverrides.Count; ++i)
            {
                radarWidgetOverrides[i].Begin();
            }

            // Visualize the targets
            for (int i = 0; i < trackers.Count; ++i)
            {
                for (int j = 0; j < trackers[i].Targets.Count; ++j)
                {
                   Visualize(trackers[i], trackers[i].Targets[j]);

                    // Don't add more than the specified amount of widgets per frame
                    if (displayedTargetCount - numTargetsLastFrame >= maxNewTargetsEachFrame)
                    {
                        break;
                    }
                }
            }

            numTargetsLastFrame = displayedTargetCount;

            // Finish using the radar widget containers
            if (defaultRadarWidgetContainer != null) defaultRadarWidgetContainer.Finish();
            for (int i = 0; i < radarWidgetOverrides.Count; ++i)
            {
                radarWidgetOverrides[i].Finish();
            }
        }
    }
}
