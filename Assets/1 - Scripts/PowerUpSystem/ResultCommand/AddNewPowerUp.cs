using UnityEngine;

namespace PowerUpSystem
{
    [System.Serializable]
    public class AddNewPowerUp : BaseEventActionWithPowerUpManager
    {
        [SerializeField] private PowerUp _newPowerUp;
        public PowerUp NewPowerUp => _newPowerUp;

        public AddNewPowerUp(PowerUp newPowerUp) => _newPowerUp = newPowerUp;
    }
}