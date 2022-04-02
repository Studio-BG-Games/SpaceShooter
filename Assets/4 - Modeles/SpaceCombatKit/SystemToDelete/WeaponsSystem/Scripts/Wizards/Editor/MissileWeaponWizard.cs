using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Wizard for creating a missile weapon.
    /// </summary>
    public class MissileWeaponWizard : EditorWindow
    {

        protected GameObject weaponMesh;

        protected ProjectileWeaponUnit missileWeaponUnitPrefab;

        protected Missile missilePrefab;

        int numWeaponUnits;

        protected WizardTurretSettings turretSettings;


        [MenuItem("Universal Vehicle Combat/Create/Weapons/Missile Weapons/Missile Weapon")]
        static void Init()
        {
            MissileWeaponWizard window = (MissileWeaponWizard)EditorWindow.GetWindow(typeof(MissileWeaponWizard), true, "Missile Weapon Creator");
            window.Show();
        }

        private void OnEnable()
        {
            // Load the default missile weapon unit prefab
            if (missileWeaponUnitPrefab == null)
            {
                missileWeaponUnitPrefab = Resources.Load<ProjectileWeaponUnit>("WeaponUnit_Missile");
            }

            // Initialize turret settings
            if (turretSettings == null)
            {
                turretSettings = new WizardTurretSettings();
            }

            // Initialize the missile prefab
            if (missilePrefab == null)
            {
                // Try to get the prefab of the currently assigned launcher
                if (missileWeaponUnitPrefab != null)
                {
                    if (missileWeaponUnitPrefab.ProjectilePrefab != null)
                    {
                        missilePrefab = missileWeaponUnitPrefab.ProjectilePrefab.GetComponent<Missile>();
                    }
                }

                // Find a new missile prefab
                if (missilePrefab == null)
                {
                    missilePrefab = Resources.Load<Missile>("Missile");
                }
            }

            numWeaponUnits = 1;
        }


        private void Create()
        {

            // Create root gameobject
            GameObject weaponRootObject = new GameObject("Missile " + (turretSettings.isTurret ? "Turret" : "Weapon"));
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

            // Create missile weapon unit(s)
            float spacing = 2;
            for (int i = 0; i < numWeaponUnits; ++i)
            {
                float span = Mathf.Max((numWeaponUnits - 1), 0) * spacing;

                if (missileWeaponUnitPrefab != null)
                {
                    ProjectileWeaponUnit obj = PrefabUtility.InstantiatePrefab(missileWeaponUnitPrefab) as ProjectileWeaponUnit;
                    obj.name = "Missile Weapon Unit";

                    obj.transform.SetParent(weaponUnitsParent.transform);
                    obj.transform.localPosition = new Vector3(-(span / 2) + i * spacing, 0, 0);
                    obj.transform.localRotation = Quaternion.identity;

                    if (missilePrefab != null)
                    {
                        obj.ProjectilePrefab = missilePrefab;
                    }
                }
                else
                {
                    GameObject launcher = new GameObject("Missile Weapon Unit");

                    launcher.transform.SetParent(weaponRootObject.transform);
                    launcher.transform.localPosition = new Vector3(-(span / 2) + i * spacing, 0, 0);
                    launcher.transform.localRotation = Quaternion.identity;

                    ProjectileWeaponUnit weaponUnit = launcher.AddComponent<ProjectileWeaponUnit>();
                    weaponUnit.ProjectilePrefab = missilePrefab;
                }
            }

            // Add the weapon component
            MissileWeapon weaponComponent = weaponRootObject.AddComponent<MissileWeapon>();

            // Create turret
            if (turretSettings.isTurret)
            {
                WizardHelpers.CreateTurret(weaponRootObject, weaponComponent, turretSettings, true);
            }

        }

        protected void OnGUI()
        {
            weaponMesh = (GameObject)EditorGUILayout.ObjectField("Weapon Model", weaponMesh, typeof(GameObject), false);

            EditorGUI.BeginChangeCheck();
            missileWeaponUnitPrefab = (ProjectileWeaponUnit)EditorGUILayout.ObjectField("Missile Weapon Unit Prefab", missileWeaponUnitPrefab, typeof(ProjectileWeaponUnit), false);
            if (EditorGUI.EndChangeCheck())
            {
                // Try to get the prefab of the currently assigned launcher
                if (missileWeaponUnitPrefab != null)
                {
                    if (missileWeaponUnitPrefab.ProjectilePrefab != null)
                    {
                        missilePrefab = missileWeaponUnitPrefab.ProjectilePrefab.GetComponent<Missile>();
                    }
                }
            }

            missilePrefab = (Missile)EditorGUILayout.ObjectField("Missile Prefab", missilePrefab, typeof(Missile), false);

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
