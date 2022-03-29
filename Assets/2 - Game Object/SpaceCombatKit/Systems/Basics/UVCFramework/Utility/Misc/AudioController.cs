using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Control the volume and pitch of an audio source within specified limits.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioController : MonoBehaviour
    {
        
        [Header("Volume")]

        [SerializeField]
        protected float minVolume = 0;

        [SerializeField]
        protected float maxVolume = 1;

        [Header("Pitch")]

        [SerializeField]
        protected float minPitch = 0;

        [SerializeField]
        protected float maxPitch = 1;

        protected AudioSource audioSource;



        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = 0;
        }

        /// <summary>
        /// Set the volume amount from the min to max limits.
        /// </summary>
        /// <param name="volumeAmount">The volume amount.</param>
        public virtual void SetVolumeAmount(float volumeAmount)
        {
            audioSource.volume = minVolume + (volumeAmount * (maxVolume - minVolume));
        }

        /// <summary>
        /// Set the pitch amount from min to max limits.
        /// </summary>
        /// <param name="pitchAmount">The pitch amount.</param>
        public virtual void SetPitchAmount(float pitchAmount)
        {
            audioSource.pitch = minPitch + (pitchAmount * (maxPitch - minPitch));
        }
    }
}