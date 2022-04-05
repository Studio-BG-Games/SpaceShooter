using DIContainer;
using UltEvents;
using UnityEngine;

namespace Services.Inputs
{
    public class InputReciverMove  : MonoBehaviour
    {
        [DI] private IInput _input;

        public UltEvent<Vector2> Move;

        private void OnDisable()
        {
            _input.Move -= Move.Invoke;
        }

        private void OnEnable()
        {
            _input.Move += Move.Invoke;
        }
    }
}