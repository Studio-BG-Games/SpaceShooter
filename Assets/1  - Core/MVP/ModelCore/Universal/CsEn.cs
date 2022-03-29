using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using DIContainer;
using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine;

namespace ModelCore.Universal
{
    public abstract class CsEn : Model
    {
        [JsonIgnore] public override string IdModel { get
            {
                var attribute = GetType().GetCustomAttribute<CustomId>();
                if (attribute == null)
                    return Prefics + GetType().Name;
                else
                    return Prefics + attribute.Id;
            } }

        [JsonIgnore] public override string Prefics => "Cs_";

        public CsEn() { }

        public T Get<T>() where T : CsEn => this as T;

        public bool Get<T>(out T result) where T : CsEn => (result = Get<T>()) is T;

        public bool Is<T>() where T : CsEn => this is T;

        protected override sealed void FinalRenane(string newName) { }

        public virtual string Valid() => "";
    }
}