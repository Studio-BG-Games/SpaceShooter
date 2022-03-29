using UnityEditor;
using UnityEngine;

namespace DIContainer.Editor
{
    [CustomPropertyDrawer(typeof(BindDIScene.ObjectToDi))]
    public class ObjectToDiDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.indentLevel = 3;
            var rects = position.Roww(new[] {0.2f, 1, 3f});
            EditorGUI.PropertyField(rects[0], property.FindPropertyRelative("IsUnbind"), new GUIContent());
            EditorGUI.PropertyField(rects[1], property.FindPropertyRelative("id"), new GUIContent());
            EditorGUI.PropertyField(rects[2], property.FindPropertyRelative("Instance"), new GUIContent());
        }
    }
}