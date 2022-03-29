using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Controls a beam for e.g. a weapon.
    /// </summary>
    public class HitScanWeaponUnit : WeaponUnit, IRootTransformUser
    {

        [Header("Hit Scan")]

        [SerializeField]
        protected Transform spawnPoint;
        public override void Aim(Vector3 aimPosition) { spawnPoint.LookAt(aimPosition, transform.up); }
        public override void ClearAim() { spawnPoint.localRotation = Quaternion.identity; }

        [SerializeField]
        protected float range = 1000;
        public override float Range { get { return range; } }

        [SerializeField]
        protected List<GameObject> hitPointSpawnObjects = new List<GameObject>();

        [Tooltip("The layer mask of colliders that can be hit.")]
        [SerializeField]
        protected LayerMask hitMask = Physics.DefaultRaycastLayers;

        [Tooltip("Whether to ignore trigger colliders.")]
        [SerializeField]
        protected bool ignoreTriggerColliders = true;

        [Tooltip("Whether to ignore collision with the object or vehicle that this object came from.")]
        [SerializeField]
        protected bool ignoreHierarchyCollision = true;

        [Header("Damage/Healing")]

        // Whether to apply the damage amount on a per-second basis (e.g. for laser beams).
        [SerializeField]
        protected bool timeBasedDamageHealing = false;
        public bool TimeBasedDamageHealing
        {
            get { return timeBasedDamageHealing; }
            set { timeBasedDamageHealing = value; }
        }

        [SerializeField]
        protected HealthModifier healthModifier;
        public HealthModifier HealthModifier { get { return healthModifier; } }

        public override float Damage(HealthType healthType)
        {

            return healthModifier.GetDamage(healthType);

        }

        [Header("Events")]

        public UnityEvent onHitScan;

        public RaycastHitEventHandler onHit;

        public UnityEvent onNoHit;

        protected Transform rootTransform;
        public Transform RootTransform
        {
            set { rootTransform = value; }
        }

        protected RaycastHit hit;

        public override float Speed
        {
            get { return Mathf.Infinity; }
        }


        protected virtual void Reset()
        {
            spawnPoint = transform;
        }


        // Called when scene starts
        protected virtual void Start()
        {
            if (rootTransform == null) rootTransform = transform.root;
        }


        // Do a hit scan
        public virtual void HitScan()
        {
            // Raycast
            RaycastHit[] hits;
            hits = Physics.RaycastAll(spawnPoint.position, spawnPoint.forward, range, hitMask, ignoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));    // Sort by distance

            for (int i = 0; i < hits.Length; ++i)
            {
                DamageReceiver damageReceiver = hits[i].collider.GetComponent<DamageReceiver>();
                if (ignoreHierarchyCollision && rootTransform != null && hits[i].collider.attachedRigidbody != null &&
                    hits[i].collider.attachedRigidbody.transform == rootTransform)
                {
                    continue;
                }

                OnHit(hits[i]);

                RaycastHitDamage(hits[i]);

                onHitScan.Invoke();

                return;
            }

            onHitScan.Invoke();

            // No hits detected
            OnNoHit();
        }


        protected virtual void OnHit(RaycastHit hit)
        {
            for(int i = 0; i < hitPointSpawnObjects.Count; ++i)
            {
                Instantiate(hitPointSpawnObjects[i], hit.point, Quaternion.identity);
            }

            onHit.Invoke(hit);
        }

        protected virtual void OnNoHit() 
        {
            onNoHit.Invoke();
        }

        public virtual void RaycastHitDamage(RaycastHit hit)
        {

            DamageReceiver damageReceiver = hit.collider.GetComponent<DamageReceiver>();
            if (damageReceiver != null)
            {
                // Damage

                float damageValue = healthModifier.GetDamage(damageReceiver.HealthType) * (timeBasedDamageHealing ? Time.deltaTime : 1);

                if (damageValue != 0)
                {
                    damageReceiver.Damage(damageValue, hit.point, healthModifier.HealthModifierType, rootTransform);
                }

                // Healing

                float healingValue = healthModifier.GetHealing(damageReceiver.HealthType) * (timeBasedDamageHealing ? Time.deltaTime : 1);

                if (healingValue != 0)
                {
                    damageReceiver.Heal(healingValue, hit.point, healthModifier.HealthModifierType, rootTransform);
                }
            }
        }
    }
}