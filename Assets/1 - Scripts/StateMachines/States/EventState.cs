using UltEvents;
using UnityEngine;
using UnityEngine.Events;

namespace StateMchines.States
{
    public class EventState : State
    {
        public UltEvent Entered;
        public UltEvent Ticked;
        public UltEvent Exited;
        
        protected override void AbsEnter() => Entered?.Invoke();

        public override void Tick() => Ticked?.Invoke();

        protected override void AbsExit() => Exited?.Invoke();
    }
}