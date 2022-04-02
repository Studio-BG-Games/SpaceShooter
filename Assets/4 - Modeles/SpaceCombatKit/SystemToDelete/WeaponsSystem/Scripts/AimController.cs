using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.CameraSystem;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{
    [DefaultExecutionOrder(50)]
    public class AimController : ModuleManager
    {

        [Header("Aim Assist")]

        [SerializeField]
        protected bool aimAssist = true;
        public bool AimAssist
        {
            get { return aimAssist; }
        }

        protected List<IAimer> aimers = new List<IAimer>();

        [SerializeField]
        protected float aimAssistAngle = 3.3f;

        [SerializeField]
        protected float aimAssistRange = 1000;

        [SerializeField]
        protected Color noAimAssistColor = Color.white;

        [SerializeField]
        protected Color aimAssistColor = Color.red;

        [SerializeField]
        protected UIColorManager hudCursorColorManager;

        [Header("Raycast Aiming")]

        [SerializeField]
        protected LayerMask raycastAimMask;

        [SerializeField]
        protected Transform aimOrigin;

        [SerializeField]
        protected Transform aimPositionMarker;

        [SerializeField]
        protected bool ignoreTriggerColliders = true;

        [SerializeField]
        protected bool useCameraAsRaycastOrigin = true;

        [SerializeField]
        protected CameraTarget cameraTarget;

        [Header("Cursor Aiming")]

        [SerializeField]
        protected bool cursorAimingEnabled = true;

        [SerializeField]
        protected HUDCursor cursor;

        [Header("Aim-Based Target Selection")]

        [SerializeField]
        protected bool aimTargetSelectionEnabled = true;

        [SerializeField]
        protected float targetSelectionAngle = 3;

        [SerializeField]
        protected Tracker tracker;

        [SerializeField]
        protected TargetSelector targetSelector;

        [SerializeField]
        protected Weapons weapons;

        [Header("Components")]

        [SerializeField]
        protected Rigidbody m_Rigidbody;

        protected virtual void Reset()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            tracker = GetComponent<Tracker>();
            cameraTarget = GetComponent<CameraTarget>();

            targetSelector = transform.GetComponentInChildren<TargetSelector>();

            cursor = transform.GetComponentInChildren<HUDCursor>();

            weapons = GetComponent<Weapons>();

            if (cursor != null)
            {
                hudCursorColorManager = cursor.GetComponentInChildren<UIColorManager>();
            }

            aimOrigin = transform;

            raycastAimMask = ~0;
        }

        protected override void Awake()
        {
            base.Awake();

            if (cameraTarget != null)
            {
                cameraTarget.onCameraTargeting.AddListener(OnCameraFollowing);
            }
        }


        // Called when a module is mounted on one of the vehicle's module mounts.
        protected override void OnModuleMounted(Module module)
        {
            // Store aim assist reference
            IAimer aimer = module.GetComponentInChildren<IAimer>();
            if (aimer != null)
            {
                aimers.Add(aimer);
            }
        }


        // Called when a module is unmounted from one of the vehicle's module mounts.
        protected override void OnModuleUnmounted(Module module)
        {
            // Remove aim assist reference
            IAimer aimer = module.GetComponentInChildren<IAimer>();
            if (aimer != null)
            {
                if (aimers.Contains(aimer))
                {
                    aimers.Remove(aimer);
                }
            }
        }


        public virtual void OnCameraFollowing(Camera cam)
        {
            if (useCameraAsRaycastOrigin && cam != null) aimOrigin = cam.transform;
        }


        protected virtual bool UpdateAimAssist(Vector3 aimDirection, Trackable target, out Vector3 _aimPosition)
        {
            if (hudCursorColorManager != null) hudCursorColorManager.SetColor(noAimAssistColor);

            _aimPosition = Vector3.zero;

            if (!aimAssist) return false;

            if (target == null) return false;

            _aimPosition = target.transform.position;
            if (weapons != null)
            {
                _aimPosition = weapons.GetAverageLeadTargetPosition(target.transform.TransformPoint(target.TrackingBounds.center), target.Rigidbody != null ? target.Rigidbody.velocity : Vector3.zero);
            }
          
            if (Vector3.Distance(aimOrigin.position, _aimPosition) > aimAssistRange)
            {
                return false;
            }

            if (Vector3.Angle(aimDirection, (_aimPosition - aimOrigin.position)) > aimAssistAngle)
            {
                return false;
            }

            if (targetSelector.SelectedTarget != null && !HasLineOfSight(_aimPosition, targetSelector.SelectedTarget))
            {
                return false;
            }

            // Aim assist found

            if (hudCursorColorManager != null)
            {
                hudCursorColorManager.SetColor(aimAssistColor);
            }

            return true;
        }


        public virtual void Aim()
        {

            if (aimOrigin == null)
            {
                return;
            }

            // Calculate the raycast direction
            Vector3 aimDirection = aimOrigin.forward;
            if (cursorAimingEnabled && cursor != null)
            {
                aimDirection = cursor.AimDirection;
            }

            // Update target selection
            if (aimTargetSelectionEnabled && targetSelector != null)
            {
                for (int i = 0; i < tracker.Targets.Count; ++i)
                {
                    Vector3 toTarget = tracker.Targets[i].transform.position - aimOrigin.position;
                    if (Vector3.Angle(toTarget, aimDirection) < targetSelectionAngle)
                    {
                        targetSelector.Select(tracker.Targets[i]);
                        break;
                    }
                }
            }


            Vector3 aimPosition = aimOrigin.position + aimDirection * 100;
            if (targetSelector != null && !UpdateAimAssist(aimDirection, targetSelector.SelectedTarget, out aimPosition))
            {
                // Get all raycast hits
                RaycastHit[] hits = Physics.RaycastAll(aimOrigin.position, aimDirection, aimAssistRange, raycastAimMask,
                                                            ignoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
                
                // Sort hits by distance
                List<RaycastHit> sortedHits = SortRaycastHitsByDistance(hits);

                // Discard hits on self and find one that is valid
                bool found = false;
                for (int i = 0; i < sortedHits.Count; ++i)
                {
                    if (sortedHits[i].collider.attachedRigidbody == m_Rigidbody)
                    {
                        continue;
                    }
                    
                    aimPosition = sortedHits[i].point;

                    found = true;

                    break;

                }
                
                // If no valid hits found, aim at the max range units along the aim direction
                if (!found)
                {
                    aimPosition = aimOrigin.position + aimDirection * aimAssistRange;
                }
            }
            
            for (int i = 0; i < aimers.Count; ++i)
            {
                aimers[i].Aim(aimPosition);
            }

            if (aimPositionMarker != null) aimPositionMarker.position = aimPosition;
        }


        bool HasLineOfSight(Vector3 aimTarget, Trackable target)
        {

            Vector3 raycastDirection = (aimTarget - aimOrigin.position).normalized;

            float raycastLength = (aimTarget - aimOrigin.position).magnitude;

            // Get all raycast hits
            RaycastHit[] hits = Physics.RaycastAll(aimOrigin.position, raycastDirection, raycastLength, raycastAimMask,
                                                        ignoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);

            // Sort hits by distance
            List<RaycastHit> sortedHits = SortRaycastHitsByDistance(hits);

            // Discard hits on self and find one that is valid
            for (int i = 0; i < sortedHits.Count; ++i)
            {
                if (sortedHits[i].collider.attachedRigidbody != null)
                {
                    // Ignore hits on self
                    if (sortedHits[i].collider.attachedRigidbody == m_Rigidbody)
                    {
                        continue;
                    }

                    // Ignore hits on target
                    if (sortedHits[i].collider.attachedRigidbody == target.Rigidbody)
                    {
                        continue;
                    }
                }


                // If first valid hit is less than distance to aim target, it's blocked
                if (sortedHits[i].distance < Vector3.Distance(aimTarget, aimOrigin.position) - 0.0001f)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return true;
        }


        protected virtual void Update()
        {
            Aim();
        }


        public List<RaycastHit> SortRaycastHitsByDistance(RaycastHit[] hits)
        {
            List<RaycastHit> sortedHits = new List<RaycastHit>();

            for (int i = 0; i < hits.Length; ++i)
            {
                if (sortedHits.Count == 0)
                {
                    sortedHits.Add(hits[i]);
                }
                else
                {
                    for (int j = 0; j < sortedHits.Count; ++j)
                    {

                        if (sortedHits[j].distance > hits[i].distance)
                        {
                            sortedHits.Insert(j, hits[i]);
                            break;
                        }

                        if (j == sortedHits.Count - 1)
                        {
                            sortedHits.Add(hits[i]);
                            break;
                        }
                    }
                }
            }

            return sortedHits;
        }
    }
}
