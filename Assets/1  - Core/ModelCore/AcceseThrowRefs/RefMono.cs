using System;
using UnityEngine;

namespace ModelCore
{
    public abstract class RefMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        public event Action<T> Updated;
        [SerializeField] private T _component;

        public T Component
        {
            get => _component;
        }

        public void Init(T value)
        {
            if(value==null)
                Debug.LogWarning("No ref in Ref Mono", this);
            _component = value;
            Updated?.Invoke(Component);
        }
    }
}