using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

namespace DIContainer
{
    /// <summary>
    /// Simple Di container with Single and Transit object
    /// </summary>
    public sealed class DiBox
    {
        /// <summary>
        /// Simple of DI 
        /// </summary>
        public static DiBox MainBox => _instance ?? (_instance = new DiBox());

        private static DiBox _instance;

        public delegate T TransitMethod<T>();

        private readonly Dictionary<Type, Dictionary<string, object>> _dictionaryTransit;
        private readonly Dictionary<Type, Dictionary<string, object>> _dictionarySingle;

        public DiBox()
        {
            _dictionaryTransit = new Dictionary<Type, Dictionary<string, object>>();
            _dictionarySingle = new Dictionary<Type, Dictionary<string, object>>();
        }

        public List<KeyValuePair<Type, Dictionary<string, object>>> GetAllSingle() => _dictionarySingle.ToList();

        public void RemoveSingel<T>(string id = "") where T : class
        {
            if (_dictionarySingle.ContainsKey(typeof(T)))
                _dictionarySingle[typeof(T)].Remove(id);
            if (_dictionarySingle.Count == 0)
                _dictionarySingle.Remove(typeof(T));
        }

        public void RemoveSingelType(Type type, string id = "")
        {
            if (_dictionarySingle.ContainsKey(type))
                _dictionarySingle[type].Remove(id);
            if (_dictionarySingle.Count == 0)
                _dictionarySingle.Remove(type);
        }
        
        public void RemoveTransit<T>(string id = "") where T : class
        {
            if (_dictionaryTransit.ContainsKey(typeof(T)))
                _dictionaryTransit[typeof(T)].Remove(id);
            if (_dictionaryTransit.Count == 0)
                _dictionaryTransit.Remove(typeof(T));
        }

        /// <summary>
        /// Requests a new instance by type 
        /// </summary>
        /// <param name="id">id of required type, default = ""</param>
        /// <typeparam name="T">The required type</typeparam>
        /// <returns>return new instance of T</returns>
        public T ResolveTransit<T>(string id = "") where T : class
        {
            if (!_dictionaryTransit.ContainsKey(typeof(T)))
                throw new Exception($"DI container does not contain this type  - Type: {typeof(T)}");

            if (!_dictionaryTransit[typeof(T)].ContainsKey(id))
                throw new Exception($"The container does not contain under this ID - Type: {typeof(T)} \\ Id: '{id}'");

            return ((TransitMethod<T>) _dictionaryTransit[typeof(T)][id]).Invoke();
        }

        public void InjectAndRegisterAsSingle<T>(T instance, string id = "")
        {
            RegisterSingle<T>(instance, id);
            InjectSingle(instance);
        }

        /// <summary>
        /// Register a new instance by type 
        /// </summary>
        /// <param name="transitMethod">Method that will create a new type </param>
        /// <param name="id">id of required type, default = ""</param>
        /// <typeparam name="T">The required type</typeparam>
        public void RegisterTransit<T>(TransitMethod<T> transitMethod, string id = "") where T : class
        {
            if (transitMethod == null)
                throw new Exception($"Transit Method Create is null - type {typeof(T)}");

            if (_dictionaryTransit.ContainsKey(typeof(T)))
                if (_dictionaryTransit[typeof(T)].ContainsKey(id))
                    throw new Exception($"DI container already contains this type '{typeof(T)}' and this ID '{id}' ");

            if (_dictionaryTransit.ContainsKey(typeof(T)))
            {
                _dictionaryTransit[typeof(T)].Add(id, transitMethod);
            }
            else
            {
                _dictionaryTransit.Add(typeof(T), new Dictionary<string, object>());
                _dictionaryTransit[typeof(T)].Add(id, transitMethod);
            }
        }

        public bool HasSingle<T>(string id = "") where T : class
        {
            if (!_dictionarySingle.ContainsKey(typeof(T))) return false;
            if (!_dictionarySingle[typeof(T)].ContainsKey(id)) return false;
            return true;
        }
        
