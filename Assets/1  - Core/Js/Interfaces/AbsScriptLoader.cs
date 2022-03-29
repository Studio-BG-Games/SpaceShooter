using Jint;
using UnityEngine;

namespace Js.Interfaces
{
    public abstract class AbsScriptLoader : MonoBehaviour, IScriptLoader
    {
        public abstract void SetScript(Engine engine);
    }
}