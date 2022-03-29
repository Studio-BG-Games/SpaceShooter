using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Targets subsystems (child trackables) of another target selector's target
    /// </summary>
    public class SubsystemTargetSelector : TargetSelector
    {

        [SerializeField]
        protected TargetSelector parentTargetSelector;


        protected virtual void Awake()
        {
            // Subscribe to the event of the parent's target changing
            parentTargetSelector.onSelectedTargetChanged.AddListener(OnParentTargetChanged);
        }

        // Called when the parent selector's target changes
        protected virtual void OnParentTargetChanged(Trackable parentTrackable)
        {
            // Update the target list
            if (parentTrackable == null)
            {
                trackables = new List<Trackable>();
            }
            else
            {
                trackables = parentTrackable.ChildTrackables;
            }

            if (scanEveryFrame) SelectFirstSelectableTarget();

        }
    }
}