using Sirenix.OdinInspector;
using UnityEngine;

namespace ModelCore
{
    public class SavedComponent<T> : MonoBehaviour where T : ComponentBase
    {
        [HideLabel][SerializeField] private T _value;
        public T Component => _value;
    }
}