using System.Collections.Generic;
using UltEvents;
using UnityEngine;

namespace ModelCore
{
    public class SearchLabels : MonoBehaviour
    {
        public List<Object> IdToSearch;

        public UltEvent<Entity> Finded;
        public UltEvent<Collider> FindedCol;

        public bool TryFind(GameObject obj)
        {
            if (obj.TryGetComponent<EntityRef>(out var e))
            {
                foreach (var o in IdToSearch)
                {
                    if (e.Component.Label.IsAlias(o))
                    {
                        Finded.Invoke(e.Component);
                        return true;
                    }
                }
            }

            return false;
        }

        public void TryFind(Collider collider)
        {
            if(TryFind(collider.gameObject)) FindedCol.Invoke(collider);
        }
    }
}