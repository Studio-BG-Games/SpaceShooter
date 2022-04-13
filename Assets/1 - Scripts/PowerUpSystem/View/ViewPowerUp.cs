using System;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace PowerUpSystem.View
{
    public class ViewPowerUp : MonoBehaviour
    {
        public Image[] SpriteOfBonus;
        public Image[] ProgressLiveOfBonus;
        private PowerUp _bonus;

        public void Init(PowerUp bonus, Sprite sprite)
        {
            _bonus = bonus;
            SpriteOfBonus.ForEach(x => x.sprite = sprite);
            _bonus.StateUpdated += OnUpdateStatusBonus;
        }

        private void OnDestroy() => _bonus.StateUpdated += OnUpdateStatusBonus;

        private void OnUpdateStatusBonus()
        {
            ProgressLiveOfBonus.ForEach(x => x.fillAmount = 1 - _bonus.LifeProgressInNormal);
        }
    }
}