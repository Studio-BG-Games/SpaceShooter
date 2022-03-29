using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Container for HUDRadarWidget components 
    /// </summary>
    [System.Serializable]
    public class HUDRadarWidgetContainer : ComponentContainer<HUDRadarWidget>
    {
        public List<TrackableType> trackableTypes = new List<TrackableType>();

        public HUDRadarWidgetContainer(HUDRadarWidget prefab) : base(prefab)
        {
            this.prefab = prefab;
        }

        protected override HUDRadarWidget CreateNew(Transform parent)
        {
            HUDRadarWidget targetBox = GameObject.Instantiate(prefab, parent);
            targetBox.transform.localScale = new Vector3(1, 1, 1);
            cachedComponents.Add(new CachedComponent(targetBox.gameObject));

            return targetBox;
        }
    }
}
