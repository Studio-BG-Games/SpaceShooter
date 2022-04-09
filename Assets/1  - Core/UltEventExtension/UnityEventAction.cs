using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace CorePresenter.UltEventExtension
{
    [AddComponentMenu("MV*/Event mediator/Unity Action")]
    public class UnityEventAction : MonoBehaviour
    {
        public UnityEvent Event;
        
        [Button]
        public void Invoke() => Event.Invoke();
    }
}