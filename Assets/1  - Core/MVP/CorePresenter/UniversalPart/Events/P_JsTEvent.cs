using ModelCore;
using ModelCore.Universal;
using UltEvents;

namespace CorePresenter.UniversalPart
{
    public abstract class P_JsTEvent<T, M> : Presenter where T : JsEventT1<M>
    {
        public T Event;
        public UltEvent<M> Evented;
        
        public override void Init(RootModel rootModel)
        {
            if(Event!=null) Event.Event -= Handler;
            Event = GetModel<T>(rootModel, x => x.Alias == PathToModel);
            if(Event!=null) Event.Event += Handler;
        }

        private void Handler(M obj) => Evented.Invoke(obj);
    }
}