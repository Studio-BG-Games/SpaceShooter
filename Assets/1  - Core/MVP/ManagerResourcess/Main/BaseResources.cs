using Sirenix.OdinInspector;
using UnityEngine;

namespace ManagerResourcess
{
    public abstract class BaseResources : SerializedScriptableObject
    {
        [PropertyOrder(-1)]
        [SerializeField][ShowInInspector]
        public string Alias { get; private set; }
    }
    
}