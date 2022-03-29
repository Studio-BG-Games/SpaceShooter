using System;
using System.Reflection;
using ConsoleModul.Logger;
using Jint;
using Js.Interfaces;
using UnityEngine;
using ILogger = ConsoleModul.Logger.ILogger;


namespace ConsoleModul.PartConsole.Js
{
    public class JsConsole : IPartConsole
    {
        private readonly ILogger _jsLogger;
        private Engine _engine;
        private string _lastTrace = "";

        public event Action<GameObject> LookAt;
        
        public JsConsole(ILogger jsLogger)
        {
            _jsLogger = jsLogger;
            ClearTypeConsole();
            LookAt += HandleLookAt;
        }

        private void HandleLookAt(GameObject obj)
        {
            if(obj)
                _engine.SetValue("LastLookAt", (object) obj);
            else
                _jsLogger.LogWarning("LookAt result is null :(");
        }

        public void ReRegisterType(ITypeRegister register)
        {
            ClearTypeConsole();
            register.Register(_engine);
        }
        
        
        public void ClearTypeConsole()
        {
            _engine = CreateEngine();
            JsHellper.DefaultCommand(_engine, _jsLogger);
            _engine.SetValue("StackTrace", new Action(
                () =>
                {
                    if(!string.IsNullOrEmpty(_lastTrace))
                        _jsLogger.Log(new ConsoleMessage().SetColor(Color.white).SetMessage(_lastTrace));
                }));
            _engine.SetValue("LookAt", new Func<GameObject>(() =>
            {
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
                if (Physics.Raycast(ray, out var result, Single.PositiveInfinity, ~(1 << 5)))
                {
                    if (result.collider.gameObject != null)
                    {
                        LookAt?.Invoke(result.collider.gameObject);
                        return result.collider.gameObject;
                    }
                }

                var hit2d = Physics2D.Raycast(ray.origin, Vector2.zero, Single.PositiveInfinity, ~(1 << 5));
                if (hit2d.collider.gameObject != null)
                {
                    LookAt?.Invoke(hit2d.collider.gameObject);
                    return hit2d.collider.gameObject;
                }

                LookAt?.Invoke(null);
                return null;
            }));
            
        }

        public void Exute(string comand)
        {
            _jsLogger.Log(new ConsoleMessage().SetMessage($">{comand}").SetColor(Color.black));
            try
            {
                _engine?.Execute(comand);
            }
            catch (Exception e)
            {
                _jsLogger.Log(new ConsoleMessage().SetMessage("Error message: "+e.Message).SetColor(Color.red));
                _lastTrace = e.StackTrace;
            }
        }

        private Engine CreateEngine()
        {
            return new Engine(cfg =>
            {
                foreach (var assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies()) cfg.AllowClr(Assembly.Load(assemblyName));
            });
        }
    }
}