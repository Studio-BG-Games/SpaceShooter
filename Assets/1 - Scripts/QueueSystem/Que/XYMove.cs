using Dreamteck.Forever;
using ModelCore;
using Services;
using Sirenix.OdinInspector;
using UnityEngine;

namespace QueueSystem
{
    public class XYMove : BaseQue
    {
        public float XDir;
        public float YDir;
        
        public EntityRef ShipData;
        public Runner Run;

        private XYMover _mover;

        public override void OnInit(GameObject parent)
        {
            _mover = null;
            _mover = ShipData.Component.Select<XYMover>();
            if(!_mover) Debug.LogWarning("XYMove не смог найти XYMover", parent);
        }

        protected override void Update(float deltaTime)
        {
            _mover?.Move(Run, new Vector2(XDir, YDir));
        }
    }
}