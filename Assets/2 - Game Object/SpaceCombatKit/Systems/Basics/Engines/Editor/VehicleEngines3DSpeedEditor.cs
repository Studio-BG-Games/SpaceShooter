using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class enables you to configure the vehicle engines with speed values rather than force values.
    /// </summary>
    public class VehicleEngines3DSpeedEditor : EditorWindow
    {

        [SerializeField]
        protected VehicleEngines3D vehicleEngines;

        [SerializeField]
        protected int vehicleEnginesInstanceID;     // Used to keep a reference to the vehicle engines when entering/exiting play mode

        [SerializeField]
        protected Rigidbody vehicleRigidbody;       // The vehicle rigidbody

        // Vehicle engines properties that will be modified

        [SerializeField]
        protected SerializedObject vehicleEnginesSerializedObject;

        [SerializeField]
        protected SerializedProperty defaultMovementForcesProperty;
        [SerializeField]
        protected SerializedProperty defaultBoostForcesProperty;
        [SerializeField]
        protected SerializedProperty maxMovementForcesProperty;

        // The speed values

        [SerializeField]
        protected Vector3 defaultMovementSpeed;
        [SerializeField]
        protected Vector3 defaultBoostSpeed;
        [SerializeField]
        protected Vector3 maxMovementSpeed;


        [MenuItem("Window/VSXGames/Vehicle Engines 3D Speed Editor")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(VehicleEngines3DSpeedEditor));
        }

        protected void OnEnable()
        {
            // Call function when entering/exiting play mode
            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;

            // Load the previous engine reference
            SetEngines(EditorUtility.InstanceIDToObject(vehicleEnginesInstanceID) as VehicleEngines3D);
        }

        protected void OnDisable()
        {
            // Stop calling function when entering/exiting play mode
            EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
        }

        // Called when the editor enters and exits play mode.
        protected void OnPlaymodeStateChanged(PlayModeStateChange change)
        {
            SetEngines(EditorUtility.InstanceIDToObject(vehicleEnginesInstanceID) as VehicleEngines3D);
            EditorWindow window = EditorWindow.GetWindow(typeof(VehicleEngines3DSpeedEditor));
            window.Repaint();
        }

        // Set a new engines reference
        protected void SetEngines(VehicleEngines3D newEngines)
        {
            if (newEngines == null)
            {
                vehicleEngines = null;
                vehicleEnginesSerializedObject = null;
            }
            else
            {
                vehicleEngines = newEngines;
                vehicleEnginesInstanceID = vehicleEngines.GetInstanceID();
                vehicleEnginesSerializedObject = new SerializedObject(vehicleEngines);
                defaultMovementForcesProperty = vehicleEnginesSerializedObject.FindProperty("defaultMovementForces");
                defaultBoostForcesProperty = vehicleEnginesSerializedObject.FindProperty("defaultBoostForces");
                maxMovementForcesProperty = vehicleEnginesSerializedObject.FindProperty("maxMovementForces");

                vehicleRigidbody = vehicleEngines.GetComponent<Rigidbody>();
            }
        }

       
        protected void OnGUI()
        {

            GUILayout.Label("Settings", EditorStyles.boldLabel);

            // Show engines reference in inspector
            EditorGUI.BeginChangeCheck();
            vehicleEngines = EditorGUILayout.ObjectField("Engines", vehicleEngines, typeof(VehicleEngines3D), true) as VehicleEngines3D;

            // If engines reference changed, update other references
            if (EditorGUI.EndChangeCheck())
            {
                if (vehicleEngines != null)
                {
                    SetEngines(vehicleEngines);
                }
                else
                {
                    vehicleEnginesSerializedObject = null;
                }
            }

            // If a vehicle engines serialized object is referenced, update its properties
            if (vehicleEnginesSerializedObject != null)
            {
                vehicleEnginesSerializedObject.Update();

                defaultMovementSpeed = EditorGUILayout.Vector3Field("Default Movement Speed", defaultMovementSpeed);
                defaultBoostSpeed = EditorGUILayout.Vector3Field("Default Boost Speed", defaultBoostSpeed);
                maxMovementSpeed = EditorGUILayout.Vector3Field("Max Movement Speed", maxMovementSpeed);

                defaultMovementForcesProperty.vector3Value = ConvertSpeedToForce(defaultMovementSpeed);
                defaultBoostForcesProperty.vector3Value = ConvertSpeedToForce(defaultBoostSpeed);
                maxMovementForcesProperty.vector3Value = ConvertSpeedToForce(maxMovementSpeed);

                vehicleEnginesSerializedObject.ApplyModifiedProperties();
            }

        }

        // Convert a Vector3 of speed values to force values, based on the rigidbody's physics properties
        protected Vector3 ConvertSpeedToForce(Vector3 speed)
        {
            float dragFactor = Time.fixedDeltaTime * vehicleRigidbody.drag;
            Vector3 acceleration = dragFactor * speed;
            Vector3 force = (vehicleRigidbody.mass * acceleration) / Time.fixedDeltaTime;
            return force;
        }
    }
}

