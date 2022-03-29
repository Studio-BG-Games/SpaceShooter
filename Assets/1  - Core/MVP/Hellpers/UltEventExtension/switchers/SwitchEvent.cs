using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace CorePresenter.UltEventExtension.switchers
{
    public abstract class SwitchEvent<T> : MonoBehaviour 
    {
        public UltEvent<T> OnDefault;
        [SerializeField] private List<Varitant<T>> _varints;
        
        private Dictionary<T, Varitant<T>> _dcit;
        //Lazy iniy
        public Dictionary<T, Varitant<T>> Dcit => _dcit == null ? _dcit = InitDict() : _dcit;

        private void Awake() => _dcit = InitDict();


        [Button]
        public void Invoke(T arg)
        {
            Dcit.TryGetValue(arg, out var r);
            if(r!=null) r.Event.Invoke(arg);
            else OnDefault.Invoke(arg);
        }

        private Dictionary<T, Varitant<T>> InitDict() => _varints.ToDictionary(x => x.TargetValue);

        [System.Serializable]
        public class Varitant<ET>
        {
            public ET TargetValue;
            public UltEvent<ET> Event;
        }
    }
}