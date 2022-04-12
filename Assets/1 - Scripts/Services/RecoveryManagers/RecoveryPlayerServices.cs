using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Services.RecoveryManagers
{
    public class RecoveryPlayerServices : MonoBehaviour
    {
        public event Action Recovered;

        [Button] public void Recovery() => Recovered?.Invoke();
    }
}