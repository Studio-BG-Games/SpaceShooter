using Sirenix.OdinInspector;
using UnityEngine;

namespace Sharp.UnityMessager
{
    [AddComponentMenu("Event Bus/Reciver messenger")]
    public class ReciverMessenger : BaseResiverMessager
    {
        [PropertyOrder(0)]
        public Event TargetEvent;
        
        protected override bool IsTargetEvent(Event e) => TargetEvent == e;
    }
}