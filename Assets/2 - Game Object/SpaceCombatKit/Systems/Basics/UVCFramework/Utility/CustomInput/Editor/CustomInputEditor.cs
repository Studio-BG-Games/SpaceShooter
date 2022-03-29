using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
namespace VSX.UniversalVehicleCombat
{

    [CustomEditor(typeof(CustomInput))]
    [CanEditMultipleObjects]
    public class CustomInputEditor : Editor
    {

        SerializedProperty groupProperty;
        SerializedProperty actionProperty;
        SerializedProperty inputTypeProperty;
        SerializedProperty keyProperty;
        SerializedProperty mouseButtonProperty;
        SerializedProperty getAxisRawProperty;
        SerializedProperty axisProperty;


        private void OnEnable()
        {
            groupProperty = serializedObject.FindProperty("group");
            actionProperty = serializedObject.FindProperty("action");
            inputTypeProperty = serializedObject.FindProperty("inputType");
            keyProperty = serializedObject.FindProperty("key");
            mouseButtonProperty = serializedObject.FindProperty("mouseButton");
            getAxisRawProperty = serializedObject.FindProperty("getAxisRaw");
            axisProperty = serializedObject.FindProperty("axis");
        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            EditorGUILayout.PropertyField(groupProperty);
            EditorGUILayout.PropertyField(actionProperty);
            EditorGUILayout.PropertyField(inputTypeProperty);

            InputType inputType = (InputType)inputTypeProperty.enumValueIndex;
            switch (inputType)
            {
                case InputType.Key:
                    EditorGUILayout.PropertyField(keyProperty);
                    break;
                case InputType.MouseButton:
                    EditorGUILayout.PropertyField(mouseButtonProperty);
                    break;
                case InputType.Axis:
                    EditorGUILayout.PropertyField(getAxisRawProperty);
                    EditorGUILayout.PropertyField(axisProperty);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
*/