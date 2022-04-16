namespace PowerUpSystem
{
    [System.Serializable]
    public abstract class BaseEventActionWithPowerUpManager
    {

        protected bool IsMyType(BaseEventActionWithPowerUpManager otherEvetn) => otherEvetn.GetType() == GetType();
    }
}