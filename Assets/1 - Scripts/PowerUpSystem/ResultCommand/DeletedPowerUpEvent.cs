using UnityEngine;

namespace PowerUpSystem
{
    [System.Serializable]
    public class DeletedPowerUpEvent : BaseEventActionWithPowerUpManager
    {
        [SerializeField] private PowerUp _deletedPowerUp;
        public PowerUp DeletedPowerUp => _deletedPowerUp;

        public DeletedPowerUpEvent(PowerUp deleteUp) => _deletedPowerUp = deleteUp;
    }
}