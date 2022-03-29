using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat.Radar
{
    [System.Serializable]
    public class LockBoxAnimation
    {
        public RectTransform rectTransform;
        public float lockingMargin;
        public float lockedMargin;
    }

    /// <summary>
    /// Manages a single lock on a target box on the HUD.
    /// </summary>
    public class HUDTargetBox_LockController : MonoBehaviour
    {
        public List<LockBoxAnimation> lockBoxAnimations = new List<LockBoxAnimation>();

        // Activate the lock box
        public virtual void Activate()
        {
            for (int i = 0; i < lockBoxAnimations.Count; ++i)
            {
                lockBoxAnimations[i].rectTransform.gameObject.SetActive(true);
            }
        }

        // Deactivate the lock box
        public virtual void Deactivate()
        {
            for (int i = 0; i < lockBoxAnimations.Count; ++i)
            {
                lockBoxAnimations[i].rectTransform.gameObject.SetActive(false);
            }
        }
    }
}