using DIContainer;
using Services;
using UnityEngine;

namespace Spawners
{
    public class SingleSpawner : BaseSpawner
    {
        private FactoryUnit _factoryUnit=>DiBox.MainBox.ResolveSingle<FactoryUnit>();
        
        public Unit SpawnObject;
        public Transform Point;

        public Transform GetPoint => Point != null ? Point : transform;

        [SerializeField] private float _sizeGizmos=10;
        
        
        public override void Generate() => _factoryUnit.CreateUnit(SpawnObject, GetPoint);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(GetPoint.position, 1*_sizeGizmos);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(GetPoint.position, 0.3f*_sizeGizmos);
        }
    }
}