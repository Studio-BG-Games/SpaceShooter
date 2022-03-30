using Lean.Transition;
using UltEvents;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UiHlpers
{
    public class PointerEnter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public LeanPlayer EnterLean;
        public UltEvent OnEnter;
        
        public LeanPlayer ExitLean;
        public UltEvent OnExit;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            EnterLean.Begin();
            OnEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ExitLean.Begin();
            OnExit.Invoke();
        }
    }
}