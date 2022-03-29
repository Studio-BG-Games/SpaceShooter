using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class MultiExplosionAudioController : MonoBehaviour
    {
        public ParticleSystem m_ParticleSystem;

        public GameObject audioPrefab;


        private void OnEnable()
        {
            StartCoroutine(ExplosionSounds());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator ExplosionSounds()
        {
            while (true)
            {
                Instantiate(audioPrefab, transform.position, transform.rotation);
                yield return new WaitForSeconds(Random.Range(0.1f, 1f));
            }
        }
    }
}

