using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Manages the locks for a target box displayed on the HUD.
    /// </summary>
    public class HUDTargetBox_LocksController : MonoBehaviour
    {

        [SerializeField]
        protected UVCText numLocksText;
        protected int numLocks;

        [SerializeField]
        protected List<HUDTargetBox_LockController> lockBoxes = new List<HUDTargetBox_LockController>();

        [SerializeField]
        protected float animationTime = 0.5f;

        protected int lastUsedIndex = -1;

        protected Coroutine resetCoroutine;


        /// <summary>
        /// Add a lock to the target box.
        /// </summary>
        /// <param name="targetLocker">The target locker that is locked onto the target.</param>
        public virtual void AddLock(TargetLocker targetLocker)
        {

            lastUsedIndex += 1;

            if (lastUsedIndex < lockBoxes.Count)
            {
                UpdateLockBox(targetLocker, lockBoxes[lastUsedIndex]);
            }
            else
            {
                return;
            }
        }

        protected virtual void UpdateLockBox(TargetLocker targetLocker, HUDTargetBox_LockController lockBox)
        {
            lockBox.gameObject.SetActive(true);

            // Update the lock state
            switch (targetLocker.LockState)
            {
                case LockState.NoLock:

                    lockBox.Deactivate();

                    break;

                case LockState.Locking:

                    lockBox.Activate();

                    for (int i = 0; i < lockBox.lockBoxAnimations.Count; ++i)
                    {
                        lockBox.lockBoxAnimations[i].rectTransform.offsetMin = new Vector2(-lockBox.lockBoxAnimations[i].lockingMargin, -lockBox.lockBoxAnimations[i].lockingMargin);
                        lockBox.lockBoxAnimations[i].rectTransform.offsetMax = new Vector2(lockBox.lockBoxAnimations[i].lockingMargin, lockBox.lockBoxAnimations[i].lockingMargin);
                    }

                    break;

                case LockState.Locked:

                    lockBox.Activate();
                    float amount = Mathf.Clamp((Time.time - targetLocker.LockStateChangeTime) / animationTime, 0, 1);

                    for (int i = 0; i < lockBox.lockBoxAnimations.Count; ++i)
                    {
                        float offset = lockBox.lockBoxAnimations[i].lockingMargin - amount * (lockBox.lockBoxAnimations[i].lockingMargin - lockBox.lockBoxAnimations[i].lockedMargin);
                        lockBox.lockBoxAnimations[i].rectTransform.offsetMin = new Vector2(-offset, -offset);
                        lockBox.lockBoxAnimations[i].rectTransform.offsetMax = new Vector2(offset, offset);
                    }

                    numLocks += 1;

                    break;
            }

            numLocksText.text = numLocks.ToString();
        }

        protected virtual void OnEnable()
        {
            resetCoroutine = StartCoroutine(ResetLockBoxesCoroutine());
        }

        protected virtual void OnDisable()
        {
            StopCoroutine(resetCoroutine);
        }

        // Coroutine for resetting the lead target boxes at the end of the frame
        protected virtual IEnumerator ResetLockBoxesCoroutine()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                for (int i = 0; i < lockBoxes.Count; ++i)
                {
                    lockBoxes[i].Deactivate();
                }
                lastUsedIndex = -1;
                numLocks = 0;
                numLocksText.text = numLocks.ToString();
            }
        }
    }
}