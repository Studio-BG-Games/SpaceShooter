using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VSX.SpaceCombatKit
{
    public class SCKInstallManager : EditorWindow
    {

        protected List<SCKRequiredInputAxis> axesToAdd = new List<SCKRequiredInputAxis>();
        protected List<SCKRequiredLayer> layersToAdd = new List<SCKRequiredLayer>();

        [MenuItem("Space Combat Kit/Install Manager")]
        static void Init()
        {
            SCKInstallManager window = (SCKInstallManager)EditorWindow.GetWindow(typeof(SCKInstallManager), true, "Space Combat Kit Installation Manager");
            window.Show();
        }

        private void OnEnable()
        {
            axesToAdd = GetMissingAxes();
            layersToAdd = GetMissingLayers();
        }

        [InitializeOnLoadMethod]
        static void RegisterCallback()
        {
            AssetDatabase.importPackageCompleted += OnPackageImported;
        }

        protected void OnGUI()
        {

            if (axesToAdd.Count > 0 || layersToAdd.Count > 0)
            {
                EditorStyles.label.wordWrap = true;
                EditorGUILayout.LabelField(new GUIContent("The Space Combat Kit requires the following additions to the project settings. If you choose to update your settings, your current project settings will not be overwritten. If you skip, the provided demos and prefabs in the kit may not work correctly."));

                if (axesToAdd.Count > 0)
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Required Input Axes", EditorStyles.boldLabel);
                    for (int i = 0; i < axesToAdd.Count; ++i)
                    {
                        EditorGUILayout.LabelField(new GUIContent(axesToAdd[i].axisName));
                    }
                }

                if (layersToAdd.Count > 0)
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Required Layers", EditorStyles.boldLabel);
                    for (int i = 0; i < layersToAdd.Count; ++i)
                    {
                        EditorGUILayout.LabelField(new GUIContent(layersToAdd[i].layerName));
                    }
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("Update Project Settings"))
                {
                    UpdateInputManager();
                    Close();
                }

                if (GUILayout.Button("Skip"))
                {
                    Close();
                }
            }
            else
            {
                EditorStyles.label.wordWrap = true;
                EditorGUILayout.LabelField(new GUIContent("All required input axes and layers are already created in your project!"));

                if (GUILayout.Button("Close"))
                {
                    Close();
                }
            }
        }

        static void OnPackageImported(string packageName)
        {
            if (packageName.Contains("SCK") || packageName.Contains("SpaceCombatKit"))

                if (GetMissingAxes().Count > 0 || GetMissingLayers().Count > 0)
                {
                    Init();
                }
        }

        static List<SCKRequiredInputAxis> GetMissingAxes()
        {
            List<SCKRequiredInputAxis> missingAxes = new List<SCKRequiredInputAxis>();

            // Get all the required input axes
            List<SCKRequiredInputAxis> requiredAxes = new List<SCKRequiredInputAxis>();
            string[] requiredAxisGUIDs = AssetDatabase.FindAssets("t:SCKRequiredInputAxis");
            foreach (string guid in requiredAxisGUIDs)
            {
                SCKRequiredInputAxis requiredAxis = (SCKRequiredInputAxis)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(SCKRequiredInputAxis));
                requiredAxes.Add(requiredAxis);
            }

            // Get the input manager axis array
            Object inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
            SerializedObject inputManagerSO = new SerializedObject(inputManager);
            SerializedProperty axisArray = inputManagerSO.FindProperty("m_Axes");


            for (int i = 0; i < requiredAxes.Count; ++i)
            {
                bool found = false;

                for (int j = 0; j < axisArray.arraySize; ++j)
                {
                    SerializedProperty axisProp = axisArray.GetArrayElementAtIndex(j);
                    string name = axisProp.FindPropertyRelative("m_Name").stringValue;

                    if (name == requiredAxes[i].axisName)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    missingAxes.Add(requiredAxes[i]);
                }
            }

            return missingAxes;
        }


        static List<SCKRequiredLayer> GetMissingLayers()
        {
            List<SCKRequiredLayer> missingLayers = new List<SCKRequiredLayer>();

            // Get all the required input axes
            List<SCKRequiredLayer> requiredLayers = new List<SCKRequiredLayer>();
            string[] requiredLayerGUIDs = AssetDatabase.FindAssets("t:" + typeof(SCKRequiredLayer).FullName);
            foreach (string guid in requiredLayerGUIDs)
            {
                SCKRequiredLayer requiredLayer = (SCKRequiredLayer)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(SCKRequiredLayer));
                requiredLayers.Add(requiredLayer);
            }

            // Get the input manager axis array
            Object tagsManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
            SerializedObject tagsManagerSO = new SerializedObject(tagsManager);
            SerializedProperty axisArray = tagsManagerSO.FindProperty("layers");


            for (int i = 0; i < requiredLayers.Count; ++i)
            {
                bool found = false;

                for (int j = 0; j < axisArray.arraySize; ++j)
                {
                    SerializedProperty layerProp = axisArray.GetArrayElementAtIndex(j);
                    string name = layerProp.stringValue;

                    if (name == requiredLayers[i].layerName)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    missingLayers.Add(requiredLayers[i]);
                }
            }

            return missingLayers;
        }


        void UpdateInputManager()
        {


            // Add inputs
            Object inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
            SerializedObject inputManagerSO = new SerializedObject(inputManager);
            SerializedProperty axisArray = inputManagerSO.FindProperty("m_Axes");

            for (int i = 0; i < axesToAdd.Count; ++i)
            {
                AddAxis(inputManagerSO, axisArray, axesToAdd[i]);
            }

            // Add layers
            Object tagsManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
            SerializedObject tagsManagerSO = new SerializedObject(tagsManager);
            SerializedProperty layersArrayProp = tagsManagerSO.FindProperty("layers");

            for (int i = 0; i < layersToAdd.Count; ++i)
            {
                AddLayer(tagsManagerSO, layersArrayProp, layersToAdd[i]);
            }
        }


        protected static void AddAxis(SerializedObject inputManagerSerializedObject, SerializedProperty axisArrayProperty, SCKRequiredInputAxis newAxis)
        {
            axisArrayProperty.arraySize++;
            inputManagerSerializedObject.ApplyModifiedProperties();

            SerializedProperty newAxisProperty = axisArrayProperty.GetArrayElementAtIndex(axisArrayProperty.arraySize - 1);

            newAxisProperty.FindPropertyRelative("m_Name").stringValue = newAxis.axisName;
            newAxisProperty.FindPropertyRelative("negativeButton").stringValue = newAxis.negativeButton;
            newAxisProperty.FindPropertyRelative("positiveButton").stringValue = newAxis.positiveButton;
            newAxisProperty.FindPropertyRelative("altNegativeButton").stringValue = newAxis.altNegativeButton;
            newAxisProperty.FindPropertyRelative("altPositiveButton").stringValue = newAxis.altPositiveButton;

            newAxisProperty.FindPropertyRelative("gravity").floatValue = newAxis.gravity;
            newAxisProperty.FindPropertyRelative("dead").floatValue = newAxis.dead;
            newAxisProperty.FindPropertyRelative("sensitivity").floatValue = newAxis.sensitivity;

            newAxisProperty.FindPropertyRelative("snap").boolValue = newAxis.snap;
            newAxisProperty.FindPropertyRelative("invert").boolValue = newAxis.invert;

            newAxisProperty.FindPropertyRelative("type").intValue = (int)newAxis.axisType;
            newAxisProperty.FindPropertyRelative("axis").intValue = newAxis.axis;
            newAxisProperty.FindPropertyRelative("joyNum").intValue = newAxis.joyNum;

            inputManagerSerializedObject.ApplyModifiedProperties();

        }

        protected static void AddLayer(SerializedObject tagsManagerSerializedObject, SerializedProperty layersArrayProp, SCKRequiredLayer newLayer)
        {
            bool success = false;
            for (int i = 0; i < layersArrayProp.arraySize; ++i)
            {
                SerializedProperty prop = layersArrayProp.GetArrayElementAtIndex(i);
                if (i > 7 && prop.stringValue == "") // There are 8 built-in layers that can't be changed
                {
                    prop.stringValue = newLayer.layerName;
                    tagsManagerSerializedObject.ApplyModifiedProperties();
                    success = true;
                    break;
                }
            }

            if (!success)
            {
                Debug.LogError("Unable to add '" + newLayer + "' layer, no empty layer slots found.");
            }
        }
    }

}
