using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ConsoleModul.Logger;
using CorePresenter.ContextAndReciver;
using DIContainer;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Debugger;
using Jint.Runtime.Interop;
using Js;
using ModelCore.Messages;
using ModelCore.Universal;
using ModelCore.Universal.AliasValue;
using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine;
using ILogger = ConsoleModul.Logger.ILogger;
using Random = UnityEngine.Random;

namespace ModelCore
{
    public class RootModel : Model
    {
        [JsonIgnore] public override string IdModel => $"{Prefics}{Alias}";
        public override string Prefics => "Root_";

        [JsonIgnore] public ILogger Logger => Factory.GetLoggerModel();
        [JsonProperty] public string Alias { get; private set; }
        public static RootModel Empty = new RootModel("FakeNull");
        
        [JsonProperty] private Dictionary<string, Model> _modeles = new Dictionary<string, Model>();
        [JsonIgnore] public IReadOnlyCollection<Model> Slaves => _modeles.Values.ToList().AsReadOnly(); 

        public event Action CountModelsCahnged;

        public Model this[string id] // example id: id = "Root_Health/F_Current"
        {
            get
            {
                var strs = id.Split('/');
                RootModel lastRootModel = this;
                for (int i = 0; i < strs.Length-1; i++)
                {
                    lastRootModel = lastRootModel[strs[i]] as RootModel;
                    if (lastRootModel == null)
                    {
                        Debug.LogWarning($"По пути {id} на моменте {strs[i]} ничего нет или нет RootModel. AliassRoot: {Alias}");
                        return null;
                    }
                }
                var result = lastRootModel.GetId(strs.Last());
                if(result==null && Application.isPlaying) Debug.LogWarning($"По пути {id} ничего нет. AliassRoot: {Alias}");
                return result;
            }
        }

        public override void SendMessage(Message message)
        {
            (message as ISetValueToChildRoot).TrySet(this);
        }

        public RootModel(string alias) => Alias = alias;

        [JsonConstructor] private RootModel() { }

        public void Update() => CountModelsCahnged.Invoke();

        public void Init() { _modeles.ForEach(x => { if (x.Value != this) x.Value.InitByModel(); }); }

        public override void InitByModel() => Init();

        public RootModel AddModel(Model model)
        {
            if (model == null) return this;
            if (model == this)
            {
                Logger.LogError("Дурак?");
                return this;
            }
            if (!_modeles.ContainsKey(model.IdModel))
            {
                _modeles.Add(model.IdModel, model);
                model.MoveToNewRoot(this);
                CountModelsCahnged?.Invoke();
            }
            return this;
        }

        protected override void FinalRenane(string newName) => Alias = newName;

        public bool CanRename(Model model, string newName)
        {
            /*
            if (GetId(model.IdModel) == null)
            {
                Debug.LogWarning($"Модель - {model.IdModel}, t:{model.GetType().Namespace}; нет в словаре у {IdModel}");
                return false;
            }*/
            var targetModel = _modeles.FirstOrDefault(x => x.Value == model);
            if (targetModel.Value == null)
            {
                Debug.LogWarning($"Модель - {model.IdModel}, t:{model.GetType().Namespace}; не является ребенком");
                return false;
            }

            string newId = "";
            if(model.GetType().IsSubclassOf(typeof(CsEn))) newId = newName;
            else newId = model.Prefics + newName;
            if (GetId(newId) != null) return false;
            return true;
        }

        public void UpdateNames()
        {
            var list = _modeles.Values.ToList();
            _modeles = list.ToDictionary(x => x.IdModel);
            CountModelsCahnged?.Invoke();
        }
        
        public RootModel AddModels(Model[] models)
        {
            models.ForEach(x => AddModel(x));
            return this;
        }

        public Model GetId(string id)
        {
            _modeles.TryGetValue(id, out var r);
            return r;
        }
        
        public T GetIdT<T>(string id) where T : Model => GetId(id) as T;

        public void DeleteId(string id) => BaseDelete(() => _modeles.Remove(id));

