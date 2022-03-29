using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class GameStateEnter : MonoBehaviour
    {
        [SerializeField]
        protected GameState defaultGameState;


        public void EnterGameState()
        {
            GameStateManager.Instance.EnterGameState(defaultGameState);
        }

        public void EnterGameState(GameState newGameState)
        {
            GameStateManager.Instance.EnterGameState(newGameState);
        }
    }
}
