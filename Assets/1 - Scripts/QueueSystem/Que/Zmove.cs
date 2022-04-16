using Dreamteck.Forever;
using ModelCore;
using Services;
using Sirenix.OdinInspector;
using UnityEngine;

namespace QueueSystem
{
    public class Zmove : BaseQue
    {
        public bool IsInvertDiraction;
        public float SpeedInNormal;
        public EntityRef ShipData;
        public Runner Run;

        private ZMover _mover;

        public override void OnInit(GameObject parent)
        {
            _mover = null;
            _mover = ShipData.Component.Select<ZMover>();
            if(!_mover) Debug.LogWarning("Zmove не смог найти ZMover", parent);
        }

        public override void OnStart()
        {
            if (!_mover) return;
            Run.followSpeed = _mover.SpeedByDiraction * SpeedInNormal * (IsInvertDiraction ? 1 : -1);
        }
    }
}