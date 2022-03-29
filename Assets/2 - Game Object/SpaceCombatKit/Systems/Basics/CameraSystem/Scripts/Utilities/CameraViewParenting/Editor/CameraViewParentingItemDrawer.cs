using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace VSX.CameraSystem
{
    [CustomPropertyDrawer(typeof(CameraViewParentingItem))]
    public class CameraViewParentingItemDrawer : PropertyDrawer
    {
        private float itemHeight = 16;
        private float borderHeight = 3;
        private float spacing = 2;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {

            int numItems = 6;

            if (property.FindPropertyRelative("cameraViews").isExpanded)
            {
                numItems += property.FindPropertyRelative("cameraViews").arraySize + 1;
            }

            if ((CameraViewParentType)property.FindPropertyRelative("parentType").intValue == CameraViewParentType.Transform) numItems += 1;

            if (property.FindPropertyRelative("setLocalPosition").boolValue) numItems += 1;
            if (property.FindPropertyRelative("setLocalRotation").boolValue) numItems += 1;
            if (property.FindPropertyRelative("setLocalScale").boolValue) numItems += 1;

            return itemHeight * numItems + borderHeight * 2 + spacing * (numItems - 1);

        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            EditorGUI.BeginProperty(position, label, property);

            GUI.Box(position, GUIContent.none);

            int itemIndex = 0;

            Rect nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * itemIndex + spacing * itemIndex, position.width, itemHeight);
            EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("m_Transform"));

            itemIndex += 1;

            nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * itemIndex + spacing * itemIndex, position.width, itemHeight);
            EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("cameraViews"), true);

            itemIndex += property.FindPropertyRelative("cameraViews").isExpanded ? property.FindPropertyRelative("cameraViews").arraySize + 2 : 1;

            nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * itemIndex + spacing * itemIndex, position.width, itemHeight);
            EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("parentType"));

            if ((CameraViewParentType)property.FindPropertyRelative("parentType").intValue == CameraViewParentType.Transform)
            {
                itemIndex += 1;

                nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * itemIndex + spacing * itemIndex, position.width, itemHeight);
                EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("parentTransform"));
            }

            itemIndex += 1;

            nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * itemIndex + spacing * itemIndex, position.width, itemHeight);
            EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("setLocalPosition"));

            if (property.FindPropertyRelative("setLocalPosition").boolValue)
            {
                itemIndex += 1;

                nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * itemIndex + spacing * itemIndex, position.width, itemHeight);
                EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("localPosition"));
            }

            itemIndex += 1;

            nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * itemIndex + spacing * itemIndex, position.width, itemHeight);
            EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("setLocalRotation"));

            if (property.FindPropertyRelative("setLocalRotation").boolValue)
            {
                itemIndex += 1;

                nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * itemIndex + spacing * itemIndex, position.width, itemHeight);
                EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("localRotationEulerAngles"));
            }

            itemIndex += 1;

            nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * itemIndex + spacing * itemIndex, position.width, itemHeight);
            EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("setLocalScale"));

            if (property.FindPropertyRelative("setLocalScale").boolValue)
            {
                itemIndex += 1;

                nextPropertyRect = new Rect(position.x, position.y + borderHeight + itemHeight * itemIndex + spacing * itemIndex, position.width, itemHeight);
                EditorGUI.PropertyField(nextPropertyRect, property.FindPropertyRelative("localScale"));
            }


        }
    }
}

