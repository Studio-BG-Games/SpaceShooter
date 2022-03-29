using System;
using ModelCore;
using Sirenix.OdinInspector;
using UnityEngine;


namespace CorePresenter.UniversalPart
{
    [AddComponentMenu(RootPresenter.PathToPresenters+"P_Debug model")]
    public partial class P_DebugModel : Presenter
    {
        public static event Action<RootModel> DebugAction;
        protected override bool isHideAll => true;
        
        private RootModel _rootModel;
        public RootModel ModelForDebug => _rootModel;

        public override void Init(RootModel rootModel) => _rootModel = rootModel;

        [OnInspectorInit]
        private void InspectrorInit() => PathToModel = "Debug";

        [Button]
        public void OpenEditor()
        {
            if(_rootModel!=null)
            DebugAction?.Invoke(_rootModel);
        }
    }
}