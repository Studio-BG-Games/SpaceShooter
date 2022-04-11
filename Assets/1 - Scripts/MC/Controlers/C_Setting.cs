using System;
using ModelCore;
using Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace MC.Controlers
{
    public class C_Setting : MonoBehaviour
    {
        public Slider Master;
        public Slider Effect;
        public Slider Music;
        private Datas _data;

        private const string PathSave = "Setting";

        private void Start()
        {
            _data = EntityAgregator.Instance.Select(x => x.Has<Datas>()).Select<Datas>();


            var loadSetting = JsonConvert.DeserializeObject<SettingData>(PlayerPrefs.GetString(PathSave, ""));
            if (loadSetting != null) _data.Setting = loadSetting;
            else SaveSetting();
            
            
            InitSlider(Master, _data.Setting.MasterSound);
            InitSlider(Effect, _data.Setting.EffectSound);
            InitSlider(Music, _data.Setting.MusicSound);
        }

        private void SaveSetting() => PlayerPrefs.SetString(PathSave, JsonConvert.SerializeObject(_data.Setting));

        private void InitSlider(Slider slider, ObjectValue<float> refValue)
        {
            slider.value = refValue.Value;
            slider.onValueChanged.AddListener(x => refValue.Value = x);
        }

        private void OnDestroy() => SaveSetting();
    }
}