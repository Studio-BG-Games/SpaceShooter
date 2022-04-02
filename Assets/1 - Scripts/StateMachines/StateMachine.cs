using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StateMchines
{
    public class StateMachine : MonoBehaviour
    {
        [SerializeField] private State _startedState;
        [SerializeField] private State[] _states = new State[0];

        [ReadOnly][ShowInInspector] private string NameCurrentState  = "No State";
        private State _currentState;
        
        private void OnEnable()
        {
            foreach (var state in _states)
            {
                state.Init();
            }
            ChangeState(_startedState);
        }
        
        public void ChangeState(State nextStaet)
        {
            if(_currentState)
                _currentState.NewState -= ChangeState;
            _currentState?.Exit();
            _currentState = nextStaet;
            NameCurrentState = _currentState.name;
            _currentState.Enter();
            _currentState.NewState += ChangeState;
        }

        private void Update() => _currentState.Tick();

        [Button]private void OnValidate() => _states = GetComponentsInChildren<State>();
    }
}