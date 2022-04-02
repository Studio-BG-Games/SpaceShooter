using System.Collections;
using UnityEngine;

namespace StateMchines.Transitions
{
    public class TimerTransition : Transition
    {
        [Min(0)][SerializeField] private float _time;

        private bool _canTransit;
        private Coroutine _timer;

        public override void OnObserve() => _timer = StartCoroutine(Timer(_time));

        public override void OffObserve()
        {
            if (_timer != null)
            {
                
                StopCoroutine(_timer);
                _timer = null;
            }
        }

        private IEnumerator Timer(float time)
        {
            yield return new WaitForSeconds(_time);
            Transit();
        }
    }
}