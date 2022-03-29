using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{

    public class ConditionTrigger : MonoBehaviour
    {
        [SerializeField]
        [TextArea]
        protected string description;

        [Header("Settings")]

        [SerializeField]
        protected BooleanConditionsEvaluationType evaluationType;

        [SerializeField]
        protected List<Condition> conditionsList = new List<Condition>();

        [Header("Events")]

        public UnityEvent onEvaluationSuccessful;

        public UnityEvent onEvaluationFailed;


        private void Awake()
        {
            for(int i = 0; i < conditionsList.Count; ++i)
            {
                conditionsList[i].Initialize();
            }
        }

        public void Trigger()
        {
            switch (evaluationType)
            {
                case BooleanConditionsEvaluationType.And:

                    for (int i = 0; i < conditionsList.Count; ++i)
                    {
                        if (!conditionsList[i].ConditionMet())
                        {
                            EvaluationFailed();
                            return;
                        }
                    }

                    EvaluationSuccessful();

                    break;

                case BooleanConditionsEvaluationType.Or:

                    for (int i = 0; i < conditionsList.Count; ++i)
                    {
                        if (conditionsList[i].ConditionMet())
                        {
                            EvaluationSuccessful();
                            return;
                        }
                    }

                    EvaluationFailed();

                    break;
            }
        }

        protected void EvaluationSuccessful()
        {
            onEvaluationSuccessful.Invoke();
        }

        protected void EvaluationFailed()
        {
            onEvaluationFailed.Invoke();
        }
    }
}
