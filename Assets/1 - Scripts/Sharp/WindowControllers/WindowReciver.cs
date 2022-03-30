using DIContainer;
using UltEvents;
using UnityEngine;

namespace Sharp
{
    public class WindowReciver : MonoBehaviour
    {
        public WindowSOId MyID;
        
        [DI] private WindowController _controller;

        public UltEvent Opened;
        public UltEvent Closed;
        public UltEvent<WindowSOAction> CustomEvent;

        [DI] private void InitDi()=> _controller.Sender += Handler;

        private void Handler(WindowSOId window, WindowSOAction action)
        {
            if(window != MyID || MyID == null) return;
            if (action is OpenWindow) Opened.Invoke();
            else if(action is CloseWindow) Closed.Invoke();
            else CustomEvent.Invoke(action);
        }

        public void SendEventToOtherWindow(WindowSOId win, WindowSOAction action) => _controller.SendCustomEvent(win, action);
    }
}