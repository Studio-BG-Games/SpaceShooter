using System.Linq;
using ModelCore;
using Sirenix.Utilities;
using UnityEngine;

namespace Models
{
    public class C_FadeScreen : MonoBehaviour
    {
        public LabelGoSo IdFadeScreens;
        public TargetWindow Load;

        private static C_FadeScreen _instance;

        public static C_FadeScreen Instance => _instance;

        void Start()
        {
            _instance = this;
            var entityWindows = EntityAgregator.Instance.Select(x => x.Select<LabelObjectGo>(x => x.IsAlias(IdFadeScreens)));
            var windows = entityWindows.SelectAll<ScreenModel>(x => true);
            TargetWindows().ForEach(t =>
            {
                var model = windows.First(w => w.Id == t.Id);
                if(model==null) Debug.LogWarning($"Нет fade скрина по id {t.Id}");
                t.Controll.Init(model);
                t.Model = model;
            });
            
            DontDestroyOnLoad(gameObject);
        }

        private TargetWindow[] TargetWindows() => new[] {Load};
    }
}