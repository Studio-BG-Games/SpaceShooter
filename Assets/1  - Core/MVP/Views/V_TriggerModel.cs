using System;
using System.Collections.Generic;
using CorePresenter;
using Jint;
using ModelCore;
using ModelCore.Universal;
using PartsPresenters.FinderRootModel;
using UnityEngine;

namespace MVP.Views
{
    [AddComponentMenu(RootPresenter.PathToView+"V_View Trigger")][RequireComponent(typeof(FinderRootModelTrigger))]
    public class V_TriggerModel : ViewBase<JsEn>
    {
        private FinderRootModelTrigger Trigger => _trigger ??= GetComponent<FinderRootModelTrigger>();
        private FinderRootModelTrigger _trigger;
        
        private List<Action> Unsubscribes = new List<Action>();

        public string PathToObject;
        
        public override void View(JsEn engine)
        {
            Unsubscribes.ForEach(x=>x.Invoke());
            Unsubscribes.Clear();
            UpdateField(engine, Trigger.Enter, PathToObject+".Enter");
            UpdateField(engine, Trigger.Stay, PathToObject+".Stay");
            UpdateField(engine, Trigger.Exit, PathToObject+".Exit");
        }
        
       
        private void UpdateField(JsEn eng, FinderRootModelTrigger.SearchGroup group, string funcPath)
        {
            var hasFunc = eng.HasFunc(funcPath);
            group.IsOn = hasFunc;
            if (hasFunc)
            {
                group.Finded.DynamicCalls+=InvokeFunc(eng, funcPath);
                Unsubscribes.Add(new Action(()=>group.Finded.DynamicCalls-=InvokeFunc(eng, funcPath)));
            }
        }

        private Action<RootModel> InvokeFunc(JsEn en, string path) => x => en.InvokeParam(path, x);
    }
}