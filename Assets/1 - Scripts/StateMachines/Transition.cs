using System;
using UnityEngine;

namespace StateMchines
{
    public abstract class Transition : MonoBehaviour
    {
        [SerializeField] private State _targetState;

        public event Action<State> NewState;

        public abstract void OnObserve();

        protected void Transit() => NewState?.Invoke(_targetState);

        public abstract void OffObserve();
    }
}