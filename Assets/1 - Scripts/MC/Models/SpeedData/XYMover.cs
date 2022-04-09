using System;
using Dreamteck.Forever;
using UltEvents;
using UnityEngine;

namespace Services
{
    public class XYMover : MonoBehaviour
    {
        [SerializeField] private Vector2 _speed;
        [Min(0)] [SerializeField] private float _muptiply;

        public Vector2 Speed => _speed * _muptiply;

        public void Move(Runner runner, Vector2 input) => runner.motion.offset += _speed * _muptiply * input;

        private void OnValidate()
        {
            if (_speed.x < 0) _speed.x = 0;
            if (_speed.y < 0) _speed.y = 0;
        }
    }
}