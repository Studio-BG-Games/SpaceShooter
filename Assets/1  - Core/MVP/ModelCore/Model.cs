using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace ModelCore
{
    public abstract class Model
    {
	    [JsonProperty] public RootModel Root { get; private set; }
        [JsonIgnore] public abstract string IdModel { get; }
        [JsonIgnore] public abstract string Prefics { get; }

        public Model() {}
        
        public void MoveToNewRoot(RootModel newRootModel)
        {
            if (Root != newRootModel && newRootModel != null)
            {
                if (Root != null)
                {
                    PreDeleteFromPrevRoot(Root, newRootModel);
                    Root.DeleteT<Model>(x=>x==this);
                }
                Root = newRootModel;
                Root.AddModel(this);
                AfterAddToNewModel(Root);
            }
        }
        
        public virtual void CopyFrom(Model other){}

        public bool Rename(string newName)
        {
            if (Root != null)
            {
                if (!Root.CanRename(this, newName)) return false;
                FinalRenane(newName);
                Root.UpdateNames();
                return true;
            }
            FinalRenane(newName);
            return true;
        }
        
        public Model Clone() => JsonConvert.DeserializeObject<Model>(Save(), RootModel.Factory.SettingJson());

        public string Save()
        {
            var prevRoot = Root;
            Root = null;
            var result = JsonConvert.SerializeObject(this, RootModel.Factory.SettingJson());
            Root = prevRoot;
            return result;
        }

        protected abstract void FinalRenane(string newName);

        public virtual void InitByModel() { }

        protected virtual void PreDeleteFromPrevRoot(RootModel old, RootModel @new) { }
        
        protected virtual void AfterAddToNewModel(RootModel @new) {}
    }
}