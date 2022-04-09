using DIContainer;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ModelCore
{
    [DisallowMultipleComponent]
    public class EntityScene : BaseEntityContext
    {
        public static EntityScene Instance { get; private set; }

        [DI] public void DiInit()
        {
            if (Instance == null) Instance = this;
            else Debug.LogError("Вы пытайтесь создать второй контекс приложения, так нельзя");
        }

        private void OnDestroy() => Instance = null;
        
        [Button] private void OnValidate()
        {
            gameObject.name = nameof(EntityScene);
        }
    }
}