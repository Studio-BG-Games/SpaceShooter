using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CorePresenter.ContextAndReciver;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Environments;
using Jint.Runtime.Interop;
using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ModelCore.Universal
{
    public class JsEn : Model
    {
        [JsonIgnore] public override string IdModel => Prefics+_script;
        [JsonIgnore] public override string Prefics => "Script_";
        
        [JsonIgnore] private Engine _brain;
        [JsonProperty] private string _script;
        public string NameScript => _script??="";

        public override void InitByModel() => Restart();

        [JsonConstructor] private JsEn(){}

        public JsEn(string scritp)
        {
            _script = scritp;
        }

        protected override void FinalRenane(string newName) => _script = newName;

        public void Restart()
        {
            _brain = new Engine();
            
            // set part
            _brain.SetValue("Root", Root);
            _brain.SetValue("Context", Context.Instance.GameModel);
            _brain.SetValue("Log", new Action<object>(x=>Root.Logger.Log(x.ToString())));
            _brain.SetValue("LogErr", new Action<object>(x=>Root.Logger.LogError(x.ToString())));
            _brain.SetValue("LogWrn", new Action<object>(x=>Root.Logger.LogWarning(x.ToString())));
            _brain.SetValue("Doc", new Func<JsValue, bool, string>((v, r) => {
                if (r)
                {
                    string result = "";
                    var obj = v.AsObject();
                    while (obj!=null)
                    {
                        result += DocObject(v.AsObject()) + "\n\n";
                        obj = obj.Prototype;
                    }

                    return result;
                }
                else
                {
                    return DocObject(v.AsObject());
                }
            }));
            _brain.SetValue("use", new Action<string>(x=>ImportMethod(x, _brain)));
            _brain.SetValue("Scope", new Func<bool, string>(all => GetScope(_brain, all)));
            _brain.SetValue("Wait", new Func<float, Action, string>(Timer.Instance.Wait));
            _brain.SetValue("StopWait", new Action<string>(Timer.Instance.StopWait));
            _brain.SetValue("Helper", HelperObject());
            RegisterTypeToEngine(typeof(LoaderRoot), _brain);
            
            // unity struct
            RegisterTypeToEngine(typeof(Vector2), _brain);
            RegisterTypeToEngine(typeof(Vector3), _brain);
            
            // reflection part
            foreach (var type in RootModel.Factory.GetTypesBModel()) RegisterTypeToEngine(type, _brain);

            //Init
            RootModel.Factory.InitEngineModel(_brain, new HashSet<string>(){_script});
            Invoke("Init");
        }

        private static void RegisterTypeToEngine(Type t, Engine e) => e.SetValue(t.Name, TypeReference.CreateTypeReference(e, t));

        private Dictionary<string, object> HelperObject()
        {
            Dictionary<string, object> helper = new Dictionary<string, object>();
            helper.Add("RandomInt", new Func<int, int, int>((x,y )=> Random.Range(x, y)));
            helper.Add("RandomFloat", new Func<float, float, float>((x,y )=> Random.Range(x, y)));
            return helper;
        }

        public JsValue InvokeParam(string name, params object[] param)
        {
            var partPath = name.Split('.');
            var startObject = GetObjectByPath(_brain.GetValue(partPath[0]), ref partPath, 1, name);
            
            if (startObject.IsUndefined())
            {
                Root.Logger.LogWarning($"Метод {name} не определён у {_script}");
                return JsValue.Null;
            }
            return _brain.Invoke(startObject, param);
        }

        public JsValue Invoke(string name)
        {
            var partPath = name.Split('.');
            var startObject = GetObjectByPath(_brain.GetValue(partPath[0]), ref partPath, 1, name);
            
            if (startObject.IsUndefined())
            {
                Root.Logger.LogWarning($"Метод {name} не определён у {_script}");
                return JsValue.Null;
            }
            return _brain.Invoke(startObject);
        }

        public bool HasFunc(string path)
        {
            var partPath = path.Split('.');
            var startObject = GetObjectByPath(_brain.GetValue(partPath[0]), ref partPath, 1, path);
            return startObject.IsObject();
        }

        JsValue GetObjectByPath(JsValue startObject, ref string[] paths, int index, string pathfuul)
        {
            if (index > paths.Length - 1) return startObject;
            if (startObject.IsUndefined())
            {
                Debug.LogError($"По пути {pathfuul} на индексе {index-1}({paths[index-1]}) ничего нет, у {_script}");
                return startObject;
            }
            return GetObjectByPath(startObject.AsObject().Get(paths[index]), ref paths, index + 1, pathfuul);
        }

        public static void ImportMethod(string nameScript, Engine e)
        {
            var nameChecker = $"__{nameScript}";
            var v = e.GetValue(nameChecker);
            if(!v.IsUndefined())
                return;
            e.SetValue(nameChecker, "0");
            RootModel.Factory.GetScript(nameScript, out var script);
            try
            {
                e.Execute(script);
            }
            catch (JavaScriptException ex)
            {
                RootModel.Factory.HandlerJsError(ex, script);
            }
        }
        
        public static string DocObject(ObjectInstance instance)
        {
            string result = "";
            string proto = instance.Prototype != null ? instance.Class : "No proto";
            result += $"{instance.Class} + {proto}\n";
            foreach (var pair in instance.GetOwnProperties())
            {
                result += $"        {pair.Key} : {LogFunc(instance, pair)}\n";
            }
            return result;

            string LogFunc(ObjectInstance instance, KeyValuePair<string, PropertyDescriptor> pair)
            {
                var type = instance.Get(pair.Key).ToString();
                return $"{type}";
            }
        }

        public static string GetScope(Engine engine, bool all = true)
        {
            string result = "";
            LexicalEnvironment env = engine.ExecutionContext.LexicalEnvironment;
            EnvironmentRecord recorderTarget = null;

            if (all)
            {
                while (env.Outer!=null) env = env.Outer;
                recorderTarget = env.Record;
            }
            else
            {
                recorderTarget = env.Record;
            }
            
            var allNames = recorderTarget.GetAllBindingNames();
            foreach (var name in allNames)
            {
                result += $"name: {name};  value: {engine.GetValue(name)}\n";
            }

            return result;
        }
       
    }
}