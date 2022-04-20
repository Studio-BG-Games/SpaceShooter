using System;
using UnityEngine;

namespace Services.Inputs
{
    public class CompositeInput : IInput, IDisposable
    {
        private IInput[] _inputs;
        public event Action<Vector2> Move;
        private Vector2 _moveDir;
        public Vector2 MoveDir => _moveDir;
        public event Action ChangeWeapon;
        public event Action Pause;
        public event Action Fire;

        public CompositeInput(IInput[] inputs)
        {
            _inputs = inputs;
            for (var i = 0; i < _inputs.Length; i++)
            {
                if(_inputs[i]==null) continue;
                _inputs[i].ChangeWeapon += ChangeHander;
                _inputs[i].Fire += FireHandler;
                _inputs[i].Pause += PauseHandler;
            }
        }

        public void UpdateCustom()
        {
            foreach (var input in _inputs)
            {
                if (input.MoveDir.magnitude > 0)
                {
                    Move?.Invoke(input.MoveDir);
                    return;
                }
            }
            Move?.Invoke(Vector2.zero);
        }

        private void PauseHandler() => Pause?.Invoke();


        private void ChangeHander() => ChangeWeapon?.Invoke();

        private void FireHandler() => Fire?.Invoke();

        public void Dispose()
        {
            for (var i = 0; i < _inputs.Length; i++)
            {
                if(_inputs[i]==null) continue;
                _inputs[i].ChangeWeapon -= ChangeHander;
                _inputs[i].Fire -= FireHandler;
                _inputs[i].Pause -= PauseHandler;
            }
        }
    }
}