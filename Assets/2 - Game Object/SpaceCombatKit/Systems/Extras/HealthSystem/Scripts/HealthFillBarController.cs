using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class HealthFillBarController : UIFillBarController
    {

        [SerializeField]
        protected HealthType healthType;
        public HealthType HealthType { get { return healthType; } }

        [SerializeField]
        protected Health health;

        private void Update()
        {
            if (health != null)
            {
                SetFillAmount(health.GetCurrentHealthFractionByType(healthType));
            }
        }

    }
}