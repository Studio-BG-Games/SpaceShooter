using System.Collections;
using DIContainer;
using Services;
using UnityEngine;

namespace Spawners
{
    public class ArraySpawner : BaseSpawner
    {
        [Min(1)] public int Count;
        [Min(0)] public float DistanceBetween;
        public Vector3 Diraction;
        public float DelaySpawnBettwenPoint;
        public Unit SpawnObject;
        
        [SerializeField] private float _sizeGizmos=10;
        private FactoryUnit _factoryUnit=>DiBox.MainBox.ResolveSingle<FactoryUnit>();
        
        public override void Generate()
        {
            for (int i = 0; i < Count; i++) StartCoroutine(Spawn(DelaySpawnBettwenPoint * i, SpawnObject, GetPosition(i), transform));
        }

        private Vector3 GetPosition(int index) => transform.position + Diraction * DistanceBetween * index;

        private IEnumerator Spawn(float delay, Unit prefab, Vector3 point, Transform parent)
        {
            yield return new WaitForSeconds(delay);
            var result = factoryUnit.CreateUnit(prefab, point);
            result.transform.SetParent(parent);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.3f*_sizeGizmos);
            Gizmos.DrawLine(transform.position, transform.position+Diraction*DistanceBetween);

            Gizmos.color = Color.cyan;
            for (int i = 0; i < Count; i++) 
                Gizmos.DrawWireSphere(GetPosition(i), 0.5f * _sizeGizmos);
        }
    }
}