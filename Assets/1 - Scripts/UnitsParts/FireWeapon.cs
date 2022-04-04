using System.Collections;
using DIContainer;
using Sirenix.Utilities;
using UltEvents;
using UnityEngine;

namespace Services
{
    public class FireWeapon : PartUnit
    {
        [DI] private FactoryUnit _factoryUnit;
        
        public ZMover zmover;
        public BulletMark Bullet;
        [Min(0.05f)][SerializeField] private float _fireRate;
        [SerializeField] private Transform _spawnPoint;
        public UltEvent Fired;
        public Collider[] ColliderForIgnore;

        private Coroutine _pause;
        public void TryFire()
        {
            if (_pause == null)
            {
                var r = _factoryUnit.CreateBullet(zmover, Bullet, _spawnPoint.position);
                r.GetComponentsInChildren<HitCast>().ForEach(x => x.AddIgnore(ColliderForIgnore));
                Fired.Invoke();
                Pause();
            }
        }

        private void Pause()
        {
            if(_pause!=null) StopCoroutine(_pause);
            _pause = StartCoroutine(PauseCor(_fireRate));
        }

        private IEnumerator PauseCor(float delay)
        {
            yield return new WaitForSeconds(delay);
            _pause = null;
        }
    }
}