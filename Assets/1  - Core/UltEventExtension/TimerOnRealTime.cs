using System.Collections;
using UltEvents;
using UnityEngine;

namespace DefaultNamespace
{
    public class TimerOnRealTime : MonoBehaviour
    {
        public float Delay;
        public UltEvent Action;

        public void Invoke()
        {
            if (Delay <= 0) Action?.Invoke();
            else StartCoroutine(Timer());
        }

        private IEnumerator Timer()
        {
            yield return new WaitForSecondsRealtime(Delay);
            Action.Invoke();
        }
    }
}