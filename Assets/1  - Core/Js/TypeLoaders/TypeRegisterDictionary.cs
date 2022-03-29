﻿using System.Collections.Generic;
using Jint;
 using Js.Interfaces;

 namespace Js.TypeLoaders
{
    public class TypeRegisterDictionary : ITypeRegister
    {
        private Dictionary<string, object> _objects;

        public TypeRegisterDictionary(Dictionary<string, object> elements) => _objects = elements;
        
        public void Register(Engine engine)
        {
            foreach (var keyValuePair in _objects)
            {
                engine.SetValue(keyValuePair.Key, (object) keyValuePair.Value);
            }
        }
    }
}