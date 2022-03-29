using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// This class is for creating a hologram of an trackable object and updating it with the trackable object's
// relative orientation

namespace VSX.UniversalVehicleCombat.Radar
{

	public class HUDHologramController : HUDComponent
    {

        [Header("General Settings")]

        [Tooltip("The transform that the hologram will be parented to.")]
        [SerializeField]
        protected Transform hologramParent;

        [SerializeField]
        protected UIColorManager colorManager;


        [Header("Hologram Orientation")]

        [Tooltip("Whether to update the orientation of the hologram according to the target's relative rotation.")]
        [SerializeField]
        protected bool updateOrientation = true;

        [Tooltip("The transform that represents the orientation of the object that the hologram is displaying.")]
        [SerializeField]
        protected Transform orientationTarget;

        [Tooltip("The transform that represents the object that is displaying the hologram.")]
        [SerializeField]
        protected Transform orientationReference;


        [Header("Appearance")]
	
        [Tooltip("The size of the hologram, defined as the average size of the hologram's height, width and length.")]
		[SerializeField]
		protected float hologramSize = 0.3f;

        [Tooltip("The coefficient that is multiplied by the saturation of the hologram color to get the saturation of the hologram's outline.")]
        [SerializeField]
        protected float outlineSaturationCoefficient = 0.5f;

        [Tooltip("Whether to allow the color of the hologram to be set.")]
        [SerializeField]
        protected bool setHologramColor = true;

        [Tooltip("The default color for the hologram.")]
        [SerializeField]
        protected Color defaultColor = Color.white;

        [Header("Label")]

        [Tooltip("The Text component for the label of the hologram.")]
        [SerializeField]
        protected Text label;

        [Tooltip("The coefficient that is multiplied by the saturation of the hologram color to get the saturation of the label text color.")]
        [SerializeField]
        protected float labelSaturationCoefficient = 0.5f;

        protected Hologram hologram;


        // Called when the component is first added to a gameobject or when it is Reset in the inspector
        protected void Reset()
        {
            hologramParent = transform;
            orientationReference = transform;
        }

        /// <summary>
        /// Set the hologram.
        /// </summary>
        /// <param name="hologram">The new hologram.</param>
        public void SetHologram(Hologram hologram)
        {
            if (this.hologram != null)
            {
                this.hologram.gameObject.SetActive(false);
                this.hologram = null;
            }

            if (hologram == null) return;

            // Set the hologram
            hologram.transform.SetParent(hologramParent);
            hologram.gameObject.layer = hologramParent.gameObject.layer;
            hologram.transform.localPosition = Vector3.zero;
            hologram.transform.localRotation = Quaternion.identity;

            // Adjust the scale
            Vector3 extents = hologram.Bounds.extents;
            float averageDimension = (extents.x + extents.y + extents.z) / 3.0f;
            float scale = hologramSize / (averageDimension * 2);
            hologram.transform.localScale = new Vector3(scale, scale, scale);

            for (int i = 0; i < hologram.Materials.Length; ++i)
            {
                hologram.Materials[i].SetFloat("_Scale", scale);
            }

            SetColor(defaultColor);

            this.hologram = hologram;
        }

		// Set the hologram color
		public void SetColor(Color col)
		{
            if (setHologramColor && hologram != null)
            {
                for(int i = 0; i < hologram.Materials.Length; ++i)
                {
                    // Create the outline color
                    float h_Outline, s_Outline, v_Outline;
                    Color.RGBToHSV(col, out h_Outline, out s_Outline, out v_Outline);
                    s_Outline *= outlineSaturationCoefficient;

                    // Set the rim color
                    hologram.Materials[i].SetColor("_RimColor", col);

                    // Set the outline color
                    hologram.Materials[i].SetColor("_OutlineColor", Color.HSVToRGB(h_Outline, s_Outline, v_Outline));

                    // Update the color manager
                    if (colorManager != null)
                    {
                        colorManager.SetColor(col);
                    }
                }
            }
		}

        protected virtual void LateUpdate()
        {
            if (hologram != null && orientationTarget != null)
            {
                // Update hologram orientation
                Vector3 direction = (orientationTarget.position - orientationReference.position).normalized;
                Quaternion temp = Quaternion.LookRotation(direction, orientationReference.up);
                Quaternion rot = Quaternion.Inverse(Quaternion.Inverse(orientationTarget.rotation) * temp);
                hologram.transform.localRotation = rot;
            }
        }
	}
}