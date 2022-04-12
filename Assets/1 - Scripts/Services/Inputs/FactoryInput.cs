using System;
using DIContainer;
using Services.Inputs;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Services.Bind
{
    public class FactoryInput : FactoryDI
    {
        [InfoBox("Регистрируется только один Input, другой уничтожается")]
        public PcInput Pc;
        public MobileInput Mobile;
        
        public InputInEditor InpInEdit;

        public override void Create(DiBox container)
        {
            IInput inputForRegister = null;
            if (IsDesctop() || IsEditorKeyborad())
            {
                inputForRegister = Pc;
                Destroy(Mobile.gameObject);   
            }
            else
            {
                inputForRegister = Mobile;
                Destroy(Pc.gameObject);
            }
            
            container.RegisterSingle<IInput>(inputForRegister);
        }

        public override void DestroyDi(DiBox container)
        {
            container.RemoveSingel<IInput>();
        }

        private bool IsEditorKeyborad() => Application.isEditor && InpInEdit == InputInEditor.KeyboardAndMouse;

        private static bool IsDesctop() => Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer;

        public enum InputInEditor
        {
            Screen, KeyboardAndMouse
        }
    }
}