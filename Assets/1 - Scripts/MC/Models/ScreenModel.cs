using System;
using UnityEngine;

namespace Models
{
    public class ScreenModel : MonoBehaviour
    {
        [SerializeField]private string _id;

        public string Id => _id;
        public event Action<bool> StatusChanged;

        [SerializeField] private bool _status;
        public bool Status
        {
            get => _status;
            set
            {
                _status = value;
                StatusChanged?.Invoke(_status);
            }
        }
    }
}