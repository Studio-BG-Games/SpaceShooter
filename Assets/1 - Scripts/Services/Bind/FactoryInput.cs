using System;
using DIContainer;
using Services.Input;
using TMPro;
using UnityEngine;

namespace Services.Bind
{
    public class FactoryInput : FactoryDI
    {
        public PcInput Pc;
        public MobileInput Mobile;
        
        private CompositeInput _composite;

        public override void Create(DiBox container)
        {
            _composite = new CompositeInput(new IInput[] {Pc, Mobile});
            container.RegisterSingle<IInput>(_composite);
        }

        public override void DestroyDi(DiBox container)
        {
            _composite.Dispose();
            container.RemoveSingel<IInput>();
        }
    }
}