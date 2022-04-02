using System;
using UnityEngine;

namespace StateMchines
{
    [DisallowMultipleComponent]
    public abstract class State : MonoBehaviour
    {
        private Transition[] _transitions;

        public void Init() => _transitions = GetComponents<Transition>();

        public event Action<State> NewState;

        public void Enter()
        {
            foreach (var trans in _transitions)
            {
                trans.NewState += Handler;
                trans.OnObserve();
            }
            AbsEnter();
        }
        
        protected abstract void AbsEnter();

        public abstract void Tick();

        public void Exit()
        {
            foreach (var trans in _transitions)
            {
                trans.NewState -= Handler;
                trans.OffObserve();
            }
            AbsExit();
        }

        protected abstract void AbsExit();
        
        private void Handler(State obj) => NewState?.Invoke(obj);
    }
}