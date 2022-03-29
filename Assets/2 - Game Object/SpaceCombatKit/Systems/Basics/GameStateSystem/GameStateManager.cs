using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Unity event for running functions when the game state changes.
    /// </summary>
    [System.Serializable]
    public class OnEnteredGameStateEventHandler : UnityEvent <GameState> { }

    /// <summary>
    /// Provides a way for the user to set parameters for each of the game states.
    /// </summary>
    [System.Serializable]
    public class GameStateInstance
    {
        // The game state that these parameters refer to
        public GameState gameState;

        // Whether time should be frozen when the game enters this state
        public bool freezeTimeOnEntry;

        public float pauseBeforeEntry = 0f;

        public bool showCursor = true;

        public bool centerCursorAtStart = true;

        public bool lockCursor = false;

        public bool restrictEntryStates;

        public List<GameState> allowedEntryStates = new List<GameState>();

    }

    /// <summary>
    /// This class provides a single location to store the current state of the game.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {

        [SerializeField]
        protected GameState startingGameState;

        protected GameState currentGameState;
        public GameState CurrentGameState { get { return currentGameState; } }

        [Header("Game States")]

        // A list that stores the parameters associated with each of the game state
        [SerializeField]
        protected List<GameStateInstance> gameStates = new List<GameStateInstance>();

        // The singleton instance for this component
        public static GameStateManager Instance;

        // Enter game state with a delay using a coroutine
        protected Coroutine enterGameStateCoroutine;
        
        [Header("Events")]

        // Event
        public OnEnteredGameStateEventHandler onEnteredGameState;

        protected bool enteringState = false;


        protected void Awake()
        {
            // Enforce the singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            // Add the function to be called when the scene is exited
            SceneManager.sceneUnloaded += OnSceneUnloaded;

        }

        // Called at start of scene
        protected virtual void Start()
        {
            EnterGameState(startingGameState);
        }

        /// <summary>
        /// Enter a game state.
        /// </summary>
        /// <param name="newGameState">The new game state.</param>
        public void EnterGameState(GameState newGameState)
        {
            if (enteringState)
            {
                return;
            }
            
            for (int i = 0; i < gameStates.Count; ++i)
            {
                if (gameStates[i].gameState == newGameState)
                {
                    
                    if (gameStates[i].restrictEntryStates)
                    {
                        bool allow = false;
                        foreach (GameState allowedOriginState in gameStates[i].allowedEntryStates)
                        {
                            if (currentGameState == allowedOriginState)
                            {
                                allow = true;
                                break;
                            }
                        }

                        if (!allow) return;
                    }
                    
                    if (!Mathf.Approximately(gameStates[i].pauseBeforeEntry, 0))
                    {
                        enterGameStateCoroutine = StartCoroutine(PauseBeforeEntryCoroutine(gameStates[i].pauseBeforeEntry, gameStates[i]));
                    }
                    else
                    {
                        EnterGameState(gameStates[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Enter a new game state.
        /// </summary>
        /// <param name="newGameStateInstance">The new game state instance.</param>
        protected void EnterGameState(GameStateInstance newGameStateInstance)
        {
            // Stop game state change coroutine
            if (enterGameStateCoroutine != null) StopCoroutine(enterGameStateCoroutine);
            
            // Update the game state
            currentGameState = newGameStateInstance.gameState;

            // Freeze time if applicable
            if (newGameStateInstance.freezeTimeOnEntry)
            {
                Time.timeScale = 0;
                AudioListener.pause = true;
            }
            else
            {
                Time.timeScale = 1;
                AudioListener.pause = false;
            }

            SetCursorVisible(newGameStateInstance.showCursor);
            
            if (newGameStateInstance.centerCursorAtStart)
            {
                CenterCursor();
            }

            SetCursorLock(newGameStateInstance.lockCursor);

            // Call event
            onEnteredGameState.Invoke(newGameStateInstance.gameState);
        }

        /// <summary>
        /// Center the cursor
        /// </summary>
        public void CenterCursor()
        {
            CursorLockMode initialState = Cursor.lockState;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.lockState = initialState;
        }

        /// <summary>
        /// Change cursor visibility.
        /// </summary>
	    public void SetCursorVisible(bool visible)
        {
            Cursor.visible = visible;
        }

        /// <summary>
        /// Set the cursor locked or not.
        /// </summary>
        /// <param name="locked">Whether to lock the cursor.</param>
        public void SetCursorLock(bool locked)
        {
            if (locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        IEnumerator PauseBeforeEntryCoroutine(float pause, GameStateInstance nextState)
        {
            enteringState = true;
            yield return new WaitForSeconds(pause);
            enteringState = false;
            EnterGameState(nextState);
        }

        // Called when the scene manager exits a scene. Disable any cursor lock and show cursor.
        protected void OnSceneUnloaded(Scene scene)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 1;
            AudioListener.pause = false;
        }
    }
}