        /// <summary>
        /// Requests a single by type 
        /// </summary>
        /// <param name="id">id of required type, default = ""</param>
        /// <typeparam name="T">The required type</typeparam>
        public T ResolveSingle<T>(string id = "") where T : class
        {
            if (!_dictionarySingle.ContainsKey(typeof(T)))
                throw new Exception($"DI container does not contain this type  - Type: {typeof(T)}");

            if (!_dictionarySingle[typeof(T)].ContainsKey(id))
                throw new Exception($"The container does not contain under this ID - Type: {typeof(T)} \\ Id: '{id}'");
            return _dictionarySingle[typeof(T)][id] as T;
        }
        
        public object ResolveSingle(Type type, string id = "")
        {
            if (!_dictionarySingle.ContainsKey(type))
                throw new Exception($"DI container does not contain this type  - Type: {type}");

            if (!_dictionarySingle[type].ContainsKey(id))
                throw new Exception($"The container does not contain under this ID - Type: {type} \\ Id: '{id}'");

            return _dictionarySingle[type][id];
        }

        /// <summary>
        /// Register a new single by type 
        /// </summary>
        /// <param name="instance">instance of single</param>
        /// <param name="id">id of required type, default = ""</param>
        /// <typeparam name="T">The required type</typeparam>
        public void RegisterSingle<T>(T instance, string id = "")
        {
            if (instance == null)
                throw new Exception($"Instance is null - type {typeof(T)}");

            if (_dictionarySingle.ContainsKey(typeof(T)))
                if (_dictionarySingle[typeof(T)].ContainsKey(id))
                    throw new Exception($"DI container already contains this type '{typeof(T)}' and this ID '{id}' ");

            AddToDict(instance, id, typeof(T));
        }

        public void RegisterSingleType(Object instance, string id = "")
        {
            if (instance == null)
                throw new Exception($"Instance is null");
            
            Type typeInstance = instance.GetType();

            if (_dictionarySingle.ContainsKey(typeInstance))
                if (_dictionarySingle[typeInstance].ContainsKey(id))
                    throw new Exception($"DI container already contains this type '{typeInstance}' and this ID '{id}' ");
            
            AddToDict(instance, id, typeInstance);
        }

        public GameObject CreatePrefab(GameObject prefab)
        {
            if (prefab == null)
                throw null;

            bool initinalActiveStatePrefab = prefab.activeSelf;
            prefab.SetActive(false);
            
            var createdPrefab = UnityEngine.Object.Instantiate(prefab);
            InjectSingle(createdPrefab);
            
            prefab.SetActive(initinalActiveStatePrefab);
            createdPrefab.SetActive(initinalActiveStatePrefab);

            return createdPrefab;
        }

        public T CreatePrefab<T>(T instance) where T : Component => CreatePrefab(instance.gameObject).GetComponent<T>();

        public void InjectSingle(GameObject gameObject)
        {
            foreach (var monoBeh in gameObject.GetComponentsInChildren<MonoBehaviour>(true)) InjectSingle(monoBeh);
        }

        public void InjectSingle(Object obj)
        {
            if(obj==null)
                return;
            
            var listFeild = obj.GetType().
                GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).
                Where(x => CustomAttributeExtensions.GetCustomAttribute<DI>((MemberInfo) x) != null);
            foreach (var field in listFeild)
            {
                var att = field.GetCustomAttribute<DI>();
                field.SetValue(obj, ResolveSingle(field.FieldType, att.Id));
            }
            
            var listProperty = obj.GetType().
                GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).
                Where(x => x.GetCustomAttribute<DI>() != null);
            foreach (var prop in listProperty)
            {
                var att = prop.GetCustomAttribute<DI>();
                prop.SetValue(obj, ResolveSingle(prop.PropertyType, att.Id));
            }

            var listMethodInfo = obj.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (listMethodInfo.Length > 0)
            {
                var methodInit = listMethodInfo.Where(x => x.GetCustomAttribute<DI>() != null);
                if(methodInit.Count() > 0)
                    methodInit.First().Invoke(obj, new object[0]);
            }
        }

        private void AddToDict(object instance, string id, Type typeInstance)
        {
            if (_dictionarySingle.ContainsKey(typeInstance))
            {
                _dictionarySingle[typeInstance].Add(id, instance);
            }
            else
            {
                _dictionarySingle.Add(typeInstance, new Dictionary<string, object>());
                _dictionarySingle[typeInstance].Add(id, instance);
            }
        }
    }
}