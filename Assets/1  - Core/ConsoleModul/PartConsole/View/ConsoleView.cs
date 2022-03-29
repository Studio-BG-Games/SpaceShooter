using System;
using ConsoleModul.Logger;
using ConsoleModul.PartConsole.ComandConsoless;
using ConsoleModul.PartConsole.Js;
using ConsoleModul.PartConsole.LoggerConsoles;
using Js;
using Js.Interfaces;
using ModelCore;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ILogger = ConsoleModul.Logger.ILogger;
using Object = UnityEngine.Object;

namespace ConsoleModul.PartConsole.View
{
    public class ConsoleView : MonoBehaviour
    {
        public UnityEvent SelectedCoomandConsole;
        public UnityEvent SelectedJsConsole;

        [SerializeField] private ViewLoggerConsole _viewLoggerConsole;
        [SerializeField] private ViewCommandConsole _viewCommandConsole;
        [SerializeField] private ViewJsConsole _viewJsConsole;

        private Console _console;
        private BaseLogger _comandLogger;
        private LoggerConsole _loggerConsole;
        private ComandConsole _commandConsole;
        private JsConsole _jsConsole;
        private BaseLogger _jsLogger;

        public Console Console => _console;

        private const string NameCommandLogger = "LoggerCommandConsole";
        private const string NameJsLogger = "JsLogger";
        
        public void Awake()
        {
            CreateLogger();
            CreateConsoles(_comandLogger, _jsLogger);
            InitView();
            
            _loggerConsole.AddLoger(_comandLogger, NameCommandLogger, 32);
            _loggerConsole.AddLoger(_jsLogger, NameJsLogger, 64);
            _loggerConsole.AddLoger(ConnecterUnityConsole.Instance, "DefaultUnityConsole", 128);
            _loggerConsole.AddLoger(LoggerForModel.Instance, "Model", 64);

            _commandConsole.Add(CreateCleanComand());
            _commandConsole.Add(CreateCleanAllComand());
            _commandConsole.Add(CreateChangeSizeBufferCommand(_comandLogger));
            _commandConsole.Cleaned += () =>
            {
                _commandConsole.Add(CreateCleanComand());
                _commandConsole.Add(CreateCleanAllComand());
                _commandConsole.Add(CreateChangeSizeBufferCommand(_comandLogger));
            };
            
            SelectCommandConsole();
        }

        private CommandBase CreateChangeSizeBufferCommand(BaseLogger comandLogger)
        {
            return new ArgumentCommand("Change buffer size console by name", "ChangeSize <name> <newSize>", x =>
            {
                string name = x[0];
                int newSize = 0;
                if (!int.TryParse(x[1], out newSize))
                {
                    comandLogger.LogError("second parametr cannton convert to int");
                    return;
                }
                _loggerConsole.ChageBuffer(name, newSize);
            });
        }

        public void AddComand(CommandBase comand) => _commandConsole.Add(comand);

        public void AddLoger(ILogger logger, string name)
        {
            if(name!=NameCommandLogger && name != NameJsLogger)
                _loggerConsole.AddLoger(logger, name);
        }

        public void RemoveLogger(string name)
        {
            if(name!=NameCommandLogger && name != NameJsLogger)
                _loggerConsole.RemoveLoger(name);
        }

        public void SelectJsConsole()
        {
            SelectedJsConsole?.Invoke();
            _viewLoggerConsole.ChangeLogger(NameJsLogger);
        }

        public void SelectCommandConsole()
        {
            SelectedCoomandConsole?.Invoke();
            _viewLoggerConsole.ChangeLogger(NameCommandLogger);
        }

        public void RegisterNewObjectJsConsole(AbsTypeRegister typeRegister) => _jsConsole.ReRegisterType(typeRegister);

        public void ClearObjectInJsConsole() => _jsConsole.ClearTypeConsole();

        //-----------------------------
        
        private void InitView()
        {
            _viewCommandConsole.Init(_commandConsole);
            _viewJsConsole.Init(_jsConsole);
            _viewLoggerConsole.Init(_loggerConsole);
        }

        private void CreateConsoles(ILogger comandlogger, ILogger jsLogger)
        {
            _console = new Console();
            _loggerConsole = new LoggerConsole();
            _commandConsole = new ComandConsole(comandlogger);
            _jsConsole = new JsConsole(jsLogger);
            
            _console.Add(_commandConsole, "Commands");
            _console.Add(_loggerConsole, "logger");
            _console.Add(_jsConsole, "JsConsole");
        }

        private void CreateLogger()
        {
            _comandLogger = new BaseLogger();
            _jsLogger = new BaseLogger();
        }

        private Command CreateCleanComand()
        {
            return new Command("Clear log console", "ClearConsole", ()=>_viewLoggerConsole.Clear());
        }
        
        private Command CreateCleanAllComand()
        {
            return new Command("Clear all log console", "ClearAllConsole", ()=>_viewLoggerConsole.ClearAllConsoles());
        }
        
        #if UNITY_EDITOR
        
        [ContextMenu("Copy Js console ID")]
        private void CopyJsConsoleID() => EditorGUIUtility.systemCopyBuffer = NameJsLogger;
        
        [ContextMenu("Copy comand console ID")]
        private void CopyCoomandConsoleID() => EditorGUIUtility.systemCopyBuffer = NameCommandLogger;

#endif
    }
}