using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UltEvents;
using UnityEngine;

namespace Services
{
    public class ChangerHealth : PartUnit
    {
        [SerializeField] private List<TargetAttack> _targets;
        public UltEvent Damaged;
        [SerializeField][HideInInspector]private bool _canEvented = true;

        private void Awake() => _canEvented = true;

        public void Change(RaycastHit hit) => Change(hit.collider);

        public void Change(Collider collider)
        {
            var healthf = new HashSet<Health>(collider.GetComponents<Health>());
            collider.GetComponents<HealthRef>().ForEach(x => healthf.Add(x.Health));
            healthf.ForEach(x =>
            {
                Change(x);
                _canEvented = false;
            });

            _canEvented = true;
        }

        public void Change(Health health)
        {
            if(_canEvented) Damaged.Invoke();
            _targets.ForEach(x => x.Change(health));
        }

        [System.Serializable]
        public class TargetAttack
        {
            [InfoBox("if null - change all health witn any id")]
            [InfoBox("Negative Value = damage, Positive = heal")]
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