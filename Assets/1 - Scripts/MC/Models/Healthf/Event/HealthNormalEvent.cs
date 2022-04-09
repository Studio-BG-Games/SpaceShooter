using UltEvents;

namespace Services
{
    public class HealthNormalEvent : HealthEvent
    {
        public UltEvent<float> Normal;

        protected override void Handler(int old, int newV)
        {
            Normal.Invoke(((float)newV)/current.Max);
        }
    }
}