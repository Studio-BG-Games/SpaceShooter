using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;


namespace VSX.UniversalVehicleCombat
{

    [CustomPropertyDrawer(typeof(LinkableVariable))]
    public class LinkableVariablePropertyDrawer : PropertyDrawer
    {
        private float itemHeight = 16;
        private int spacing = 2;

        private int numItemsStatic = 6;
        private int numItemsDynamic = 8;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty numArgsProperty = property.FindPropertyRelative("numArgs");
            int numArgs = numArgsProperty.intValue;

            bool isDynamic = property.FindPropertyRelative("isLinkedVariable").boolValue;
            int numItems = isDynamic ? numItemsDynamic + numArgs : numItemsStatic;

            return itemHeight * numItems + spacing * 2 + spacing * (numItems - 1);
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            
            // Draw box background
            GUI.Box(position, GUIContent.none);

            // Keep a running reference to the next GUI height position, because checkboxes on/off will change the position of things
            Rect runningPosition = position;
            runningPosition.height = itemHeight;
            runningPosition.y += spacing;

            EditorGUI.LabelField(runningPosition, label, EditorStyles.boldLabel);

            runningPosition.y += itemHeight + spacing;

            // Keep a reference to the suffix position for items preceeded by a label.
            Rect suffixPosition;

            // Draw property - list index
            SerializedProperty listIndexProperty = property.FindPropertyRelative("listIndex");
            EditorGUI.PropertyField(runningPosition, listIndexProperty);

            runningPosition.y += itemHeight + spacing;

            // Draw property - key
            SerializedProperty keyProperty = property.FindPropertyRelative("key");
            EditorGUI.PropertyField(runningPosition, keyProperty);

            runningPosition.y += itemHeight + spacing;          

            // Draw property - variable type
            SerializedProperty variableTypeProperty = property.FindPropertyRelative("variableType");
            EditorGUI.PropertyField(runningPosition, variableTypeProperty);

            // Get reference to the variable type and assembly qualified name
            LinkableVariableType trackableVariableType = (LinkableVariableType)variableTypeProperty.enumValueIndex;
            string typeName = "";
            switch (trackableVariableType)
            {
                case LinkableVariableType.Object:
                    typeName = typeof(UnityEngine.Object).AssemblyQualifiedName;
                    break;
                case LinkableVariableType.Bool:
                    typeName = typeof(bool).AssemblyQualifiedName;
                    break;
                case LinkableVariableType.Int:
                    typeName = typeof(int).AssemblyQualifiedName;
                    break;
                case LinkableVariableType.Float:
                    typeName = typeof(float).AssemblyQualifiedName;
                    break;
                case LinkableVariableType.String:
                    typeName = typeof(string).AssemblyQualifiedName;
                    break;
                case LinkableVariableType.Vector3:
                    typeName = typeof(Vector3).AssemblyQualifiedName;
                    break;
            }

            runningPosition.y += itemHeight + spacing;

            // Draw property - is dynamic (checkbox)
            SerializedProperty isDynamicVariableProperty = property.FindPropertyRelative("isLinkedVariable");

            EditorGUI.PropertyField(runningPosition, isDynamicVariableProperty);

            
            
