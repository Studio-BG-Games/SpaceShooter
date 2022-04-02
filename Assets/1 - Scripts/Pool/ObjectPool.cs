using System.Collections.Generic;
using DIContainer;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Event = Sharp.UnityMessager.Event;

namespace DefaultNamespace
{
    public class ObjectPool : MonoBehaviour
    {
        [UnityEngine.Min(5)] public int BaseCount = 20;
        
        private Dictionary<GameObject, Pool> _pools = new Dictionary<GameObject, Pool>();
        
        public GameObject Get(GameObject id)
        {
            if (_pools.TryGetValue(id, out var r)) return r.Get();
            else _pools.Add(id, new Pool(id, BaseCount, transform));

            return Get(id);
        }

        public class Pool
        {
            private List<GameObject> _free;
            private List<GameObject> _bysu=new List<GameObject>();
            private Transform _parent;

            public Pool(GameObject instance, int countBase, Transform parrent)
            {
                _parent = parrent;
                _free = new List<GameObject>();
                var injectPrefab = DiBox.MainBox.CreatePrefab(instance);
                injectPrefab.SetActive(false);
                injectPrefab.AddComponent<BackToPoll>();
                for (int i = 0; i < countBase; i++)
                {
                    var prefavToAdd = Instantiate(injectPrefab, parrent);
                    prefavToAdd.GetComponent<BackToPoll>().Returned+=()=>Back(prefavToAdd);
                    _free.Add(prefavToAdd);
                }
                Destroy(injectPrefab);
            }

            private void Back(GameObject o)
            {
                _bysu.Remove(o);
                _free.Add(o);
            }

            public GameObject Get()
            {
                if (_free.Count > 0)
                {
                    var toReturn = _free[0];
                    if (toReturn == null)
                    {
                        _free.Remove(toReturn);
                        return Get();
                    }
                    _free.Remove(toReturn);
                    _bysu.Add(toReturn);
                    toReturn.SetActive(true);
                    return toReturn;
                }
                else
                {
                    IncresePool(10);
                    return Get();
                }
            }

            private void IncresePool(int i)
            {
                var template = _bysu[0];
                for (int j = 0; j < i; j++)
                {
                    var newO = Instantiate(template, _parent);
                    newO.SetActive(false);
                    newO.GetComponent<BackToPoll>().Returned+=()=>Back(newO);
                    _free.Add(newO);
                }
            }
        }
    }
}