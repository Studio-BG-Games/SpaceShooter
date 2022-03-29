using UnityEngine;

namespace ManagerResourcess
{
    public abstract class ResourcedDictDefault<T> : ResourcesDict<T>
    {
        [SerializeField] private T _default;

        public override T Get(string id)
        {
            var r = base.Get(id);
            return r == null ? _default : r;
        }
        
        public override T[] GetAll()
        {
            T[] result = new T[Resources.Count+1];
            result[0] = _default;
            int counter = 1;
            foreach (var keyValuePair in Resources)
            {
                result[counter] = keyValuePair.Value;
                counter++;
            }

            return result;
        }
    }
}