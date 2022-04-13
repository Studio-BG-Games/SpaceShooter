namespace PowerUpSystem
{
    [System.Serializable]
    public class DeletedPowerUpEvent : BaseEventActionWithPowerUpManager
    {
        public PowerUp _deletedPowerUp;
        public PowerUp DeletedPowerUp => _deletedPowerUp;

        public DeletedPowerUpEvent(PowerUp deleteUp) => _deletedPowerUp = deleteUp;
        
        public override bool IsMe(BaseEventActionWithPowerUpManager otherevent)
        {
            if (!IsMyType(otherevent)) return false;

            var cast = otherevent as DeletedPowerUpEvent;
            return cast.DeletedPowerUp.TypePowerUp == DeletedPowerUp.TypePowerUp;
        }
    }
}