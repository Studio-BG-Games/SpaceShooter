using System;
using Dreamteck;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Services
{
    public class Health : MonoBehaviour
    {
        public HealthID Id;

        public event Action<int, int> ChangedOldNew;
        
        public int Min => 0;
        public int Max => _max;
        [SerializeField] private int _current;
        [SerializeField] private int _max;
        
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