using DIContainer;
using UltEvents;
using UnityEngine;

namespace Services.RecoveryManagers
{
    public class RecoveryPlayerReciver : MonoBehaviour
    {
        private ResolveSingle<RecoveryPlayerServices> _recovery = new ResolveSingle<RecoveryPlayerServices>();

        public UltEvent Recovered;
        
        private void Start() => _recovery.Depence.Recovered += Recovered.Invoke;
    }
}