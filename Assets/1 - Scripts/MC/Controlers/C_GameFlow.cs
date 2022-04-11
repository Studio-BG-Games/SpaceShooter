using System;
using ModelCore;
using UnityEngine;

namespace MC.Controlers
{
    public class C_GameFlow : MonoBehaviour
    {
        private GameFlowModel _model;

        private void OnEnable()
        {
            _model = EntityAgregator.Instance.Select(x => x.Has<GameFlowModel>()).Select<GameFlowModel>();
        }

        public void Win() => _model.GameWin.Invoke();
        
        public void PlayerDead() => _model.PlayerDead.Invoke();
        
        public void ToMenu() => _model.ExitMenu.Invoke();
        
        public void Restart() => _model.RestartGame.Invoke();
        
        public void RecoveryPlayer() => _model.RecoveryPlayer.Invoke();
    }
}