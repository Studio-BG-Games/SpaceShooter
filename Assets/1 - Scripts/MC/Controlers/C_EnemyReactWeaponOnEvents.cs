using System;
using System.Collections.Generic;
using System.Linq;
using DIContainer;
using ModelCore;
using PowerUpSystem;
using Services;
using Services.PauseManagers;
using UnityEngine;

namespace MC.Controlers
{
    public class C_EnemyReactWeaponOnEvents : MonoBehaviour
    {
        public List<C_WeaponProjectTile> Waepons;
        public PowerUpType StelsType;
        
        private PowerUpManager _manager;
        private PowerUpManager Manager => _manager ??= EntityAgregator.Instance.Select(x => x.Has<PowerUpManager>()).Select<PowerUpManager>();

        private ResolveSingle<PauseGame> _pause = new ResolveSingle<PauseGame>();

        private void Start()
        {
            Manager.ResultOfCommand += Hanler;
            _pause.Depence.IsPause.Updated += UpdatePause;
            _isStels = Manager.PowerUp.FirstOrDefault(x => x.TypePowerUp == StelsType) != null;
            UpdateStateWeapons();
        }

        private void OnDestroy()
        {
            Manager.ResultOfCommand -= Hanler;
            _pause.Depence.IsPause.Updated -= UpdatePause;
        }

        private void Hanler(BaseEventActionWithPowerUpManager obj)
        {
            if (obj is AddNewPowerUp) OnAddBonus(obj as AddNewPowerUp);
            else if (obj is DeletedPowerUpEvent) OnRemoveBonus(obj as DeletedPowerUpEvent);
        }

        private bool _isStels = false;
        private bool _isPause => _pause.Depence.IsPause.Value;
        
        private void UpdatePause(bool obj) => UpdateStateWeapons();

        private void OnRemoveBonus(DeletedPowerUpEvent deletedPowerUpEvent)
        {
            if (deletedPowerUpEvent.DeletedPowerUp.TypePowerUp == StelsType) _isStels = false;
            UpdateStateWeapons();
        }

        private void OnAddBonus(AddNewPowerUp addNewPowerUp)
        {
            if (addNewPowerUp.NewPowerUp.TypePowerUp == StelsType) _isStels = true;
            UpdateStateWeapons();
        }

        private void UpdateStateWeapons()
        {
            if (_isPause) SetEnableWeapons(false);
            else
            {
                if (_isStels) SetEnableWeapons(false);
                else SetEnableWeapons(true);
            }
        }

        private void SetEnableWeapons(bool b) => Waepons.ForEach(x => x.enabled = b);
    }
}