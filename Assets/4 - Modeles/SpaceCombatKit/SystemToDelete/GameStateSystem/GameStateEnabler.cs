using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    public class GameStateEnabler : MonoBehaviour
    {

        [SerializeField]
        protected List<GameState> compatibleGameStates = new List<GameState>();

        public UnityEvent onCompatibleGameStateEntered;

        public UnityEvent onIncompatibleGameStateEntered;


        protected virtual void Awake()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.onEnteredGameState.AddListener(OnEnteredGameState);
            }
        }

        protected virtual void OnEnteredGameState(GameState gameState)
        {
            if (compatibleGameStates.IndexOf(gameState) != -1)
            {
                onCompatibleGameStateEntered.Invoke();
            }
            else
            {
                onIncompatibleGameStateEntered.Invoke();
            }
        }
    }
}

