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
        protected HealthSpaceCombat healthSpaceCombat;

        private void Update()
        {
            if (healthSpaceCombat != null)
            {
                SetFillAmount(healthSpaceCombat.GetCurrentHealthFractionByType(healthType));
            }
        }

    }
}