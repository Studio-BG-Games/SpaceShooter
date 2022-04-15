using Dreamteck.Forever;
using ModelCore;
using Services;
using UnityEngine;

namespace QueueSystem
{
    public class ClampOffsetByLevel : BaseQue
    {
        private XYClamp _xyClamp;
        private Runner _runner;
        
        public override void OnInit(GameObject parent)
        {
            _xyClamp = EntityAgregator.Instance.Select(x => x.Has<XYClamp>()).Select<XYClamp>();
            if(_xyClamp==null) Debug.LogError("На уровне нет XYClamp");
            _runner = parent.GetComponent<Runner>();
        }

        protected override void Update(float deltaTime) { if(_runner && _xyClamp) _xyClamp.Clamp(_runner); }
    }
}