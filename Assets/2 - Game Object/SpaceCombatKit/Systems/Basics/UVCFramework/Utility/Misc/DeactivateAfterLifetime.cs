using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Deactivate a gameobject after a set period of time has passed since it was activated.
    /// </summary>
    public class DeactivateAfterLifetime : MonoBehaviour
    {
        
        [SerializeField]
        protected float lifeTime;

        protected float startTime;


        // Reset lifetime beginning point when enabled
        protected virtual void OnEnable()
        {
            startTime = Time.time;
        }

        // Called every frame
        private void Update()
        {
            if (Time.time - startTime > lifeTime)
            {
                gameObject.SetActive(false);
            }
        }
    }
}