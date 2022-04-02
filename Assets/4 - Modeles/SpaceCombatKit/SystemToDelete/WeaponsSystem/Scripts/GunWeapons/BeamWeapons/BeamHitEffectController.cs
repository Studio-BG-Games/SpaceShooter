using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    
	/// <summary>
    /// Controls the hit effect that is shown when a beam strikes a surface.
    /// </summary>
	public class BeamHitEffectController : MonoBehaviour 
	{

        [Header("Events")]

        public UnityEvent onActivated;
        public UnityEvent onDeactivated;

        public UnityEvent onHit;


        protected virtual void Reset()
        {
            EffectsColorManager effectsColorManager = GetComponentInChildren<EffectsColorManager>();
            if (effectsColorManager == null)
            {
                effectsColorManager = gameObject.AddComponent<EffectsColorManager>();
            }
        }

        /// <summary>
        /// Set the 'on' level of the hit effect.
        /// </summary>
        /// <param name="level">The 'on' level.</param>
        public virtual void SetLevel(float level) 
        {
            
        }

        /// <summary>
        /// Set the activation of the hit effect.
        /// </summary>
        /// <param name="activate">Whether it is activated or not.</param>
        public virtual void SetActivation(bool activate)
        {

            gameObject.SetActive(activate);

            // Call the right event
            if (activate)
            {
                onActivated.Invoke();
            }
            else
            {
                onDeactivated.Invoke();
            }
        }

        /// <summary>
        /// Do stuff when the beam hit something.
        /// </summary>
        /// <param name="hit">The hit information.</param>
        public virtual void OnHit(RaycastHit hit)
        {
            gameObject.SetActive(true);
            transform.position = hit.point;
            transform.rotation = Quaternion.LookRotation(hit.normal);

            // Call the event
            onHit.Invoke();
        }
    }
}