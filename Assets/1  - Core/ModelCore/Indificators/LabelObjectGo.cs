using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ModelCore
{
    [DisallowMultipleComponent][RequireComponent(typeof(Entity))]
    public class LabelObjectGo : MonoBehaviour
    {
        [SerializeField] private Object[] _alias;

        public bool IsAlias(Object obj) => _alias.Contains(obj);

        [Button] public void OnValidate() => _alias.Where(x => x != null);
    }
}