using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Services
{
    public class DamageInfo : MonoBehaviour
    {
        [SerializeField] private List<TargetAttack> _targets;

        [System.Serializable]
        public class TargetAttack
        {
            [InfoBox("if null - change all health witn any id")] [InfoBox("Negative Value = damage, Positive = heal")]
            public HealthID TargetId;

            public int ChangeAt;

            public void Change(Health health)
            {
                if (TargetId == null) health.Current += ChangeAt;
                else if (health.Id == TargetId) health.Current += ChangeAt;
            }
        }

        public void GoOverDamageElelemnt(Action<TargetAttack> callback)=>_targets.ForEach(x=>callback(x));
    }
}