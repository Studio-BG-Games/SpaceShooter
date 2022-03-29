using ModelCore;
using ModelCore.Universal.AliasValue;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace CorePresenter.UniversalPart.P_AliasValue
{
    public abstract class P_AliasValue<T> : Presenter
    {
        [Sirenix.OdinInspector.ReadOnly][ShowInInspector][PropertyOrder(-1)] public T Value => _model != null ? _model.Value : _default;
        [HideIf("_showDefault")][PropertyOrder(-1)][SerializeField] private T _default;

        [PropertyOrder(2)] public UltEvent<T> Inited;
        [PropertyOrder(2)] public UltEvent<T> Updated;
        
        private BaseAliasValue<T> _model;

        private bool _showDefault => _model == null;

        public override void Init(RootModel rootModel)
        {
            if (_model != null) _model.Update -= Handler;
            _model = GetModel<BaseAliasValue<T>>(rootModel, x=>x.Alias==PathToModel);
            if (_model == null)
                Debug.LogWarning($"{gameObject.name}, модель не имеет параметр под имение {PathToModel}, будет использоваться дефолт - {_default}", this);
            Inited?.Invoke(Value);
            if (_model != null) _model.Update += Handler;
        }

        [Button][GUIColor(0,1,0)] public void ManualUpdate() => Updated.Invoke(Value);
        
        [Button][ButtonGroup("1")][GUIColor(0.86f, 0.76f, 0.9f, 0.8f)] public void CopyFromUpdate() => Inited.CopyFrom(Updated);

        [Button][ButtonGroup("1")][GUIColor(0.9f, 0.76f, 0.8f, 0.8f)] public void CopyFromInit() => Updated.CopyFrom(Inited);

        [Button]
        private void SetDefaultToModel() => SetValueToModel(_default);

        [Button]
        public void SetValueToModel(T value) { if (_model != null) _model.Value = value; }

        private void Handler(T old, T newV) => Updated.Invoke(newV);
    }
}