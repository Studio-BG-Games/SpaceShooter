using UnityEngine;

namespace Services
{
    public abstract class PartUnit : MonoBehaviour
    {
        public Unit Parent { get; private set; }

        public void Init(Unit unit) => Parent = unit;
    }
}