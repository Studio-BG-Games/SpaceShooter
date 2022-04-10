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

        public void Move(Runner runner, Vector2 input) => runner.motion.offset += new Vector2(GetSpeedAxis(Axis.X, input), GetSpeedAxis(Axis.Y, input));

        private float GetSpeedAxis(Axis axis, Vector2 input) => 
            axis == Axis.X ? _speed.x * _muptiply * input.x * Time.deltaTime : _speed.y * _muptiply * input.y * Time.deltaTime;

        private enum Axis { X, Y }

        private void OnValidate()
        {
            if (_speed.x < 0) _speed.x = 0;
            if (_speed.y < 0) _speed.y = 0;
        }
    }
}