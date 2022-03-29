using UnityEngine;
using UnityEditor;

namespace VSX.UniversalVehicleCombat
{
    public class ObstacleAvoidanceDebugging : EditorWindow
    {
        public ObstacleAvoidanceBehaviour obstacleAvoidanceBehaviour;
        public int instanceID;
    
        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/Obstacle Avoidance Debugging")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            ObstacleAvoidanceDebugging window = (ObstacleAvoidanceDebugging)EditorWindow.GetWindow(typeof(ObstacleAvoidanceDebugging));
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        private void PlayModeStateChanged(PlayModeStateChange change)
        {
            Object o = EditorUtility.InstanceIDToObject(instanceID);
            if (o != null)
            {
                obstacleAvoidanceBehaviour = o as ObstacleAvoidanceBehaviour;
            }
        }

        void OnGUI()
        {
            
            if (!EditorApplication.isPlaying)
            {
                obstacleAvoidanceBehaviour = EditorGUILayout.ObjectField("Behaviour", obstacleAvoidanceBehaviour, typeof(ObstacleAvoidanceBehaviour), true) as ObstacleAvoidanceBehaviour;
                if (obstacleAvoidanceBehaviour != null)
                {
                    instanceID = obstacleAvoidanceBehaviour.GetInstanceID();
                }
            }

            if (obstacleAvoidanceBehaviour != null)
            {
                GUILayout.Label("Obstacle Avoidance Strength: " + obstacleAvoidanceBehaviour.ObstacleAvoidanceStrength);

                if (obstacleAvoidanceBehaviour.ObstacleDataList.Count > 0)
                {
                    GUILayout.Label("Detected Obstacles");
                }

                foreach (ObstacleData obstacleData in obstacleAvoidanceBehaviour.ObstacleDataList)
                {
                    GUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Obstacle: " + obstacleData.raycastHit.collider.name);
                    EditorGUILayout.LabelField("Risk Factor: " + obstacleData.riskFactor.ToString(), EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Directionality Factor: " + obstacleData.directionalityFactor.ToString());
                    EditorGUILayout.LabelField("Proximity Factor: " + obstacleData.proximityFactor.ToString());
                    EditorGUILayout.LabelField("Time-To-Impact Factor: " + obstacleData.timeToImpactFactor.ToString());
                    EditorGUILayout.LabelField("Memory Fade Factor: " + obstacleData.memoryFadeFactor.ToString());
                    GUILayout.EndVertical();
                }
                
            }
        }

        public void Update()
        {
            // This is necessary to make the framerate normal for the editor window.
            Repaint();
        }
    }
}
