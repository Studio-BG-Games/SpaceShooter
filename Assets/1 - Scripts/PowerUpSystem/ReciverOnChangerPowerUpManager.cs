using System;
using ModelCore;
using Sirenix.Utilities;
using UltEvents;
using UnityEngine;

namespace PowerUpSystem
{
    public class ReciverOnChangerPowerUpManager : MonoBehaviour
    {
        private PowerUpManager _manager;
        private PowerUpManager Manager => _manager ??= EntityAgregator.Instance.Select(x => x.Has<PowerUpManager>()).Select<PowerUpManager>();

        public ReciverTarget[] TargetsRecivers = new ReciverTarget[]{};

        private void OnEnable() => TargetsRecivers.ForEach(x => Manager.ResultOfCommand += x.TryInvoke);

        private void OnDisable() => TargetsRecivers.ForEach(x => Manager.ResultOfCommand -= x.TryInvoke);

        [System.Serializable]
        public class ReciverTarget
        {
            [SerializeReference][SerializeField]public BaseEventActionWithPowerUpManager TypeEvent;
            public UltEvent Event;

            public void TryInvoke(BaseEventActionWithPowerUpManager result)
            {
                if(result.GetType() == TypeEvent.GetType()) Event.Invoke();
            }
        }
    }
}