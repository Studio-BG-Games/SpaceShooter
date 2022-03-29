using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class ModuleStatsOverrideController : MonoBehaviour
    {
        [SerializeField]
        protected ModuleType moduleType;
        public ModuleType ModuleType
        {
            get { return moduleType; }
        }

        protected ModuleStatsController statsController;
        public ModuleStatsController StatsController
        {
            set { statsController = value; }
        }

        public virtual void OnModulesListUpdated(List<Module> modules)
        {
            // Update max stats values
        }

        public virtual bool ShowStats(Module module) 
        {
            return false;
        }
    }
}
