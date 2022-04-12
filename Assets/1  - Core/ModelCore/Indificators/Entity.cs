using System;
using System.Collections.Generic;
using System.Linq;
using DIContainer;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ModelCore
{
    [DisallowMultipleComponent][RequireComponent(typeof(LabelObjectGo))]
    public class Entity : MonoBehaviour
    {
        private LabelObjectGo _label;
        public LabelObjectGo Label => _label ??= GetComponent<LabelObjectGo>();
        [SerializeField] private List<MonoBehaviour> _components = new List<MonoBehaviour>();

        public T Select<T>(Func<T, bool> predict, bool checknull = false) where T : MonoBehaviour
        {
            var r = _components.FirstOrDefault(x =>
            {
                var target = x as T;
                if (target == null) return false;
                return predict(target);
            }) as T;
            if (checknull && r == null) Debug.LogError($"На сущности нет компонента типа {typeof(T).Name}", this);
            return r;
        }

        public T Select<T>(bool checkNull=false) where T : MonoBehaviour
        {
            var r = _components.FirstOrDefault(x => x is T) as T;
            if (checkNull && r == null) Debug.LogError($"На сущности нет компонента типа {typeof(T).Name}", this);
            return r;
        }

        public bool Has<T>() where T : MonoBehaviour => Select<T>() != null;
        
        public bool Has<T>(Func<T, bool> predict) where T : MonoBehaviour => Select<T>(predict) != null;

        public T[] SelectAll<T>() where T : MonoBehaviour => _components.Where(x => x is T).Cast<T>().ToArray();

        public T SelectOrCreate<T>() where T : MonoBehaviour
        {
            T result = Select<T>();

            if (result) return result;
            else
            {
                var newCom = gameObject.AddComponent<T>();
                _components.Add(newCom);
                return newCom;
            }
        }

        public T[] SelectAll<T>(Func<T, bool> predict) where T : MonoBehaviour
        {
            return _components.Where(x => x is T).Cast<T>().Where(predict).ToArray();
        }

        
        [DI]private void Awake() => EntityAgregator.Instance.Add(this);

        private void OnDestroy() => EntityAgregator.Instance.Remove(this);

        [Button]
        public void OnValidate()
        {
            if(!Application.isEditor) return;
            GetComponents<MonoBehaviour>().ForEach(x => _components.Add(x));
            _components = new HashSet<MonoBehaviour>(_components).ToList();
            _components.RemoveAll(x => x == null);
        }
    }
}