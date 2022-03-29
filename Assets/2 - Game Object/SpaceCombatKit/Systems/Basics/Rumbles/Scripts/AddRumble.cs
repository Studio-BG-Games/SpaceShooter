using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.Effects
{
    /// <summary>
    /// This class enables you to create a rumble when a gameobject is enabled, or call the rumble by adding a function from this script to a Unity Event.
    /// </summary>
    public class AddRumble : MonoBehaviour
    {

        [Header("Settings")]

        [Tooltip("Whether to begin the rumble when the gameobject is activated.")]
        [SerializeField]
        protected bool runOnEnable = true;

        [Header("Rumble Parameters")]

        [Tooltip("Whether the rumble is based on distance or global (felt equally regardless of distance).")]
        [SerializeField]
        protected bool distanceBased = true;

        [Tooltip("The delay before the rumble occurs after it is called.")]
        [SerializeField]
        protected float delay = 0;

        [Tooltip("The peak rumble level.")]
        [SerializeField]
        protected float maxLevel = 1;

        [Tooltip("The rumble duration.")]
        [SerializeField]
        protected float duration = 1;

        [Tooltip("How long the rumble takes to go from 0 to maximum.")]
        [SerializeField]
        protected AnimationCurve rumbleCurve = AnimationCurve.Linear(0, 1, 1, 0);

        

        private void OnEnable()
        {
            if (runOnEnable)
            {
                Run();
            }
        }

        /// <summary>
        /// Add this rumble.
        /// </summary>
        public void Run()
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(RunCoroutine(delay));
            }
        }

        /// <summary>
        /// Add this rumble with a specified delay.
        /// </summary>
        /// <param name="delay">The default delay before the rumble.</param>
        public void Run(float delay)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(RunCoroutine(delay));
            }
        }

        // The delayed rumble coroutine.
        protected IEnumerator RunCoroutine(float delay)
        {

            yield return new WaitForSeconds(delay);

            // Add a rumble
            if (RumbleManager.Instance != null)
            {
                RumbleManager.Instance.AddRumble(distanceBased, transform.position, maxLevel, duration, rumbleCurve);
            }
        }
    }
}