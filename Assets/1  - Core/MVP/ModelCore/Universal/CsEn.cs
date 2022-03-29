using System;
using System.Security.Claims;
using DIContainer;
using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine;

namespace ModelCore.Universal
{
    public class CsEn : Model
    {
        [JsonIgnore] public override string IdModel { get
            {
                var attribute = Script.GetType().GetCustomAttribute<CustomId>();
                if (attribute == null)
                    return Prefics + Script.GetType().Name;
                else
                    return Prefics + attribute.Id;
            } }

        [JsonIgnore] public override string Prefics => "Cs_";

        [JsonProperty] public BaseScript Script { get; private set; }

        public CsEn(BaseScript script) => Script = script;

        public T Get<T>() where T : BaseScript => Script as T;

        public bool Get<T>(out T result) where T : BaseScript => (result = Get<T>()) is T;

        public bool Is<T>() where T : BaseScript => Script is T;

        protected override void FinalRenane(string newName) => Root.Logger.LogWarning("Я не могу переименоваться");

        public override void InitByModel() => Script.Init();

        protected override void PreDeleteFromPrevRoot(RootModel old, RootModel @new) => Script.PreDeleteFromPrevRoot();

        protected override void AfterAddToNewModel(RootModel @new) => Script.AfterAddToNewModel();

        public class BaseScript
        {
            public BaseScript(){}
            public virtual void Init(){}
            public virtual void PreDeleteFromPrevRoot(){}
            public virtual void AfterAddToNewModel(){}
            public virtual string Valid() => "Всё нама";
        }
    }

    [CustomId("Custom name")]
    [CustomPath("Folder 1/Folder2")]
    [Info("Example C# script. Эта информация взята из аттрибута")]
    public class ExampleSctipt : CsEn.BaseScript
    {
        public float publicF;
        public int publicI;
        public string publicS;
        public bool publicB;
        public Vector3 v3;
        public Vector2 v2;
        public SomeSctruct CustomStruct;
        [JsonProperty] private float privateJsonF;
        public MyEnum TestEnum = MyEnum.First;
        private float privateF;

        public void PublicM()
        {
            
        }

        public void MethodWithArgument(float t, string y)
        {
            
        }
        
        public enum MyEnum
        {
            First, Second, Third
        }
    }

    public struct SomeSctruct
    {
        public int a;
        public int B;
    }


    public class ExampleScript2 : CsEn.BaseScript
    {
        
    }
}