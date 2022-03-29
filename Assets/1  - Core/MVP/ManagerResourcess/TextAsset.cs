using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ManagerResourcess
{
    [CreateAssetMenu(order = 51, menuName = "Resources MVP/TextAssets")]
    public class TextAsset : Resources<UnityEngine.TextAsset>
    {
        [SerializeField] private List<UnityEngine.TextAsset> _texts;
        private Dictionary<string, UnityEngine.TextAsset> _dicts;

        public override UnityEngine.TextAsset Get(string id)
        {
            if (_dicts == null) _dicts = _texts.ToDictionary(x => x.name);
            _dicts.TryGetValue(id, out var r);
            return r;
        }

        public override UnityEngine.TextAsset[] GetAll() => _texts.ToArray();

        [Button]
        public void DeleteEmptyAndDouble() => _texts = _texts.Distinct().Where(x => x != null).ToList();

        private void OnValidate() => DeleteEmptyAndDouble();
    }
}