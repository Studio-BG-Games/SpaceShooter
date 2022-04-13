using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DIContainer;
using Services.PauseManagers;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace PowerUpSystem
{
    public class PowerUpManager : SerializedMonoBehaviour
    {
        [ReadOnly][SerializeReference][SerializeField] private Dictionary<PowerUpType, PowerUp> _powerUps = new Dictionary<PowerUpType, PowerUp>();
        public ReadOnlyCollection<PowerUp> PowerUp => _powerUps.Values.ToList().AsReadOnly();

        private ResolveSingle<PauseGame> _pause = new ResolveSingle<PauseGame>();

        public event Action<BaseEventActionWithPowerUpManager> ResultOfCommand;

        public bool TryAdd(PowerUp powerUp)
        {
            if (Has(powerUp.TypePowerUp)) return false;
            _powerUps.Add(powerUp.TypePowerUp, powerUp);
            return true;
        }

        public bool TryGet(PowerUpType type, out PowerUp result) => _powerUps.TryGetValue(type, out result);

        public bool Has(PowerUpType type) => _powerUps.ContainsKey(type);

        public void Remove(PowerUpType type)
        {
            _powerUps.Remove(type);
        }

        [Button] public void ExuteCommnad(CommandToPowerUpManager.ICommandPowerUp command) => command.Make(this).ForEach(x => ResultOfCommand?.Invoke(x));

        public void ExuteCommnad(IEnumerable<CommandToPowerUpManager.ICommandPowerUp> commands) => commands.ForEach(x => ExuteCommnad(x));

        private List<CommandToPowerUpManager.ICommandPowerUp> _commnasOnUpdate = new List<CommandToPowerUpManager.ICommandPowerUp>();
        private void Update()
        {
            if (_pause.Depence.IsPause.Value) return;
            
            _powerUps.ForEach(x =>
            {
                x.Value.Life(Time.deltaTime);
                if(x.Value.IsEnd)
                    _commnasOnUpdate.Add(new CommandToPowerUpManager.RemovePowerUp(x.Value.TypePowerUp));
            });
            if (_commnasOnUpdate.Count > 0)
            {
                ExuteCommnad(_commnasOnUpdate);
                _commnasOnUpdate.Clear();
            }
        }
    }
}