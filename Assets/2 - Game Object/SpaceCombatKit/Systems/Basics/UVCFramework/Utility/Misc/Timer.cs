using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Unity event for running functions related to a timer.
    /// </summary>
    [System.Serializable]
    public class TimerEventHandler : UnityEvent { }

    [System.Serializable]
    public class OnTimerDisplayUpdatedEventHandler : UnityEvent<string> { }

    /// <summary>
    /// Timer component for running events when a timer is started and when it is finished.
    /// </summary>
    public class Timer : MonoBehaviour
    {
        [Header("General Settings")]

        [SerializeField]
        protected bool startTimerOnEnable = false;

        [SerializeField]
        protected float duration = 5;

        protected float nextDuration = 5;

        protected float startTime;

        protected bool running = false;

        [SerializeField]
        protected float delay = 0f;
        protected float nextDelay;
        protected bool delayRunning = false;
        protected float delayStartTime;

        [Header("Randomization")]

        [SerializeField]
        protected bool randomizedDuration = false;

        [SerializeField]
        protected float minRandomDuration = 1;

        [SerializeField]
        protected float maxRandomDuration = 5;

        [Header("Events")]

        public TimerEventHandler onTimerFinished;

        public TimerEventHandler onTimerStarted;

        public OnTimerDisplayUpdatedEventHandler onTimerDisplayUpdated;



        protected virtual void OnEnable()
        {
            if (startTimerOnEnable)
            {
                StartTimer();
            }
        }

        /// <summary>
        /// Start the timer
        /// </summary>
        public virtual void StartTimer()
        {
            nextDuration = randomizedDuration ? Random.Range(minRandomDuration, maxRandomDuration) : duration;
            StartTimer(nextDuration);
        }

        public virtual void StartTimerDelayed(float delay)
        {
            nextDelay = delay;
            delayRunning = true;
            delayStartTime = Time.time;
        }

        /// <summary>
        /// Start the timer with a specific duration.
        /// </summary>
        /// <param name="newDuration">The new timer duration.</param>
        public virtual void StartTimer(float newDuration)
        {
            nextDuration = newDuration;
            startTime = Time.time;
            running = true;
            onTimerStarted.Invoke();
        }

        protected void DisplayUpdate()
        {
            float timeSinceStart = Time.time - startTime;
            int minutes = (int)((nextDuration - timeSinceStart) / 60);
            int seconds = (int)((nextDuration - timeSinceStart) - (minutes * 60));

            string minutesString = minutes < 10 ? "0" + minutes.ToString() : minutes.ToString();
            string secondsString = seconds < 10 ? "0" + seconds.ToString() : seconds.ToString();

            onTimerDisplayUpdated.Invoke(minutesString + ":" + secondsString);
        }

        // Called every frame
        protected virtual void Update()
        {

            if (delayRunning)
            {
                if (Time.time - delayStartTime >= nextDelay)
                {
                    delayRunning = false;
                    StartTimer();
                }
            }

            // If it's running, check if the timer has finished.
            if (running)
            {
                if (Time.time - startTime >= nextDuration)
                {
                    running = false;
                    onTimerFinished.Invoke();
                }
                else
                {
                    DisplayUpdate();
                }
            }
        }
    }
}

