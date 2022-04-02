using System;
using System.Collections.Generic;
using System.Reflection;
using ConsoleModul.PartConsole;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Js.Interfaces;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Js
{
    
    public class JsBrain : MonoBehaviour
    {
        [InfoBox("Есть следующие методы для скрита -\n " +
                 "Awake() Start() Update() FixedUpdate() -\n" +
                 "LateUpdate() OnEnable() OnDisable() OnDestroy()-\n")]
        
        [InfoBox("$lastMessage")]
        [InlineButton("LoadScriptFromFile", "Load Script From File")]
        public TextAsset[] ScriptFiles;

        [MultiLineProperty(25)]
        [HideLabel]
        [Title("Scrits text in js")]
        [SerializeField]
        public string UserScript;
        
        /*private static string DefalutVarible = "let u = importNamespace(\"UnityEngine\")"+
                                        "\nlet GM = u.GameObject"+
                                        "\nlet UObject = u.Object\n";*/
        
        [HideInInspector]public string lastMessage="Last message";
        
        private Engine _engine=>__engine??=CreateEngine(UserScript);
        private Engine __engine;
        private JsValue _awake, _start, _update, _fixedUpdate, _lateUpdate, _enable, _disable, _destroy;


        [SerializeField] private AbsTypeRegister[] _typeRegister;
        [SerializeField] public List<ComponentToScript> Components = new List<ComponentToScript>();

        //
        // EdtiorMethods
        //
        

        [Button("Обнулить сообщение")]
        private void ZeroMes() => lastMessage = "I am zerong";


        [Button("Boostrap Me")]
        private void Boostrap() => __engine = CreateEngine(UserScript);

        [Button("Invoke func Test object")]
        private void InvokeFunctionTest(string nameFunc, params object[] args)
        {
            __engine = CreateEngine(UserScript);
            _engine.Invoke(nameFunc, args);
        }
        
        [Button("Invoke func Test unityObject")]
        private void InvokeFunctionTest(string nameFunc, params UnityEngine.Object[] args)
        {
            __engine = CreateEngine(UserScript);
            _engine.Invoke(nameFunc, args);
        }

        public void SetValue(string name, Component component) => _engine.SetValue(name, (object) component);

        private Engine CreateEngine(string script)
        {
            var e = new Engine(cfg =>
            {
                //foreach (var assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies()) cfg.AllowClr(Assembly.Load(assemblyName));
            });
            RegisterType(e, typeof(Vector3));
            _typeRegister.ForEach(x => x.Register(e));
            Components.ForEach(x=>e.SetValue(x.Name, (object)x.Component));
            JsHellper.DefaultCommand(e, new LoggerToUnity());
            e.Execute(script);
            return e;
        }

        private void RegisterType(Engine e, Type type) => e.SetValue(type.Name, TypeReference.CreateTypeReference(e, type));

        private void LoadScriptFromFile()
        {
            if (ScriptFiles.Length > 0)
            {
                //UserScript = DefalutVarible;
                foreach (var f in ScriptFiles)
                {
                    if(f==null)
                        continue;
                    UserScript+="\n\n"+"//"+f.name+"\n\n"+f.text;    
                }
            }
            else
            {
                //UserScript = DefalutVarible;
                lastMessage = "==!!!== No Script File ==!!!==";
            }
        }
        
        //
        //Flow program
        //
        
        public void InvokeFunc_object(string name, params object[] args) => _engine.Invoke(name, args);

        public void Invoke(string name) => _engine.Invoke(name);

        public void Invoke(string name, System.Object[] t) => _engine.Invoke(name, t);
        
        public void Invoke(string name, UnityEngine.Object t, System.Object t2, UnityEngine.Object t3) => _engine.Invoke(name, t, t2, t3);
        
        public void Invoke(string name, System.Object t) => _engine.Invoke(name, new object[]{t});

        public void Invoke(string name, System.Object t, System.Object t2) => _engine.Invoke(name, t, t2);

        public void Invoke(string name, System.Object t, System.Object t2, System.Object t3) => _engine.Invoke(name, t, t2, t3);

        public void Invoke(string name, System.Object t, System.Object t2, System.Object t3, System.Object t4) => _engine.Invoke(name, t, t2, t3, t4);

        public void Invoke(string name, System.Object t, System.Object t2 , System.Object t3 , System.Object t4 , System.Object t5) => _engine.Invoke(name, t, t2, t3, t4, t5);

        public void Invoke(string name, System.Object t, System.Object t2, System.Object t3, System.Object t4, System.Object t5, System.Object t6) => _engine.Invoke(name, t, t2, t3, t4, t5, t6);

        public void Invoke(string name, UnityEngine.Object t) => _engine.Invoke(name, t);

        public void Invoke(string name, UnityEngine.Object t, UnityEngine.Object t2) => _engine.Invoke(name, t, t2);

        public void Invoke(string name, UnityEngine.Object t, UnityEngine.Object t2, UnityEngine.Object t3) => _engine.Invoke(name, t, t2, t3);

        public void Invoke(string name, UnityEngine.Object t, UnityEngine.Object t2, UnityEngine.Object t3, UnityEngine.Object t4) => _engine.Invoke(name, t, t2, t3, t4);

        public void Invoke(string name, UnityEngine.Object t, UnityEngine.Object t2 , UnityEngine.Object t3 , UnityEngine.Object t4 , UnityEngine.Object t5) => _engine.Invoke(name, t, t2, t3, t4, t5);

        public void Invoke(string name, UnityEngine.Object t, UnityEngine.Object t2, UnityEngine.Object t3, UnityEngine.Object t4, UnityEngine.Object t5, UnityEngine.Object t6) => _engine.Invoke(name, t, t2, t3, t4, t5, t6);
       
        private void Start()
        {
            lastMessage = "";
            __engine = CreateEngine(UserScript);
            
            
            _awake = _engine.GetValue("Awake");
            
            _start = _engine.GetValue("Start");
            _update = _engine.GetValue("Update");
            _fixedUpdate = _engine.GetValue("FixedUpdate");
            _lateUpdate = _engine.GetValue("LateUpdate");
            _enable = _engine.GetValue("OnEnable");
            _disable = _engine.GetValue("OnDisable");
            _destroy = _engine.GetValue("OnDestroy");
            
            TryInvokeFunc(_start);
        }

        private void OnEnable() => TryInvokeFunc(_enable);

        private void OnDisable() => TryInvokeFunc(_disable);

        private void Update() => TryInvokeFunc(_update);

        private void FixedUpdate() => TryInvokeFunc(_fixedUpdate);

        private void LateUpdate() => TryInvokeFunc(_lateUpdate);

        private void OnDestroy() => TryInvokeFunc(_destroy);


        private void TryInvokeFunc(JsValue value)
        {
            if(value!=null)
                if (!value.IsUndefined()) value.Invoke();
        }
    }
}