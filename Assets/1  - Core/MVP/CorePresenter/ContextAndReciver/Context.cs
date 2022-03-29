using System;
using System.Collections.Generic;
using CorePresenter.UniversalPart;
using ModelCore;
using ModelCore.Universal;
using UnityEditorInternal;
using UnityEngine;

namespace CorePresenter.ContextAndReciver
{
    [AddComponentMenu("MV*/Context", 0)][RequireComponent(typeof(P_DebugModel))]
    public class Context : MonoBehaviour
    {
        public RootModel GameModel { get; private set; }

        public static Context Instance { get; private set; }
        public static Context CreateContext(string name, RootModel model)
        {
            if (Instance != null) return Instance;
            
            var obj = new GameObject(name, typeof(Context));
            DontDestroyOnLoad(obj);
            var deb = obj.AddComponent<P_DebugModel>();
            Instance = obj.GetComponent<Context>();
            Instance.GameModel = model;
            deb.Init(Instance.GameModel);
            return Instance;
        }
    }
}