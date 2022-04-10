using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.Effects
{
    public class RumbleShaker : Shaker
    {

        protected virtual void Reset()
        {
            shakenTransform = transform;
        }

        protected override void Awake()
        {
            base.Awake();
            if (RumbleManager.Instance != null)
            {
                RumbleManager.Instance.onRumble.AddListener(Shake);
            }
        }
    }
}
