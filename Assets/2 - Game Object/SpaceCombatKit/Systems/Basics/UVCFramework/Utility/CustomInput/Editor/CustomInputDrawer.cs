using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class draws the inspector for a CustomInput class instance.
    /// </summary>
    [CustomPropertyDrawer(typeof(CustomInput))]
    public class CustomInputDrawer : PropertyDrawer
    {

        private float itemHeight = 16;
        private float borderHeight = 3;
        private float spacing = 2;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {

            int numItems = 5;

            CustomInputType inputType = (CustomInputType)property.FindPropertyRelative("inputType").intValue;
            if (inputType == CustomInputType.Axis)
            {
                numItems = 6;
            }
            return itemHeight * numItems + borderHeight * 2 + spacing * (numItems - 1);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            EditorGUI.BeginProperty(position, label, property);

            GUI.Box(position, GUIContent.none);

            Rect nextPropertyRect = new Rect(position.x, position.y + borderHeight, position.width, itemHeight);
            EditorGUI.LabelField(nextPropertyRect, label);

            nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight + spacing, position.width, itemHeight);
            EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("group"));

            nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * 2 + spacing * 2, position.width, itemHeight);
            EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("action"));

            nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * 3 + spacing * 3, position.width, itemHeight);
            EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("inputType"));

            CustomInputType inputType = (CustomInputType)property.FindPropertyRelative("inputType").intValue;

            switch (inputType)
            {
                case CustomInputType.Key:

                    nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * 4 + spacing * 4, position.width, itemHeight);
                    EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("key"));
                    break;

                case CustomInputType.MouseButton:

                    nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * 4 + spacing * 4, position.width, itemHeight);
                    EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("mouseButton"));
                    break;

                case CustomInputType.Axis:

                    nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * 4 + spacing * 4, position.width, itemHeight);
                    EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("getAxisRaw"));

                    nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * 5 + spacing * 5, position.width, itemHeight);
                    EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("axis"));
                    break;

            }

            EditorGUI.EndProperty();
        }
    }
}
