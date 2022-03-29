using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.CameraSystem
{
    /// <summary>
    /// Orbit the camera around a point with a specified offset and rotation axis.
    /// </summary>
    public class CameraOrbiter : MonoBehaviour
    {
        
        [Tooltip("The camera entity that this orbiting camera animates.")]
        [SerializeField]
        protected CameraEntity m_CameraEntity;

        [Tooltip("The point around which the camera will orbit.")]
        [SerializeField]
        protected Vector3 orbitTargetPosition;

        [Tooltip("The axis around which the camera will orbit.")]
        [SerializeField]
        protected Vector3 orbitRotationAxis = Vector3.up;

        [Tooltip("The position offset to maintain when orbiting.")]
        [SerializeField]
        protected Vector3 orbitOffset = new Vector3(0, 3, 10);

        [Tooltip("The orbit speed.")]
        [SerializeField]
        protected float orbitSpeed = 20;
        
        [SerializeField]
        protected bool orbiting = false;


        protected virtual void Reset()
        {
            m_CameraEntity = GetComponent<CameraEntity>();
        }

        protected virtual void Start()
        {
            if (orbiting)
            {
                Orbit(orbitTargetPosition);
            }
        }

        /// <summary>
        /// Begin orbiting around a point.
        /// </summary>
        /// <param name="orbitTargetPosition">The orbit target position.</param>
        public virtual void Orbit(Vector3 orbitTargetPosition)
        {
            this.orbitTargetPosition = orbitTargetPosition;
            m_CameraEntity.CameraControlEnabled = false;

            m_CameraEntity.transform.position = Quaternion.LookRotation(Vector3.forward, orbitRotationAxis) * orbitOffset + orbitTargetPosition;
            orbiting = true;
        }

        
        // Called every frame
        protected virtual void FixedUpdate()
        {
            if (orbiting)
            {
                m_CameraEntity.transform.RotateAround(orbitTargetPosition, orbitRotationAxis, orbitSpeed * Time.fixedDeltaTime);
                m_CameraEntity.transform.LookAt(orbitTargetPosition, orbitRotationAxis);
            }
        }
    }
}