using DIContainer;
using UltEvents;
using UnityEngine;

namespace Services.Inputs
{
    public class InputReciverMove  : MonoBehaviour
    { 
        private ResolveSingle<IInput> _input = new ResolveSingle<IInput>();
        public UltEvent<Vector2> Move;

        private void OnDisable()
        {
            _input.Depence.Move -= Move.Invoke;
        }

        private void OnEnable()
        {
            _input.Depence.Move += Move.Invoke;
        }
    }
}