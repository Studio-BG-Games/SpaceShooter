using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class GameStateCompatibilityChecker : MonoBehaviour
    {
      
        [SerializeField]
        protected List<GameState> compatibleGameStates = new List<GameState>();

        public bool IsCompatibleGameState
        {
            get
            {
                if (GameStateManager.Instance != null)
                {
                    for(int i = 0; i < compatibleGameStates.Count; ++i)
                    {
                        if (compatibleGameStates[i] == GameStateManager.Instance.CurrentGameState)
                        {
                            return true;
                        }
                    }

                    return false;

                }
                else
                {
                    return true;
                }
            }
        }
    }
}
