using System;
using System.Linq;
using ModelCore;
using ModelCore.Universal;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace CorePresenter.Editor
{
    [CustomEditor(typeof(P_SendMessage))]
    public class EditorP_SendMessage : UnityEditor.Editor
    {
        private static string Path = "Assets/1  - Core/MVP/Editor/UXML/ModelViews/Parts/View_P_SendMessage.uxml";
        private static Type[] Types = new Type[0];
        private P_SendMessage sender;
        private VisualElement _tree;

        private void OnEnable() => sender = (P_SendMessage)target;

        public override VisualElement CreateInspectorGUI()
        {
            var  root = new VisualElement();
            
            _tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path).Instantiate();
            root.Add(_tree);
            _tree.Q<Button>("SelectMessage").clickable.clicked += SelectMessage;
            Show();
            root.Add(new IMGUIContainer(()=>DrawDefaultInspector()));
            return root;
        }

        
        [InitializeOnLoadMethod]
        private static void RefreshAllMessage() => Types = typeof(Message).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Message)) && !x.IsAbstract).ToArray();

        private void Show()
        {
            var panel = _tree.Q("PanelMessageEditor");
            panel.hierarchy.Clear();
            if (sender.Message == null)
                panel.Add(new Label("No massege =("));
            else
            {
                RootEditor.ViewCsEn.SpawnFields(panel, sender.Message, () =>
                {
                    EditorUtility.SetDirty(sender);
                    sender.Message.Valid();
                });
                
                var id = sender.Message.GetType().GetCustomAttribute<CustomId>();
                var info = sender.Message.GetType().GetCustomAttribute<Info>();
                
                var labelName = new Label(id==null ? $"{sender.Message.GetType().Name}" : $"{id.Id} : ({sender.Message.GetType().Name})");
                
                panel.Add(labelName);
                if(info!=null) panel.Add(new Label(info.Value));
            }
        }
        
        private void SelectMessage()
        {
            SearchCsModelScript.Show(Types, x=>
            {
                
                sender.Message = (Message)Activator.CreateInstance(x);
                Debug.Log(sender.Message);
                Show();
            });
        }
    }
}