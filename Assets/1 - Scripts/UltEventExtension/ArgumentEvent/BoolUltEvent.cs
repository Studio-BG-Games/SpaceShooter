using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace CorePresenter.UltEventExtension.ArgumentEvent
{
    [AddComponentMenu("MV*/Event mediator/Event Bool")]
    public class BoolUltEvent : MonoBehaviour
    {
        public UltEvent OnTrue;
        public UltEvent OnFalse;

        [Button]
        public void Invoke(bool arg) { if (arg) OnTrue.Invoke();else OnFalse.Invoke(); }

        public void Invoke( TypeCondition condition, bool arg, bool arg2) => BaseInvoke(condition, arg, arg2);
        
        public void Invoke( TypeCondition condition, bool arg, bool arg2, bool arg3) => BaseInvoke(condition, arg, arg2, arg3);
        
        public void Invoke( TypeCondition condition, bool arg, bool arg2, bool arg3, bool arg4) => BaseInvoke(condition, arg, arg2, arg3, arg4);

        private void BaseInvoke(TypeCondition condition, params bool[] arg)
        {
            bool result = false;
            switch (condition)
            {
                case TypeCondition.And:
                    result = true;
                    for (int i = 0; i < arg.Length; i++)
                    {
                        result = arg[i];
                        if(result==false) break;
                    }
                    break;
                case TypeCondition.Or:
                    result = false;
                    for (int i = 0; i < arg.Length; i++)
                    {
                        result = arg[i];
                        if(result==true) break;
                    }
                    break;
            }
            if (result) OnTrue.Invoke(); else OnFalse.Invoke();
        }

        public enum TypeCondition
        {
            And, Or
        }
    }
}