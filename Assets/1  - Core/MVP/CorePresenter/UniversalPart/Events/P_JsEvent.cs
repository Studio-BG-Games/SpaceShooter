using ModelCore;
using ModelCore.Universal;
using UltEvents;
using UnityEngine;
using UnityEngine.UI;

namespace CorePresenter.UniversalPart
{
    [AddComponentMenu(RootPresenter.PathToPresenters+"P_JsEvent")]
    public class P_JsEvent : Presenter
    {
        public JsEvent Event;

        public UltEvent Evented;
        
        public override void Init(RootModel rootModel)
        {
            if(Event!=null) Event.Event -= Handler;
            Event = GetModel<JsEvent>(rootModel, x => x.Alias == PathToModel);
            if(Event!=null) Event.Event += Handler;
        }

        public void Trig() => Event?.Trig();

        private void Handler() => Evented.Invoke();
    }
}