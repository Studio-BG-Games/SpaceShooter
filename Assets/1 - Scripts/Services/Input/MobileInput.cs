using System;
using UnityEngine;

namespace Services.Input
{
    public class MobileInput : MonoBehaviour, IInput
    {
        public event Action<Vector2> Move;
        public event Action ChangeWeapon;
        public event Action Fire;

        public void MakeChangeWeapon() => ChangeWeapon?.Invoke();

        public void MakeFire() => Fire?.Invoke();

        public void MakeMove(Vector2 v) => Move?.Invoke(v);
    }
}