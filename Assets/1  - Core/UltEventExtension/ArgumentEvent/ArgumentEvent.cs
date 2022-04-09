using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace CorePresenter.UltEventExtension.ArgumentEvent
{
    public class ArgumentEvent<T> : MonoBehaviour
    {
        public UltEvent<T> OnEvent;
        
        [Button]
        public void Invoke(T value) => OnEvent.Invoke(value);
    }
}