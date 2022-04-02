using System;
using UnityEngine;

namespace Services.Input
{
    public class PcInput : IInput
    {
        public event Action<Vector2> Move;
        public event Action ChangeWeapon;
        public event Action Fire;

        private Vector3 lastPosition;
        
        public void Update()
        {
            Move?.Invoke(UnityEngine.Input.mousePosition - lastPosition);
            lastPosition = UnityEngine.Input.mousePosition;
            
            if(UnityEngine.Input.GetKeyDown(KeyCode.Mouse1)) ChangeWeapon?.Invoke();
            if(UnityEngine.Input.GetKey(KeyCode.Mouse0)) Fire?.Invoke();
        }
    }
}