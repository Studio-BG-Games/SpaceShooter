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
        private CompositeInput _composite;

        public override void Create(DiBox container)
        {
            IInput inputForRegister = null;
            if (InpInEdit == InputInEditor.Bouth)
            {
                _composite = new CompositeInput(new IInput[]{ Pc, Mobile});
                inputForRegister = _composite;
            }
            else if (IsDesctop() || IsEditorKeyborad())
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

        private void Update()
        {
            _composite?.UpdateCustom();
        }

        public override void DestroyDi(DiBox container)
        {
            container.RemoveSingel<IInput>();
        }

        private bool IsEditorKeyborad() => Application.isEditor && InpInEdit == InputInEditor.KeyboardAndMouse;

        private static bool IsDesctop() => Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer;

        public enum InputInEditor
        {
            Screen, KeyboardAndMouse, Bouth
        }
    }
}