        public List<T> SelectAll<T>(Func<T, bool> predict) where T : Model
        {
            List<T> result = new List<T>();
            _modeles.Where(x =>
            {
                var m = x.Value as T;
                if (m == null) return false;
                return predict(m);
            }).ForEach(x=>result.Add(x.Value as T));
            return result;
        }
        
        public T Select<T>(Func<T, bool> predicate) where T : Model
        {
            return _modeles.FirstOrDefault(x =>
            {
                var m = x.Value as T;
                if (m == null) return false;
                return predicate(m);
            }).Value as T;
        }
        
        public bool TrySelect<T>(Func<T, bool> predicate, T result) where T : Model
        {
            result = _modeles.FirstOrDefault(x =>
            {
                var m = x.Value as T;
                if (m == null) return false;
                return predicate(m);
            }).Value as T;
            return result != null;
        }

        public List<RootModel> SelectAllRoot(Func<RootModel, bool> predict) => SelectAll<RootModel>(predict);

        public RootModel SelectRoot(Func<RootModel, bool> predict) => Select<RootModel>(predict);

        public void DeleteT<T>(Func<T, bool> predicate) where T : Model
        {
            var model = Select<T>(predicate);
            if(model!=null) BaseDelete(()=>
            {
                _modeles.Remove(_modeles.FirstOrDefault(x => x.Value == model).Key);
            });
        }

        public void DeleteAll() { BaseDelete(() => _modeles.Clear()); }

        public void LogToConsole()
        {
            string result = "_______________\n";
            _modeles.ForEach(x => result += x.Key + "\n");
            result += "_______________";
            LoggerForModel.Instance.Log(result);
            Factory.LogToUnityConsole.Instance.Log(result);
        }

        private void BaseDelete(Action callback)
        {
            callback?.Invoke();
            CountModelsCahnged?.Invoke();
        }
       
        public static class Factory
        {
            private static Dictionary<string, TextAsset> _scriptsFile = new Dictionary<string, TextAsset>();
            private static Engine _engine => GetJsEngine();

            public static bool GetScript(string nameScript, out string script)
            {
                var result = _scriptsFile.TryGetValue(nameScript, out var r);
                script = r ? r.text : "";
                return result;
            }
            
            public static void AddScriptFile(TextAsset asset)
            {
                if(asset==null)
                    return;
                if(_scriptsFile.ContainsKey(asset.name))
                    Debug.LogWarning($"У вас есть два варианта скрипта {asset.name}. Предыдущий скрипт не будет перезаписан");
                else
                    _scriptsFile.Add(asset.name, asset);
            }
            
            public static void AddScriptFile(TextAsset[] asset) => asset.ForEach(x => AddScriptFile(x));
            
            public static void RemoveScriptFile(TextAsset asset) => RemoveScriptFile(asset.name);

            public static void RemoveScriptFile(TextAsset[] asset) => asset.ForEach(x => RemoveScriptFile(x));

            public static void RemoveScriptFile(string name) => _scriptsFile.Remove(name);

            public static void RemoveScriptFile(string[] name) => name.ForEach(x => RemoveScriptFile(x));

            public static ILogger GetLoggerModel()
            {
                if (Application.isEditor)
                    return LogToUnityConsole.Instance;
                else
                    return LoggerForModel.Instance;
            }

            public static RootModel CreateFromJson(string json) => LoaderRoot.ModelFromJson(json);
            
            public static RootModel CreateFromJs(string jsSctips, string nameFile)
            {
                _engine.Execute(jsSctips);
                RootModel result = null;
                try
                {
                    result = _engine.Invoke("Create").ToObject() as RootModel;
                }
                catch (JavaScriptException e)
                {
                    Debug.LogError(jsSctips);
                    Debug.LogError($"Line: {e.LineNumber}; Colume: {e.Column}\n" + $"Js Error: {e.Error.ToString()}\n" + $"CallStack: {e.CallStack}\n" + $"Sharp error: {e.Message}\n" + $"Sharp source: {e.Source}\n");
                }
                catch(Exception e)
                {
                    Debug.LogError("Незвивестная ошибка\n" + $"{e.Message}\n" + $"{e.Source}");
                }
                
                if (result == null)
                    Debug.LogError($"{nameFile} не возвращает RootModel\n {jsSctips}");
                else
                    result.Init();

                return result;
            }

