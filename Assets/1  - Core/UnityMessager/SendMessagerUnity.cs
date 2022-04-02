using System;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace Sharp.UnityMessager
{
    [AddComponentMenu("Event Bus/Sender")]
    public class SendMessagerUnity : MonoBehaviour
    {
        public static event Action<IdReciver, Event> OnMes;

        public IdReciver defaultTo;
        public Event defaultEvent;

        [Button]public void SendDefault() => Send(defaultTo, defaultEvent);

        [Button] public void Send(IdReciver to, Event e)
        {
            if(to!=null)
                OnMes?.Invoke(to, e);
        }
    }
}