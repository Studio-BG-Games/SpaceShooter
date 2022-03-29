using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using VSX.Pooling;

// This script is for managing the hologram of the target

namespace VSX.UniversalVehicleCombat.Radar
{

    public class TargetHologramController : HUDHologramController
    {

        protected Trackable targetTrackable;

        [Header("Target Selector")]

        [Tooltip("The target selector to get the target which will be displayed on the hologram.")]
        [SerializeField]
        protected TargetSelector targetSelector;

        [Header("Colors")]
        
        [Tooltip("Override the hologram color for a specified Team that the Trackable belongs to.")]
        [SerializeField]
        protected List<TeamColor> teamColors = new List<TeamColor>();

        [Header("Keys")]

        [Tooltip("The key to use to find the hologram in the target Trackable's variables dictionary.")]
        [SerializeField]
        protected string hologramPrefabKey = "Hologram";

        [Tooltip("The key to use to find the label of the target in the target Trackable's variables dictionary.")]
        [SerializeField]
        protected string labelKey = "Label";

        [Tooltip("Whether to disable the label if the target Trackable does not have one.")]
        [SerializeField]
        protected bool disableLabelIfValueMissing;



        protected override void Awake()
        {

            base.Awake();

            if (targetSelector != null)
            {
                targetSelector.onSelectedTargetChanged.AddListener(SetTarget);
            }
        }

        /// <summary>
        /// Set the target Trackable from which to get the hologram.
        /// </summary>
        /// <param name="newTarget">The target Trackable.</param>
        public virtual void SetTarget(Trackable newTarget)
        {
            targetTrackable = newTarget;

            // Clear hologram if null
            if (newTarget == null)
            {
                targetTrackable = null;
                orientationTarget = null;
                SetHologram(null);
                return;
            }

            orientationTarget = newTarget.transform;

            // Update target label
            if (label != null)
            {
                if (targetTrackable.variablesDictionary.ContainsKey(labelKey))
                {
                    label.gameObject.SetActive(true);
                    label.text = targetTrackable.variablesDictionary[labelKey].StringValue;
                }
                else
                {
                    if (disableLabelIfValueMissing)
                    {
                        label.gameObject.SetActive(false);
                    }
                }
            }

            // Create the hologram
            GameObject hologramPrefab = null;
            if (targetTrackable.variablesDictionary.ContainsKey(hologramPrefabKey))
            {

                hologramPrefab = (GameObject)targetTrackable.variablesDictionary[hologramPrefabKey].ObjectValue;

                // Get the hologram component
                GameObject hologramObject;
                if (PoolManager.Instance != null)
                {
                    hologramObject = PoolManager.Instance.Get(hologramPrefab);
                }
                else
                {
                    hologramObject = GameObject.Instantiate(hologramPrefab);
                }
                
                Hologram hologram = hologramObject.GetComponent<Hologram>();
                if (hologram == null)
                {
                    Debug.LogError("The hologram prefab " + hologramObject.name + " must have a Hologram component on the root transform, please add one.");
                }

                // Set the hologram
                SetHologram(hologram);

                // Determine the color of the hologram
                Color col = defaultColor;
                if (targetTrackable.Team != null)
                {
                    col = targetTrackable.Team.DefaultColor;
                    for (int i = 0; i < teamColors.Count; ++i)
                    {
                        if (teamColors[i].team == targetTrackable.Team)
                        {
                            col = teamColors[i].color;
                        }
                    }
                }
                
                // Set the hologram color
                SetColor(col);

            }
        }
	}
}
