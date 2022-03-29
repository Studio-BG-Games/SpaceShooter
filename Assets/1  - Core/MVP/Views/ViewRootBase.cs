using CorePresenter;
using ModelCore;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace MVP.Views
{
    public abstract class ViewRootBase : ViewBase<RootModel>
    {
        [InfoBox("$GetInfo")]
        [ToggleLeft][SerializeField]private bool IsManual;
        
        [DisableIf("IsManual")]
        public P_SecondRoot P_Root;

        private void Awake()
        {
            CustomAwake();

            if (!IsManual)
            {
                P_Root.Updated += View;
                if (P_Root.CurrentRootModel != null) View(P_Root.CurrentRootModel);
            }
        }

        protected virtual void CustomAwake() { }

        public abstract override void View(RootModel engine);

        [OnInspectorInit]
        private void InInspectorInit()
        {
            if (P_Root == null) P_Root = GetComponent<P_SecondRoot>();
        }

        protected virtual string GetInfo() => "Non override";
        
        protected bool Log(object objForCheck, string mes)
        {
            if(objForCheck==null) Debug.LogWarning(mes);
            return objForCheck == null;
        }
    }
}