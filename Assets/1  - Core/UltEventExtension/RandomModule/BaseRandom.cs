using System;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace RandomModule
{
    public abstract class BaseRandom<T> : MonoBehaviour
    {
        public TimeGenerate TimeActivated;
        public T Min;
        public T Max;
        public UltEvent<T> Generated;

        private void Awake() { if (TimeActivated == TimeGenerate.Awake) Generate(); }
        
        private void Start() { if (TimeActivated == TimeGenerate.Start) Generate(); }
        
        private void OnEnable() { if (TimeActivated == TimeGenerate.Enable) Generate(); }

        public void Generate() => Generated.Invoke(Generate(Min, Max));

        public abstract T Generate(T min, T Max);

        public enum TimeGenerate
        {
            Awake, Start, Enable, OnlyManual
        }
    }
}