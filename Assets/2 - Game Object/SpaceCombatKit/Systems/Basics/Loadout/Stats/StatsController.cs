using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    public class StatsController : MonoBehaviour
    {

        [Header("Stats Controller")]

        [SerializeField]
        protected GameObject statsObject;

        [SerializeField]
        protected UVCText labelText;
        public UVCText LabelText
        {
            get { return labelText; }
            set { labelText = value; }
        }

        [SerializeField]
        protected UVCText descriptionText;
        public UVCText DescriptionText
        {
            get { return descriptionText; }
            set { descriptionText = value; }
        }

        [SerializeField]
        protected StatsInstance statsInstancePrefab;
        protected List<StatsInstance> statsInstances = new List<StatsInstance>();

        [SerializeField]
        protected Transform statsInstanceParent;


        public virtual void SetActivated(bool activated)
        {
            statsObject.SetActive(activated);
        }

        public virtual StatsInstance GetStatsInstance()
        {
            foreach(StatsInstance statsInstance in statsInstances)
            {
                if (!statsInstance.gameObject.activeSelf)
                {
                    statsInstance.gameObject.SetActive(true);
                    return statsInstance;
                }
            }

            StatsInstance newStatsInstance = Instantiate(statsInstancePrefab, statsInstanceParent);
            statsInstances.Add(newStatsInstance);

            return newStatsInstance;
        }

        public virtual void ClearStatsInstances()
        {
            foreach (StatsInstance statsInstance in statsInstances)
            {
                statsInstance.gameObject.SetActive(false);
            }
        }
    }
}

