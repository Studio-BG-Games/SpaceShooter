using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Manages the lead target boxes on a target box on the HUD.
    /// </summary>
    public class HUDTargetBox_LeadTargetBoxesController : MonoBehaviour
    {

        [SerializeField]
        protected List<HUDTargetBox_LeadTargetBoxController> leadTargetBoxes = new List<HUDTargetBox_LeadTargetBoxController>();

        protected int lastUsedIndex = -1;

        protected Coroutine resetCoroutine;


        protected virtual void OnEnable()
        {
            resetCoroutine = StartCoroutine(ResetLeadTargetBoxesCoroutine());
        }

        protected virtual void OnDisable()
        {
            StopCoroutine(resetCoroutine);
        }

        // Coroutine for resetting the lead target boxes at the end of the frame
        protected virtual IEnumerator ResetLeadTargetBoxesCoroutine()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                // Deactivate all the lead target boxes
                for (int i = 0; i < leadTargetBoxes.Count; ++i)
                {
                    leadTargetBoxes[i].Deactivate();
                }
                lastUsedIndex = -1;
            }
        }
        
        /// <summary>
        /// Get a new lead target box.
        /// </summary>
        /// <returns>The lead target box controller.</returns>
        public HUDTargetBox_LeadTargetBoxController GetLeadTargetBox()
        {

            lastUsedIndex += 1;

            if (lastUsedIndex >= leadTargetBoxes.Count)
            {
                return null;
            }
            else
            {
                leadTargetBoxes[lastUsedIndex].Activate();
                return leadTargetBoxes[lastUsedIndex];
            }
        }
    }
}