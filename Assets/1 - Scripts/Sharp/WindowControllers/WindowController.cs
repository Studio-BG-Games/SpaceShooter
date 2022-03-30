using System;
using UnityEngine;

namespace Sharp
{
    public class WindowController : MonoBehaviour
    {
        public event Action<WindowSOId, WindowSOAction> Sender;
        public OpenWindow OpenEvent;
        public CloseWindow CloseEvent;

        public void Open(WindowSOId window) => Sender?.Invoke(window, OpenEvent);
        
        public void Close(WindowSOId window) => Sender?.Invoke(window, CloseEvent);

        public void SendCustomEvent(WindowSOId window, WindowSOAction action) => Sender?.Invoke(window, action);
    }
}