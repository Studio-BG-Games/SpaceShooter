using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;
using UnityEngine.UI;

public class ResourceContainerFillBar : MonoBehaviour
{
    public ResourceContainer resourceContainer;

    public ModuleMount moduleMount;

    public Image fillBarImage;


    private void Awake()
    {
        if (moduleMount != null)
        {
            moduleMount.onModuleMounted.AddListener(OnModuleMounted);
            moduleMount.onModuleUnmounted.AddListener(OnModuleUnmounted);
        }
    }

    void OnModuleMounted(Module module)
    {
        resourceContainer = module.GetComponent<ResourceContainer>();
    }

    void OnModuleUnmounted(Module module)
    {
        resourceContainer = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (resourceContainer != null)
        {
            fillBarImage.fillAmount = resourceContainer.CurrentAmountFloat / resourceContainer.CapacityFloat;
        }
    }
}
