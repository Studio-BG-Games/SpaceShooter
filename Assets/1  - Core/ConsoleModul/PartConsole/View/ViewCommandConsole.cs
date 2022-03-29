using ConsoleModul.PartConsole.ComandConsoless;
using UnityEngine;
using UnityEngine.UI;

namespace ConsoleModul.PartConsole.View
{
    public class ViewCommandConsole : MonoBehaviour
    {
        [SerializeField] private InputField _inputField;
        private ComandConsole _comandConsole;

        public void Init(ComandConsole comandConsole)
        {
            _comandConsole = comandConsole;
            _inputField.onEndEdit.AddListener(OnEndEdit);
        }

        private void OnEndEdit(string arg0)
        {
            if(string.IsNullOrWhiteSpace(arg0))
                return;
            _comandConsole.Invoke(arg0);
            _inputField.text = "";
        }
    }
}