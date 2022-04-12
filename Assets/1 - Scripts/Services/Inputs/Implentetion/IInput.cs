using System;
using UnityEngine;

namespace Services.Inputs
{
    public interface IInput
    {
        event Action<Vector2> Move;
        event Action ChangeWeapon;
        event Action Pause;
        event Action Fire;
    }
}