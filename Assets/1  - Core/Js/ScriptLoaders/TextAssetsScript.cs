﻿using Jint;
 using Js.Interfaces;
 using UnityEngine;

namespace Js.ScriptLoaders
{
    public class TextAssetsScript : AbsScriptLoader
    {
        [SerializeField] private TextAsset _textAsset;
        
        public override void SetScript(Engine engine) => engine.Execute(_textAsset.text);
    }
}