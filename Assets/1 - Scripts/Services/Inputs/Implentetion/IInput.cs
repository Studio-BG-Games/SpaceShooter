using System;
using UnityEngine;

namespace Services.Inputs
{
    public interface IInput
    {
        event Action<Vector2> Move;
        Vector2 MoveDir { get; }
        event Action ChangeWeapon;
        event Action Pause;
        event Action Fire;
    }
}