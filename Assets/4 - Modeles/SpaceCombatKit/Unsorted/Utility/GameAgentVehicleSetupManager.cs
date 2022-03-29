using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    public class GameAgentVehicleSetupManager : MonoBehaviour
    {

        [SerializeField]
        protected Vehicle vehicle;

        public UnityEvent onPlayerEnteredVehicle;

        public UnityEvent onAIEnteredVehicle;

        public UnityEvent onVehicleExited;

        protected Trackable[] trackables;
        protected TargetSelector[] targetSelectors;

        protected Trackable rootTrackable;

        [SerializeField]
        protected bool overrideRootTrackableLabel = true;

        [SerializeField]
        protected string labelKey = "Label";

        protected string originalLabel = "";
        protected Team originalTeam;

        protected bool dataInitialized = false;


        protected void Awake()
        {
            trackables = transform.GetComponentsInChildren<Trackable>();

            rootTrackable = GetComponent<Trackable>();
            
            targetSelectors = transform.GetComponentsInChildren<TargetSelector>();

            vehicle.onGameAgentEntered.AddListener(OnVehicleEntered);
            vehicle.onGameAgentExited.AddListener(OnVehicleExited);
        }

        protected virtual void Start()
        {
            InitializeData();
        }


        protected void InitializeData()
        {
            if (trackables.Length > 0)
            {

                originalTeam = trackables[0].Team;

                if (trackables[0].variablesDictionary.ContainsKey(labelKey))
                {
                    LinkableVariable labelVariable = trackables[0].variablesDictionary[labelKey];
                    if (labelVariable != null)
                    {
                        originalLabel = labelVariable.StringValue;
                    }
                }
            }

            dataInitialized = true;
        }


        protected void Reset()
        {
            vehicle = transform.GetComponent<Vehicle>();
        }

        protected virtual void UpdateTrackables(GameAgent gameAgent)
        {
            // Update the vehicle's team
            Team team = gameAgent == null ? originalTeam : gameAgent.Team;

            // Update the Team for all the trackables on this vehicle
            for (int i = 0; i < trackables.Length; ++i)
            {
                trackables[i].Team = team;
            }

            // Update the label on the root trackable
            string label = gameAgent == null ? originalLabel : gameAgent.Label;
            if (overrideRootTrackableLabel && rootTrackable != null)
            {
                if (rootTrackable.variablesDictionary.ContainsKey(labelKey))
                {
                    LinkableVariable labelVariable = rootTrackable.variablesDictionary[labelKey];
                    if (labelVariable != null)
                    {
                        labelVariable.StringValue = label;
                    }
                }
            }
        }

        protected virtual void UpdateTargetSelectors(GameAgent gameAgent)
        {
            // Update the vehicle's team
            Team team = gameAgent == null ? originalTeam : gameAgent.Team;
          
            if (team != null)
            {
                for (int i = 0; i < targetSelectors.Length; ++i)
                {
                    targetSelectors[i].SelectableTeams = team.HostileTeams;
                }
            }
        }

        protected virtual void OnVehicleEntered(GameAgent gameAgent)
        {
            if (!dataInitialized)
            {
                InitializeData();
            }

            UpdateTrackables(gameAgent);
            UpdateTargetSelectors(gameAgent);

            // Call the event
            if (gameAgent != null)
            {
                if (gameAgent.IsPlayer)
                {
                    onPlayerEnteredVehicle.Invoke();
                }
                else
                {
                    onAIEnteredVehicle.Invoke();
                }
            }
        }

        protected virtual void OnVehicleExited(GameAgent gameAgent)
        {
            UpdateTrackables(null);
            UpdateTargetSelectors(null);
            onVehicleExited.Invoke();
        }
    }
}

