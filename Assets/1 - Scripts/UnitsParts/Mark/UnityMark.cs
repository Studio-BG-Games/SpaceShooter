using UnityEngine;

namespace Services
{
    [RequireComponent(typeof(Unit))]
    public class UnityMark : MonoBehaviour
    {
        [SerializeField][HideInInspector]private Unit unit;
        public Unit Unit => unit;

        void OnValidate() { if(unit==null) unit = GetComponent<Unit>(); }
    }
}