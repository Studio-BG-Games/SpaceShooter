using System;
using Lean.Transition;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;
using Object = System.Object;

namespace UiHlpers
{
    public class PanelUI : MonoBehaviour
    {
        [SerializeField] private bool _startValue;
        [SerializeField] private bool _useStartValue;
        
        public UltEvent OnShow;
        public LeanManualAnimation OnShowAnim;
        public UltEvent OnHide;
        public LeanManualAnimation OnHideAnim;

        private void Awake()
        {
            if (_useStartValue)
            {
                if(_startValue) Show();
                else Close();
            }
        }

        public void SetVal(bool b)
        {
            if(b) Show(); else Close();
        }
        

        public void Show()
        {
            OnShow.Invoke();
            OnShowAnim?.BeginTransitions();
        }

        public void Close()
        {
            OnHide.Invoke();
            OnHideAnim?.BeginTransitions();
        }

        [Button]private void CreateObjectAnims()
        {
            if (!OnShowAnim) OnShowAnim = CreateObject("[Open]");
            if (!OnHideAnim) OnHideAnim = CreateObject("[Close]");
        }

        private LeanManualAnimation CreateObject(string name)
        {
            var r = new GameObject(name, new[] {typeof(LeanManualAnimation), typeof(RectTransform)}).GetComponent<LeanManualAnimation>();
            r.transform.SetParent(transform);
            return r;
        }
    }
}