using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Sharp.UnityMessager
{
    [AddComponentMenu("Event Bus/SendMessagerValueUnity")]
    public abstract class SendMessagerValueUnity<T> : MonoBehaviour
    {
        public static event Action<IdReciver, Event, T> OnMes;

        public IdReciver defaultTo;
        public Event defaultEvent;
        public T DefaultValue;

        [Button]public void SendDefault() => Send(defaultTo, defaultEvent, DefaultValue);

        [Button] public void Send(IdReciver to, Event e, T value)
        {
            if(to!=null)
                OnMes?.Invoke(to, e, value);
        }
    }
}