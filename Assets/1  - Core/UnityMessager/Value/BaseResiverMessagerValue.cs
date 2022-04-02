using System;
using UltEvents;
using UnityEngine;

namespace Sharp.UnityMessager
{
    public abstract class BaseResiverMessagerValue<T> : MonoBehaviour
    {
        public IdReciver MyId;
        public SubscribeTime TimeSubscribe;
        public T TargetValue;

        public UltEvent EventedTarget;
        public UltEvent<T> Evented;
        
        protected bool DependesByEnableComponent = false;

        private void Awake()
        {
            if (TimeSubscribe == SubscribeTime.Awake) SendMessagerValueUnity<T>.OnMes += Handler;
        }

        private void Start()
        {
            if (TimeSubscribe == SubscribeTime.Start) SendMessagerValueUnity<T>.OnMes += Handler;
        }

        private void OnEnable()
        {
            if (TimeSubscribe == SubscribeTime.Enable || DependesByEnableComponent) SendMessagerValueUnity<T>.OnMes += Handler;
        }

        private void OnDisable()
        {
            if (TimeSubscribe == SubscribeTime.Enable || DependesByEnableComponent) SendMessagerValueUnity<T>.OnMes -= Handler;
        }

        private void OnDestroy()
        {
            SendMessagerValueUnity<T>.OnMes -= Handler;
        }


        private void Handler(IdReciver targetID, Event e, T Value)
        {
            if (Value.Equals(TargetValue)) EventedTarget.Invoke();
            if (targetID==MyId && IsTargetEvent(e)) Evented.Invoke(Value);
        }

        abstract protected bool IsTargetEvent(Event e);
        
        public enum SubscribeTime
        {
            Awake, Start, Enable
        }
    }
}