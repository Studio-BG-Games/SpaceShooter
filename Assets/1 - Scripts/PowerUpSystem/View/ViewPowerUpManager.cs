using System;
using System.Collections.Generic;
using ModelCore;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace PowerUpSystem.View
{
    public class ViewPowerUpManager : MonoBehaviour
    {
        public HorizontalOrVerticalLayoutGroup LayoutGroup;
        public ViewPowerUp PrefabViewPowerUp;
        public Sprite DefaultSpriteOfPowerUp;
        public List<TypeAndIcon> TypeAndIcons;

        private PowerUpManager Manager => _manager ??= EntityAgregator.Instance.Select(x => x.Has<PowerUpManager>()).Select<PowerUpManager>();
        private Dictionary<PowerUpType, Sprite> TypePowerUpAndSprite=>_typePowerUpAndSprite??=CreateDict();
        
        private PowerUpManager _manager;
        private Dictionary<PowerUpType, Sprite> _typePowerUpAndSprite;
        
        [ShowInInspector] private Dictionary<PowerUp, ViewPowerUp> _powerUpsAndViews = new Dictionary<PowerUp, ViewPowerUp>();

        private void Start() => Manager.ResultOfCommand += OnResultManager;

        private void OnResultManager(BaseEventActionWithPowerUpManager obj)
        {
            if (obj is AddNewPowerUp) OnAddBonus(obj as AddNewPowerUp);
            else if (obj is DeletedPowerUpEvent) OnDeleteBonus(obj as DeletedPowerUpEvent);
        }

        private void OnAddBonus(AddNewPowerUp addNewPowerUp)
        {
            var view = Instantiate(PrefabViewPowerUp, LayoutGroup.transform);
            view.Init(addNewPowerUp.NewPowerUp, TypePowerUpAndSprite.TryGetValue(addNewPowerUp.NewPowerUp.TypePowerUp, out var s) ? s : DefaultSpriteOfPowerUp);
            _powerUpsAndViews.Add(addNewPowerUp.NewPowerUp, view);
        }

        private void OnDeleteBonus(DeletedPowerUpEvent deletedPowerUpEvent)
        {
            if (_powerUpsAndViews.TryGetValue(deletedPowerUpEvent.DeletedPowerUp, out var view))
            {
                Destroy(view.gameObject);
                _powerUpsAndViews.Remove(deletedPowerUpEvent.DeletedPowerUp);
            }
        }
        
        private Dictionary<PowerUpType, Sprite> CreateDict()
        {
            Dictionary<PowerUpType, Sprite> result = new Dictionary<PowerUpType, Sprite>();
            foreach (var o in TypeAndIcons)
            {
                if(result.ContainsKey(o.Typ)) continue;
                result.Add(o.Typ, o.Icon);
            }
            return result;
        }
        
        [System.Serializable]
        public class TypeAndIcon
        {
            public PowerUpType Typ;
            public Sprite Icon;
        }
    }
}