using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Represents a health type for damage/healing.
    /// </summary>
    [CreateAssetMenu]
    public class HealthType : ScriptableObject 
    {
        [SerializeField]
        protected Color m_Color = Color.white;
        public Color Color
        {
            get { return m_Color; }
        }
    }
}

