using System;
using DIContainer;
using ModelCore;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace CorePresenter.ContextAndReciver
{
    [AddComponentMenu("MV*/Reciver", 0)]
    [RequireComponent(typeof(RootPresenter))]
    public class Reciver : Presenter
    {
        [DI] private Context _context;
        
        public TimeForRecive TimeRecive;

        public UltEvent<RootModel> SuccessfulGet;
        public UltEvent LoseGet;
        
        private RootModel _model;

        private void Awake()
        {
            SuccessfulGet.DynamicCalls += x => InitPresenter(x);
        }

        private void Start()
        {
            if(TimeRecive == TimeForRecive.OnStart) ForceReciver();
        }

        private bool  hasReciveOnEnable = false;
        private void OnEnable()
        {
            if (TimeRecive == TimeForRecive.OnEnable && hasReciveOnEnable == false)
            {
                hasReciveOnEnable = true;
                ForceReciver();
            }

            if (TimeRecive == TimeForRecive.OnEnableRepeat)
            {
                ForceReciver();
            }
        }

        [Button]
        public void ReciveIfHasNot()
        {
            if(_model==null) ForceReciver();
        }
        
        [Button]
        public void ForceReciver()
        {
            if (_model != null) _model.CountModelsCahnged -= OnUpdateModel;
            
            _model = GetModel<RootModel>(Context.Instance.GameModel, x => x.Alias == PathToModel);
            if(_model!=null) SuccessfulGet.Invoke(_model);
            else
            {
                Debug.LogWarning($"Reciver ничего не может взять по пути ({PathToModel}) у Context", this);
                LoseGet.Invoke();
            }
            
            if (_model != null) _model.CountModelsCahnged += OnUpdateModel;
        }

        private void OnUpdateModel() => InitPresenter(_model);

        private void InitPresenter(RootModel x) => GetComponent<RootPresenter>().Init(x);

        public override void Init(RootModel rootModel){Debug.LogWarning("Не вызывай Init у baseReciver, он ничего не делает");}

        public enum TimeForRecive
        {
            OnStart, OnEnable, OnEnableRepeat, Manual
        }
    }
}