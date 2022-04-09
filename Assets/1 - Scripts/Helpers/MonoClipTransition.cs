using System;
using Animancer;
using UltEvents;
using UnityEngine;

namespace DefaultNamespace.AnimScripts
{
    public class MonoClipTransition : MonoBehaviour
    {
        public AnimancerComponent AnimancerComponent;
        [Min(0)]public int Layer;
        public ClipTransition Clip;

        public UltEvent OnStart;
        public UltEvent OnEnd;
        public UltEvent OnStop;

        [ContextMenu("Play")]
        public void Play()
        {
            if(Clip==null)
                return;
            var state = AnimancerComponent.Layers[Layer].Play(Clip);
            if (state != null)
            {
                OnStart.Invoke();
                state.Events.OnEnd = ()=> OnEnd.Invoke();
            }
        }

        [ContextMenu("Stop")]
        public void Stop()
        {
            AnimancerComponent.Layers[Layer].GetOrCreateState(Clip).Stop();
            OnStop.Invoke();
        }

        private void OnValidate()
        {
            if (AnimancerComponent == null)
                AnimancerComponent = GetComponent<AnimancerComponent>();
        }
    }
}