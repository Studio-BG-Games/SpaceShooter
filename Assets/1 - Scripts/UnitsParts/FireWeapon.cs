using System.Collections;
using DIContainer;
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

        private Coroutine _pause;
        public void TryFire()
        {
            if (_pause == null)
            {
                _factoryUnit.CreateBullet(zmover, Bullet, _spawnPoint.position); 
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