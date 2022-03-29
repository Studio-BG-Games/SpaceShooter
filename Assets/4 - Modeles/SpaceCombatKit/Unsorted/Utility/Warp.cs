using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.Effects;

namespace VSX.UniversalVehicleCombat
{
    public class Warp : MonoBehaviour
    {

        [SerializeField]
        protected Vehicle vehicle;

        [Header("Warp Parameters")]

        [SerializeField]
        protected float warpDelay = 0;

        [SerializeField]
        protected float warpTime = 0.5f;

        [SerializeField]
        public AnimationCurve warpPositionCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        protected Vector3 startPosition = new Vector3(0, 0, 20000);

        [SerializeField]
        protected Vector3 endPosition = Vector3.zero;

        [Header("Audio")]

        [SerializeField]
        protected AudioSource warpAudio;
        protected bool audioPlayed = false;

        [Range(0, 1)]
        [SerializeField]
        protected float audioPosition = 0;

        [Header("Rumble")]

        [SerializeField]
        protected bool addRumble;

        [Range(0, 1)]
        [SerializeField]
        protected float rumblePosition = 1;

        [Range(0, 1)]
        [SerializeField]
        protected float rumbleLevel;

        [SerializeField]
        protected float rumbleDuration = 0.5f;

        [SerializeField]
        protected AnimationCurve rumbleCurve = AnimationCurve.Linear(0, 1, 1, 0);

        protected bool rumbleAdded = false;

        protected bool warping = false;

        protected float warpStartTime;


        /// <summary>
        /// Start warping in.
        /// </summary>
        public void StartWarp()
        {
            vehicle.CachedGameObject.SetActive(true);
            warping = true;
            audioPlayed = false;
            warpStartTime = Time.time;
            rumbleAdded = false;
            vehicle.transform.LookAt(endPosition, Vector3.up);
        }

        
        void Update()
        {
        
            if (warping)
            {
                // Wait for the delay
                if (Time.time - warpStartTime > warpDelay)
                {
                    // Calculate the amount of warp (0-1)
                    float warpAmount = (Time.time - warpDelay - warpStartTime) / warpTime;

                    // Feed that amount into the position curve
                    float warpCurveValue = warpPositionCurve.Evaluate(warpAmount);

                    // Update the vehicle position
                    vehicle.transform.position = (1 - warpCurveValue) * startPosition + warpCurveValue * endPosition;

                    // Play audio
                    if (warpAudio != null && !audioPlayed && warpAmount > audioPosition)
                    {
                        warpAudio.Play();
                        audioPlayed = true;
                    }

                    // Add rumble
                    if (addRumble && !rumbleAdded && warpAmount > rumblePosition)
                    {
                        if (RumbleManager.Instance != null)
                        {
                            RumbleManager.Instance.AddRumble(false, endPosition, rumbleLevel, rumbleDuration, rumbleCurve);
                            rumbleAdded = true;
                        }
                    }

                    if (warpAmount > 1)
                    {
                        vehicle.transform.position = endPosition;

                        // Play audio if not already
                        if (warpAudio != null && !audioPlayed) warpAudio.Play();

                        // Add rumble if not already
                        if (RumbleManager.Instance != null && !rumbleAdded)
                        {
                            RumbleManager.Instance.AddRumble(false, endPosition, rumbleLevel, rumbleDuration, rumbleCurve);
                        }

                        warping = false;
                    }
                }
            }
        }
    }
}
