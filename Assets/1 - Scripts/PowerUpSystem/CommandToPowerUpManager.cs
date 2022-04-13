using System.Collections.Generic;
using ModelCore;
using Sirenix.Utilities;
using UnityEngine;

namespace PowerUpSystem
{
    public class CommandToPowerUpManager : MonoBehaviour
    {
        [SerializeReference][SerializeField] private ICommandPowerUp[] _commands;

        private PowerUpManager _manager;
        private PowerUpManager Manager => _manager ??= EntityAgregator.Instance.Select(x => x.Has<PowerUpManager>()).Select<PowerUpManager>();

        public void InvokeCommands() => Manager.ExuteCommnad(_commands);
        
        public interface ICommandPowerUp
        {
            List<BaseEventActionWithPowerUpManager> Make(PowerUpManager powerUpManager);
        }
    
        [System.Serializable]
        public class AddOrReplace : ICommandPowerUp
        {
            [SerializeField] private PowerUp PowerUp;

            public AddOrReplace(PowerUp up) => PowerUp = up;
            
            public List<BaseEventActionWithPowerUpManager> Make(PowerUpManager powerUpManager)
            {
                List<BaseEventActionWithPowerUpManager> changers = new List<BaseEventActionWithPowerUpManager>();
                if (powerUpManager.TryGet(PowerUp.TypePowerUp, out var r))
                {
                    changers.Add(new DeletedPowerUpEvent(r));
                    powerUpManager.Remove(PowerUp.TypePowerUp);
                }
                powerUpManager.TryAdd(PowerUp);
                changers.Add(new AddNewPowerUp(PowerUp));

                return changers;
            }
        }

        public class RemovePowerUp : ICommandPowerUp
        {
            [SerializeField] private PowerUpType TypeUp;

            public RemovePowerUp(PowerUpType typeUp) => TypeUp = typeUp;
            
            public List<BaseEventActionWithPowerUpManager> Make(PowerUpManager powerUpManager)
            {
                if(!powerUpManager.TryGet(TypeUp, out var r)) return new List<BaseEventActionWithPowerUpManager>();

                powerUpManager.Remove(TypeUp);
                return new List<BaseEventActionWithPowerUpManager>(){new DeletedPowerUpEvent(r)};
            }
        }
    }
}