namespace ManagerResourcess
{
    public abstract class Resources<T> : BaseResources
    {
        public abstract T Get(string id);

        public abstract T[] GetAll();
    }
}