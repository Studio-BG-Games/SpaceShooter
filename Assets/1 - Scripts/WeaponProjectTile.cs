using System;
using System.Collections;
using DefaultNamespace;
using DIContainer;
using Dreamteck.Forever;
using Dreamteck.Splines;
using Infrastructure;
using MC.Controlers;
using ModelCore;
using Sirenix.Utilities;
using UltEvents;
using UnityEngine;

namespace Services
{
    public class WeaponProjectTile : MonoBehaviour
    {
        public Runner Runner;
        [Min(0.05f)][SerializeField] private float _fireRate;
        [SerializeField] private Transform _spawnPoint;
        
        public UltEvent Fired;
        
        private Collider[] ColliderForIgnore;
        private Coroutine _pause;
        [SerializeField] private bool _canAttack = true;

        public void Init(Collider[] coliderInfoner) => ColliderForIgnore = coliderInfoner;

        private void OnEnable()
        {
            _canAttack = true;
        }

        private void OnDisable()
        {
            _canAttack = false;
        }

        public void TryFire(Entity ship, Entity dataBullet, C_ProjectTile prefab)
        {
            if (_pause == null && (enabled && gameObject.activeSelf) && _canAttack)
            {
                var r = Instantiate(prefab, _spawnPoint.position, _spawnPoint.rotation);
                r.HitCast.AddIgnore(ColliderForIgnore);
                GlobalHelp.SetOffsetProjectTile(r.Runner, _spawnPoint.position);
                r.Init(ship, dataBullet);
                Fired.Invoke();
                Pause();
            }
        }

        private void Pause()
        {
            if(_pause!=null) CorutineGame.Instance.StopCoroutine(_pause);
            _pause = CorutineGame.Instance.StartCoroutine(PauseCor(_fireRate));
        }

        private IEnumerator PauseCor(float delay)
        {
            yield return new WaitForSeconds(delay);
            _pause = null;
        }

        private void OnValidate()
        {
            if(_spawnPoint) _spawnPoint.eulerAngles = Vector3.zero;
        }
    }
}