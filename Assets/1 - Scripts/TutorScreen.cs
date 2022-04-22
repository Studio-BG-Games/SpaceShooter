using System;
using UiHlpers;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    [RequireComponent(typeof(PanelUI))]
    public class TutorScreen : MonoBehaviour
    {
        public Action Confirned;
        
        [SerializeField] private PanelUI _panel;

        public void ConfirmWindow()
        {
            Confirned?.Invoke();
            _panel.Close();
        }

        public void Show() => _panel.Show();

        private void OnValidate()
        {
            _panel = GetComponent<PanelUI>();
        }
    }
}