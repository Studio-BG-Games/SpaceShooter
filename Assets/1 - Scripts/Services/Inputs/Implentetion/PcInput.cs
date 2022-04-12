using System;
using UnityEngine;

namespace Services.Inputs
{
    public class PcInput : MonoBehaviour, IInput
    {
        public event Action<Vector2> Move;
        public event Action ChangeWeapon;
        public event Action Pause;
        public event Action Fire;

        public void Update()
        {
            var keyboardVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if(keyboardVector.x!=0 || keyboardVector.y!=0)
                Move?.Invoke(keyboardVector);
            else 
                Move?.Invoke(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).normalized);
            
            if(Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKey(KeyCode.LeftShift)) ChangeWeapon?.Invoke();
            if(Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Space)) Fire?.Invoke();
            if(Input.GetKeyDown(KeyCode.Escape)) Pause?.Invoke();
        }
    }
}