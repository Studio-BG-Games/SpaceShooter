using UnityEngine;

namespace DIContainer
{
    public abstract class FactoryDI : MonoBehaviour
    {
        public abstract void Create(DiBox container);

        public abstract void DestroyDi(DiBox container);
    }
}