using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;
using UnityEditor;
using System.Reflection;
using System;

[CustomPropertyDrawer(typeof(Condition))]
public class ConditionDrawer : PropertyDrawer
{
    private float itemHeight = 16;
    private int spacing = 2;

    private int numItems = 6;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {

        SerializedProperty linkableVariableProperty = property.FindPropertyRelative("condition");
        SerializedProperty numArgsProperty = linkableVariableProperty.FindPropertyRelative("numArgs");
        int numArgs = numArgsProperty.intValue;

        int numItemsCurrent = numItems + numArgs;

        if (linkableVariableProperty.FindPropertyRelative("targetObject").objectReferenceValue == null)
        {
            numItemsCurrent -= 4;
        }

        return itemHeight * numItemsCurrent + spacing * 2 + spacing * (numItemsCurrent - 1);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        SerializedProperty linkableVariableProperty = property.FindPropertyRelative("condition");

        SerializedProperty isLinkedVariableProperty = linkableVariableProperty.FindPropertyRelative("isLinkedVariable");
        isLinkedVariableProperty.boolValue = true;

        // Draw box background
        Rect boxRect = position;
        boxRect.x = position.x + EditorGUI.indentLevel * 10;
        GUI.Box(boxRect, GUIContent.none);

        // Keep a running reference to the next GUI height position, because checkboxes on/off will change the position of things
        Rect runningPosition = position;
        runningPosition.height = itemHeight;
        runningPosition.y += spacing;

        EditorGUI.LabelField(runningPosition, label, EditorStyles.boldLabel);

        runningPosition.y += itemHeight + spacing;

        // Keep a reference to the suffix position for items preceeded by a label.
        Rect suffixPosition;

        // Get property - target component
        SerializedProperty targetComponentProperty = linkableVariableProperty.FindPropertyRelative("targetComponent");
        
        // Draw property - target object
        SerializedProperty targetObjectProperty = linkableVariableProperty.FindPropertyRelative("targetObject");
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(runningPosition, targetObjectProperty);
        bool targetObjectChanged = EditorGUI.EndChangeCheck();

        // Draw the components options menu for the target object
        if (targetObjectProperty.objectReferenceValue != null)
        {

            GameObject targetGameObject = targetObjectProperty.objectReferenceValue as GameObject;
            Component targetObjectComponent = targetObjectProperty.objectReferenceValue as Component;

            if (targetGameObject == null)
            {
                if (targetObjectComponent != null)
                {
                    targetGameObject = targetObjectComponent.gameObject;
                }
            }

            List<Component> components = new List<Component>(targetGameObject.GetComponents<Component>());

            List<UnityEngine.Object> componentObjects = new List<UnityEngine.Object>();
            componentObjects.Add(targetGameObject);
            for (int i = 0; i < components.Count; ++i)
            {
                componentObjects.Add(components[i]);
            }


            int selectedComponentIndex = -1;

            selectedComponentIndex = componentObjects.IndexOf(targetComponentProperty.objectReferenceValue);
            string buttonLabel;
            if (selectedComponentIndex == -1 || targetObjectChanged)
            {
                if (targetObjectComponent != null && components.IndexOf(targetObjectComponent) != -1)
                {
                    buttonLabel = targetObjectComponent.GetType().Name;
                    targetComponentProperty.objectReferenceValue = targetObjectComponent;
                }
                else
                {
                    buttonLabel = "No Component Selected";
                    targetComponentProperty.objectReferenceValue = null;
                }
            }
            else
            {
                buttonLabel = componentObjects[selectedComponentIndex].GetType().Name;
            }

            runningPosition.y += itemHeight + spacing;

                
            // Draw component options menu button
            suffixPosition = EditorGUI.PrefixLabel(runningPosition, new GUIContent("Component"));
            suffixPosition = runningPosition;
            suffixPosition.x = EditorGUIUtility.labelWidth + 14;
            suffixPosition.width = runningPosition.width - EditorGUIUtility.labelWidth;
                
            if (GUI.Button(suffixPosition, new GUIContent(buttonLabel)))
            {
                GenericMenu componentMenu = new GenericMenu();

                componentMenu.AddItem(new GUIContent("None"), false, () =>
                {
                    property.serializedObject.Update();
                    targetComponentProperty.objectReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                });

                foreach (UnityEngine.Object obj in componentObjects)
                {
                    string menuLabel = obj.GetType().Name;
                    componentMenu.AddItem(new GUIContent(obj.GetType().Name), false, () =>
                    {
                        property.serializedObject.Update();
                        targetComponentProperty.objectReferenceValue = obj;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }

                componentMenu.ShowAsContext();

            }

            // Get the target component as an object
            UnityEngine.Object targetObject = targetComponentProperty.objectReferenceValue;

            // Get all methods
            MethodInfo[] methodInfos = new MethodInfo[] { };
            if (targetObject != null)
            {
                methodInfos = targetObject.GetType().GetMethods();
            }

            // Get all the compatible methods
            List<MethodInfo> compatibleMethodInfos = GetMethodInfos(methodInfos, typeof(bool).AssemblyQualifiedName);

            // Get the number of arguments of the currently selected method on the object
            SerializedProperty numArgsProperty = linkableVariableProperty.FindPropertyRelative("numArgs");
            if (targetObject == null)
            {
                numArgsProperty.intValue = 0;
            }

            // Get information about the currently selected method
            SerializedProperty methodInfoNameProp = linkableVariableProperty.FindPropertyRelative("methodInfoName");
            SerializedProperty arg0ObjectProperty = linkableVariableProperty.FindPropertyRelative("arg0ObjectValue");
            SerializedProperty argo0TypeProperty = linkableVariableProperty.FindPropertyRelative("arg0Type");

            // Get index of currently selected method
            int selectedMethodIndex = -1;

            // Get index of currently selected method
            if (targetObject != null)
            {
                MethodInfo selectedMethodInfo = null;

                // Get the currently selected method
                if (numArgsProperty.intValue == 1 && Type.GetType(argo0TypeProperty.stringValue) != null)
                {
                    selectedMethodInfo = targetObject.GetType().GetMethod(methodInfoNameProp.stringValue, new Type[] { Type.GetType(argo0TypeProperty.stringValue) });
                }
                else
                {
                    selectedMethodInfo = targetObject.GetType().GetMethod(methodInfoNameProp.stringValue, new Type[] { });
                }

                // Update the selected index
                if (selectedMethodInfo != null)
                {
                    selectedMethodIndex = compatibleMethodInfos.IndexOf(selectedMethodInfo);
                }
            }
            if (selectedMethodIndex == -1 && compatibleMethodInfos.Count > 0) selectedMethodIndex = 0;


            // Get the display info for each of the compatible methods on the currently selected object
            List<string> methodDisplayNames = new List<string>();
            for (int i = 0; i < compatibleMethodInfos.Count; ++i)
            {
                string s = compatibleMethodInfos[i].Name;
                if (s.StartsWith("get_"))
                {
                    s = s.Substring("get_".Length);
                    s += " (get)";
                }
                if (compatibleMethodInfos[i].GetParameters().Length > 0)
                {
                    s += "(";
                    s += compatibleMethodInfos[i].GetParameters()[0].ParameterType.Name.ToString();
                    s += ")";
                }
                methodDisplayNames.Add(s);
            }

            if (targetObjectProperty.objectReferenceValue != null)
            {
                // Display the methods on the selected object
                runningPosition.y += itemHeight + spacing;
                suffixPosition = EditorGUI.PrefixLabel(runningPosition, new GUIContent("Function"));
                suffixPosition = runningPosition;

                int margin = (14 - EditorGUI.indentLevel * 15);
                suffixPosition.x = EditorGUIUtility.labelWidth + margin;
                suffixPosition.width = runningPosition.width - EditorGUIUtility.labelWidth + EditorGUI.indentLevel * 15;
                selectedMethodIndex = EditorGUI.Popup(suffixPosition, selectedMethodIndex, methodDisplayNames.ToArray());

                // Update the argument type for the method 
                if (selectedMethodIndex != -1)
                {
                    methodInfoNameProp.stringValue = compatibleMethodInfos[selectedMethodIndex].Name;
                    numArgsProperty.intValue = compatibleMethodInfos[selectedMethodIndex].GetParameters().Length;
                    if (compatibleMethodInfos[selectedMethodIndex].GetParameters().Length > 0)
                    {
                        linkableVariableProperty.FindPropertyRelative("arg0Type").stringValue = compatibleMethodInfos[selectedMethodIndex].GetParameters()[0].ParameterType.AssemblyQualifiedName;
                    }
                }
                else
                {
                    numArgsProperty.intValue = 0;
                }
            }

            // Display the input for the argument to the selected method.
            System.Type argType = Type.GetType(linkableVariableProperty.FindPropertyRelative("arg0Type").stringValue);
            if (numArgsProperty.intValue != 0 && argType != null)
            {
                runningPosition.y += itemHeight + spacing;

                if (argType == typeof(bool))
                {
                    SerializedProperty arg0BoolProperty = linkableVariableProperty.FindPropertyRelative("arg0BoolValue");
                    EditorGUI.PropertyField(runningPosition, arg0BoolProperty, new GUIContent("Argument"));
                }
                else if (argType == typeof(int))
                {
                    SerializedProperty arg0IntProperty = linkableVariableProperty.FindPropertyRelative("arg0IntValue");
                    EditorGUI.PropertyField(runningPosition, arg0IntProperty, new GUIContent("Argument"));
                }
                else if (argType.IsEnum)
                {
                    string[] enumNames = System.Enum.GetNames(argType);
                    SerializedProperty arg0IntProperty = linkableVariableProperty.FindPropertyRelative("arg0IntValue");
                    suffixPosition = EditorGUI.PrefixLabel(runningPosition, new GUIContent("Argument"));
                    suffixPosition = runningPosition;
                    suffixPosition.x = EditorGUIUtility.labelWidth;
                    arg0IntProperty.intValue = EditorGUI.Popup(suffixPosition, arg0IntProperty.intValue, enumNames);
                }
                else if (argType == typeof(float))
                {
                    SerializedProperty arg0FloatProperty = linkableVariableProperty.FindPropertyRelative("arg0FloatValue");
                    EditorGUI.PropertyField(runningPosition, arg0FloatProperty, new GUIContent("Argument"));
                }
                else if (argType == typeof(string))
                {
                    SerializedProperty arg0StringProperty = linkableVariableProperty.FindPropertyRelative("arg0StringValue");
                    EditorGUI.PropertyField(runningPosition, arg0StringProperty, new GUIContent("Argument"));
                }
                else
                {
                    EditorGUI.PropertyField(runningPosition, arg0ObjectProperty, new GUIContent("Argument"));
                }
            }
            
            // Show the condition check type
            runningPosition.y += itemHeight + spacing;
            SerializedProperty evaluationTypeProperty = property.FindPropertyRelative("boolEvaluationType");
            EditorGUI.PropertyField(runningPosition, evaluationTypeProperty, new GUIContent("Evaluation Type"));

            // Show the reference value for the type of the 
            runningPosition.y += itemHeight + spacing;
            SerializedProperty referenceValueProperty = property.FindPropertyRelative("boolReferenceValue");
            EditorGUI.PropertyField(runningPosition, referenceValueProperty, new GUIContent("Reference Value"));

        }
    }

    private List<MethodInfo> GetMethodInfos(MethodInfo[] methodInfos, string returnType)
    {

        Type[] allowedArgumentTypes = new Type[] { typeof(bool), typeof(string), typeof(int), typeof(float), typeof(Enum), typeof(UnityEngine.Object), typeof(Vector3) };

        // Get the methods with the return type specified
        System.Type type = System.Type.GetType(returnType);
        List<MethodInfo> compatMethodInfos = new List<MethodInfo>();
        for (int i = 0; i < methodInfos.Length; ++i)
        {

            if (methodInfos[i].ReturnType != type) continue;

            // Add the method if it has no arguments
            if (methodInfos[i].GetParameters().Length == 0)
            {
                compatMethodInfos.Add(methodInfos[i]);
            }
            else if (methodInfos[i].GetParameters().Length == 1)
            {
                // If the argument type is an enum, add the method.
                if (methodInfos[i].GetParameters()[0].ParameterType.IsEnum)
                {
                    compatMethodInfos.Add(methodInfos[i]);
                }
                else
                {
                    // If the argument type is a Unity object, add the method.
                    if (typeof(UnityEngine.Object).IsAssignableFrom(methodInfos[i].GetParameters()[0].ParameterType))
                    {
                        compatMethodInfos.Add(methodInfos[i]);
                    }
                    else
                    {
                        // If the argument type is one of the allowed types, add the method.
                        for (int j = 0; j < allowedArgumentTypes.Length; ++j)
                        {
                            //Debug.Log(methodInfos[i].GetParameters()[0].ParameterType.BaseType + "  " + allowedArgumentTypes[j]);
                            if (allowedArgumentTypes[j] == methodInfos[i].GetParameters()[0].ParameterType)
                            {
                                compatMethodInfos.Add(methodInfos[i]);
                                break;
                            }
                        }
                    }
                }
            }
        }

        return compatMethodInfos;
    }
}