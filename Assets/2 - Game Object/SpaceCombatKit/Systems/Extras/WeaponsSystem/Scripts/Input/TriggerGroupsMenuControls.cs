using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{

    [System.Serializable]
    public class TriggerIndexInput
    {
        public int triggerIndex;
        public CustomInput input;
    }

    public class TriggerGroupsMenuControls : GeneralInput
    {

        [Header("Settings")]

        [SerializeField]
        protected TriggerableGroupsMenuController triggerGroupsMenuController;

        [SerializeField]
        protected List<TriggerIndexInput> triggerIndexInputs = new List<TriggerIndexInput>();


        protected override bool Initialize()
        {
            return (triggerGroupsMenuController != null);
        }

        protected override void InputUpdate()
        {
            for(int i = 0; i < triggerIndexInputs.Count; ++i)
            {
                if (triggerIndexInputs[i].input.Down())
                {
                    triggerGroupsMenuController.SetTriggerGroupTriggerValue(triggerIndexInputs[i].triggerIndex);
                }
            }
        }
    }
}

