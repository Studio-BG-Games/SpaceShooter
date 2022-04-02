using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dreamteck
{
    public class LazySingleton<T> : Singleton<T> where T : Component
    {
        public new static T instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }
    }
}