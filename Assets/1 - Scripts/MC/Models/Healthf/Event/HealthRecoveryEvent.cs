using UltEvents;

namespace Services
{
    public class HealthRecoveryEvent : HealthEvent
    {
        public UltEvent Full;
        public UltEvent<int> RecoveryAt;

        protected override void Handler(int old, int current)
        {
            if (current == RefHp.Component.Max) Full.Invoke();
            if(current>old) RecoveryAt.Invoke(current-old);
        }
    }
}