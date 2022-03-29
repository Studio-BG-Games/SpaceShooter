using Jint;
using UnityEngine;

namespace Js.Interfaces
{
    public abstract class AbsTypeRegister : MonoBehaviour, ITypeRegister
    {
        public abstract void Register(Engine engine);
    }
}