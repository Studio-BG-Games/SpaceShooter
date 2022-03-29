using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    public enum BoolEvaluationType
    {
        IsEqualTo
    }


    [System.Serializable]
    public class Condition
    {

        [SerializeField]
        protected LinkableVariable condition;

        [SerializeField]
        protected BoolEvaluationType boolEvaluationType;

        [SerializeField]
        protected bool boolReferenceValue;

        public void Initialize()
        {
            condition.InitializeLinkDelegate();
        }

        public bool ConditionMet()
        {
            switch (boolEvaluationType)
            {
                case BoolEvaluationType.IsEqualTo:
                    return condition.BoolValue == boolReferenceValue;
                default:
                    return true;
            }
        }

        public bool ConditionValue()
        {
            return condition.BoolValue;
        }
    }

    public enum BooleanConditionsEvaluationType
    {
        And,
        Or
    }

    [System.Serializable]
    public class Conditions
    {

        [SerializeField]
        protected BooleanConditionsEvaluationType evaluationType;

        [SerializeField]
        protected List<Condition> conditionsList = new List<Condition>();

        public UnityEvent onConditionsMet;

        public UnityEvent onConditionsFailed;

        public void Initialize()
        {
            for(int i = 0; i < conditionsList.Count; ++i)
            {
                conditionsList[i].Initialize();
            }
        }

        public bool ConditionsMet
        {
            get
            {
                switch (evaluationType)
                {
                    case BooleanConditionsEvaluationType.And:

                        for (int i = 0; i < conditionsList.Count; ++i)
                        {
                            if (!conditionsList[i].ConditionMet())
                            {
                                onConditionsFailed.Invoke();
                                return false;
                            }
                        }

                        onConditionsMet.Invoke();
                        return true;

                    case BooleanConditionsEvaluationType.Or:

                        for (int i = 0; i < conditionsList.Count; ++i)
                        {
                            if (conditionsList[i].ConditionMet())
                            {
                                onConditionsMet.Invoke();
                                return true;
                            }
                        }

                        onConditionsFailed.Invoke();
                        return true;
                }

                onConditionsFailed.Invoke();
                return false;
            }
        }

        public List<bool> ConditionsValues()
        {
            List<bool> conditionValues = new List<bool>();
            for (int i = 0; i < conditionsList.Count; ++i)
            {
                conditionValues.Add(conditionsList[i].ConditionValue());
            }
            return conditionValues;
        }
    }
}

