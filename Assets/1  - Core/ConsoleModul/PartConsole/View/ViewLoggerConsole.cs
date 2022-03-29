using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ConsoleModul.Logger;
using ConsoleModul.PartConsole.LoggerConsoles;
using UnityEngine;
using UnityEngine.UI;

namespace ConsoleModul.PartConsole.View
{
    public class ViewLoggerConsole : MonoBehaviour
    {
        [SerializeField] private ButtonChoiseConsole _buttonTemplate;
        [SerializeField] private HorizontalLayoutGroup _buttonGroup;
        [SerializeField] private VerticalLayoutGroup _groupText;
        [SerializeField] private Text _textTemplate;

        private LoggerConsole _logerConsole;
        private LoggerObserver _choiseLogger;
        private List<ButtonChoiseConsole> _buttons = new List<ButtonChoiseConsole>();

        public void Init(LoggerConsole loggerConsole)
        {
            if (_logerConsole != null)
            {
                _logerConsole.AddedLoger -= RegenerateButton;
                _logerConsole.RemovedLoger -= RegenerateButton;
            }

            _logerConsole = loggerConsole;
            
            _logerConsole.AddedLoger += RegenerateButton;
            _logerConsole.RemovedLoger += RegenerateButton;
            
            GeneratteButton(_logerConsole.AvaibleLoggers());
        }

        private void OnDestroy()
        {
            if (_logerConsole != null)
            {
                _logerConsole.AddedLoger -= RegenerateButton;
                _logerConsole.RemovedLoger -= RegenerateButton;
            }
            if(_choiseLogger!=null)
                _choiseLogger.NewLog -= OnNewLog;
        }

        private void RegenerateButton() => GeneratteButton(_logerConsole.AvaibleLoggers());
        
        private void GeneratteButton(List<string> avaibleLoggers)
        {
            DeleteAllChild(_buttonGroup.gameObject);
            _buttons?.Clear();
            avaibleLoggers.ForEach(x =>
            {
                var button = Instantiate(_buttonTemplate);
                button.Button.onClick.AddListener(()=>ChangeLogger(x));
                button.transform.SetParent(_buttonGroup.transform);
                button.transform.localScale=Vector3.one;
                button.Init(x);
                _buttons.Add(button);
            });
        }

        public void Clear()
        {
            DeleteAllChild(_groupText.gameObject);
            _choiseLogger.Clear();
        }

        public void ClearAllConsoles()
        {
            DeleteAllChild(_groupText.gameObject);
            _logerConsole.AvaibleLoggers().ForEach(x =>
            {
                _logerConsole.GetObserver(x).Clear();
            });
        }
        
        private void DeleteAllChild(GameObject gameObject)
        {
            var transforms = gameObject.GetComponentsInChildren<RectTransform>().Except(new RectTransform[]{(RectTransform)gameObject.transform});
            foreach (var rectTransform in transforms) Destroy(rectTransform.gameObject);
        }
        
        public void ChangeLogger(string s)
        {
            var tmpConsole = _logerConsole.GetObserver(s);
            if(tmpConsole==null)
                return;
            if(_choiseLogger!=null)
                _choiseLogger.NewLog -= OnNewLog;
            _choiseLogger = tmpConsole;
            _choiseLogger.NewLog += OnNewLog;
            _buttons.ForEach(x=>x.SetSelected(s));
            GenerateText();
        }

        private void OnNewLog(ConsoleMessage obj) => GenerateText();

        private void GenerateText()
        {
            DeleteAllChild(_groupText.gameObject);
            foreach (var message in _choiseLogger.GetAllMessage())
            {
                if(message==null)
                    continue;
                var text = Instantiate(_textTemplate);
                text.color = message.Color;
                text.text ="@ - " +message.Message;
                if (message.IsBold)
                    text.fontStyle = FontStyle.Bold;
                text.transform.SetParent(_groupText.transform);
                text.transform.localScale=Vector3.one;
            }
        }
    }
}