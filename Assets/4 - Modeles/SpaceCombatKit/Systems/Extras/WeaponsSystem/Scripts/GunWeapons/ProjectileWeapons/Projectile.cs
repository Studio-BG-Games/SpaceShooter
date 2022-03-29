using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Base class for a projectile.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [SerializeField]
        protected HealthModifier healthModifier;
        public HealthModifier HealthModifier { get { return healthModifier; } }

        [SerializeField]
        protected CollisionScanner collisionScanner;

        [SerializeField]
        protected Detonator detonator;

        protected Transform senderRootTransform;
        protected List<IRootTransformUser> rootTransformUsers = new List<IRootTransformUser>();

        [SerializeField]
        protected float speed = 100;

        [Header("Disable After Lifetime")]

        [SerializeField]
        protected bool disableAfterLifetime = false;

        [SerializeField]
        protected float lifetime = 3;
        protected float lifeStartTime;

        [Header("Disable After Distance")]

        [SerializeField]
        protected bool disableAfterDistanceCovered = true;
        protected Vector3 lastPosition;
        protected float distanceCovered = 0;

        [SerializeField]
        protected float maxDistance = 1000;


        protected virtual void Reset()
        {
            collisionScanner = transform.GetComponent<CollisionScanner>();
            if (collisionScanner == null)
            {
                collisionScanner = gameObject.AddComponent<CollisionScanner>();
            }

            detonator = transform.GetComponent<Detonator>();
            if (detonator == null)
            {
                detonator = gameObject.AddComponent<Detonator>();
            }
        }


        protected virtual void Awake()
        {
            rootTransformUsers = new List<IRootTransformUser>(GetComponentsInChildren<IRootTransformUser>());

            if (collisionScanner != null) collisionScanner.onHitDetected.AddListener(OnCollision);
        }


        protected virtual void OnEnable()
        {
            lastPosition = transform.position;
            distanceCovered = 0;
            lifeStartTime = Time.time;
        }


        /// <summary>
        /// Set the sender's root transform.
        /// </summary>
        /// <param name="senderRootTransform">The sender's root transform.</param>
        public virtual void SetSenderRootTransform(Transform senderRootTransform)
        {
            this.senderRootTransform = senderRootTransform;

            for (int i = 0; i < rootTransformUsers.Count; ++i)
            {
                rootTransformUsers[i].RootTransform = senderRootTransform;
            }
        }


        public virtual float Damage(HealthType healthType)
        {
            return healthModifier.GetDamage(healthType);
        }


        public virtual void AddVelocity(Vector3 addedVelocity) { }


        public virtual float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public virtual float Range
        {
            get { return Mathf.Min(disableAfterLifetime ? lifetime * Speed : Mathf.Infinity, disableAfterDistanceCovered ? maxDistance : Mathf.Infinity); }
        }

        protected virtual void OnCollision(RaycastHit hit)
        {
            DamageReceiver damageReceiver = hit.collider.GetComponent<DamageReceiver>();
            if (damageReceiver != null)
            {
                // Damage

                float damageValue = healthModifier.GetDamage(damageReceiver.HealthType);

                if (damageValue != 0)
                {
                    damageReceiver.Damage(damageValue, hit.point, healthModifier.HealthModifierType, senderRootTransform);
                }

                // Healing

                float healingValue = healthModifier.GetHealing(damageReceiver.HealthType);

                if (healingValue != 0)
                {
                    damageReceiver.Heal(healingValue, hit.point, healthModifier.HealthModifierType, senderRootTransform);
                }
            }

            detonator.Detonate(hit);
        }

        protected virtual void MovementUpdate()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        protected virtual void DisableProjectile()
        {
            gameObject.SetActive(false);
        }

        protected virtual void Update()
        {
            MovementUpdate();

            distanceCovered += (transform.position - lastPosition).magnitude;

            if (disableAfterLifetime)
            {
                if (Time.time - lifeStartTime > lifetime)
                {
                    DisableProjectile();
                }
            }
            
            if (disableAfterDistanceCovered)
            {
                if (distanceCovered >= maxDistance)
                {
                    DisableProjectile();
                }
            }

            lastPosition = transform.position;
        }
    }
}