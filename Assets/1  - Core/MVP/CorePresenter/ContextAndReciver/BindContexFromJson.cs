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
            var contextModel = LoaderRoot.ModelFromJson(Assets.text, false);
            if (contextModel == null)
            {
                Debug.LogError("Не удалось создать контекст", this);
                return;
            }
            Context.CreateContext("Context Json", contextModel);
            Context.Instance.GameModel.Init();
            container.RegisterSingle(Context.Instance);
        }

        public override void DestroyDi(DiBox container)
        {
        }
    }
}