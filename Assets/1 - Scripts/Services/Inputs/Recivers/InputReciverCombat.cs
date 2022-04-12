using System;
using DIContainer;
using UltEvents;
using UnityEngine;

namespace Services.Inputs
{
    public class InputReciverCombat : MonoBehaviour
    {
        private ResolveSingle<IInput> _input = new ResolveSingle<IInput>();

        public UltEvent Attack;
        public UltEvent ChangeWeapon;
        
        private void OnEnable()
        {
            _input.Depence.Fire += Attack.Invoke;
            _input.Depence.ChangeWeapon += ChangeWeapon.Invoke;
        }

        private void OnDisable()
        {
            _input.Depence.Fire -= Attack.Invoke;
            _input.Depence.ChangeWeapon -= ChangeWeapon.Invoke;
        }
    }
}