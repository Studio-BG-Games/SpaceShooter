using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    public class ModuleStatsController : StatsController
    {

        protected List<ModuleStatsOverrideController> moduleStatsOverrideControllers = new List<ModuleStatsOverrideController>();


        protected List<Module> modulesList;
        public List<Module> ModulesList
        {
            set
            {
                modulesList = value;

                foreach(ModuleStatsOverrideController moduleStatsOverrideController in moduleStatsOverrideControllers)
                {
                    moduleStatsOverrideController.OnModulesListUpdated(value);
                }
            }
        }


        protected virtual void Awake()
        {
            moduleStatsOverrideControllers = new List<ModuleStatsOverrideController>(transform.GetComponentsInChildren<ModuleStatsOverrideController>());
            foreach(ModuleStatsOverrideController statsOverride in moduleStatsOverrideControllers)
            {
                statsOverride.StatsController = this;
            }
        }


        public void ShowStats(Module module)
        {
            ClearStatsInstances();

            bool found = false;
            foreach (ModuleStatsOverrideController controller in moduleStatsOverrideControllers)
            {
                if (controller.ShowStats(module))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                labelText.text = module.Label;
                descriptionText.text = module.Description;
            }
        }
    }
}

