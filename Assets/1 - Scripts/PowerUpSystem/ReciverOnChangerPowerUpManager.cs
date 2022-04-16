using System;
using System.Linq;
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

        [SerializeField, SerializeReference] public BaseReciverTarget[] TargetsRecivers = new BaseReciverTarget[]{};

        private void Start()
        {
            TargetsRecivers = TargetsRecivers.Where(x => x != null).ToArray();
        }

        private void OnEnable()
        {
            TargetsRecivers?.ForEach(x =>
            {
                if (x != null)
                {
                    Manager.ResultOfCommand += x.Handler;
                    x.TryInvokeOnStart(Manager);
                }
            });
        }

        private void OnDisable()
        {
            TargetsRecivers?.ForEach(x =>
            {
                if(x!=null)
                    Manager.ResultOfCommand -= x.Handler;
            });
        }

        [System.Serializable]
        public abstract class BaseReciverTarget
        {
            [SerializeField]protected UltEvent Event;

            public virtual void TryInvokeOnStart(PowerUpManager manag) { }
            
            public abstract void Handler(BaseEventActionWithPowerUpManager e);
        }

        public class OnAddPowerUp : BaseReciverTarget
        {
            public PowerUpType Type;

            public override void TryInvokeOnStart(PowerUpManager manag)
            {
                Debug.Log("Start invoke");
                if (manag.PowerUp.FirstOrDefault(x => x.TypePowerUp == Type) != null)
                {
                    Debug.Log("Suces start on invoke");
                    Event?.Invoke();
                }
                Debug.Log("__________");
            }

            public override void Handler(BaseEventActionWithPowerUpManager e)
            {
                var casted = e as AddNewPowerUp;
                if (casted == null) return;
                
                if(Type==null && casted.NewPowerUp!=null) Event?.Invoke();
                else if(casted.NewPowerUp.TypePowerUp == Type) Event?.Invoke();
            }
        }
        
        public class OnRemovePowerUp : BaseReciverTarget
        {
            public PowerUpType Type;

            public override void Handler(BaseEventActionWithPowerUpManager e)
            {
                var casted = e as DeletedPowerUpEvent;
                if (casted == null) return;
                
                if(Type==null && casted.DeletedPowerUp!=null) Event?.Invoke();
                else if(casted.DeletedPowerUp.TypePowerUp == Type) Event?.Invoke();
            }
        }
    }
}