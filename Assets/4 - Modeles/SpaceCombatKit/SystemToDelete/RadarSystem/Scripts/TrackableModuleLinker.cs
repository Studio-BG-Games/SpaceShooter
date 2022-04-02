using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{
    public class TrackableModuleLinker : ModuleManager
    {
        protected override void OnModuleMounted(Module module)
        {
            base.OnModuleMounted(module);

            Trackable moduleTrackable = module.GetComponent<Trackable>();
            Trackable thisTrackable = GetComponent<Trackable>();
            if (thisTrackable != null && moduleTrackable != null)
            {
                thisTrackable.AddChildTrackable(moduleTrackable);
            }
        }
    }
}

