using UltEvents;
using UnityEngine;

namespace UiHlpers
{
    public class PanelUI : MonoBehaviour
    {
        public UltEvent OnShow;
        public UltEvent OnHide;
        
        public void Show() => OnShow.Invoke();

        public void Close() => OnHide.Invoke();
    }
}