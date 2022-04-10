using System;
using UnityEngine;

namespace MC.Models
{
    public class OnOffInput : MonoBehaviour
    {
        private bool _isOn;
        public event Action Updated;

        public bool IsOn
        {
            get => _isOn;
            set
            {
                _isOn = value;
                Updated?.Invoke();
            }
        }
    }
}