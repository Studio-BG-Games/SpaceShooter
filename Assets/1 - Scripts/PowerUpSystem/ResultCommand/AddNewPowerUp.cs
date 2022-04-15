using UnityEngine;

namespace PowerUpSystem
{
    [System.Serializable]
    public class AddNewPowerUp : BaseEventActionWithPowerUpManager
    {
        [SerializeField] private PowerUp _newPowerUp;
        public PowerUp NewPowerUp => _newPowerUp;

        public AddNewPowerUp(PowerUp newPowerUp) => _newPowerUp = newPowerUp;
        
        public override bool IsMe(BaseEventActionWithPowerUpManager otherevent)
        {
            if (_newPowerUp.TypePowerUp == null) return true;
            if (!IsMyType(otherevent)) return false;

            var cast = otherevent as AddNewPowerUp;
            return cast.NewPowerUp.TypePowerUp == NewPowerUp.TypePowerUp;
        }
    }
}