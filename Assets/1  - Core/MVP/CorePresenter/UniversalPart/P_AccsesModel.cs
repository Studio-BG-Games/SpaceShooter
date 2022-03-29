using ModelCore;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CorePresenter.UniversalPart
{
    [AddComponentMenu(RootPresenter.PathToPresenters+"P_AccsesModel")]
    public class P_AccsesModel : Presenter
    {
        protected override bool isHideAll => true;
        public RootModel Model { get; private set; }
        public override void Init(RootModel rootModel) => Model = rootModel;
        
        [OnInspectorInit]
        private void InspectrorInit() => PathToModel = "AccessToModelRoot";
    }
}