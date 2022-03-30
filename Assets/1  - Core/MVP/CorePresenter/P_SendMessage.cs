using System;
using ModelCore;
using Newtonsoft.Json;
using UnityEngine;

namespace CorePresenter
{
    public class P_SendMessage : Presenter, ISerializationCallbackReceiver
    {
        private Model _model;
        [SerializeField][HideInInspector] private string __save;
        [SerializeField] public Message Message;

        public override void Init(RootModel rootModel)
        {
            if (string.IsNullOrWhiteSpace(PathToModel)) _model = rootModel;
            _model = GetModel<Model>(rootModel, x=>x.IdModel==PathToModel);
        }

        [ContextMenu("Send")]
        public void Send()
        {
            if(Message!=null) _model?.SendMessage(Message);
        }
        
        private static JsonSerializerSettings SettingJson()
        {
            return new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects,
                PreserveReferencesHandling  = PreserveReferencesHandling.Objects,
            };
        }

        public void OnBeforeSerialize()
        {
            // to str
            if(Message==null) return;
            __save = JsonConvert.SerializeObject(Message, SettingJson());
        }

        public void OnAfterDeserialize()
        {
            // to object
            if(string.IsNullOrWhiteSpace(__save)) return;
            try
            {
                Message = JsonConvert.DeserializeObject<Message>(__save, SettingJson());
            }
            catch(Exception e)
            {
                Message = null;
            }
        }
    }
}