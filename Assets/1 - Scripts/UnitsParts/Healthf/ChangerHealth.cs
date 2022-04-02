using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Services
{
    public class ChangerHealth : PartUnit
    {
        [SerializeField] private List<TargetAttack> _targets;

        public void Change(RaycastHit hit) => hit.collider.GetComponents<Health>().ForEach(x => Change((Health) x));

        public void Change(Health health) => _targets.ForEach(x=>x.Change(health));

        [System.Serializable]
        public class TargetAttack
        {
            [InfoBox("if null - change all health witn any id")]
            public HealthID TargetId;
            public int ChangeAt;

            public void Change(Health health)
            {
                if (TargetId == null) health.Current += ChangeAt;
                else if(health.Id == TargetId) health.Current += ChangeAt;
            }
        }
    }
}