            // Dynamic variable stuff
            if (isDynamicVariableProperty.boolValue)
            {

                // Get property - target component
                SerializedProperty targetComponentProperty = property.FindPropertyRelative("targetComponent");

                runningPosition.y += itemHeight + spacing;

                // Draw property - target object
                SerializedProperty targetObjectProperty = property.FindPropertyRelative("targetObject");
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
                    suffixPosition.x = EditorGUIUtility.labelWidth;
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
                List<MethodInfo> compatibleMethodInfos = GetMethodInfos(methodInfos, typeName);

                // Get the number of arguments of the currently selected method on the object
                SerializedProperty numArgsProperty = property.FindPropertyRelative("numArgs");
                if (targetObject == null)
                {
                    numArgsProperty.intValue = 0;
                }
                
                // Get information about the currently selected method
                SerializedProperty methodInfoNameProp = property.FindPropertyRelative("methodInfoName");
                SerializedProperty arg0ObjectProperty = property.FindPropertyRelative("arg0ObjectValue");
                SerializedProperty argo0TypeProperty = property.FindPropertyRelative("arg0Type");

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

                // Display the methods on the selected object
                runningPosition.y += itemHeight + spacing;
                suffixPosition = EditorGUI.PrefixLabel(runningPosition, new GUIContent("Function"));
                suffixPosition = runningPosition;
                suffixPosition.x = EditorGUIUtility.labelWidth;
                suffixPosition.width = runningPosition.width - EditorGUIUtility.labelWidth;
                selectedMethodIndex = EditorGUI.Popup(suffixPosition, selectedMethodIndex, methodDisplayNames.ToArray());

                // Update the argument type for the method 
                if (selectedMethodIndex != -1)
                {
                    methodInfoNameProp.stringValue = compatibleMethodInfos[selectedMethodIndex].Name;
                    numArgsProperty.intValue = compatibleMethodInfos[selectedMethodIndex].GetParameters().Length;
                    if (compatibleMethodInfos[selectedMethodIndex].GetParameters().Length > 0)
                    {
                        property.FindPropertyRelative("arg0Type").stringValue = compatibleMethodInfos[selectedMethodIndex].GetParameters()[0].ParameterType.AssemblyQualifiedName;
                    }
                }
                else
                {
                    numArgsProperty.intValue = 0;
                }
                
                // Display the input for the argument to the selected method.
                System.Type argType = Type.GetType(property.FindPropertyRelative("arg0Type").stringValue);
                if (numArgsProperty.intValue != 0 && argType != null)
                {
                    runningPosition.y += itemHeight + spacing;

                    if (argType == typeof(bool))
                    {
                        SerializedProperty arg0BoolProperty = property.FindPropertyRelative("arg0BoolValue");
                        EditorGUI.PropertyField(runningPosition, arg0BoolProperty, new GUIContent("Argument"));
                    }
                    else if (argType == typeof(int))
                    {
                        SerializedProperty arg0IntProperty = property.FindPropertyRelative("arg0IntValue");
                        EditorGUI.PropertyField(runningPosition, arg0IntProperty, new GUIContent("Argument"));
                    }
                    else if (argType.IsEnum)
                    {
                        string[] enumNames = System.Enum.GetNames(argType);
                        SerializedProperty arg0IntProperty = property.FindPropertyRelative("arg0IntValue");
                        suffixPosition = EditorGUI.PrefixLabel(runningPosition, new GUIContent("Argument"));
                        suffixPosition = runningPosition;
                        suffixPosition.x = EditorGUIUtility.labelWidth;
                        arg0IntProperty.intValue = EditorGUI.Popup(suffixPosition, arg0IntProperty.intValue, enumNames);
                    }
                    else if (argType == typeof(float))
                    {
                        SerializedProperty arg0FloatProperty = property.FindPropertyRelative("arg0FloatValue");
                        EditorGUI.PropertyField(runningPosition, arg0FloatProperty, new GUIContent("Argument"));
                    }
                    else if (argType == typeof(string))
                    {
                        SerializedProperty arg0StringProperty = property.FindPropertyRelative("arg0StringValue");
                        EditorGUI.PropertyField(runningPosition, arg0StringProperty, new GUIContent("Argument"));
                    }
                    else
                    {
                        EditorGUI.PropertyField(runningPosition, arg0ObjectProperty, new GUIContent("Argument"));
                    }
                }
            }
            else
            {
                runningPosition.y += itemHeight + spacing;

                LinkableVariableType type = (LinkableVariableType)variableTypeProperty.enumValueIndex;
                switch (type)
                {
                    case LinkableVariableType.Object:
                        SerializedProperty objectValueProperty = property.FindPropertyRelative("objectValue");
                        EditorGUI.PropertyField(runningPosition, objectValueProperty);
                        break;
                    case LinkableVariableType.Bool:
                        SerializedProperty boolValueProperty = property.FindPropertyRelative("boolValue");
                        EditorGUI.PropertyField(runningPosition, boolValueProperty);
                        break;
                    case LinkableVariableType.Int:
                        SerializedProperty intValueProperty = property.FindPropertyRelative("intValue");
                        EditorGUI.PropertyField(runningPosition, intValueProperty);
                        break;
                    case LinkableVariableType.Float:
                        SerializedProperty floatValueProperty = property.FindPropertyRelative("floatValue");
                        EditorGUI.PropertyField(runningPosition, floatValueProperty);
                        break;
                    case LinkableVariableType.String:
                        SerializedProperty stringValueProperty = property.FindPropertyRelative("stringValue");
                        EditorGUI.PropertyField(runningPosition, stringValueProperty);
                        break;
                    case LinkableVariableType.Vector3:
                        SerializedProperty vector3ValueProperty = property.FindPropertyRelative("vector3Value");
                        EditorGUI.PropertyField(runningPosition, vector3ValueProperty);
                        break;
                }
            }
            /*
            object obj = fieldInfo.GetValue(property.serializedObject.targetObject);
            LinkableVariable variable = obj as LinkableVariable;
            IEnumerable enumerable = obj as IEnumerable;
            
            if (enumerable != null)
            {               
                int index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
                List<LinkableVariable> variablesList = (List<LinkableVariable>)obj;
                if (variablesList != null && index >= 0 && variablesList.Count > index) variable = ((List<LinkableVariable>)obj)[index];
            }

            if (variable != null)
            {
                MethodInfo initializeMethod = variable.GetType().GetMethod("InitializeLinkDelegate", BindingFlags.NonPublic | BindingFlags.Instance);
                if (initializeMethod == null)
                {
                    Debug.LogError("Unable to find InitializeLinkDelegate method on LinkableVariable component");
                }
                else
                {
                    initializeMethod.Invoke(variable, null);
                }
            }
            */
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
}
