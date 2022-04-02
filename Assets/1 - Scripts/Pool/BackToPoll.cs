using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class BackToPoll : MonoBehaviour
    {
        public event Action Returned;

        private void OnDisable() => Returned?.Invoke();
    }
}