namespace Sharp.UnityMessager
{
    public abstract class SingleBaseReciverMessager<T> : BaseResiverMessagerValue<T>
    {
        public Event TargetEvet;
        
        protected override bool IsTargetEvent(Event e) => e == TargetEvet;
    }
}