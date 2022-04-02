using System;
using Dreamteck;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Services
{
    public class Health : PartUnit
    {
        public HealthID Id;

        public event Action<int, int> ChangedOldNew;

        public int Min => 0;
        [ShowInInspector] public int Max { get; private set; }
        [SerializeField] private int _current;

        public int Current
        {
            get => _current;
            set
            {
                var old = Current;
                _current = Mathf.Clamp(value, Min, Max);
                ChangedOldNew?.Invoke(old, _current);
            }
        }
    }
}