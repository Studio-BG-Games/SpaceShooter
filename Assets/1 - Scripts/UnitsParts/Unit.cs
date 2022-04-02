using System;
using System.Linq;
using Dreamteck.Forever;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace Services
{
    public class Unit : MonoBehaviour
    {
        [SerializeField] private PartUnit[] _parts;

        public T Select<T>() where T : PartUnit => (T) _parts.FirstOrDefault(x => x is T);

        public T Select<T>(Func<T, bool> predicate) where T : PartUnit => (T) _parts.FirstOrDefault(x =>
        {
            var part = x as T;
            if (part != null) return predicate.Invoke(part);
            return false;
        });

        private void OnValidate()
        {
            _parts = GetComponents<PartUnit>();
            _parts.ForEach(x => x.Init(this));
        }
    }
}