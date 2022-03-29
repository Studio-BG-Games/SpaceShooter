using ConsoleModul.PartConsole.Js;
using UnityEngine;
using UnityEngine.UI;

namespace ConsoleModul.PartConsole.View
{
    public class ViewJsConsole : MonoBehaviour
    {
        public JsConsole JsConsole => _jsConsole;
        private JsConsole _jsConsole;
        [SerializeField] private Text _lookAtText;
        [SerializeField] private InputField _inputField;

        public void Init(JsConsole jsConsole)
        {
            _jsConsole = jsConsole;
            _jsConsole.LookAt += OnLookAt;
            _inputField.onEndEdit.AddListener(s =>
            {
                _jsConsole.Exute(s);
                _inputField.text = "";
            });
        }

        public void MakeLookAt() => _jsConsole.Exute("LookAt()");

        private void OnLookAt(GameObject obj) => _lookAtText.text = obj != null ? $"Look at: {obj.name}" : "Look at: null";
    }
}