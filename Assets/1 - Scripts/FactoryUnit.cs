using DefaultNamespace;
using DIContainer;
using Dreamteck.Forever;
using UnityEngine;

namespace Services
{
    public class FactoryUnit : MonoBehaviour
    {
        [DI] private ObjectPool _objectPool;
        
        public Unit CreateBullet(ZMover moverStarter, BulletMark bulletPrefab, Vector3 position)
        {
            //var bullet = _objectPool.Get(bulletPrefab.gameObject).GetComponent<BulletMark>().Unit;
            var bullet = DiBox.MainBox.CreatePrefab(bulletPrefab).Unit;
            var bulletMover = bullet.Select<ZMover>();
            if (bulletMover != null)
            {
                //bulletMover.Speed += moverStarter.Speed;
                var ruuner = moverStarter.Runner;
                bullet.transform.position = position;
                if(LevelGenerator.instance.ready)
                    ruuner.StartFollow();
                bulletMover.IsPositive = moverStarter.IsPositive;
                bulletMover.Runner.motion.offset = moverStarter.Runner.motion.offset;
            }

            return bullet;
        }
    }
}