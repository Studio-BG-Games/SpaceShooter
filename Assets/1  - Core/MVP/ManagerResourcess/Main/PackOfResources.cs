using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ManagerResourcess
{
    [CreateAssetMenu(order = 50, menuName = "Resources MVVM/1 Main - Pack")]
    public class PackOfResources : ScriptableObject
    {
        [SerializeField] private List<BaseResources> Containers;
        private Dictionary<string, BaseResources> _dictsContainer;

        public BaseResources Get(string aliasContainer) => GetByAlias(aliasContainer);

        [Button][PropertyOrder(-1)]
        public void DeleteDublicate() => Containers = Containers.Distinct().ToList();

        private BaseResources GetByAlias(string aliasContainer)
        {
            if (_dictsContainer == null) _dictsContainer = Containers.ToDictionary(x => x.Alias);
            _dictsContainer.TryGetValue(aliasContainer, out var r);
            return r;
        }
    }
}