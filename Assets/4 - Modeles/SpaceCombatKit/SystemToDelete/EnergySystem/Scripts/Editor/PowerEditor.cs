using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat 
{

	/// <summary>
    /// This editor script modifies the inspector of the Power component to display a set of configurable parameters for each
    /// of the values defined in the PoweredSubsystem enum.
    /// </summary>
	[CustomEditor(typeof(Power))]
	public class PowerEditor : Editor 
	{
		
        // Reference to the object
		Power script;

        SerializedProperty powerPlantProperty;
        SerializedProperty fillStorageOnPowerPlantLoadedProperty;

		void OnEnable()
		{
            // Assign the reference to the object
			script = (Power)target;

            powerPlantProperty = serializedObject.FindProperty("powerPlant");
            fillStorageOnPowerPlantLoadedProperty = serializedObject.FindProperty("fillStorageOnPowerPlantLoaded");

		}

        // Called every time the inspector is redrawn
 		public override void OnInspectorGUI()
		{
	
			// Setup
			serializedObject.Update();

            EditorGUI.BeginChangeCheck();

			// Resize the list of PoweredSubsystem instances depending on the PoweredSubsystemType
			string[] subsystemTypeNames = Enum.GetNames(typeof(PoweredSubsystemType));
            PoweredSubsystemType[] subsystemValues = (PoweredSubsystemType[])Enum.GetValues(typeof(PoweredSubsystemType));

            // Resize the list of subsystem settings depending on the number of powered subsystem types
            if (Event.current.type == EventType.Layout)
			{
				ResizeList(script.poweredSubsystems, subsystemTypeNames.Length);
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
			}
			
			// Keep track of certain values that must be normalized
			float totalFixedPower = 0;
			float totalDistributablePowerFraction = 0;

            EditorGUILayout.PropertyField(powerPlantProperty);

            EditorGUILayout.PropertyField(fillStorageOnPowerPlantLoadedProperty);

            // Show each of the subsystem settings in the inspector
            for (int i = 0; i < script.poweredSubsystems.Count; ++i)
			{

				EditorGUILayout.Space();

				EditorGUILayout.BeginVertical("box");
			
				script.poweredSubsystems[i].type = subsystemValues[i];

				EditorGUILayout.LabelField(script.poweredSubsystems[i].type.ToString(), EditorStyles.boldLabel);

				script.poweredSubsystems[i].powerConfiguration = (SubsystemPowerConfiguration)EditorGUILayout.EnumPopup("Power Configuration", script.poweredSubsystems[i].powerConfiguration);

				if (script.poweredSubsystems[i].powerConfiguration == SubsystemPowerConfiguration.Collective ||
				    script.poweredSubsystems[i].powerConfiguration == SubsystemPowerConfiguration.Independent)
				{

                    // Show settings for Collectively powered subsystem
					if (script.poweredSubsystems[i].powerConfiguration == SubsystemPowerConfiguration.Collective)
					{
						script.poweredSubsystems[i].fixedPowerFraction = EditorGUILayout.FloatField("Fixed Power Fraction", script.poweredSubsystems[i].fixedPowerFraction);
						script.poweredSubsystems[i].fixedPowerFraction = Mathf.Clamp(script.poweredSubsystems[i].fixedPowerFraction, 0f, 1 - totalFixedPower);
						totalFixedPower += script.poweredSubsystems[i].fixedPowerFraction;

						script.poweredSubsystems[i].defaultDistributablePowerFraction = EditorGUILayout.FloatField("Default Distributable Power Fraction", script.poweredSubsystems[i].defaultDistributablePowerFraction);
						script.poweredSubsystems[i].defaultDistributablePowerFraction = Mathf.Clamp(script.poweredSubsystems[i].defaultDistributablePowerFraction, 0f, 1 - totalDistributablePowerFraction);
						totalDistributablePowerFraction += script.poweredSubsystems[i].defaultDistributablePowerFraction;		
					}
                    // Show settings for Independently powered subsystem
					else if (script.poweredSubsystems[i].powerConfiguration == SubsystemPowerConfiguration.Independent)
					{
						script.poweredSubsystems[i].independentPowerOutput = EditorGUILayout.FloatField("Independent Power Output", script.poweredSubsystems[i].independentPowerOutput);
						script.poweredSubsystems[i].fixedPowerFraction = script.poweredSubsystems[i].independentPowerOutput;
					}

                    // Show the slider for Recharge <-> Direct power fractions
					EditorGUILayout.BeginVertical("box");
					script.poweredSubsystems[i].rechargePowerFraction = EditorGUILayout.Slider("", script.poweredSubsystems[i].rechargePowerFraction, 0f, 1f);

					EditorGUILayout.BeginHorizontal();

                    // Show Direct value field
					GUILayout.Label("Direct");
					float val = (1 - script.poweredSubsystems[i].rechargePowerFraction) * script.poweredSubsystems[i].fixedPowerFraction;
					GUILayout.Label(val.ToString("F1"));
					TextAnchor defaultAlignment = GUI.skin.label.alignment;
					GUI.skin.label.alignment = TextAnchor.UpperRight;

                    // Show Recharge value field
					GUILayout.Label("Recharge");
					val = (script.poweredSubsystems[i].rechargePowerFraction) * script.poweredSubsystems[i].fixedPowerFraction;
					GUILayout.Label(val.ToString("F1"));
					GUI.skin.label.alignment = defaultAlignment;
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.EndVertical();

					script.poweredSubsystems[i].maxRechargeRate = EditorGUILayout.FloatField("Max Recharge Rate", script.poweredSubsystems[i].maxRechargeRate);

					script.poweredSubsystems[i].storageCapacity = EditorGUILayout.FloatField("Storage Capacity", script.poweredSubsystems[i].storageCapacity);
				}

                // If not collectively powered, distributable power fraction is always zero
				if (script.poweredSubsystems[i].powerConfiguration != SubsystemPowerConfiguration.Collective)
				{
					script.poweredSubsystems[i].distributablePowerFraction = 0;
				}

				EditorGUILayout.EndVertical();
	
			}

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Power Settings");
                EditorUtility.SetDirty(script);
            }

			// Apply modifications
			serializedObject.ApplyModifiedProperties();
			
		}

        /// <summary>
        /// Resize a generic list, inserting null references into new spaces
        /// </summary>
        /// <typeparam name="T">The object type that the List is holding.</typeparam>
        /// <param name="list">The list to be resized.</param>
        /// <param name="newSize">The new size for the list.</param>
		public static void ResizeList<T>(List<T> list, int newSize)
        {
            if (list.Count == newSize)
                return;

            if (list.Count < newSize)
            {
                int numAdditions = newSize - list.Count;
                for (int i = 0; i < numAdditions; ++i)
                {
                    list.Add(default(T));
                }
            }
            else
            {
                int numRemovals = list.Count - newSize;
                for (int i = 0; i < numRemovals; ++i)
                {
                    //Remove the last one in the list
                    list.RemoveAt(list.Count - 1);

                }
            }
        }

    }
}
