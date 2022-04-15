using UnityEngine;

namespace PowerUpSystem
{
    [System.Serializable]
    public class DeletedPowerUpEvent : BaseEventActionWithPowerUpManager
    {
        [SerializeField] private PowerUp _deletedPowerUp;
        public PowerUp DeletedPowerUp => _deletedPowerUp;

        public DeletedPowerUpEvent(PowerUp deleteUp) => _deletedPowerUp = deleteUp;
        
        public override bool IsMe(BaseEventActionWithPowerUpManager otherevent)
        {
            if (_deletedPowerUp.TypePowerUp == null) return true;
            if (!IsMyType(otherevent)) return false;

            var cast = otherevent as DeletedPowerUpEvent;
            return cast.DeletedPowerUp.TypePowerUp == DeletedPowerUp.TypePowerUp;
        }
    }
}