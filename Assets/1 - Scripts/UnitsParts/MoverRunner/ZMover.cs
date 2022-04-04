using System;
using Dreamteck.Forever;
using UltEvents;
using UnityEngine;

namespace Services
{
    public class ZMover : PartUnit
    {
        [SerializeField]private Runner _runner;
        public Runner Runner => _runner;
        [SerializeField] private bool _isPositive;

        public UltEvent<bool> NewDiraction;

        public bool IsPositive
        {
            get => _isPositive;
            set
            {
                _isPositive = value;
                _runner.followSpeed = Speed;
                NewDiraction.Invoke(_isPositive);
            }
        }

        [Min(0)][SerializeField] private float _speed;

        public float Speed
        {
            get => _speed * (IsPositive?1:-1);
            set
            {
                _speed = Mathf.Clamp(value, 0, 2000);
                _runner.followSpeed = Speed;
            }
        }

        private void Awake()
        {
            NewDiraction.Invoke(IsPositive);
            _runner.followSpeed = Speed;
        }
    }
}