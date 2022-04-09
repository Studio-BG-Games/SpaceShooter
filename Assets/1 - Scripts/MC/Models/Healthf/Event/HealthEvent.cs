using UnityEngine;

namespace Services
{
    public abstract class HealthEvent : MonoBehaviour
    {
        [SerializeField] protected HealthfRef RefHp;
        protected Health current;

        private void Start()
        {
            current = RefHp.Component;
            RefHp.Updated += HandlerOnInit;
            if (RefHp.Component != null)
            {
                RefHp.Component.ChangedOldNew += Handler;
                Handler(RefHp.Component.Current, RefHp.Component.Current);
            }
        }

        private void HandlerOnInit(Health obj)
        {
            if (current) current.ChangedOldNew -= Handler;
            current = obj;
            obj.ChangedOldNew += Handler;
            Handler(obj.Current, obj.Current);
        }

        protected abstract void Handler(int old, int current);
    }
}