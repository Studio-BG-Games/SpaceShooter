using System;
using DIContainer;
using Models;
using Services.Inputs;
using UnityEngine;

namespace Services.PauseManagers
{
    public class PauseGame : MonoBehaviour
    {
        private ResolveSingle<IInput> _input = new ResolveSingle<IInput>();
        
        [SerializeField] private ObjectValue<bool> isPause;

        public ObjectValue<bool> IsPause => isPause;

        private void Start() => _input.Depence.Pause += () => SetPause(!isPause.Value);

        public void SetPause(bool val) => isPause.Value = val;

        public void Pause() => SetPause(true);
        
        public void Unpause() => SetPause(false);
    }
}