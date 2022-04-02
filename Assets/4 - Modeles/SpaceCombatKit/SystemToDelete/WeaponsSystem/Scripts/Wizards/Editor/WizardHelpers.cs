using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;
using VSX.UniversalVehicleCombat.Radar;
using UnityEditor;

public class WizardTurretSettings
{
    public bool isTurret;

    protected GameObject turretPrefab;
    public GameObject TurretPrefab
    {
        get { return turretPrefab; }
        set
        {
            if (turretPrefab == value) return;

            turretPrefab = value;

            if (turretPrefab != null)
            {
                Transform[] turretPrefabObjects = turretPrefab.GetComponentsInChildren<Transform>();
                for (int i = 0; i < turretPrefabObjects.Length; ++i)
                {
                    if (turretPrefabObjects[i].name.IndexOf("horizontal", System.StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        horizontalPivotIndex = i;
                    }
                    if (turretPrefabObjects[i].name.IndexOf("vertical", System.StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        verticalPivotIndex = i;
                    }
                }

                weaponHolderIndex = verticalPivotIndex;
            }
        }
    }

    public int horizontalPivotIndex;
    public int verticalPivotIndex;
    public int weaponHolderIndex;

    public bool isAutoTurret;
    public bool isTrackable;
    public Team team;

    public WizardTurretSettings()
    {
        if (turretPrefab == null)
        {
            TurretPrefab = Resources.Load<GameObject>("TurretBase");
        }
    }
}

public static class WizardHelpers
{
    public static void CreateTurret(GameObject weaponRootObject, Weapon weaponComponent, WizardTurretSettings turretSettings, bool isMissileTurret = false)
    {
        Turret turretComponent;
        if (isMissileTurret) 
        {
            turretComponent = weaponRootObject.AddComponent<MissileTurret>();
        }
        else
        {
            turretComponent = weaponRootObject.AddComponent<GunTurret>();
        }

        GimbalController gimbalController = weaponRootObject.GetComponent<GimbalController>();
        TargetLocker targetLocker = turretComponent.GetComponent<TargetLocker>();
        Transform weaponUnitsParent = weaponComponent.transform;

        if (turretSettings.TurretPrefab != null)
        {
            GameObject turretObj = PrefabUtility.InstantiatePrefab(turretSettings.TurretPrefab) as GameObject;
            turretObj.name = "Turret";

            turretObj.transform.SetParent(weaponRootObject.transform);
            turretObj.transform.localPosition = Vector3.zero;
            turretObj.transform.localRotation = Quaternion.identity;

            Transform[] turretObjects = turretObj.GetComponentsInChildren<Transform>();

            if (turretSettings.horizontalPivotIndex != -1)
            {
                gimbalController.HorizontalPivot = turretObjects[turretSettings.horizontalPivotIndex];
            }

            if (turretSettings.verticalPivotIndex != -1)
            {
                gimbalController.VerticalPivot = turretObjects[turretSettings.verticalPivotIndex];
                weaponUnitsParent = turretObjects[turretSettings.verticalPivotIndex];
            }

            if (turretSettings.weaponHolderIndex != -1)
            {
                weaponUnitsParent = turretObjects[turretSettings.weaponHolderIndex];
            }
        }
        else
        {
            GameObject horizontalPivotObject = new GameObject("Horizontal Pivot");
            horizontalPivotObject.transform.SetParent(weaponRootObject.transform);
            horizontalPivotObject.transform.localPosition = Vector3.zero;
            horizontalPivotObject.transform.localRotation = Quaternion.identity;

            gimbalController.HorizontalPivot = horizontalPivotObject.transform;

            GameObject verticalPivotObject = new GameObject("Vertical Pivot");
            verticalPivotObject.transform.SetParent(horizontalPivotObject.transform);
            verticalPivotObject.transform.localPosition = Vector3.zero;
            verticalPivotObject.transform.localRotation = Quaternion.identity;

            gimbalController.VerticalPivot = verticalPivotObject.transform;

            weaponUnitsParent = verticalPivotObject.transform;
        }

        if (isMissileTurret) targetLocker.LockingReferenceTransform = gimbalController.VerticalPivot;

        // Create projectile weapon unit(s)
        float spacing = 2;
        for (int i = 0; i < weaponComponent.WeaponUnits.Count; ++i)
        {
            float span = Mathf.Max((weaponComponent.WeaponUnits.Count - 1), 0) * spacing;

            weaponComponent.WeaponUnits[i].transform.SetParent(weaponUnitsParent);
            weaponComponent.WeaponUnits[i].transform.localPosition = new Vector3(-(span / 2) + i * spacing, 0, 0);
            weaponComponent.WeaponUnits[i].transform.localRotation = Quaternion.identity;
        }

        turretComponent.AimAssistReferenceTransform = turretComponent.GetComponentInChildren<GimbalController>().VerticalPivot;

        // Set up auto turret
        if (turretSettings.isAutoTurret)
        {
            weaponRootObject.AddComponent<Tracker>();

            TrackerTargetSelector trackerTargetSelector = weaponRootObject.AddComponent<TrackerTargetSelector>();

            turretComponent.TurretMode = TurretMode.Independent;
            turretComponent.TargetSelector = trackerTargetSelector;

            // Add targetable teams
            if (turretSettings.team != null)
            {
                trackerTargetSelector.SpecifySelectableTeams = true;
                trackerTargetSelector.SelectableTeams.Clear();

                for (int i = 0; i < turretSettings.team.HostileTeams.Count; ++i)
                {
                    trackerTargetSelector.SelectableTeams.Add(turretSettings.team.HostileTeams[i]);
                }
            }
        }

        // Set up trackable
        if (turretSettings.isTrackable)
        {
            Trackable trackable = weaponRootObject.AddComponent<Trackable>();
            trackable.Team = turretSettings.team;
        }
    }


    public static void OnGUITurret(WizardTurretSettings turretSettings)
    {
        EditorGUILayout.LabelField("Turret Setup", EditorStyles.boldLabel);

        turretSettings.isTurret = EditorGUILayout.Toggle("Is Turret", turretSettings.isTurret);

        if (turretSettings.isTurret)
        {
            turretSettings.TurretPrefab = (GameObject)EditorGUILayout.ObjectField("Turret Prefab", turretSettings.TurretPrefab, typeof(GameObject), false);
            
            // Get the names of all the objects in the turret prefab hierarchy
            if (turretSettings.TurretPrefab != null)
            {
                Transform[] turretPrefabObjects = turretSettings.TurretPrefab.GetComponentsInChildren<Transform>();

                string[] turretPrefabObjectNames = new string[turretPrefabObjects.Length];
                for (int i = 0; i < turretPrefabObjects.Length; ++i)
                {
                    turretPrefabObjectNames[i] = i.ToString() + " " + turretPrefabObjects[i].name; // Needs to have index or duplicate names in popup will be marged
                }

                turretSettings.horizontalPivotIndex = EditorGUILayout.Popup("Horizontal Pivot", turretSettings.horizontalPivotIndex, turretPrefabObjectNames);
                turretSettings.verticalPivotIndex = EditorGUILayout.Popup("Vertical Pivot", turretSettings.verticalPivotIndex, turretPrefabObjectNames);
                turretSettings.weaponHolderIndex = EditorGUILayout.Popup("Weapon Holder", turretSettings.weaponHolderIndex, turretPrefabObjectNames);
            }

            turretSettings.isAutoTurret = EditorGUILayout.Toggle("Is Automatic Turret", turretSettings.isAutoTurret);
            turretSettings.isTrackable = EditorGUILayout.Toggle("Is Trackable", turretSettings.isTrackable);
            turretSettings.team = (Team)EditorGUILayout.ObjectField("Team", turretSettings.team, typeof(Team), false);

        }
    }
}
