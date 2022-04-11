using System;
using MaxyGames;
using UnityEngine;

namespace Services.Inputs
{
    public class MobileInput : MonoBehaviour, IInput
    {
        public event Action<Vector2> Move;
        public event Action ChangeWeapon;
        public event Action Pause;
        public event Action Fire;

        public void MakeChangeWeapon() => ChangeWeapon?.Invoke();

        public void MakeFire() => Fire?.Invoke();

        public void MakeMove(Vector2 v) => Move?.Invoke(v);

        public void MakePause() => Pause?.Invoke();
    }
}