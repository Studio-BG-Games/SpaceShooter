﻿using Jint;
 using Js.Interfaces;
 using UnityEngine;

namespace Js.TypeLoaders
{
    public class MonoTypeRegisterComplex : AbsTypeRegister
    {
        [SerializeField] private AbsTypeRegister[] _registers;
        
        public override void Register(Engine engine)
        {
            foreach (var absTypeRegister in _registers) absTypeRegister.Register(engine);
        }
    }
}