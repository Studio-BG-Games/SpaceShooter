using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace Services
{
    public class Health : PartUnit
    {
        public HealthID Id;

        public UltEvent<int, int> ChangedOldNew;

        public int Min => 0;
        [ShowInInspector] public int Max { get; private set; }
        [SerializeField] private int _current;

        private int Current
        {
            get => _current;
            set
            {
                var old = Current;
                _current = Mathf.Clamp(value, Min, Max);
                ChangedOldNew.Invoke(old, _current);
            }
        }
    }
}