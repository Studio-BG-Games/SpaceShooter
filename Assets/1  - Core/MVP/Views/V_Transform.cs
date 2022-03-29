using System;
using System.Collections.Generic;
using CorePresenter;
using ModelCore;
using ModelCore.Universal.AliasValue;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace MVP.Views
{
    [AddComponentMenu(RootPresenter.PathToView+"V_Transform")]
    public class V_Transform : ViewRootBase
    {
        public Transform Target;
        [TabGroup("Position"), HideLabel] public PosPart Pos = new PosPart();
        [TabGroup("Rotation"), HideLabel] public RotPart Rot = new RotPart();
        [TabGroup("Scale"), HideLabel] public ScalePart Scale = new ScalePart();
        
        private RootModel _model;
        private V_PartTransform[] parts;
        private List<Action> UpdateActions=new List<Action>();
        private List<Action> UnsubcribeCallbcal = new List<Action>();
        private Dictionary<V_PartTransform, Vector3> dictVect = new Dictionary<V_PartTransform, Vector3>();

        protected override void CustomAwake()
        {
            parts = new V_PartTransform[] {Pos, Rot, Scale};
            parts.ForEach(x => dictVect.Add(x, Vector3.zero));
        }

        public override void View(RootModel engine)
        {
            _model = engine;
            if(_model!=null)
                Subscribe(_model);
        }

        private void Subscribe(RootModel model)
        {
            UnsubcribeCallbcal.ForEach(x => x.Invoke());
            UnsubcribeCallbcal.Clear();
            UpdateActions.Clear();
            parts.ForEach(x=>
            {
                if (x.IsOn == false) return;
                AliasVector3 vector3 = model.Select<AliasVector3>(y => y.Alias == x.AliasVector3);
                if (vector3 == null)
                {
                    Debug.LogWarning($"У меня нет представления вектора {x.AliasVector3}", this);
                    return;
                }

                if(x.ReadFromModel) vector3.Update += Handler(x);
                if(x.WriteToModel) UpdateActions.Add(new Action(() =>
                {
                    var current = Get(x.AliasVector3);
                    if (dictVect[x] != current)
                    {
                        dictVect[x] = current;
                        vector3.Value = current;
                    }
                }));
                
                UnsubcribeCallbcal.Add(()=>vector3.Update-=Handler(x));
            });
        }

        private void Update() => UpdateActions.ForEach(x => x.Invoke());

        private Action<Vector3, Vector3> Handler(V_PartTransform vPartTransform) 
        { return (past, nyw) => { if (past != nyw) Set(vPartTransform.AliasVector3, nyw); }; }


        private void Set(string alias, Vector3 forSet)
        {
            switch (alias)
            {
                case "Position": Target.position = forSet;
                    break;
                case "Rotation": Target.eulerAngles = forSet;
                    break;
                case "Scale": Target.localScale = forSet;
                    break;
            }
        }

        protected override string GetInfo()
        {
            return $"V3_Position, V3_ROtation, V3_Scale";
        }

        private Vector3 Get(string alias)
        {
            switch (alias)
            {
                case "Position": return Target.position;
                case "Rotation": return Target.eulerAngles;
                case "Scale": return Target.localScale;
                default: return Vector3.zero;
            }
        }
        
        [Serializable]
        public abstract class V_PartTransform
        {
            public abstract string AliasVector3 { get; }
            public bool IsOn;
            public bool ReadFromModel;
            public bool WriteToModel;
        }
        
        [Serializable]
        public class PosPart : V_PartTransform
        {
            public override string AliasVector3 => "Position";
        }
        
        [Serializable]
        public class RotPart : V_PartTransform
        {
            public override string AliasVector3 => "Rotation";
        }
        
        [Serializable]
        public class ScalePart : V_PartTransform
        {
            public override string AliasVector3 => "Scale";
        }
    }
}