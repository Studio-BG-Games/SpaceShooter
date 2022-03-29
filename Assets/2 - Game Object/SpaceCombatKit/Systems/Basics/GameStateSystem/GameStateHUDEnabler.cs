using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{
    public class GameStateHUDEnabler : MonoBehaviour
    {
        protected HUDManager hudManager;

        [SerializeField]
        protected List<GameState> HUDActiveGameStates = new List<GameState>();

        protected virtual void Awake()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.onEnteredGameState.AddListener(OnEnteredGameState);
            }
        }

        public void SetVehicle(Vehicle vehicle)
        {
            hudManager = vehicle.GetComponentInChildren<HUDManager>();
        }

        public void ClearReferences()
        {
            hudManager = null;
        }

        protected virtual void OnEnteredGameState(GameState gameState)
        {
            if (hudManager != null)
            {
                if (HUDActiveGameStates.IndexOf(gameState) != -1)
                {
                    hudManager.ActivateHUD();
                }
                else
                {
                    hudManager.DeactivateHUD();
                }
            }
        }
    }

}
