using Dreamteck.Forever;
using UltEvents;
using UnityEngine;

namespace Services
{
    public class XYMover : PartUnit
    {
        public Runner Runner;
        [SerializeField] private float Speed;

        public UltEvent Moved;

        public void MoveDir(Vector2 dir)
        {
            Runner.motion.offset += dir * Time.deltaTime * Speed;
            Moved.Invoke();
        }
    }
}