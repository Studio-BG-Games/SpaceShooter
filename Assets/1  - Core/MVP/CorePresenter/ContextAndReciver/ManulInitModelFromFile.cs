using ModelCore;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CorePresenter.ContextAndReciver
{
    [RequireComponent(typeof(RootPresenter))]
    [AddComponentMenu("MV*/Manual Init from file", 0)]
    public class ManulInitModelFromFile : MonoBehaviour
    {
        public TextAsset TextAsset;

        [Button(Name = "Load")]
        private void Start()
        {
            var model = RootModel.Factory.CreateFromJs(TextAsset.text, TextAsset.name);
            if(model!=null) GetComponent<RootPresenter>().Init(model);
            else Debug.LogWarning("Не удалось создать модель из скрипта в ручную", this);
        }
    }
}