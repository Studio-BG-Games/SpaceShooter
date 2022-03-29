using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Manage a set of waves.
    /// </summary>
    public class WavesController : MonoBehaviour
    {
        [Header("General")]

        [SerializeField]
        protected List<WaveController> waveControllers = new List<WaveController>();
        public List<WaveController> WaveControllers { get { return waveControllers; } }

        [SerializeField]
        protected bool loopWaves = false;

        protected int lastSpawnedWaveIndex = -1;
        public int LastSpawnedWaveIndex
        {
            get { return lastSpawnedWaveIndex; }
        }

        protected bool wavesDestroyed = false;

        [Header("Events")]

        public UnityEvent onWavesDestroyed;


        protected virtual void Awake()
        {
            foreach (WaveController waveController in waveControllers)
            {
                waveController.onWaveDestroyed.AddListener(OnWaveDestroyed);
            }
        }

        /// <summary>
        /// Spawn a wave at a specific index in the list.
        /// </summary>
        /// <param name="index">The wave index to spawn.</param>
        public virtual void SpawnWave(int index)
        {

            if (index < 0 || index >= waveControllers.Count) return;

            waveControllers[index].Spawn();
            lastSpawnedWaveIndex = index;
        }

        /// <summary>
        /// Spawn a random wave in the list.
        /// </summary>
        public virtual void SpawnRandomWave()
        {
            SpawnWave(Random.Range(0, waveControllers.Count));
        }

        /// <summary>
        /// Spawn the next wave in the list.
        /// </summary>
        public virtual void SpawnNextWave()
        {
            // Iterate
            int nextWaveSpawnIndex = lastSpawnedWaveIndex + 1;
            if (nextWaveSpawnIndex >= waveControllers.Count)
            {
                if (loopWaves)
                {
                    ResetWaves();
                    nextWaveSpawnIndex = 0;
                }
                else
                {
                    return;
                }
            }

            SpawnWave(nextWaveSpawnIndex);

        }

        public virtual void ResetWaves()
        {
            // Make sure all the wave controllers are reset
            foreach (WaveController waveController in waveControllers)
            {
                waveController.ResetWave();
            }

            // Reset destroyed flag
            wavesDestroyed = false;
        }

        protected virtual void OnWaveDestroyed()
        {
            // Check if all the waves have been destroyed
            if (!wavesDestroyed)
            {
                wavesDestroyed = true;
                for (int i = 0; i < waveControllers.Count; ++i)
                {
                    if (!waveControllers[i].Destroyed)
                    {
                        wavesDestroyed = false;
                    }
                }

                if (wavesDestroyed)
                {
                    onWavesDestroyed.Invoke();
                }
            }
        }
    }
}

