using UnityEngine;

namespace Services
{
    public class HealthRef : PartUnit
    {
        [SerializeField] private Health _health;

        public Health Health => _health;
    }
}