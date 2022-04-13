namespace PowerUpSystem
{
    [System.Serializable]
    public abstract class BaseEventActionWithPowerUpManager
    {
        public abstract bool IsMe(BaseEventActionWithPowerUpManager otherevent);

        protected bool IsMyType(BaseEventActionWithPowerUpManager otherEvetn) => otherEvetn.GetType() == GetType();
    }
}