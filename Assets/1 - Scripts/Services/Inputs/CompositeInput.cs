using System;
using UnityEngine;

namespace Services.Inputs
{
    public class CompositeInput : IInput, IDisposable
    {
        private IInput[] _inputs;
        public event Action<Vector2> Move;
        public event Action ChangeWeapon;
        public event Action Fire;

        public CompositeInput(IInput[] inputs)
        {
            _inputs = inputs;
            for (var i = 0; i < _inputs.Length; i++)
            {
                if(_inputs[i]==null) continue;
                _inputs[i].Move += MoveHandler;
                _inputs[i].ChangeWeapon += ChangeHander;
                _inputs[i].Fire += FireHandler;
            }
        }

        private void MoveHandler(Vector2 obj) => Move?.Invoke(obj);

        private void ChangeHander() => ChangeWeapon?.Invoke();

        private void FireHandler() => Fire?.Invoke();

        public void Dispose()
        {
            for (var i = 0; i < _inputs.Length; i++)
            {
                if(_inputs[i]==null) continue;
                _inputs[i].Move -= MoveHandler;
                _inputs[i].ChangeWeapon -= ChangeHander;
                _inputs[i].Fire -= FireHandler;
            }
        }
    }
}