            public static JsonSerializerSettings SettingJson()
            {
                return new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Objects,
                    PreserveReferencesHandling  = PreserveReferencesHandling.Objects,
                };
            }

            public static void InitEngineModel(Engine brain, HashSet<string> scriptsName)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var scriptName in scriptsName)
                {
                    if (!_scriptsFile.TryGetValue(scriptName, out var file))
                    {
                        Debug.LogWarning($"нет скрипта под именем {scriptName}");
                        continue;
                    }
                    builder.Append(file.text);
                }
                var r = builder.ToString();
                try
                {
                    brain.Execute(r);
                }
                catch (JavaScriptException e)
                {
                    HandlerJsError(e, r);
                }
            }

            public static void HandlerJsError(JavaScriptException e, string script)
            {
                Debug.LogError($"Line: {e.LineNumber}; Colume: {e.Column}\n" +
                               $"JsValue: {e.Error.ToString()}\n" +
                               $"Scrit:\n" +
                               $"{script}\n\n" +
                               $"Error by csharp: {e.Message}\n"+ 
                               $"CallStack: {e.CallStack}");
            }

            public static Type[] GetTypesBModel()
            {
                Type ourtype = typeof(Model); // Базовый тип
                var firstPart = Assembly.GetAssembly(ourtype).GetTypes().Where(type => type.IsSubclassOf(ourtype) && type.IsAbstract == false);
                Type typeRoot = typeof(RootModel);
                var second = Assembly.GetAssembly(typeRoot).GetTypes().Where(type => type.IsSubclassOf(typeRoot));
                return firstPart.Union(second).Union(new[] {typeof(RootModel)}).ToArray();
            }

            private static Engine GetJsEngine()
            {
                var engine = new Engine();
                engine.SetValue("use", new Action<string>(x=>JsEn.ImportMethod(x, engine)));
                engine.SetValue("Log", new Action<object>(x => Debug.Log(x)));
                engine.SetValue("Doc", new Func<JsValue, bool, string>(DocMethod));
                engine.SetValue("Scope", new Func<bool, string>(all => JsEn.GetScope(engine, all)));
                engine.SetValue(typeof(Vector2).Name, TypeReference.CreateTypeReference(engine, typeof(Vector2)));
                engine.SetValue(typeof(Vector3).Name, TypeReference.CreateTypeReference(engine, typeof(Vector3)));
                engine.SetValue("Contex", Context.Instance.GameModel);
                string str = "";
                foreach (var type in GetTypesBModel())
                {
                    var re = TypeReference.CreateTypeReference(engine, type);
                    str += $"{type.Name} : {type}+\n";
                    
                    engine.SetValue(type.Name, re);
                }
                Debug.Log(str);
                return engine;
            }
            
            private static string DocMethod(JsValue v, bool r)
            {
                if (r)
                {
                    string result = "";
                    var obj = v.AsObject();
                    while (obj != null)
                    {
                        result += JsEn.DocObject(v.AsObject())+"\n\n";
                        obj = obj.Prototype;
                    }

                    return result;
                }
                else
                {
                    return JsEn.DocObject(v.AsObject());
                }
            }

            public class LogToUnityConsole : ILogger
            {
                private static LogToUnityConsole _instance;
                public static LogToUnityConsole Instance => _instance != null ? _instance : _instance = new LogToUnityConsole();
                private static string prefix = "Model: ";

                private LogToUnityConsole() { HasLog?.Invoke(null); }
                
                public event Action<ConsoleMessage> HasLog;
                
                public void Log(string mes) => Debug.Log(prefix+mes);

                public void Log(string mes, ConsoleMessage setting) => Debug.Log(prefix+mes);

                public void Log(ConsoleMessage message) => Debug.Log(prefix+message.Message);

                public void LogError(string mes) => Debug.LogError(prefix+mes);

                public void LogWarning(string mes) => Debug.LogWarning(prefix+mes);
            }
        }
    }
}