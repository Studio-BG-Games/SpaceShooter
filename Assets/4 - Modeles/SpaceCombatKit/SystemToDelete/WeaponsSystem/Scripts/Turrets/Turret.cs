using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// A turret is an gimballed weapon that can be operated manually by the player or completely independently.
    /// </summary>
    public class Turret : MonoBehaviour
    {
        [SerializeField]
        protected TurretMode turretMode;
        public TurretMode TurretMode
        {
            get { return turretMode; }
            set { turretMode = value; }
        }

        [SerializeField]
        protected bool lockToTarget = false;

        [Header("Aim Assist")]

        [SerializeField]
        protected bool aimAssist = true;

        [SerializeField]
        protected float aimAssistAngleThreshold = 3;

        [SerializeField]
        protected Transform aimAssistReferenceTransform;
        public Transform AimAssistReferenceTransform
        {
            get { return aimAssistReferenceTransform; }
            set { aimAssistReferenceTransform = value; }
        }

        [Header("Components")]

        [Tooltip("The turret gimbal controller.")]
        [SerializeField]
        protected GimbalController gimbalController;
        public GimbalController GimbalController { get { return gimbalController; } }

        [Tooltip("The weapon component for this turret.")]
        [SerializeField]
        protected Weapon weapon;
        public Weapon Weapon
        {
            get { return weapon; }
            set { weapon = value; }
        }

        [SerializeField]
        protected Trackable target;

        [SerializeField]
        protected TargetSelector targetSelector;
        public TargetSelector TargetSelector
        {
            get { return targetSelector; }
            set { targetSelector = value; }
        }

        [SerializeField]
        protected bool leadTarget = true;

        protected float firingAngle;
       


        // Called when this component is first added to a gameobject, or when it is reset in the inspector
        protected virtual void Reset()
        {
            // Get gimbal controller
            gimbalController = GetComponentInChildren<GimbalController>();
            if (gimbalController == null)
            {
                gimbalController = gameObject.AddComponent<GimbalController>();
            }

            // Get weapon
            weapon = GetComponentInChildren<Weapon>();
          
            // Get/add a target selector
            targetSelector = GetComponentInChildren<TargetSelector>();

            if (weapon != null)
            {
                if (weapon.WeaponUnits.Count > 0)
                {
                    aimAssistReferenceTransform = weapon.WeaponUnits[0].transform;
                }
            }
        }


        public virtual void SetTarget(Trackable target)
        {
            this.target = target;
        }


        protected virtual void UpdateTarget()
        {
            if (targetSelector != null && targetSelector.SelectedTarget != target) 
                SetTarget(targetSelector.SelectedTarget);
        }


        protected virtual void TrackTarget()
        {
            if (target == null) return;

            Vector3 targetPosition = target.transform.position;
            if (leadTarget && target.Rigidbody != null)
            {
                targetPosition = TargetLeader.GetLeadPosition(gimbalController.VerticalPivot.position, weapon.Speed, targetPosition, target.Rigidbody.velocity);
            }

            gimbalController.TrackPosition(targetPosition, out firingAngle, lockToTarget);

            if (aimAssist)
            {
                if (Vector3.Angle(aimAssistReferenceTransform.forward, targetPosition - aimAssistReferenceTransform.position) < aimAssistAngleThreshold)
                {
                    weapon.Aim(targetPosition);
                }
                else
                {
                    weapon.ClearAim();
                }
            }
        }


        protected virtual void TurretControlUpdate() { }


        // Called every frame
        protected virtual void Update()
        {
            switch (turretMode)
            {
                case TurretMode.Auto:

                    UpdateTarget();
                    break;

                case TurretMode.Independent:

                    UpdateTarget();
                    TurretControlUpdate();

                    break;
            }
        }
    }
}
