using DIContainer;
using UltEvents;
using UnityEngine;

namespace Services.PauseManagers
{
    public class PauseReciver : MonoBehaviour
    {
        private ResolveSingle<PauseGame> _pause = new ResolveSingle<PauseGame>();

        public UltEvent Pause;
        public UltEvent Unpause;
        public UltEvent<bool> NewPause;

        private void Start()
        {
            _pause.Depence.IsPause.Updated += Hanler;
            Hanler(_pause.Depence.IsPause.Value);
        }

        public void SetPause(bool value) => _pause.Depence.IsPause.Value = value;

        private void Hanler(bool obj)
        {
            if(obj) Pause.Invoke();
            else Unpause.Invoke();
            NewPause.Invoke(obj);
        }
    }
}