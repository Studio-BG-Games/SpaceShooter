using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Represents a team for target selection etc.
    /// </summary>
    [CreateAssetMenu]
    public class Team : ScriptableObject
    {
        [SerializeField]
        protected Color defaultColor;
        public Color DefaultColor { get { return defaultColor; } }

        [SerializeField]
        protected List<Team> hostileTeams = new List<Team>();
        public List<Team> HostileTeams
        {
            get { return hostileTeams; }
            set { hostileTeams = value; }
        }
    }
}
