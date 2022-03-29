using DIContainer;

namespace ManagerResourcess
{
    public class BindPackResources : FactoryDI
    {
        public PackOfResources Pack;
        
        public override void Create(DiBox container) => container.RegisterSingle(Pack);

        public override void DestroyDi(DiBox container) => container.RemoveSingel<PackOfResources>();
    }
}