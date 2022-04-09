using UltEvents;

namespace Services
{
    public class HealthDamageEvent : HealthEvent
    {
        public UltEvent Empty;
        public UltEvent<int> DamageAt;

        protected override void Handler(int old, int current)
        {
            if (current == RefHp.Component.Min) Empty.Invoke();
            if(current<old) DamageAt.Invoke(old-current);
        }
    }
}