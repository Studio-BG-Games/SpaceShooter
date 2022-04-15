using System;
using Dreamteck.Forever;
using ModelCore;
using Services;
using UnityEngine;

namespace MC.Controlers
{
    public class C_ProjectTile : MonoBehaviour
    {
        public DamageInfoRef DamageInfoRef;
        public Runner Runner;
        private ZMover _moverShip;
        private ZMover _moverBullet;

        [SerializeField] private HitCast _hitCast;
        public HitCast HitCast => _hitCast;

        public void Init(Entity ship, Entity infoBullet)
        {
            DamageInfoRef.Init(infoBullet.Select<DamageInfo>());
            _moverShip = ship.Select<ZMover>(true);
            _moverBullet = infoBullet.Select<ZMover>(true);
            
            DamageInfoRef.Init(infoBullet.Select<DamageInfo>(true));

            Runner.motion.rotationOffset = _moverShip.IsPositive ? new Vector3(0, 0, 0) : new Vector3(180, 0, 0); 
            
            SetSpeedBullet();
            _moverBullet.Changed += SetSpeedBullet;
            _moverShip.Changed += SetSpeedBullet;

        }

        private void SetSpeedBullet()
        {
            Runner.followSpeed = (_moverBullet.AbsolutlySpeed + _moverShip.AbsolutlySpeed) * (_moverShip.IsPositive ? 1 : -1);
        }

        private void OnDestroy()
        {
            _moverBullet.Changed -= SetSpeedBullet;
            _moverShip.Changed -= SetSpeedBullet;
        }
    }
}