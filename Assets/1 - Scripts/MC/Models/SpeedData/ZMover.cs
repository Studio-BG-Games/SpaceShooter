using System;
using Dreamteck.Forever;
using UltEvents;
using UnityEngine;

namespace Services
{
    public class ZMover : MonoBehaviour
    {
        public event Action Changed;
        
        [Min(0)][SerializeField] private float _speed;
        [SerializeField] private bool _isPositive;

        public bool IsPositive
        {
            get => _isPositive;
            set
            {
                _isPositive = value;
                Changed?.Invoke();
            }
        }

        public float AbsolutlySpeed => _speed;
        
        public float SpeedByDiraction
        {
            get => _speed * (IsPositive?1:-1);
            set
            {
                _speed = Mathf.Clamp(value, 0, 2000);
                Changed?.Invoke();
            }
        }
    }
}