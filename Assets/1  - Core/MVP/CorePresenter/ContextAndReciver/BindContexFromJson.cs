using DIContainer;
using ModelCore;
using UnityEngine;

namespace CorePresenter.ContextAndReciver
{
    public class BindContexFromJson : FactoryDI
    {
        public TextAsset Assets;
        
        public override void Create(DiBox container)
        {
            if(container.HasSingle<Context>()) return; 
            Debug.Log(Assets.text);
            Context.CreateContext("Context Json", LoaderRoot.ModelFromJson(Assets.text, false));
            Context.Instance.GameModel.Init();
            container.RegisterSingle(Context.Instance);
        }

        public override void DestroyDi(DiBox container)
        {
        }
    }
}