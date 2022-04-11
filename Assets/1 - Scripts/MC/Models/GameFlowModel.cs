using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MC.Controlers
{
    public class GameFlowModel : MonoBehaviour
    {
        public GameEvent PlayerDead;
        public GameEvent GameWin;

        public GameEvent ExitMenu;
        public GameEvent RestartGame;
        public GameEvent RecoveryPlayer;

        
        [System.Serializable]
        public struct GameEvent
        {
            public event Action Evented;
            [Button]public void Invoke() => Evented?.Invoke();
        }
    }
}