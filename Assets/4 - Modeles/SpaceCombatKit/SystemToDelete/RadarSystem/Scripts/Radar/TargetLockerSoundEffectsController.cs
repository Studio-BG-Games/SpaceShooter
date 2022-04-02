using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Implements sound effects for a target locker.
    /// </summary>
    public class TargetLockerSoundEffectsController : MonoBehaviour
    {
        [SerializeField]
        protected AudioSource lockingAudioSource;

        [SerializeField]
        protected AudioSource lockedAudioSource;

        /// <summary>
        /// Called when the lock state of the associated target locker changes.
        /// </summary>
        /// <param name="newLockState">The new lock state.</param>
        public virtual void OnLockStateChanged(LockState newLockState)
        {
            switch (newLockState)
            {
                case LockState.NoLock:
                    lockingAudioSource.Stop();
                    break;
                case LockState.Locking:
                    lockingAudioSource.Play();
                    break;
                case LockState.Locked:
                    lockingAudioSource.Stop();
                    lockedAudioSource.Play();
                    break;
            }
        }
    }
}
