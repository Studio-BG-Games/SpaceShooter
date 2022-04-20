using System;
using MaxyGames;
using UnityEngine;

namespace Services.Inputs
{
    public class MobileInput : MonoBehaviour, IInput
    {
        public event Action<Vector2> Move;
        private Vector2 _moveDir;
        public Vector2 MoveDir=>_moveDir; 
        public event Action ChangeWeapon;
        public event Action Pause;
        public event Action Fire;

        public void MakeChangeWeapon() => ChangeWeapon?.Invoke();

        public void MakeFire() => Fire?.Invoke();

        public void MakeMove(Vector2 v)
        {
            _moveDir = v;
            Move?.Invoke(v);
        }

        public void MakePause() => Pause?.Invoke();
    }
}