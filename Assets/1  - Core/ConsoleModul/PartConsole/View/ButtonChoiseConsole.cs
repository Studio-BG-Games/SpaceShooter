using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ConsoleModul.PartConsole.View
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class ButtonChoiseConsole : MonoBehaviour
    {
        private Button _button;
        [SerializeField] private Image _image;
        [SerializeField] private Text _text;
        [SerializeField] private Color _selecColor;
        [SerializeField] private Color _usselecColor;
        [SerializeField] private UnityEvent Selected;
        [SerializeField] private UnityEvent Unselected;
        private string _name;

        private void Awake()
        {
            _button = GetComponent<Button>();
            Selected.AddListener(()=>_image.color = _selecColor);
            Unselected.AddListener(()=>_image.color = _usselecColor);
        }

        public Button Button => _button;

        public void Init(string name)
        {
            _text.text = name;
            _name = name;
        }

        public void SetSelected(string choiseId)
        {
            if(_name==choiseId) Selected?.Invoke(); 
            else Unselected?.Invoke();
        }
    }
}