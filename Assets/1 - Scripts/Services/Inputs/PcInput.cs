using System;
using UnityEngine;

namespace Services.Inputs
{
    public class PcInput : MonoBehaviour, IInput
    {
        public event Action<Vector2> Move;
        public event Action ChangeWeapon;
        public event Action Fire;

        private Vector3 lastPosition;
        
        public void Update()
        {
            Move?.Invoke(UnityEngine.Input.mousePosition - lastPosition);
            lastPosition = UnityEngine.Input.mousePosition;
            
            if(Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKey(KeyCode.LeftShift)) ChangeWeapon?.Invoke();
            if(Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Space)) Fire?.Invoke();
        }
    }
}