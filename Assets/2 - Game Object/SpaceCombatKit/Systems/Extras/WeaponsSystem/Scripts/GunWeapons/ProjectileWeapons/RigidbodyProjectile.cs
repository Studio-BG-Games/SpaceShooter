using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Base class for a projectile.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyProjectile : Projectile
    {
        [Header("Components")]

        [SerializeField]
        protected Rigidbody m_Rigidbody;
        public Rigidbody Rigidbody { get { return m_Rigidbody; } }

        public enum ProjectilePropulsionType
        {
            Speed,
            Force
        }

        [Header("Physics")]

        [SerializeField]
        protected ProjectilePropulsionType propulsionType;

        [SerializeField]
        protected float force = 500;


        protected override void Reset()
        {
            base.Reset();

            // Get/add a rigidbody
            m_Rigidbody = GetComponent<Rigidbody>();
            
            // Initialize the rigidbody settings
            m_Rigidbody.useGravity = false;
            m_Rigidbody.drag = 0;
        }


        protected override void Awake()
        {
            base.Awake();

            m_Rigidbody = GetComponent<Rigidbody>();
        }


        // Reset rigidbody when enabled
        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_Rigidbody.velocity = m_Rigidbody.transform.forward * speed;
            m_Rigidbody.angularVelocity = Vector3.zero;
        }


        public override float Damage(HealthType healthType)
        {
            for(int i = 0; i < healthModifier.DamageOverrideValues.Count; ++i)
            {
                if (healthModifier.DamageOverrideValues[i].HealthType == healthType)
                {
                    return healthModifier.DamageOverrideValues[i].Value;
                }
            }

            return healthModifier.DefaultDamageValue;
        }


        public override void AddVelocity(Vector3 addedVelocity)
        {
            m_Rigidbody.velocity += addedVelocity;
        }


        public override float Speed
        {
            get 
            { 
                if (propulsionType == ProjectilePropulsionType.Force)
                {
                    return ((force / m_Rigidbody.mass) / m_Rigidbody.drag);
                }
                else
                {
                    return speed;
                }
            }

            set { m_Rigidbody.velocity = Vector3.forward * value; }
        }


        public virtual void SetRigidbodyKinematic() 
        {
            m_Rigidbody.isKinematic = true;
        }

        public virtual void SetRigidbodyNonKinematic()
        {
            m_Rigidbody.isKinematic = false;
        }

        // Don't apply movement as this is being done in fixed update
        protected override void MovementUpdate() { }

        // Physics update
        protected virtual void FixedUpdate()
        {
            if (propulsionType == ProjectilePropulsionType.Force)
            {
                m_Rigidbody.AddRelativeForce(Vector3.forward * force);
            }
        }
    }
}