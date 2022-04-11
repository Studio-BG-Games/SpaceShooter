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

            var liveData = EntityAgregator.Instance.Select(x => x.Has<Datas>()).Select<Datas>().Save.Live;
            LifeCount.text = liveData.Value.ToString();
            liveData.Updated += x => LifeCount.text = x.ToString();
            
            _infoPath = player.SelectOrCreate<DataPathShip>();
        }

        private void HanlerHp(Entity player)
        {
            var hp = player.Select<Health>();
            hp.ChangedOldNew += (old, n) => HpBar.fillAmount = (float)n / hp.Max;
            HpBar.fillAmount = (float)hp.Current / hp.Max;
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