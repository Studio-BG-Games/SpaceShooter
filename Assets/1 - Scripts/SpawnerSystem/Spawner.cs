using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using LinqExtensions = Sirenix.Utilities.LinqExtensions;

namespace SpawnerSystem
{
    public class Spawner : MonoBehaviour
    {
        [HideLabel] [SerializeField] [SerializeReference] private ICreateEntity _prefab;
        [HideLabel] [SerializeField] [SerializeReference] private ISpawnMethod _spawnMethod;
        [HideLabel] [SerializeField] [SerializeReference] private List<IAddActionSpawn> _addAction;

        public void Spawn()
        {
            if (_prefab == null)
            {
                Debug.LogError("Нет Entity для спавна", this);
                return;
            }
            if (_spawnMethod == null)
            {
                Debug.LogError("Нет Spawn method для спавна", this);
                return;
            }
            _spawnMethod.Spawn(_prefab, _addAction);
        }
        
        private void OnDrawGizmos() => _spawnMethod?.DrawGizmos();
    }
}