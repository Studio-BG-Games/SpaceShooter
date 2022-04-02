using System;
using UnityEngine;

namespace Services.Input
{
    public interface IInput
    {
        event Action<Vector2> Move;
        event Action ChangeWeapon;
        event Action Fire;
    }
}