using System;
using UnityEngine;

namespace ModelCore
{
    public abstract class RefMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        public event Action<T> Updated;
        public T Component { get; private set; }
        
        public void Init(T value)
        {
            if(value==null)
                Debug.LogWarning("No ref in Ref Mono", this);
            Component = value;
            Updated?.Invoke(Component);
        }
    }
}