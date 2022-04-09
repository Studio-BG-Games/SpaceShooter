using DIContainer;
using UltEvents;
using UnityEngine;

namespace Services.Inputs
{
    public class InputReciverCombat : MonoBehaviour
    {
        [DI] private IInput _input;

        public UltEvent Attack;
        public UltEvent ChangeWeapon;
        
        private void OnEnable()
        {
            _input.Fire += Attack.Invoke;
            _input.ChangeWeapon += ChangeWeapon.Invoke;
        }
    }
}