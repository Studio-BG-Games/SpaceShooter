using System;
using DIContainer;
using Services.PauseManagers;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace Helpers
{
    public class PhysicalTimer : MonoBehaviour
    {
        public UltEvent Event;
        [Min(0)] public float Timer;
        [ReadOnly, ShowInInspector] private float _pastTime;
        
        private ResolveSingle<PauseGame> _pause = new ResolveSingle<PauseGame>();
        private bool IsEnd => _pastTime > Timer;

        private void Update()
        {
            if(_pause.Depence.IsPause.Value || IsEnd) return;
            _pastTime += Time.deltaTime;
            if(IsEnd) Event.Invoke();
        }
    }
}