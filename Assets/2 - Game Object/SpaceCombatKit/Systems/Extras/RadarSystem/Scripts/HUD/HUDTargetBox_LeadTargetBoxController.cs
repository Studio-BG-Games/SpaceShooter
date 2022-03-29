using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Manages a single lead target box on a target box on the HUD.
    /// </summary>
    public class HUDTargetBox_LeadTargetBoxController : MonoBehaviour
    {

        [SerializeField]
        protected Image box;
        public Image Box { get { return box; } }

        public Image line;
        public Image Line { get { return line; } }


        /// <summary>
        /// Update the lead target box
        /// </summary>
        public void UpdateLeadTargetBox()
        {
            
            // Set the line position
            line.rectTransform.localPosition = 0.5f * box.rectTransform.localPosition;

            if ((box.rectTransform.position - box.rectTransform.parent.position).magnitude < 0.0001f)
            {
                line.rectTransform.rotation = Quaternion.identity;
            }
            else
            {
                line.rectTransform.rotation = Quaternion.LookRotation(box.rectTransform.position - box.rectTransform.parent.position, Vector3.up);
            }
                
            // Set the line rotation
            line.transform.Rotate(Vector3.up, 90, UnityEngine.Space.Self);

            // Set the line size
            Vector2 size = line.rectTransform.sizeDelta;
            size.x = 2 * Vector3.Magnitude(line.rectTransform.localPosition);

            line.rectTransform.sizeDelta = size;
        
        }

        /// <summary>
        /// Activate the lead target box.
        /// </summary>
        public void Activate()
        {
            box.gameObject.SetActive(true);
            line.gameObject.SetActive(true);
        }

        /// <summary>
        /// Deactivate the lead target box.
        /// </summary>
        public void Deactivate()
        {
            box.gameObject.SetActive(false);
            line.gameObject.SetActive(false);
        }
    }
}