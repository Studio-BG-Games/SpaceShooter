using System;
using System.Linq;
using ModelCore;
using Services;
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

        private void OnDestroy()
        {
            TargetsRecivers?.ForEach(x =>
            {
                if (x != null) x.OnDestroy();
            });
        }

        [System.Serializable]
        public abstract class BaseReciverTarget
        {
            [SerializeField]protected UltEvent Event;

            public virtual void TryInvokeOnStart(PowerUpManager manag) { }
            
            public abstract void Handler(BaseEventActionWithPowerUpManager e);

            public virtual void OnDestroy() {}
        }
        
        public class FreezRecive : BaseReciverTarget
        {
            public UltEvent Freezed;
            public UltEvent Unfreezed;
            
            public PowerUpType TypeFreez;
            public HealthDamageEvent Damage;

            private bool _hasFreesed = false;

            public override void TryInvokeOnStart(PowerUpManager manag)
            {
                if (manag.PowerUp.FirstOrDefault(x => x.TypePowerUp == TypeFreez) != null) OnAdd();
            }

            public override void Handler(BaseEventActionWithPowerUpManager e)
            {
                if (TryCast<AddNewPowerUp>(e, out var added) && added.NewPowerUp.TypePowerUp == TypeFreez) OnAdd();
                if (TryCast<DeletedPowerUpEvent>(e, out var deleted) && deleted.DeletedPowerUp.TypePowerUp == TypeFreez) OnDelete();
            }

            public override void OnDestroy()
            {
                OnDelete();
            }

            private void OnAdd() => Damage.DamageAt += HandlerDamage;

            private void OnDelete()
            {
                Damage.DamageAt -= HandlerDamage;
                if (_hasFreesed)
                {
                    Unfreezed.Invoke();
                    _hasFreesed = false;
                }
            }

            private void HandlerDamage(int obj)
            {
                Freezed.Invoke();
                _hasFreesed = true;
            }

            private bool TryCast<T>(object obj, out T r) where T : class
            {
                r = obj as T;
                return r != null;
            }
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