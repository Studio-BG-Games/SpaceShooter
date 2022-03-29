using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Wizard for creating a projectile weapon.
    /// </summary>
    public class ProjectileWeaponWizard : EditorWindow
    {

        protected GameObject weaponMesh;

        protected ProjectileWeaponUnit projectileWeaponUnitPrefab;

        protected Projectile projectilePrefab;

        int numWeaponUnits;

        protected WizardTurretSettings turretSettings;


        [MenuItem("Universal Vehicle Combat/Create/Weapons/Gun Weapons/Gun Weapon (Projectile)")]
        static void Init()
        {
            ProjectileWeaponWizard window = (ProjectileWeaponWizard)EditorWindow.GetWindow(typeof(ProjectileWeaponWizard), true, "Projectile Gun Weapon Creator");
            window.Show();
        }

        private void OnEnable()
        {
            // Load the default projectile weapon unit prefab
            if (projectileWeaponUnitPrefab == null)
            {
                projectileWeaponUnitPrefab = Resources.Load<ProjectileWeaponUnit>("WeaponUnit_Gun_Projectile");
            }

            // Initialize turret settings
            if (turretSettings == null)
            {
                turretSettings = new WizardTurretSettings();
            }

            // Initialize the projectile prefab
            if (projectilePrefab == null)
            {
                // Try to get the prefab of the currently assigned launcher
                if (projectileWeaponUnitPrefab != null)
                {
                    if (projectileWeaponUnitPrefab.ProjectilePrefab != null)
                    {
                        projectilePrefab = projectileWeaponUnitPrefab.ProjectilePrefab;
                    }
                }

                // Find a new projectile prefab
                if (projectilePrefab == null)
                {
                    projectilePrefab = Resources.Load<Projectile>("Projectile_Energy");
                }
            }

            numWeaponUnits = 1;
        }


        private void Create()
        {

            // Create root gameobject
            GameObject weaponRootObject = new GameObject("Projectile Gun " + (turretSettings.isTurret ? "Turret" : "Weapon"));
            Selection.activeGameObject = weaponRootObject;

            GameObject weaponUnitsParent = weaponRootObject;

            // Create weapon mesh
            if (weaponMesh != null)
            {
                GameObject obj = PrefabUtility.InstantiatePrefab(weaponMesh) as GameObject;
                obj.name = "Model";

                obj.transform.SetParent(weaponUnitsParent.transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
            }

            // Create projectile weapon unit(s)
            float spacing = 2;
            for (int i = 0; i < numWeaponUnits; ++i)
            {
                float span = Mathf.Max((numWeaponUnits - 1), 0) * spacing;

                if (projectileWeaponUnitPrefab != null)
                {
                    ProjectileWeaponUnit obj = PrefabUtility.InstantiatePrefab(projectileWeaponUnitPrefab) as ProjectileWeaponUnit;
                    obj.name = "Projectile Weapon Unit";

                    obj.transform.SetParent(weaponUnitsParent.transform);
                    obj.transform.localPosition = new Vector3(-(span / 2) + i * spacing, 0, 0);
                    obj.transform.localRotation = Quaternion.identity;

                    if (projectilePrefab != null)
                    {
                        obj.ProjectilePrefab = projectilePrefab;
                    }
                }
                else
                {
                    GameObject launcher = new GameObject("Projectile Weapon Unit");

                    launcher.transform.SetParent(weaponUnitsParent.transform);
                    launcher.transform.localPosition = new Vector3(-(span / 2) + i * spacing, 0, 0);
                    launcher.transform.localRotation = Quaternion.identity;

                    ProjectileWeaponUnit weaponUnit = launcher.AddComponent<ProjectileWeaponUnit>();
                    weaponUnit.ProjectilePrefab = projectilePrefab;
                }
            }

            // Add the weapon component
            GunWeapon weaponComponent = weaponRootObject.AddComponent<GunWeapon>();

            // Create turret
            if (turretSettings.isTurret)
            {
                WizardHelpers.CreateTurret(weaponRootObject, weaponComponent, turretSettings);
            }
        }

        protected void OnGUI()
        {
            weaponMesh = (GameObject)EditorGUILayout.ObjectField("Weapon Model", weaponMesh, typeof(GameObject), false);

            EditorGUI.BeginChangeCheck();
            projectileWeaponUnitPrefab = (ProjectileWeaponUnit)EditorGUILayout.ObjectField("Projectile Weapon Unit Prefab", projectileWeaponUnitPrefab, typeof(ProjectileWeaponUnit), false);
            if (EditorGUI.EndChangeCheck())
            {
                // Try to get the prefab of the currently assigned launcher
                if (projectileWeaponUnitPrefab != null)
                {
                    if (projectileWeaponUnitPrefab.ProjectilePrefab != null)
                    {
                        projectilePrefab = projectileWeaponUnitPrefab.ProjectilePrefab;
                    }
                }
            }
            
            projectilePrefab = (Projectile)EditorGUILayout.ObjectField("Projectile Prefab", projectilePrefab, typeof(Projectile), false);

            numWeaponUnits = EditorGUILayout.IntField("Num Weapon Units", numWeaponUnits);

            EditorGUILayout.Space();

            WizardHelpers.OnGUITurret(turretSettings);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Weapon"))
            {
                Create();
                Close();
            }
        }
    }
}
