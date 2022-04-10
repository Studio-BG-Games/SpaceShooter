using System;
using DefaultNamespace;
using MC.Models;
using ModelCore;
using Models;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MC.Controlers
{
    public class C_PlayerShipUI : MonoBehaviour
    {
        [SerializeField] private Image HpBar;
        [SerializeField] private Image LevelProgress;
        [SerializeField] private SliderPoint HorizontalSlider;
        [SerializeField] private SliderPoint VerticalSlider;
        [SerializeField] private TextMeshProUGUI LifeCount;
        
        private DataPathShip _infoPath;

        public void Init(Entity player)
        {
            HanlerHp(player);

            LifeCount.text = EntityAgregator.Instance.Select(x => x.Has<Datas>()).Select<Datas>().Save.Live.ToString();
            
            _infoPath = player.SelectOrCreate<DataPathShip>();
        }

        private void HanlerHp(Entity player)
        {
            var hp = player.Select<Health>();
            hp.ChangedOldNew += (old, n) => HpBar.fillAmount = n / hp.Max;
            HpBar.fillAmount = hp.Current / hp.Max;
        }

        private void Update()
        {
            if (_infoPath==null) return;
            HanlerInfoShip();
        }

        private void HanlerInfoShip()
        {
            LevelProgress.fillAmount = (float) (_infoPath.Progress / 1);
            HorizontalSlider.SetPoint(_infoPath.OffsetInNomal.x);
            VerticalSlider.SetPoint(_infoPath.OffsetInNomal.y);
        }
    }
}