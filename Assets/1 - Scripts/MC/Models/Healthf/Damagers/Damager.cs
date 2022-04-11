using System.Collections.Generic;
using Sirenix.Utilities;
using UltEvents;
using UnityEngine;

namespace Services
{
    public class Damager : MonoBehaviour
    {
        public DamageInfoRef DamageInfoRef;
        public UltEvent Damaged;
        [SerializeField][HideInInspector]private bool _canEvented = true;

        private void Awake() => _canEvented = true;

        public void Change(RaycastHit hit) => Change(hit.collider);

        public void Change(Collider collider)
        {
            var healthf = new HashSet<Health>(collider.GetComponents<Health>());
            collider.GetComponents<HealthfRef>().ForEach(x => healthf.Add(x.Component));
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
            DamageInfoRef.Component.GoOverDamageElelemnt(x => x.Change(health));
        }
    }
}