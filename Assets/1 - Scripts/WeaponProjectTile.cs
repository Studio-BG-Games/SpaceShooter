using System;
using System.Collections;
using DIContainer;
using Dreamteck.Forever;
using Dreamteck.Splines;
using MC.Controlers;
using ModelCore;
using Sirenix.Utilities;
using UltEvents;
using UnityEngine;

namespace Services
{
    public class WeaponProjectTile : MonoBehaviour
    {
        [Min(0.05f)][SerializeField] private float _fireRate;
        [SerializeField] private Transform _spawnPoint;
        
        public UltEvent Fired;
        
        private Collider[] ColliderForIgnore;
        private Coroutine _pause;

        public void Init(Collider[] coliderInfoner) => ColliderForIgnore = coliderInfoner;
        
        public void TryFire(Entity ship, Entity dataBullet, C_ProjectTile prefab)
        {
            if (_pause == null)
            {
                var r = Instantiate(prefab, _spawnPoint.position, _spawnPoint.rotation);
                r.HitCast.AddIgnore(ColliderForIgnore);
                SetOffsetProjectTile(r);
                r.Init(ship, dataBullet);
                Fired.Invoke();
                Pause();
            }
        }

        private void SetOffsetProjectTile(C_ProjectTile r)
        {
            // Раннер ввсегда начинает с нулевым оффестом. Здесь мы вычисляем нужный оффсет для ранера. Сначала 
            // Вычисляем направление от точки с нулевым оффестом к месту стрельбы
            // Мы получаем оффест в глобальных кординатах, затем через  InverseTransformVector делаем оффет локальный и уже его устанавливаем в оффест раннера
            SplineSample sample = new SplineSample();
            LevelGenerator.instance.Project(transform.position, sample);
            var globalOffset = _spawnPoint.position - sample.position;
            var localOffset = r.transform.InverseTransformVector(globalOffset);
            r.Runner.motion.offset = localOffset;
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