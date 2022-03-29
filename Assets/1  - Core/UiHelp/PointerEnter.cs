using Lean.Transition;
using UltEvents;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UiHlpers
{
    public class PointerEnter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UltEvent OnEnter;
        
        public UltEvent OnExit;
        
        public void OnPointerEnter(PointerEventData eventData) => OnEnter.Invoke();

        public void OnPointerExit(PointerEventData eventData) => OnExit.Invoke();
    }
}