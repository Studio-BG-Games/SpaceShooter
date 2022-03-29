﻿using System;
using System.Collections.Generic;
using Jint;
 using Js.Interfaces;
 using UnityEngine;
using Object = System.Object;

namespace Js.TypeLoaders
{
    public class MonoTypeRegisterDictionary : AbsTypeRegister
    {
        [SerializeField] private List<DictT> _elemtns = new List<DictT>();
        
        public override void Register(Engine engine) => _elemtns.ForEach(x=> engine.SetValue(x.Name, (object)x.Component));

        [System.Serializable]
        public class DictT
        {
            public Component Component;
            public string Name;
        }

        private void OnValidate()
        {
            HashSet<string> names = new HashSet<string>();
            for (var i = 0; i < _elemtns.Count; i++)
            {
                if(!names.Add(_elemtns[i].Name))
                   throw new Exception($"Double name in {i} - {_elemtns[i].Name}");
            }
        }
    }
}