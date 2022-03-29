using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace DIContainer.Editor
{
    [CustomEditor(typeof(InjectDI))]
    public class InjectDiEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(GUILayout.Button("Show Single")) ViewDi.Show();
        }

    }

    public class ViewDi : EditorWindow
    {
        [MenuItem("MVP/DI/Show register single")]
        public static void Show() => GetWindow<ViewDi>().ShowAuxWindow();

        public void CreateGUI()
        {
            if (!Application.isPlaying)
            {
                var labelError = new Label("Работаю только в play mode");
                labelError.style.fontSize = 15;
                labelError.style.color = new StyleColor(Color.red);
                rootVisualElement.Add(labelError);
                return;
            }
            var label = new Label("Список Single объектов в DI");
            label.style.color = new StyleColor(new Color(0.19f, 0.04f, 0.49f));
            label.style.fontSize = 15;
            rootVisualElement.Add(label);
            var border = new VisualElement();
            border.style.height = 6;
            border.style.backgroundColor = new StyleColor(new Color(0.19f, 0.19f, 0.19f));
            rootVisualElement.Add(border);
            DiBox.MainBox.GetAllSingle().ForEach(type => 
                type.Value.ForEach(id =>
                {
                    var label = new Label($"Type - {type.Key.Name}; Id - '{id.Key}'");
                    label.style.fontSize = 12;
                    rootVisualElement.Add(label);
                }));
        }
    }
}