using System;
using DIContainer;
using UltEvents;
using UnityEngine;

namespace Sharp.UnityMessager
{
    public abstract class BaseResiverMessager : MonoBehaviour
    {
        public IdReciver MyId;
        public SubscribeTime TimeSubscribe;

        public UltEvent Evented;
        
        protected bool DependesByEnableComponent = false;

        private void Awake()
        {
            if (TimeSubscribe == SubscribeTime.Awake) SendMessagerUnity.OnMes += Handler;
        }

        private void Start()
        {
            if (TimeSubscribe == SubscribeTime.Start) SendMessagerUnity.OnMes += Handler;
        }

        private void OnEnable()
        {
            if (TimeSubscribe == SubscribeTime.Enable || DependesByEnableComponent) SendMessagerUnity.OnMes += Handler;
        }

        private void OnDisable()
        {
            if (TimeSubscribe == SubscribeTime.Enable || DependesByEnableComponent) SendMessagerUnity.OnMes -= Handler;
        }

        private void OnDestroy()
        {
            SendMessagerUnity.OnMes -= Handler;
        }


        private void Handler(IdReciver targetID, Event e)
        {
            if (targetID==MyId && IsTargetEvent(e))
                Evented.Invoke();
        }

        abstract protected bool IsTargetEvent(Event e);
        
        public enum SubscribeTime
        {
            Awake, Start, Enable
        }
    }
}