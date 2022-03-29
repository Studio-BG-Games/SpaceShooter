using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    
    /// <summary>
    /// Unity Event for attaching functions to be called when the focused game agent changes.
    /// </summary>
    [System.Serializable]
    public class OnFocusedGameAgentChangedEventHandler : UnityEvent <GameAgent> { };

    /// <summary>
    /// Unity Event for attaching functions to be called when the focused vehicle changes.
    /// </summary>
    [System.Serializable]
    public class OnFocusedVehicleChangedEventHandler : UnityEvent<Vehicle> { };

    /// <summary>
    /// Unity Event for attaching functions to be called when the focused game agent dies.
    /// </summary>
    [System.Serializable]
    public class OnFocusedGameAgentDiedEventHandler : UnityEvent { };


    /// <summary>
    /// The Game Agent Manager allows game agents to be registered and deregistered in the scene at a place where they
    /// can easily be found by any script. This component also allows the scene to focus on a single game agent such that scene level
    /// components (e.g. the HUD and the camera) refer to it.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameAgentManager : MonoBehaviour 
	{

        [SerializeField]
        protected GameAgent startingFocusedGameAgent;

        [SerializeField]
        protected bool findFirstPlayerInScene = true;

        [Header("Events")]

        /// <summary>
        /// Event called when the focused game agent changes.
        /// </summary>
        public OnFocusedGameAgentChangedEventHandler onFocusedGameAgentChanged;

        /// <summary>
        /// Event called when the focused game agent dies.
        /// </summary>
        public OnFocusedGameAgentDiedEventHandler onFocusedGameAgentDied;

        /// <summary>
        /// Event called when the focused game agent changes vehicles.
        /// </summary>
        public OnFocusedVehicleChangedEventHandler onFocusedVehicleChanged;

        /// <summary>
        /// A list of all the game agents in the scene.
        /// </summary>
		[HideInInspector]
		protected List <GameAgent> gameAgents = new List<GameAgent>();
		public List<GameAgent> GameAgents { get { return gameAgents; } }
		
        /// <summary>
        ///  The game agent currently focused on by the scene.
        /// </summary>
		protected GameAgent focusedGameAgent;
		public GameAgent FocusedGameAgent { get { return focusedGameAgent; } }
	
        /// <summary>
        /// Singleton reference.
        /// </summary>
		public static GameAgentManager Instance;

		
		protected virtual void Awake()
		{
			// Singleton - make sure only one instance in scene
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				Destroy(gameObject);
			}
		}

        protected virtual void Start()
        {
            if (startingFocusedGameAgent == null && findFirstPlayerInScene)
            {
                GameAgent[] gameAgents = GameObject.FindObjectsOfType<GameAgent>();
                foreach (GameAgent gameAgent in gameAgents)
                {
                    if (gameAgent.IsPlayer)
                    {
                        startingFocusedGameAgent = gameAgent;
                        break;
                    }
                }
            }

            if (startingFocusedGameAgent != null)
            {
                SetFocusedGameAgent(startingFocusedGameAgent);

                if (startingFocusedGameAgent.Vehicle != null)
                {
                    onFocusedVehicleChanged.Invoke(startingFocusedGameAgent.Vehicle);
                }
            }
            else
            {
                if (findFirstPlayerInScene)
                {

                }
            }
        }

        /// <summary>
        /// Register a new game agent in the scene.
        /// </summary>
        /// <param name="newAgent"> New game agent. </param>
        /// <param name="focusThisAgent"> Whether to focus the scene on this game agent or not. </param>
        public virtual void Register(GameAgent newAgent, bool focusThisAgent = false)
		{

			gameAgents.Add(newAgent);
			
			if (focusThisAgent)
			{ 
				SetFocusedGameAgent(newAgent);
			}
		}

        /// <summary>
        /// Deregister a game agent from the scene.
        /// </summary>
        /// <param name="gameAgent"> Game agent to be deregistered from the scene. </param>
        public virtual void Unregister(GameAgent gameAgent)
        {
            // Update the focused game agent
            if (gameAgent == focusedGameAgent)
            {
                SetFocusedGameAgent(null);
            }

            // Remove this agent from the list
            int index = gameAgents.IndexOf(gameAgent);
            if (index != -1)
            {
                gameAgents.RemoveAt(index);
            }
        }

        /// <summary>
        /// Focus the scene on a different game agent.
        /// </summary>
        /// <param name="newFocusedGameAgent"> Game agent to be focused on. </param>
        public virtual void SetFocusedGameAgent(GameAgent newFocusedGameAgent)
		{
            if (this.focusedGameAgent != null)
            {
                this.focusedGameAgent.onEnteredVehicle.RemoveListener(OnFocusedVehicleChanged);
                this.focusedGameAgent.onDied.RemoveListener(OnFocusedGameAgentDied);
            }

            // Update the focused game agent
			this.focusedGameAgent = newFocusedGameAgent;

            focusedGameAgent.onEnteredVehicle.AddListener(OnFocusedVehicleChanged);
            focusedGameAgent.onDied.AddListener(OnFocusedGameAgentDied);

            // Call the event
            onFocusedGameAgentChanged.Invoke(focusedGameAgent);
		}

        protected void OnFocusedVehicleChanged(Vehicle vehicle)
        {
            onFocusedVehicleChanged.Invoke(vehicle);
        }

        protected virtual void OnFocusedGameAgentDied()
        {
            onFocusedGameAgentDied.Invoke();
        }
    }
}
