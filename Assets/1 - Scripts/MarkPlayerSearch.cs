using Infrastructure;
using UltEvents;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Collider))]
    public class MarkPlayerSearch : MonoBehaviour
    {
        public UltEvent Finded;
        public UltEvent<Collider> FindedCollider;
        
        [SerializeField] private Collider _collider;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<MarkPlayer>(out var mark))
            {
                FindedCollider.Invoke(other);
                Finded?.Invoke();
            }
        }

        private void OnValidate()
        {
            if (!_collider) _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }
    }
}