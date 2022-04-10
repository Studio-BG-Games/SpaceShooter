using System.Collections.Generic;
using UltEvents;
using UnityEngine;

namespace ModelCore
{
    public class SearchLabels : MonoBehaviour
    {
        public List<Object> IdToSearch;

        public UltEvent<Entity> Finded; 
        
        public void TryFind(GameObject obj)
        {
            if (obj.TryGetComponent<EntityRef>(out var e))
            {
                foreach (var o in IdToSearch)
                {
                    Debug.Log(e);
                    Debug.Log(e.Component);
                    Debug.Log(e.Component.Label);
                    if (e.Component.Label.IsAlias(o))
                    {
                        Finded.Invoke(e.Component);
                        break;
                    }
                }
            }
        }

        public void TryFind(Collider collider) => TryFind(collider.gameObject);
    }
}