using DIContainer;
using Plugins.GameStateMachines;
using Plugins.GameStateMachines.States;
using UnityEngine;

namespace Services.Bind
{
    public class BindAppSM : FactoryDI
    {
        public static bool IsInit = false;
        
        public override void Create(DiBox container)
        {
            if(IsInit) return;
            var sm = new AppStateMachine();
            container.RegisterSingle(sm);
            sm.Enter<MainMenu>();
            IsInit = true;
        }

        public override void DestroyDi(DiBox container)
        {
            
        }
    }
}