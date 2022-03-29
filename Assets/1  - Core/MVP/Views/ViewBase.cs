using ModelCore;
using Sirenix.OdinInspector;

namespace MVP.Views
{
    public abstract class ViewBase<T> : SerializedMonoBehaviour where T : Model
    {
        public abstract void View(T engine);
    }
}