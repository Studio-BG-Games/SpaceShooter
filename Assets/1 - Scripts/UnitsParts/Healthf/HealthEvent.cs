using UnityEngine;

namespace Services
{
    public abstract class HealthEvent : PartUnit
    {
        [SerializeField] protected Health Helthf;

        private void Start()
        {
            Helthf.ChangedOldNew += Handler;
        }

        protected abstract void Handler(int old, int current);
    }
}