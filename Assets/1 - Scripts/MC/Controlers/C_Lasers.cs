using System;
using System.Collections;
using System.Collections.Generic;
using Infrastructure;
using ModelCore;
using Services;
using UnityEngine;

namespace MC.Controlers
{
    public class C_Lasers : MonoBehaviour
    {
        public List<WeaponLaser> Lasers;
        public Damager Damager;
        [Min(0.05f)]public float _delayBetwenAttack;

        private DamageInfo _info;
        private Coroutine _pause;

        private void Start()
        {
            
        }

        public bool _hasBeAttask;
        private void Update()
        {
            if (_hasBeAttask) _hasBeAttask = false;
            else Lasers.ForEach(x=>x.Zero());
        }

        public void TryAttack()
        {
            if (!(gameObject.activeSelf && enabled)) return;
            Lasers.ForEach(x =>
            {
                var collider = x.TryFire();
                _hasBeAttask = true;
                if (_pause == null && collider)
                {
                    Debug.Log("Make Damage + "+collider.name, Damager.DamageInfoRef.Component);
                    Damager.Change(collider);
                }
            });
            if(_pause==null) Pause();
        }
        
        private void Pause()
        {
            if(_pause!=null) CorutineGame.Instance.StopCoroutine(_pause);
            _pause = CorutineGame.Instance.StartCoroutine(PauseCor(_delayBetwenAttack));
        }

        private IEnumerator PauseCor(float delay)
        {
            yield return new WaitForSeconds(delay);
            _pause = null;
        }
    }
}