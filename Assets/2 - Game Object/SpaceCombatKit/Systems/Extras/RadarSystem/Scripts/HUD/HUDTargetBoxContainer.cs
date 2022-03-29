using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    // Container for target boxes for the HUD
    [System.Serializable]
    public class HUDTargetBoxContainer : ComponentContainer<HUDTargetBox>
    {
        public List<TrackableType> trackableTypes = new List<TrackableType>();

        public HUDTargetBoxContainer(HUDTargetBox prefab) : base (prefab)
        {
            this.prefab = prefab;
        }

        protected override HUDTargetBox CreateNew(Transform parent)
        {
            HUDTargetBox targetBox = GameObject.Instantiate(prefab, parent);
            targetBox.transform.localScale = new Vector3(1, 1, 1);
            cachedComponents.Add(new CachedComponent(targetBox.gameObject));

            return targetBox;
        }
    }
}
