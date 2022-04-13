using System.Collections.Generic;
using ModelCore;
using Sirenix.Utilities;
using UnityEngine;

namespace PowerUpSystem
{
    public class ComandToPowerUpManager : MonoBehaviour
    {
        [SerializeReference][SerializeField] private ICommandPowerUp[] _commands;

        private PowerUpManager _manager;
        private PowerUpManager Manager => _manager ??= EntityAgregator.Instance.Select(x => x.Has<PowerUpManager>()).Select<PowerUpManager>();

        void Invoke() => Manager.ExuteCommnad(_commands);
        
        public interface ICommandPowerUp
        {
            List<BaseEventActionWithPowerUpManager> Make(PowerUpManager powerUpManager);
        }
    
        [System.Serializable]
        public class AddOrReplace : ICommandPowerUp
        {
            [SerializeField] private PowerUp PowerUp;
            
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

    }
}