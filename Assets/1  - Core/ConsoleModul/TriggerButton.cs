using UltEvents;
using UnityEngine;

namespace UiHlpers
{
    public class TriggerButton : MonoBehaviour
    {
        public UltEvent OnTrue;
        public UltEvent OnFalse;
        public UltEvent<bool> ValueEvent;

        [SerializeField] private bool _value;

        public void On() => Set(true);

        public void Off() => Set(false);

        private void OnEnable() => InvokeEvents();

        public void Set(bool value)
        {
            _value = value;
            InvokeEvents();
        }
        
        public void Change()
        {
            _value = !_value;
            InvokeEvents();
        }
        
        private void InvokeEvents()
        {
            if(_value) OnTrue.Invoke();
            else OnFalse.Invoke();
            ValueEvent.Invoke(_value);
        }
    }
}