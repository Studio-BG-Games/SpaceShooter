using System;
using DIContainer;
using Services.Input;
using TMPro;
using UnityEngine;

namespace Services.Bind
{
    public class FactoryInput : FactoryDI
    {
        private PcInput _pc;

        public override void Create(DiBox container)
        {
            _pc = new PcInput();
            container.RegisterSingle<IInput>(_pc);
        }

        private void Update()
        {
            _pc.Update();
        }

        public override void DestroyDi(DiBox container)
        {
            container.RemoveSingel<IInput>();
        }
    }
}