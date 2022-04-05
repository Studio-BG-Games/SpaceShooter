using UltEvents;

namespace Services
{
    public class HealthNormalEvent : PartUnit
    {
        public Health Health;
        public UltEvent<float> Normal;

        private void OnEnable()
        {
            Health.ChangedOldNew += Handler;
            Handler(Health.Current, Health.Current);
        }

        private void OnDisable() => Health.ChangedOldNew -= Handler;

        private void Handler(int old, int newV)
        {
            Normal.Invoke(((float)newV)/Health.Max);
        }
    }
}