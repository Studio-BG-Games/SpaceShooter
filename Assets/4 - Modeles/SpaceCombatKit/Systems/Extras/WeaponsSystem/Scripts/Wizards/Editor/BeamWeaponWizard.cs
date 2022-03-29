using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Wizard for creating a beam weapon.
    /// </summary>
    public class BeamWeaponWizard : EditorWindow
    {
        protected bool isPulsed;

        protected GameObject weaponMesh;

        protected BeamWeaponUnit beamWeaponUnitPrefab;

        int numWeaponUnits;

        protected WizardTurretSettings turretSettings;


        [MenuItem("Universal Vehicle Combat/Create/Weapons/Gun Weapons/Gun Weapon (Beam)")]
        static void Init()
        {
            BeamWeaponWizard window = (BeamWeaponWizard)EditorWindow.GetWindow(typeof(BeamWeaponWizard), true, "Beam Gun Weapon Creator");
            window.Show();
        }

        private void OnEnable()
        {
            // Load the default beam unit prefab
            if (beamWeaponUnitPrefab == null)
            {
                beamWeaponUnitPrefab = Resources.Load<BeamWeaponUnit>("WeaponUnit_Gun_Beam_Constant");
            }

            // Initialize turret settings
            if (turretSettings == null)
            {
                turretSettings = new WizardTurretSettings();
            }

            numWeaponUnits = 1;
        }


        private void Create()
        {

            // Create root gameobject
            GameObject weaponRootObject = new GameObject("Beam Gun " + (turretSettings.isTurret ? "Turret" : "Weapon"));
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

            // Create beam weapon unit(s)
            float spacing = 2;
            for (int i = 0; i < numWeaponUnits; ++i)
            {
                float span = Mathf.Max((numWeaponUnits - 1), 0) * spacing;

                if (beamWeaponUnitPrefab != null)
                {
                    BeamWeaponUnit obj = PrefabUtility.InstantiatePrefab(beamWeaponUnitPrefab) as BeamWeaponUnit;
                    obj.name = "Beam Weapon Unit";

                    obj.transform.SetParent(weaponUnitsParent.transform);
                    obj.transform.localPosition = new Vector3(-(span / 2) + i * spacing, 0, 0);
                    obj.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    GameObject beamWeaponUnit = new GameObject("Beam Weapon Unit");

                    beamWeaponUnit.transform.SetParent(weaponRootObject.transform);
                    beamWeaponUnit.transform.localPosition = new Vector3(-(span / 2) + i * spacing, 0, 0);
                    beamWeaponUnit.transform.localRotation = Quaternion.identity;

                    beamWeaponUnit.AddComponent<BeamWeaponUnit>();
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
            isPulsed = EditorGUILayout.Toggle("Is Pulsed Beam", isPulsed);
            if (EditorGUI.EndChangeCheck())
            {
                if (isPulsed)
                {
                    beamWeaponUnitPrefab = Resources.Load<BeamWeaponUnit>("WeaponUnit_Gun_Beam_Pulsed");
                }
                else
                {
                    beamWeaponUnitPrefab = Resources.Load<BeamWeaponUnit>("WeaponUnit_Gun_Beam_Constant");
                }
            }

            beamWeaponUnitPrefab = (BeamWeaponUnit)EditorGUILayout.ObjectField("Beam Weapon Unit Prefab", beamWeaponUnitPrefab, typeof(BeamWeaponUnit), false);

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
