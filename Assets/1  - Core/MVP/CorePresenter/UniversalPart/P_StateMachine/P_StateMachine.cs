using ModelCore;
using ModelCore.Universal.StateMachines;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace CorePresenter.UniversalPart.P_StateMachine
{
    public abstract class P_StateMachine<T> : Presenter
    {
        private StateMachine<T> _model;
        
        [InfoBox("$CurrentState")]
        public UltEvent<T> EnteredTo;
        public UltEvent<T> ExitedFrom;

        private string CurrentState => _model != null ? _model.CurrentState.Alias.ToString() : "No state";

        public override void Init(RootModel rootModel)
        {
            if(_model!=null)
                _model.Changed -= Handler;
            _model = GetModel<StateMachine<T>>(rootModel, x => x.Alias == PathToModel);
            
            if(_model==null)
                return;
            EnteredTo.Invoke(_model.CurrentState.Alias);
            _model.Changed += Handler;
        }

        private void Handler(T oldstate, T newstate)
        {
            ExitedFrom.Invoke(oldstate);
            EnteredTo.Invoke(newstate);
        }
    }
}