using DIContainer;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ModelCore
{
    [DisallowMultipleComponent]
    public class EntityApp : BaseEntityContext
    {
        public static EntityApp Instance { get; private set; }
        private static bool _isInit = false;

        [DI] public void DiInit()
        {
            if(_isInit) return;
            if (Instance == null) Instance = this;
            else Debug.LogError("Вы пытайтесь создать второй контекс приложения, так нельзя");
            DontDestroyOnLoad(gameObject);
            _isInit = true;
        }

        [Button] private void OnValidate()
        {
            gameObject.name = nameof(EntityApp);
        }
    }
}