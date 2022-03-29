using UltEvents;
using UnityEngine;

namespace UiHlpers
{
    public class KeyTrigget : MonoBehaviour
    {
        [SerializeField] private bool _value = false;
        [SerializeField] private KeyCode code = KeyCode.F1;

        public UltEvent On;
        public UltEvent Off;

        private void OnEnable()
        {
            if(_value) On.Invoke();
            else Off.Invoke();
        }

        private void Update()
        {
            if (Input.GetKeyDown(code))
            {
                _value = !_value;
                if(_value) On.Invoke();
                else Off.Invoke();
            }
        }
    }
}