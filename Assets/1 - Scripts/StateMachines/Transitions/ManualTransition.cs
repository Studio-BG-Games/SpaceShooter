using System;
using UnityEngine;

namespace StateMchines.Transitions
{
    public class ManualTransition : Transition
    {
        private bool _isOn;

        [ContextMenu("Make Transit")]
        public void ManualTransit()
        {
            if(!_isOn)
                return;
            Transit();
        }
        
        public override void OnObserve() => _isOn = true;

        public override void OffObserve() => _isOn = false;
    }
}