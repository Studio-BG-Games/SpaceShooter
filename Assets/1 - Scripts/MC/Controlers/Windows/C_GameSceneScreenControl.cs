using System;
using System.Linq;
using Cinemachine;
using ModelCore;
using Sirenix.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Models
{
    public class C_GameSceneScreenControl : MonoBehaviour
    {
        public Object IdWindowsEntity;
        public TargetWindow WinScreen;
        public TargetWindow GameHud;
        public TargetWindow LoseScreen;
        public TargetWindow PauseWin;

        private TargetWindow[] TargetWindows() => new[] { WinScreen, GameHud, LoseScreen, PauseWin};
        
        private void Awake()
        {
            var entityWindows = EntityAgregator.Instance.Select(x => x.Label.IsAlias(IdWindowsEntity));
            var windows = entityWindows.SelectAll<ScreenModel>(x => true);
            TargetWindows().ForEach(t =>
            {
                var model = windows.First(w => w.Id == t.Id);
                t.Controll.Init(model);
                t.Model = model;
            });
        }
    }
}