using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VSX.FloatingOriginSystem;

namespace VSX.UniversalVehicleCombat
{
    public class ProjectileWizard : EditorWindow
    {

        protected GameObject projectileMesh;

        protected GameObject hitEffectPrefab;

        protected bool isRigidbodyProjectile;

        protected bool isFloatingOriginObject = true;


        [MenuItem("Universal Vehicle Combat/Create/Weapons/Gun Weapons/Projectile")]
        static void Init()
        {
            ProjectileWizard window = (ProjectileWizard)EditorWindow.GetWindow(typeof(ProjectileWizard), true, "Projectile Wizard");
            window.Show();
        }

        private void OnEnable()
        {
            if (hitEffectPrefab == null)
            {
                hitEffectPrefab = Resources.Load<GameObject>("HitEffect_Gun_Projectile");
            }
        }

        private void Create()
        {

            // Create root gameobject
            GameObject projectileObject = new GameObject(isRigidbodyProjectile ? "Rigidbody Projectile" : "Projectile");
            if (isRigidbodyProjectile)
            {
                projectileObject.AddComponent<RigidbodyProjectile>();
            }
            else
            {
                projectileObject.AddComponent<Projectile>();
            }

            Selection.activeGameObject = projectileObject;

            // Create the projectile mesh
            if (projectileMesh != null)
            {
                GameObject obj = PrefabUtility.InstantiatePrefab(projectileMesh, projectileObject.transform) as GameObject;
                obj.name = "Mesh";
            }

            // Add hit effects
            Detonator detonator = projectileObject.GetComponent<Detonator>();
            if (hitEffectPrefab != null)
            {
                detonator.DetonatedStateSpawnObjects.Add(hitEffectPrefab);
            }

            // Add floating origin object component
            if (isFloatingOriginObject)
            {
                projectileObject.AddComponent<FloatingOriginObject>();
            }
        }

        protected void OnGUI()
        {
            projectileMesh = (GameObject)EditorGUILayout.ObjectField("Projectile Mesh", projectileMesh, typeof(GameObject), false);

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
