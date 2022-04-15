using UltEvents;
using UnityEngine;

namespace QueueSystem
{
    public class LifeEventQue : BaseQue
    {
        public UltEvent InitEvent;
        public UltEvent StartEvent;
        public UltEvent FinishEvent;
        public UltEvent<float> UpdateEvent;

        public override void OnInit(GameObject parent) => InitEvent.Invoke();

        public override void OnStart() => StartEvent.Invoke();

        public override void OnFinish() => FinishEvent.Invoke();

        protected override void Update(float deltaTime) => UpdateEvent.Invoke(deltaTime);
    }
}