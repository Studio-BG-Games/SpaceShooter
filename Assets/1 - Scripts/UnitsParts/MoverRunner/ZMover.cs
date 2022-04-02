using System;
using Dreamteck.Forever;
using UnityEngine;

namespace Services
{
    public class ZMover : PartUnit
    {
        [SerializeField]private Runner _runner;
        public Runner Runner => _runner;
        public bool IsPositive;

        [SerializeField] private float _speed;

        public float Speed
        {
            get => _speed * (IsPositive?1:-1);
            set
            {
                _speed = Mathf.Clamp(value, 0, 2000);
                _runner.followSpeed = Speed;
            }
        }

        private void Awake() => _runner.followSpeed = Speed;
    }
}