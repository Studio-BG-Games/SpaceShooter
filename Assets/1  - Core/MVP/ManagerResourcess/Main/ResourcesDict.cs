using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ManagerResourcess
{
    public abstract class ResourcesDict<T> : Resources<T>
    {
        [SerializeField][ShowInInspector]
        protected Dictionary<string, T> Resources;

        public override T Get(string id)
        {
            Resources.TryGetValue(id, out var r);
            return r;
        }

        public override T[] GetAll()
        {
            T[] result = new T[Resources.Count];
            int counter = 0;
            foreach (var keyValuePair in Resources)
            {
                result[counter] = keyValuePair.Value;
                counter++;
            }

            return result;
        }


        [Button]
        private void FastCreate(T value)
        {
            var valuasAsObject = value as UnityEngine.Object;
            if (valuasAsObject)
            {
                Resources.Add(valuasAsObject.name, value);                
            }
            else
            {
                Resources.Add(Guid.NewGuid().ToString(), value);    
            }
        }

        [Button]
        private void FastCreate(T[] values) => values.ForEach(x=>FastCreate(x));
    }
}