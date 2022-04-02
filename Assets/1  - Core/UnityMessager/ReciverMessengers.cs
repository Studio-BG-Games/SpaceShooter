using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Sharp.UnityMessager
{
    [AddComponentMenu("Event Bus/Reciver messengers")]
    public class ReciverMessengers : BaseResiverMessager
    {
        [PropertyOrder(0)]
        public Event[] TargetEvent;

        protected override bool IsTargetEvent(Event e) => TargetEvent.Contains(e);
    }
}