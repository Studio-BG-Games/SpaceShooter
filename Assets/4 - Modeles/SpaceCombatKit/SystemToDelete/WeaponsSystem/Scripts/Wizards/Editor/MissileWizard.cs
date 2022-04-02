using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using VSX.FloatingOriginSystem;

namespace VSX.UniversalVehicleCombat
{
    public class MissileWizard : EditorWindow
    {

        protected GameObject missileMesh;

        protected GameObject exhaustPrefab;

        protected GameObject hitEffectPrefab;

        protected bool isFloatingOriginObject = true;


        [MenuItem("Universal Vehicle Combat/Create/Weapons/Missile Weapons/Missile")]
        static void Init()
        {
            MissileWizard window = (MissileWizard)EditorWindow.GetWindow(typeof(MissileWizard), true, "Missile Wizard");
            window.Show();
        }

        private void OnEnable()
        {
            if (hitEffectPrefab == null)
            {
                hitEffectPrefab = Resources.Load<GameObject>("Explosion_Small");
            }

            if (exhaustPrefab == null)
            {
                exhaustPrefab = Resources.Load<GameObject>("Exhaust_Missile");
            }
        }

        private void Create()
        {
            // Create root gameobject
            GameObject missileObject = new GameObject("Missile");
            Missile missileComponent = missileObject.AddComponent<Missile>();

            Selection.activeGameObject = missileObject;

            // Get the detonator and initialize events
            Detonator detonator = missileObject.GetComponent<Detonator>();
            detonator.onDetonating = new UnityEngine.Events.UnityEvent();
            detonator.onReset = new UnityEngine.Events.UnityEvent();

            // Make missile rigidbody kinematic when exploded
            UnityEventTools.AddPersistentListener(detonator.onDetonating, missileComponent.SetRigidbodyKinematic);
            UnityEventTools.AddPersistentListener(detonator.onReset, missileComponent.SetRigidbodyNonKinematic);

            // Create the missile mesh
            if (missileMesh != null)
            {
                GameObject obj = PrefabUtility.InstantiatePrefab(missileMesh, missileObject.transform) as GameObject;
                obj.name = "Mesh";

                UnityEventTools.AddBoolPersistentListener(detonator.onDetonating, obj.SetActive, false);
                UnityEventTools.AddBoolPersistentListener(detonator.onReset, obj.SetActive, true);
            }

            // Add hit effects
            if (hitEffectPrefab != null)
            {
                detonator.DetonatingStateSpawnObjects.Add(hitEffectPrefab);
            }

            // Add exhaust
            if (exhaustPrefab != null)
            {
                GameObject obj = PrefabUtility.InstantiatePrefab(exhaustPrefab, missileObject.transform) as GameObject;
                obj.name = "Exhaust";

                TrailRendererScroller[] trailRendererScrollers = obj.GetComponentsInChildren<TrailRendererScroller>();
                foreach(TrailRendererScroller trailRendererScroller in trailRendererScrollers)
                {
                    trailRendererScroller.Rigidbody = missileComponent.Rigidbody;
                }
            }

            // Add floating origin object component
            if (isFloatingOriginObject)
            {
                missileObject.AddComponent<FloatingOriginObject>();
            }
        }

        protected void OnGUI()
        {
            missileMesh = (GameObject)EditorGUILayout.ObjectField("Missile Mesh", missileMesh, typeof(GameObject), false);

            exhaustPrefab = (GameObject)EditorGUILayout.ObjectField("Exhaust Prefab", exhaustPrefab, typeof(GameObject), false);

            hitEffectPrefab = (GameObject)EditorGUILayout.ObjectField("Hit Effect Prefab", hitEffectPrefab, typeof(GameObject), false);

            isFloatingOriginObject = EditorGUILayout.Toggle("Using Floating Origin", isFloatingOriginObject);

            if (GUILayout.Button("Create"))
            {
                Create();
                Close();
            }
        }
    }
}
