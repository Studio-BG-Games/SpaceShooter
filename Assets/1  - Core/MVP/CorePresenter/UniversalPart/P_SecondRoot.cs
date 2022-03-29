using ModelCore;
using Sirenix.OdinInspector;
using TMPro;
using UltEvents;
using UnityEngine;

namespace CorePresenter
{
    [AddComponentMenu(RootPresenter.PathToPresenters+"P_Second Root")]
    public class P_SecondRoot : Presenter
    {
        public UltEvent<RootModel> Updated;
        private RootModel _targetRootModel;
        public RootModel CurrentRootModel => _targetRootModel;
        [ShowInInspector]public string HasRoot => CurrentRootModel != null ? CurrentRootModel.Alias : "Empty";

        public override void Init(RootModel rootModel)
        {
            if (_targetRootModel != null) _targetRootModel.CountModelsCahnged -= ManualUpdate;
            
            _targetRootModel = GetModel<RootModel>(rootModel, x => x.Alias == PathToModel);
            if (_targetRootModel == null) Debug.LogWarning($"Нет рут объекта по алису: {PathToModel}");


            if (_targetRootModel != null)
            {
                ManualUpdate();
                _targetRootModel.CountModelsCahnged += ManualUpdate;
            }
        }

        public void ManualUpdate() => Updated.Invoke(_targetRootModel);
    }
}